using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class StylePropsLoadFxTests
    {
        sealed class EmptyServices : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        [Fact]
        public void CanCreateGeo_requires_styles_inited()
        {
            Assert.False(NoteStyle.HasStylesInited);
            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasGraphicsDevice);
            Assert.False(NoteStyle.CanCreateGeo);
        }

        [Fact]
        public void SetContent_failure_restores_previous_content()
        {
            NoteStyle.SetProject(null);
            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasProject);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var cm1 = new ContentManager(new EmptyServices(), "Content-missing-1");
            var cm2 = new ContentManager(new EmptyServices(), "Content-missing-2");
            try
            {
                // Build views without Content so StyleProps.LoadFx soft-skips during construction.
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);

                // Install cm1 with an empty view list so SetProject does not hit LoadFx yet.
                var holder = new Project
                {
                    Notes = project.Notes,
                    TrackViews = new List<TrackView>(),
                };
                NoteStyle.SetContent(cm1);
                NoteStyle.SetProject(holder);
                Assert.True(NoteStyle.HasContent);

                // Attach styles, then SetContent(cm2) retries LoadFx and fails — cm1 must remain.
                // Re-bake with cm1 also fails (empty root); FX stay cleared but HasContent stays true.
                holder.TrackViews = project.TrackViews;
                Assert.Throws<ContentLoadException>(() => NoteStyle.SetContent(cm2));
                Assert.True(NoteStyle.HasContent);
                Assert.Equal(cm1.RootDirectory, NoteStyle.ContentRootDirectory);
                Assert.NotEqual(cm2.RootDirectory, NoteStyle.ContentRootDirectory);
                Assert.True(NoteStyle.HasProject);
                Assert.False(project.TrackViews[1].TrackProps.StyleProps.GetBarStyle().HasFx);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
        }

        [Fact]
        public void SetContent_failure_clears_style_fx_and_geo_on_project()
        {
            Assert.False(NoteStyle.HasContent);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                NoteStyle.SetProject(project);

                var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
                Assert.Throws<ContentLoadException>(() => NoteStyle.SetContent(cm));
                // First Content install keeps cm on bake failure (HwndHost never re-Initialize).
                Assert.True(NoteStyle.HasContent);
                Assert.True(NoteStyle.HasProject);
                Assert.Null(project.TrackViews[1].Geo);
                Assert.False(project.TrackViews[0].TrackProps.StyleProps.GetBarStyle().HasFx);
                Assert.False(project.TrackViews[1].TrackProps.StyleProps.GetBarStyle().HasFx);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void StyleProps_ctor_and_LoadFx_are_safe_without_content()
        {
            Assert.False(NoteStyle.HasContent);

            var styles = new StyleProps(1);

            Assert.NotNull(styles.GetBarStyle());
            Assert.NotNull(styles.GetLineStyle());
            Assert.Equal(NoteStyleType.Default, styles.Type);

            // CreateTrackViews / SetContent re-call this once Content exists; no-op until then.
            styles.LoadFx();
        }

        [Fact]
        public void NoteStyle_LoadFx_fails_loudly_without_content()
        {
            Assert.False(NoteStyle.HasContent);

            var bar = new NoteStyle_Bar();
            var line = new NoteStyle_Line();

            Assert.Throws<InvalidOperationException>(() => bar.LoadFx());
            Assert.Throws<InvalidOperationException>(() => line.LoadFx());
        }

        [Fact]
        public void SetContent_retries_LoadStyleFxAndCreateGeos_on_current_project()
        {
            Assert.False(NoteStyle.HasContent);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                Assert.Null(project.TrackViews[1].Geo);

                NoteStyle.SetProject(project);
                // Empty root → Content.Load("Bar") fails, proving SetContent entered the retry path.
                var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
                Assert.Throws<ContentLoadException>(() => NoteStyle.SetContent(cm));
                // First Content stays installed so Initialize/host can finish; FX were cleared.
                Assert.True(NoteStyle.HasContent);
                Assert.False(project.TrackViews[1].TrackProps.StyleProps.GetBarStyle().HasFx);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
        }

        [Fact]
        public void SetProject_retries_LoadStyleFxAndCreateGeos_when_HasContent()
        {
            // Content installed before Project (SongRenderer.Initialize can race Open/Import):
            // SetProject must finish the deferred bake that SetContent skipped.
            Assert.False(NoteStyle.HasContent);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                Assert.Null(project.TrackViews[1].Geo);

                NoteStyle.SetContent(cm);
                Assert.True(NoteStyle.HasContent);

                Assert.Throws<ContentLoadException>(() => NoteStyle.SetProject(project));
                // Failed SetProject must not leave the project override installed (mirrors SetContent).
                Assert.False(NoteStyle.HasProject);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
        }

        [Fact]
        public void SetProject_bake_failure_with_null_previous_preserves_geos()
        {
            // Open restore: SetProject(null) then SetProject(live). Bake failure must not
            // ClearStyleFxAndGeos(live) — that blanks a project that was already rendering.
            Assert.False(NoteStyle.HasContent);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                var marker = new MarkerGeo().AddRef();
                project.TrackViews[1].Geo = marker;

                NoteStyle.SetContent(cm);
                Assert.Throws<ContentLoadException>(() => NoteStyle.SetProject(project));

                Assert.False(NoteStyle.HasProject);
                Assert.Same(marker, project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void SetProject_bake_failure_with_previous_clears_new_project_geos()
        {
            // Switching Project A → B: failed bake on B must clear B's partial state and keep A.
            Assert.False(NoteStyle.HasContent);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
            try
            {
                // Build incoming before Content is installed (StyleProps ctor LoadFx soft-skips).
                var incoming = BuildProjectWithNoteOnTrack1();
                incoming.CreateTrackViews(2, eraseCurrent: true);
                var marker = new MarkerGeo().AddRef();
                incoming.TrackViews[1].Geo = marker;

                var holder = new Project
                {
                    Notes = incoming.Notes,
                    TrackViews = new List<TrackView>(),
                };
                NoteStyle.SetContent(cm);
                NoteStyle.SetProject(holder);
                Assert.True(NoteStyle.HasProject);

                Assert.Throws<ContentLoadException>(() => NoteStyle.SetProject(incoming));

                Assert.True(NoteStyle.HasProject);
                Assert.Null(incoming.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        sealed class MarkerGeo : Geo
        {
            protected override void ReleaseResources() { }
        }

        [Fact]
        public void StyleProps_LoadFx_attempts_Content_Load_when_HasContent()
        {
            NoteStyle.SetProject(null);
            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasProject);
            var styles = new StyleProps(1);
            var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
            try
            {
                // Install Content without a Project so SetContent does not retry LoadStyleFxAndCreateGeos.
                NoteStyle.SetContent(cm);
                Assert.True(NoteStyle.HasContent);
                // Soft-skip is skipped: LoadFx reaches Content.Load and fails on the empty root.
                Assert.Throws<ContentLoadException>(() => styles.LoadFx());
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
            }

            Assert.False(NoteStyle.HasContent);
        }

        [Fact]
        public void CreateGeo_soft_skips_without_styles_inited()
        {
            // Content + device are not enough — SInitAllStyles must run before CanCreateGeo.
            Assert.False(NoteStyle.HasStylesInited);
            Assert.False(NoteStyle.CanCreateGeo);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);

                NoteStyle.SetContent(cm);
                // Isolate the SInit gate: assume a device without allocating one (unit test).
                NoteStyle.TestAssumeGraphicsDevice = true;
                Assert.True(NoteStyle.HasContent);
                Assert.True(NoteStyle.HasGraphicsDevice);
                Assert.False(NoteStyle.HasStylesInited);
                Assert.False(NoteStyle.CanCreateGeo);

                project.TrackViews[1].CreateGeo(project, project.GlobalTrackProps);
                Assert.Null(project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.TestAssumeGraphicsDevice = false;
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void CreateGeo_soft_skips_without_graphics_device()
        {
            // HasContent alone is not enough — vertex buffers need a GraphicsDevice.
            Assert.False(NoteStyle.HasContent);
            Assert.False(NoteStyle.HasGraphicsDevice);
            Assert.False(NoteStyle.CanCreateGeo);
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
            try
            {
                var project = BuildProjectWithNoteOnTrack1();
                project.CreateTrackViews(2, eraseCurrent: true);
                Assert.Null(project.TrackViews[1].Geo);

                // Content without Project (avoids SetContent retry LoadFx) and without GraphicsDevice.
                NoteStyle.SetContent(cm);
                Assert.True(NoteStyle.HasContent);
                Assert.False(NoteStyle.CanCreateGeo);

                project.TrackViews[1].CreateGeo(project, project.GlobalTrackProps);
                Assert.Null(project.TrackViews[1].Geo);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }

            Assert.False(NoteStyle.HasContent);
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
    }
}
