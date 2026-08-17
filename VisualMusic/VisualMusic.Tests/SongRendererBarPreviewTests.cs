using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class SongRendererBarPreviewTests
    {
        [Fact]
        public void Bar_preview_restores_style_and_geo_on_release()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var renderer = new SongRenderer();
            try
            {
                var project = BuildProjectWithLineTrack();
                var marker = new MarkerGeo().AddRef();
                project.TrackViews[1].Geo = marker;
                renderer.ForceProjectReference(project);

                renderer.BeginBarStylePreview();
                Assert.True(renderer.ForceDefaultNoteStyle);
                Assert.Equal(NoteStyleType.Line, project.TrackViews[1].TrackProps.StyleProps.Type);

                renderer.HandleKeyUp(0x5A);
                Assert.False(renderer.ForceDefaultNoteStyle);
                Assert.Equal(NoteStyleType.Line, project.TrackViews[1].TrackProps.StyleProps.Type);
                Assert.Same(marker, project.TrackViews[1].Geo);
            }
            finally
            {
                renderer.ForceProjectReference(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Bar_preview_is_idempotent_until_ended()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var renderer = new SongRenderer();
            try
            {
                var project = BuildProjectWithLineTrack();
                renderer.ForceProjectReference(project);

                renderer.BeginBarStylePreview();
                renderer.BeginBarStylePreview();
                Assert.True(renderer.ForceDefaultNoteStyle);

                renderer.EndBarStylePreview();
                renderer.EndBarStylePreview();
                Assert.False(renderer.ForceDefaultNoteStyle);
            }
            finally
            {
                renderer.ForceProjectReference(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Focus_lost_while_previewing_restores()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var renderer = new SongRenderer();
            try
            {
                var project = BuildProjectWithLineTrack();
                var marker = new MarkerGeo().AddRef();
                project.TrackViews[1].Geo = marker;
                renderer.ForceProjectReference(project);

                renderer.BeginBarStylePreview();
                Assert.True(renderer.ForceDefaultNoteStyle);

                renderer.HandleFocusLost();
                Assert.False(renderer.ForceDefaultNoteStyle);
                Assert.Same(marker, project.TrackViews[1].Geo);
            }
            finally
            {
                renderer.ForceProjectReference(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Bar_preview_end_does_not_restore_line_geo_after_style_became_bar()
        {
            // Undo during hold-Z used to write the saved LineGeo onto a track whose Type was
            // restored to Bar, then DrawGeoChunk cast LineGeo → BarGeo.
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var renderer = new SongRenderer();
            try
            {
                var project = BuildProjectWithLineTrack();
                project.TrackViews[1].Geo = new LineGeo();
                renderer.ForceProjectReference(project);

                renderer.BeginBarStylePreview();
                // Preview bake is skipped without a GPU; simulate the BarGeo Begin would have installed.
                var barGeo = new BarGeo();
                project.TrackViews[1].Geo = barGeo;
                project.TrackViews[1].TrackProps.StyleProps.Type = NoteStyleType.Bar;
                renderer.EndBarStylePreview();

                Assert.False(renderer.ForceDefaultNoteStyle);
                Assert.Equal(NoteStyleType.Bar, project.TrackViews[1].TrackProps.StyleProps.Type);
                Assert.Same(barGeo, project.TrackViews[1].Geo);
            }
            finally
            {
                renderer.ForceProjectReference(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        static Project BuildProjectWithLineTrack()
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new System.Collections.Generic.List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new System.Collections.Generic.List<Track>
                {
                    new Track { Length = 480 },
                    new Track
                    {
                        Length = 480,
                        Notes = new System.Collections.Generic.List<Note>
                        {
                            new Note { start = 0, stop = 120, channel = 0, pitch = 60, velocity = 100 },
                        },
                    },
                },
                SongLengthT = 480,
            };
            var project = new Project { Notes = song };
            project.CreateTrackViews(2, eraseCurrent: true);
            project.TrackViews[1].TrackProps.StyleProps.Type = NoteStyleType.Line;
            return project;
        }

        sealed class MarkerGeo : Geo
        {
            protected override void ReleaseResources() { }
        }
    }
}
