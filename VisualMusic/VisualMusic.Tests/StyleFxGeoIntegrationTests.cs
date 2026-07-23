using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Midi;
using VisualMusic.MonoGameInterop;
using Xunit;

namespace VisualMusic.Tests
{
    /// <summary>
    /// Successful FX load + geo bake with real MonoGame Content and a GraphicsDevice.
    /// Unit tests only prove the retry path via ContentLoadException on a missing root.
    /// </summary>
    public class StyleFxGeoIntegrationTests
    {
        const string WindowClassName = "VMStyleFxGeoTestHost";

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern ushort RegisterClassEx(ref WndClassEx lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateWindowEx(uint exStyle, string className, string windowName,
            uint style, int x, int y, int width, int height,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WndClassEx
        {
            public uint cbSize;
            public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        // Keep the delegate alive for the process (RegisterClassEx requirement).
        static WndProc s_wndProc;
        static bool s_classRegistered;

        static string EnsureContentRoot()
        {
            string beside = Path.Combine(AppContext.BaseDirectory, "Content");
            if (File.Exists(Path.Combine(beside, "Bar.xnb")) &&
                File.Exists(Path.Combine(beside, "Line.xnb")))
                return beside;

            throw new FileNotFoundException(
                "MonoGame Content (Bar.xnb / Line.xnb) not found beside the test assembly under Content\\. " +
                "Build VisualMusic (mgcb) so Content\\bin\\Windows exists; VisualMusic.Tests copies *.xnb into output.");
        }

        static IntPtr CreateHostWindow()
        {
            if (!s_classRegistered)
            {
                s_wndProc = DefWindowProc;
                var wc = new WndClassEx
                {
                    cbSize = (uint)Marshal.SizeOf<WndClassEx>(),
                    lpfnWndProc = s_wndProc,
                    hInstance = GetModuleHandle(null),
                    lpszClassName = WindowClassName,
                };
                ushort atom = RegisterClassEx(ref wc);
                if (atom == 0)
                {
                    const int ERROR_CLASS_ALREADY_EXISTS = 1410;
                    int err = Marshal.GetLastWin32Error();
                    if (err != ERROR_CLASS_ALREADY_EXISTS)
                        throw new InvalidOperationException($"RegisterClassEx failed: {err}");
                }
                s_classRegistered = true;
            }

            const uint WS_POPUP = 0x80000000;
            IntPtr hwnd = CreateWindowEx(0, WindowClassName, "",
                WS_POPUP, 0, 0, 64, 64,
                IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException($"CreateWindowEx failed: {Marshal.GetLastWin32Error()}");
            return hwnd;
        }

        static Project BuildProjectWithNoteOnTrack1()
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>
                {
                    new Track { Length = 480 },
                    new Track
                    {
                        Length = 480,
                        Notes = new List<Note>
                        {
                            new Note { start = 0, stop = 120, channel = 0, pitch = 60, velocity = 100 },
                        },
                    },
                },
                SongLengthT = 480,
            };
            var project = new Project();
            project.Notes = song;
            return project;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetContent_then_SetGraphicsDevice_then_SetProject_bakes_geo()
        {
            // Content installed before Project (Initialize vs Open race). SInitAllStyles must run with
            // a GraphicsDevice before geo bake (same as SongRenderer.Initialize).
            string contentRoot = EnsureContentRoot();
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager cm = null;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                Assert.Null(project.TrackViews[1].Geo);
                Assert.False(NoteStyle.HasContent);
                Assert.False(NoteStyle.CanCreateGeo);

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                var svc = new ServiceContainer();
                svc.AddService<IGraphicsDeviceService>(gds);
                cm = new ContentManager(svc, contentRoot);

                NoteStyle.SetContent(cm);
                Assert.True(NoteStyle.HasContent);
                Assert.False(NoteStyle.CanCreateGeo);
                Assert.Null(project.TrackViews[1].Geo);

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SInitAllStyles();
                Assert.True(NoteStyle.CanCreateGeo);
                Assert.Null(project.TrackViews[1].Geo); // Project not set yet — no deferred bake

                NoteStyle.SetProject(project);
                Assert.NotNull(project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                NoteStyle.SetGraphicsDevice(null);
                cm?.Dispose();
                gds?.Release(disposing: true);
                if (hwnd != IntPtr.Zero)
                    DestroyWindow(hwnd);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasGraphicsDevice);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetGraphicsDevice_first_then_SetContent_with_project_bakes_geo()
        {
            // Reverse order: GD + SInit first; SetContent with a current Project finishes the bake.
            string contentRoot = EnsureContentRoot();
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager cm = null;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                Assert.Null(project.TrackViews[1].Geo);

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                var svc = new ServiceContainer();
                svc.AddService<IGraphicsDeviceService>(gds);
                cm = new ContentManager(svc, contentRoot);

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SInitAllStyles();
                NoteStyle.SetProject(project);
                Assert.Null(project.TrackViews[1].Geo); // still no Content

                NoteStyle.SetContent(cm);
                Assert.True(NoteStyle.CanCreateGeo);
                Assert.NotNull(project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                NoteStyle.SetGraphicsDevice(null);
                cm?.Dispose();
                gds?.Release(disposing: true);
                if (hwnd != IntPtr.Zero)
                    DestroyWindow(hwnd);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasGraphicsDevice);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Undo_LoadStyleFxAndCreateGeos_resetVertScale_rebakes_at_restored_ViewWidthQn()
        {
            // ApplyUndoItem: CopyPropsFrom keeps live Geo (stale RefWidthQn) while restoring SpatialProps.
            // CreateGeos(false) would re-bake at Geo.RefWidthQn; undo must pass resetVertScale: true.
            string contentRoot = EnsureContentRoot();
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager cm = null;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                var svc = new ServiceContainer();
                svc.AddService<IGraphicsDeviceService>(gds);
                cm = new ContentManager(svc, contentRoot);

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SInitAllStyles();
                NoteStyle.SetContent(cm);
                NoteStyle.SetProject(project);
                Assert.NotNull(project.TrackViews[1].Geo);
                Assert.Equal(ProjProps.DefaultViewWidthQn, project.TrackViews[1].Geo.RefWidthQn);

                var undo = new UndoItems();
                undo.Add("before width", project);

                const float widenedQn = 32f;
                project.TrackViews[1].TrackProps.SpatialProps.ViewWidthQn = widenedQn;
                project.CreateGeos(); // UI commit path: resetVertScale true
                Assert.Equal(widenedQn, project.TrackViews[1].Geo.RefWidthQn);

                undo.Add("after width", project);
                undo--;
                project.CopyPropsFrom(undo.Current.Project);
                Assert.Equal(ProjProps.DefaultViewWidthQn,
                    project.EffectiveViewWidthQn(project.TrackViews[1].TrackProps));
                // Live Geo still reflects the widened bake until LoadStyleFxAndCreateGeos.
                Assert.Equal(widenedQn, project.TrackViews[1].Geo.RefWidthQn);

                project.LoadStyleFxAndCreateGeos(resetVertScale: false);
                Assert.Equal(widenedQn, project.TrackViews[1].Geo.RefWidthQn); // stale path

                project.LoadStyleFxAndCreateGeos(resetVertScale: true);
                Assert.Equal(ProjProps.DefaultViewWidthQn, project.TrackViews[1].Geo.RefWidthQn);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                NoteStyle.SetGraphicsDevice(null);
                cm?.Dispose();
                gds?.Release(disposing: true);
                if (hwnd != IntPtr.Zero)
                    DestroyWindow(hwnd);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasGraphicsDevice);
            Assert.False(NoteStyle.HasProject);
        }
    }
}
