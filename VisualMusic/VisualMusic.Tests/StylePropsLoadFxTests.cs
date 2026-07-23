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
                // Failed install must not leave HasContent true with half-loaded FX.
                Assert.False(NoteStyle.HasContent);
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
        public void StyleProps_LoadFx_attempts_Content_Load_when_HasContent()
        {
            Assert.False(NoteStyle.HasContent);
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
            }

            Assert.False(NoteStyle.HasContent);
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
