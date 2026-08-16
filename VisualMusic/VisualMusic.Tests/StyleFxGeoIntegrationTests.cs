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
                    new Track { Length = 6000 },
                    new Track
                    {
                        Length = 6000,
                        Notes = new List<Note>
                        {
                            new Note { start = 1920, stop = 3360, channel = 0, pitch = 70, velocity = 100 },
                            new Note { start = 3360, stop = 3600, channel = 0, pitch = 70, velocity = 100 },
                            new Note { start = 3600, stop = 3840, channel = 0, pitch = 69, velocity = 100 },
                            new Note { start = 3600, stop = 3840, channel = 0, pitch = 69, velocity = 100 },
                            new Note { start = 4320, stop = 5279, channel = 0, pitch = 67, velocity = 100 },
                            new Note { start = 5280, stop = 5520, channel = 0, pitch = 65, velocity = 100 },
                        },
                    },
                },
                SongLengthT = 6000,
            };
            var project = new Project();
            project.Notes = song;
            // Synthetic Song does not run the MIDI parser that populates Song.MinPitch/MaxPitch.
            project.Props.MinPitch = 65;
            project.Props.MaxPitch = 70;
            return project;
        }

        static Project BuildSharpPeakProject()
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>
                {
                    new Track { Length = 10000 },
                    new Track
                    {
                        Length = 10000,
                        Notes = new List<Note>
                        {
                            new Note { start = 5520, stop = 5760, channel = 0, pitch = 72, velocity = 100 },
                            new Note { start = 6000, stop = 6720, channel = 0, pitch = 72, velocity = 100 },
                            new Note { start = 6480, stop = 6960, channel = 0, pitch = 82, velocity = 100 },
                            new Note { start = 7200, stop = 9120, channel = 0, pitch = 69, velocity = 100 },
                            new Note { start = 8160, stop = 8640, channel = 0, pitch = 67, velocity = 100 },
                        },
                    },
                },
                SongLengthT = 10000,
            };
            var project = new Project { Notes = song };
            project.Props.MinPitch = 67;
            project.Props.MaxPitch = 82;
            return project;
        }

        static Project BuildGentleSeamsProject()
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>
                {
                    new Track { Length = 7000 },
                    new Track
                    {
                        Length = 7000,
                        Notes = new List<Note>
                        {
                            new Note { start = 480, stop = 1439, channel = 0, pitch = 65, velocity = 50 },
                            new Note { start = 1440, stop = 1680, channel = 0, pitch = 65, velocity = 50 },
                            new Note { start = 1680, stop = 3359, channel = 0, pitch = 67, velocity = 37 },
                            new Note { start = 2400, stop = 3480, channel = 0, pitch = 67, velocity = 50 },
                            new Note { start = 3360, stop = 3600, channel = 0, pitch = 67, velocity = 50 },
                            new Note { start = 3600, stop = 4320, channel = 0, pitch = 69, velocity = 46 },
                            new Note { start = 4320, stop = 5040, channel = 0, pitch = 67, velocity = 50 },
                            new Note { start = 5280, stop = 5520, channel = 0, pitch = 67, velocity = 50 },
                            new Note { start = 5520, stop = 6240, channel = 0, pitch = 69, velocity = 43 },
                        },
                    },
                },
                SongLengthT = 7000,
            };
            var project = new Project { Notes = song };
            project.Props.MinPitch = 56;
            project.Props.MaxPitch = 78;
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
                Assert.False(NoteStyle.HasStylesInited);
                Assert.False(NoteStyle.CanCreateGeo);
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
        public void Zero_line_width_bakes_geometry_reused_by_width_keyframes()
        {
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
                var track = project.TrackViews[1];
                track.TrackProps.StyleProps.Type = NoteStyleType.Line;
                track.TrackProps.StyleProps.GetLineStyle().LineWidth = 0;
                project.InitPropertyAccessors();

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                var svc = new ServiceContainer();
                svc.AddService<IGraphicsDeviceService>(gds);
                cm = new ContentManager(svc, contentRoot);

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SInitAllStyles();
                NoteStyle.SetContent(cm);
                NoteStyle.SetProject(project);

                var bakedGeo = Assert.IsType<LineGeo>(track.Geo);
                Assert.True(NoteStyle_Line.HardwareTessellationAvailable);
                NoteStyle_Line.SetHardwareTessellation(true);
                Assert.True(NoteStyle_Line.ToggleHardwareTessellation());
                Assert.False(NoteStyle_Line.UseHardwareTessellation);
                Assert.True(NoteStyle_Line.ToggleHardwareTessellation());
                Assert.True(NoteStyle_Line.UseHardwareTessellation);
                Assert.NotEmpty(bakedGeo.patchVb);
                Assert.All(bakedGeo.patchVb,
                    vb => Assert.Equal(0, vb.VertexCount % 4));
                Assert.NotEmpty(bakedGeo.lineVb);

                // t.vmp contains two identical notes at tick 3600. The zero-duration interval
                // between those curve keys must not break the edge carried between valid patches.
                Assert.Equal(4, bakedGeo.seamlessPatchJoinCount);
                Assert.Equal(1, bakedGeo.hardwareStartCapCount);
                Assert.Equal(1, bakedGeo.hardwareEndCapCount);

                project.PropertyKeyframes.Add("track/1/LineWidth", 0,
                    Keyframes.KfInterpolation.Linear, new Keyframes.ScalarKfValue(0));
                project.PropertyKeyframes.Add("track/1/LineWidth", 6000,
                    Keyframes.KfInterpolation.Linear, new Keyframes.ScalarKfValue(20));
                project.NormSongPos = 0.5;
                project.InterpolatePropertyKeyframes();

                Assert.Equal(10f, track.TrackProps.StyleProps.GetLineStyle().LineWidth!.Value, 3);
                Assert.Same(bakedGeo, track.Geo);

                // Exercise both resident representations without replacing the geometry.
                track.TrackProps.StyleProps.GetLineStyle().LineWidth = 50;
                track.TrackProps.SpatialProps.ViewWidthQn = 1;
                Assert.Same(bakedGeo, track.Geo);
                track.TrackProps.SpatialProps.ViewWidthQn = 16;
                Assert.Same(bakedGeo, track.Geo);
                // At the UI maximum, the centers collapse into an almost vertical line.
                // Hardware tessellation must still account for the rotating, wide outer edges.
                track.TrackProps.SpatialProps.ViewWidthQn = 1024;
                Assert.Same(bakedGeo, track.Geo);
                project.NormSongPos = 0.96; // puts tick 1920 at the left edge; playback is after the last note

                const int captureWidth = 768;
                const int captureHeight = 456;
                var clearColor = new Microsoft.Xna.Framework.Color(1, 2, 3, 4);

                Microsoft.Xna.Framework.Color[] RenderLine(bool hardware)
                {
                    NoteStyle_Line.SetHardwareTessellation(hardware);
                    Assert.Equal(hardware, NoteStyle_Line.UseHardwareTessellation);
                    using var target = new RenderTarget2D(gds.GraphicsDevice,
                        captureWidth, captureHeight, false,
                        SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                    gds.GraphicsDevice.SetRenderTarget(target);
                    gds.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                    gds.GraphicsDevice.Clear(clearColor);
                    try
                    {
                        track.DrawTrack(project.GlobalTrackProps, defaultStyle: false);
                    }
                    finally
                    {
                        gds.GraphicsDevice.SetRenderTarget(null);
                    }

                    var pixels = new Microsoft.Xna.Framework.Color[captureWidth * captureHeight];
                    target.GetData(pixels);
                    Assert.Contains(pixels, pixel => pixel != clearColor);
                    return pixels;
                }

                List<(int MinX, int MaxX)> GetTopProfile(
                    Microsoft.Xna.Framework.Color[] pixels, int rowCount)
                {
                    var profile = new List<(int MinX, int MaxX)>();
                    for (int y = 0; y < captureHeight && profile.Count < rowCount; y++)
                    {
                        int minX = captureWidth;
                        int maxX = -1;
                        for (int x = 0; x < captureWidth; x++)
                        {
                            if (pixels[y * captureWidth + x] == clearColor)
                                continue;
                            minX = Math.Min(minX, x);
                            maxX = Math.Max(maxX, x);
                        }
                        if (maxX >= 0)
                            profile.Add((minX, maxX));
                    }
                    return profile;
                }

                var hardwarePixels = RenderLine(hardware: true); // VS/HS/DS/PS patch path
                Assert.Same(bakedGeo, track.Geo);
                var legacyPixels = RenderLine(hardware: false);  // legacy triangle-strip path
                Assert.Same(bakedGeo, track.Geo);

                // The top of the max-width vertical line is the compressed first bend. Compare
                // its silhouette to the dense legacy mesh; centerline-only tessellation rendered
                // this as a visibly sharp corner.
                var hardwareProfile = GetTopProfile(hardwarePixels, rowCount: captureHeight);
                var legacyProfile = GetTopProfile(legacyPixels, rowCount: captureHeight);
                Assert.True(hardwareProfile.Count >= 24);
                Assert.True(legacyProfile.Count >= 24);
                for (int row = 0; row < 24; row++)
                {
                    Assert.InRange(Math.Abs(hardwareProfile[row].MinX - legacyProfile[row].MinX), 0, 2);
                    Assert.InRange(Math.Abs(hardwareProfile[row].MaxX - legacyProfile[row].MaxX), 0, 2);
                }

                // The legacy end is flat. The hardware-only semicircle extends by roughly one
                // radius and converges to a narrow final row instead of another full-width edge.
                Assert.True(hardwareProfile.Count >= legacyProfile.Count + 8);
                int widestHardwareRow = 0;
                foreach (var row in hardwareProfile)
                    widestHardwareRow = Math.Max(widestHardwareRow, row.MaxX - row.MinX + 1);
                int lastHardwareRowWidth = hardwareProfile[^1].MaxX - hardwareProfile[^1].MinX + 1;
                Assert.True(lastHardwareRowWidth < widestHardwareRow / 2);
            }
            finally
            {
                NoteStyle_Line.SetHardwareTessellation(true);
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
            Assert.False(NoteStyle_Line.HardwareTessellationAvailable);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Hardware_line_rounds_local_extremum()
        {
            string contentRoot = EnsureContentRoot();
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager cm = null;
            try
            {
                var project = BuildSharpPeakProject();
                project.Props.MaxPitch = 85;
                project.CreateTrackViews(2, eraseCurrent: true);
                var track = project.TrackViews[1];
                track.TrackProps.StyleProps.Type = NoteStyleType.Line;
                var lineStyle = track.TrackProps.StyleProps.GetLineStyle();
                lineStyle.LineWidth = 50;
                lineStyle.HlSize = 0;
                track.TrackProps.SpatialProps.ViewWidthQn = 12.771335f;

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                var svc = new ServiceContainer();
                svc.AddService<IGraphicsDeviceService>(gds);
                cm = new ContentManager(svc, contentRoot);

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SInitAllStyles();
                NoteStyle.SetContent(cm);
                NoteStyle.SetProject(project);
                project.NormSongPos = 0.72;

                var bakedGeo = Assert.IsType<LineGeo>(track.Geo);
                Assert.Equal(1, bakedGeo.hardwareRoundJoinCount);

                const int width = 768;
                const int height = 456;
                Microsoft.Xna.Framework.Color[] Render(bool hardware)
                {
                    NoteStyle_Line.SetHardwareTessellation(hardware);
                    using var target = new RenderTarget2D(gds.GraphicsDevice, width, height,
                        false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                    using var depthStencil = new DepthStencilState
                    {
                        StencilEnable = true,
                        StencilFunction = CompareFunction.Greater,
                        StencilPass = StencilOperation.Replace,
                        ReferenceStencil = 1,
                    };
                    var clearColor = new Microsoft.Xna.Framework.Color(1, 2, 3, 4);
                    gds.GraphicsDevice.SetRenderTarget(target);
                    gds.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                    var oldDepthStencil = gds.GraphicsDevice.DepthStencilState;
                    gds.GraphicsDevice.DepthStencilState = depthStencil;
                    gds.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer
                        | ClearOptions.Stencil, clearColor, 1, 0);
                    try
                    {
                        track.DrawTrack(project.GlobalTrackProps, defaultStyle: false);
                    }
                    finally
                    {
                        gds.GraphicsDevice.SetRenderTarget(null);
                        gds.GraphicsDevice.DepthStencilState = oldDepthStencil;
                    }

                    var pixels = new Microsoft.Xna.Framework.Color[width * height];
                    target.GetData(pixels);
                    Assert.Contains(pixels, pixel => pixel != clearColor);
                    return pixels;
                }

                (int Row, int Width) FirstInteriorGap(
                    Microsoft.Xna.Framework.Color[] pixels)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int firstSolid = -1;
                        int lastSolid = -1;
                        for (int x = 0; x < width; x++)
                        {
                            if (pixels[y * width + x].R < 100)
                                continue;
                            if (firstSolid < 0)
                                firstSolid = x;
                            lastSolid = x;
                        }

                        int longestGap = 0;
                        int currentGap = 0;
                        for (int x = firstSolid + 1; x < lastSolid; x++)
                        {
                            if (pixels[y * width + x].R < 100)
                            {
                                currentGap++;
                                longestGap = Math.Max(longestGap, currentGap);
                            }
                            else
                                currentGap = 0;
                        }
                        if (longestGap > 0)
                            return (y, longestGap);
                    }
                    return (-1, 0);
                }

                var hardwarePixels = Render(hardware: true);
                var legacyPixels = Render(hardware: false);
                var hardwareGap = FirstInteriorGap(hardwarePixels);
                var legacyGap = FirstInteriorGap(legacyPixels);

                Assert.True(legacyGap.Row >= 0);
                // A stretched inner disk used to push this 20+ pixels past legacy (the
                // hanging blob). Matching the offset curve's inner corner is enough to
                // hide the clamped needle without a semicircle under the arch.
                Assert.InRange(hardwareGap.Row - legacyGap.Row, -2, 8);
                Assert.InRange(hardwareGap.Width, 1, 8);

                int FirstSolidRow(Microsoft.Xna.Framework.Color[] pixels)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (pixels[y * width + x].R >= 100)
                                return y;
                        }
                    }
                    return -1;
                }

                (int Top, int FullWidthRow, int StraightWidth) OuterCap(Microsoft.Xna.Framework.Color[] pixels)
                {
                    var topRows = new List<(int Row, int Width)>();
                    for (int y = 0; y < height; y++)
                    {
                        int minX = width;
                        int maxX = -1;
                        for (int x = 0; x < width; x++)
                        {
                            if (pixels[y * width + x].R < 100)
                                continue;
                            minX = Math.Min(minX, x);
                            maxX = Math.Max(maxX, x);
                        }
                        if (maxX >= 0)
                            topRows.Add((y, maxX - minX + 1));
                    }

                    Assert.NotEmpty(topRows);
                    int topRow = topRows[0].Row;
                    int capProbeEnd = Math.Min(topRow + 80, height - 1);
                    int straightWidth = 0;
                    foreach (var row in topRows)
                    {
                        if (row.Row > capProbeEnd)
                            break;
                        straightWidth = Math.Max(straightWidth, row.Width);
                    }
                    int fullWidthRow = topRow;
                    foreach (var row in topRows)
                    {
                        if (row.Row > capProbeEnd)
                            break;
                        if (row.Width >= straightWidth)
                        {
                            fullWidthRow = row.Row;
                            break;
                        }
                    }
                    return (topRow, fullWidthRow, straightWidth);
                }

                // At a high viewport width the two arms are almost vertical. Their remaining
                // center separation widens the combined stroke, so a one-radius cap develops
                // short sloping shoulders unless its outward radius accounts for that span.
                track.TrackProps.SpatialProps.ViewWidthQn = 1024;
                var wideHardwareCap = OuterCap(Render(hardware: true));
                Assert.True(wideHardwareCap.StraightWidth >= 30);
                int outwardCapRadius = wideHardwareCap.FullWidthRow - wideHardwareCap.Top;
                Assert.InRange(outwardCapRadius,
                    wideHardwareCap.StraightWidth / 2 - 5, wideHardwareCap.StraightWidth / 2 + 1);

                // Width 6 used to hang a stretched inner disk under the arch. Width 14 used to
                // inflate the outer cap of a still-open bend. Compare both silhouettes to legacy.
                foreach (var (viewWidth, maxExtraInner, maxExtraOuter) in new[]
                {
                    (6f, 8, 3),
                    (8f, 8, 3),
                    (10f, 8, 3),
                    (12f, 8, 3),
                    (14f, 8, 3),
                })
                {
                    track.TrackProps.SpatialProps.ViewWidthQn = viewWidth;
                    var viewportHardware = Render(hardware: true);
                    var viewportLegacy = Render(hardware: false);
                    var viewportHardwareGap = FirstInteriorGap(viewportHardware);
                    var viewportLegacyGap = FirstInteriorGap(viewportLegacy);
                    Assert.True(viewportHardwareGap.Row >= 0);
                    Assert.True(viewportLegacyGap.Row >= 0);
                    Assert.InRange(viewportHardwareGap.Row - viewportLegacyGap.Row,
                        0, maxExtraInner);

                    int hardwareTop = FirstSolidRow(viewportHardware);
                    int legacyTop = FirstSolidRow(viewportLegacy);
                    Assert.True(hardwareTop >= 0 && legacyTop >= 0);
                    Assert.InRange(legacyTop - hardwareTop, -2, maxExtraOuter);
                }
            }
            finally
            {
                NoteStyle_Line.SetHardwareTessellation(true);
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
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Hardware_line_does_not_add_blobs_at_gentle_note_seams()
        {
            string contentRoot = EnsureContentRoot();
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager cm = null;
            try
            {
                var project = BuildGentleSeamsProject();
                project.CreateTrackViews(2, eraseCurrent: true);
                var track = project.TrackViews[1];
                track.TrackProps.StyleProps.Type = NoteStyleType.Line;
                var lineStyle = track.TrackProps.StyleProps.GetLineStyle();
                lineStyle.LineWidth = 7.6f;
                lineStyle.HlSize = 0;
                track.TrackProps.SpatialProps.ViewWidthQn = 16;

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                var svc = new ServiceContainer();
                svc.AddService<IGraphicsDeviceService>(gds);
                cm = new ContentManager(svc, contentRoot);

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SInitAllStyles();
                NoteStyle.SetContent(cm);
                NoteStyle.SetProject(project);
                project.NormSongPos = 0.5;

                var bakedGeo = Assert.IsType<LineGeo>(track.Geo);
                Assert.Equal(1, bakedGeo.hardwareRoundJoinCount);

                const int width = 896;
                const int height = 504;
                Microsoft.Xna.Framework.Color[] Render(bool hardware)
                {
                    NoteStyle_Line.SetHardwareTessellation(hardware);
                    using var target = new RenderTarget2D(gds.GraphicsDevice, width, height,
                        false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                    using var depthStencil = new DepthStencilState
                    {
                        StencilEnable = true,
                        StencilFunction = CompareFunction.Greater,
                        StencilPass = StencilOperation.Replace,
                        ReferenceStencil = 1,
                    };
                    var clearColor = new Microsoft.Xna.Framework.Color(1, 2, 3, 4);
                    gds.GraphicsDevice.SetRenderTarget(target);
                    gds.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                    var oldDepthStencil = gds.GraphicsDevice.DepthStencilState;
                    gds.GraphicsDevice.DepthStencilState = depthStencil;
                    gds.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer
                        | ClearOptions.Stencil, clearColor, 1, 0);
                    try
                    {
                        track.DrawTrack(project.GlobalTrackProps, defaultStyle: false);
                    }
                    finally
                    {
                        gds.GraphicsDevice.SetRenderTarget(null);
                        gds.GraphicsDevice.DepthStencilState = oldDepthStencil;
                    }

                    var pixels = new Microsoft.Xna.Framework.Color[width * height];
                    target.GetData(pixels);
                    Assert.Contains(pixels, pixel => pixel != clearColor);
                    return pixels;
                }

                var hardwarePixels = Render(hardware: true);
                var legacyPixels = Render(hardware: false);

                int StrongBottom(Microsoft.Xna.Framework.Color[] pixels, int x)
                {
                    int bottom = -1;
                    for (int y = 190; y < 230; y++)
                    {
                        if (pixels[y * width + x].R >= 100)
                            bottom = y;
                    }
                    return bottom;
                }

                // The strict peak is centered near x=460. The old unconditional 2.5x join
                // produced a five-pixel downward blob here; a gentle turn should track the
                // legacy silhouette to rasterization tolerance.
                for (int x = 450; x <= 470; x++)
                {
                    int legacyBottom = StrongBottom(legacyPixels, x);
                    int hardwareBottom = StrongBottom(hardwarePixels, x);
                    Assert.True(legacyBottom >= 0 && hardwareBottom >= 0);
                    Assert.True(hardwareBottom <= legacyBottom + 1,
                        $"Hardware line protruded at x={x}: legacy bottom {legacyBottom}, "
                        + $"hardware bottom {hardwareBottom}.");
                }
            }
            finally
            {
                NoteStyle_Line.SetHardwareTessellation(true);
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
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Initialize_order_SetContent_before_SInit_soft_skips_geo_then_SInit_bakes()
        {
            // SongRenderer.Initialize now calls SInit before SetContent, but Open/Import can
            // SetProject first and force a first-time host build — SetContent must not bake geo
            // with null vertex decls; SInitAllStyles finishes the deferred bake.
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

                NoteStyle.SetProject(project);
                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                // Deliberately SetContent before SInit (pre-fix Initialize order).
                NoteStyle.SetContent(cm);
                Assert.True(NoteStyle.HasContent);
                Assert.False(NoteStyle.HasStylesInited);
                Assert.False(NoteStyle.CanCreateGeo);
                Assert.Null(project.TrackViews[1].Geo);

                NoteStyle.SInitAllStyles();
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
            Assert.False(NoteStyle.HasStylesInited);
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

        [Fact]
        [Trait("Category", "Integration")]
        public void SetContent_failure_rebakes_fx_and_geo_with_previous_content()
        {
            // After a successful bake, a failed SetContent must restore the previous Content and
            // re-bake so HasContent is not left true with empty FX / null Geo.
            string contentRoot = EnsureContentRoot();
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager goodCm = null;
            ContentManager badCm = null;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                var svc = new ServiceContainer();
                svc.AddService<IGraphicsDeviceService>(gds);
                goodCm = new ContentManager(svc, contentRoot);
                badCm = new ContentManager(new ServiceContainer(), "Content-missing-for-rebake-test");

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SInitAllStyles();
                NoteStyle.SetContent(goodCm);
                NoteStyle.SetProject(project);
                Assert.True(project.TrackViews[1].TrackProps.StyleProps.GetBarStyle().HasFx);
                Assert.NotNull(project.TrackViews[1].Geo);

                Assert.Throws<ContentLoadException>(() => NoteStyle.SetContent(badCm));
                Assert.True(NoteStyle.HasContent);
                Assert.True(project.TrackViews[1].TrackProps.StyleProps.GetBarStyle().HasFx);
                Assert.NotNull(project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                NoteStyle.SetGraphicsDevice(null);
                goodCm?.Dispose();
                badCm?.Dispose();
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
        public void SInitAllStyles_failure_clears_partial_fx_and_keeps_styles_inited()
        {
            // Content-before-SInit: bake runs inside SInitAllStyles. On LoadFx failure, decls stay
            // (HasStylesInited) but partial FX/geo must be cleared.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager badCm = null;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                badCm = new ContentManager(new ServiceContainer(), "Content-missing-for-sinit-test");

                NoteStyle.SetGraphicsDevice(gds.GraphicsDevice);
                NoteStyle.SetContent(badCm);
                var holder = new Project
                {
                    Notes = project.Notes,
                    TrackViews = new List<TrackView>(),
                };
                NoteStyle.SetProject(holder);
                holder.TrackViews = project.TrackViews;

                Assert.Throws<ContentLoadException>(() => NoteStyle.SInitAllStyles());
                Assert.True(NoteStyle.HasStylesInited);
                Assert.True(NoteStyle.HasContent);
                Assert.False(project.TrackViews[1].TrackProps.StyleProps.GetBarStyle().HasFx);
                Assert.Null(project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                NoteStyle.SetGraphicsDevice(null);
                badCm?.Dispose();
                gds?.Release(disposing: true);
                if (hwnd != IntPtr.Zero)
                    DestroyWindow(hwnd);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasGraphicsDevice);
            Assert.False(NoteStyle.HasStylesInited);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetGraphicsDevice_failure_restores_previous_device()
        {
            // SetGraphicsDevice triggers deferred FX bake; on failure it must roll back the device
            // override (and HasStylesInited) so HasGraphicsDevice is not left true with empty FX.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            IntPtr hwnd = IntPtr.Zero;
            GraphicsDeviceService gds = null;
            ContentManager badCm = null;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);

                hwnd = CreateHostWindow();
                gds = GraphicsDeviceService.AddRef(hwnd, 64, 64);
                badCm = new ContentManager(new ServiceContainer(), "Content-missing-for-gd-rollback");

                // First Content install with a Project that has styles — bake fails, cm stays installed.
                NoteStyle.SetProject(project);
                Assert.Throws<ContentLoadException>(() => NoteStyle.SetContent(badCm));
                Assert.True(NoteStyle.HasContent);
                Assert.False(NoteStyle.HasGraphicsDevice);

                // Installing GD retries bake and fails — must restore previous GD (null).
                Assert.Throws<ContentLoadException>(() =>
                    NoteStyle.SetGraphicsDevice(gds.GraphicsDevice));
                Assert.False(NoteStyle.HasGraphicsDevice);
                Assert.False(NoteStyle.HasStylesInited);
                Assert.False(project.TrackViews[1].TrackProps.StyleProps.GetBarStyle().HasFx);
                Assert.Null(project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                NoteStyle.SetGraphicsDevice(null);
                badCm?.Dispose();
                gds?.Release(disposing: true);
                if (hwnd != IntPtr.Zero)
                    DestroyWindow(hwnd);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasGraphicsDevice);
        }
    }
}
