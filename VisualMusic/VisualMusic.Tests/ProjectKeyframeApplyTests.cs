using System;
using System.Collections.Generic;
using System.Linq;
using Midi;
using VisualMusic.Keyframes;
using Xunit;

namespace VisualMusic.Tests
{
    public class ProjectKeyframeApplyTests
    {
        static Project BuildProjectWithSong(int songLengthT = 100)
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>
                {
                    new Track { Length = songLengthT },
                    new Track { Length = songLengthT },
                },
                SongLengthT = songLengthT,
            };

            var project = new Project();
            project.Notes = song;
            project.TrackViews = new List<TrackView>
            {
                new TrackView(0, 2, song),
                new TrackView(1, 2, song),
            };
            project.TrackViews[1].TrackProps.GlobalProps = project.TrackViews[0].TrackProps;
            project.PropertyKeyframes = new KeyframeSet();
            project.InitPropertyAccessors();

            // Fire tempo-map init (same pattern as ProjectTempoTests).
            project.Props.PlaybackOffsetS = 0.001f;
            project.Props.PlaybackOffsetS = 0f;
            return project;
        }

        [Fact]
        public void InterpolatePropertyKeyframes_lerps_background_opacity()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProjectWithSong();
                project.PropertyKeyframes.Add("proj/BackgroundImageOpacity", 0, KfInterpolation.Linear,
                    new ScalarKfValue(0));
                project.PropertyKeyframes.Add("proj/BackgroundImageOpacity", 100, KfInterpolation.Linear,
                    new ScalarKfValue(1));

                project.NormSongPos = 0.5; // SongPosT == 50
                project.InterpolatePropertyKeyframes();

                Assert.Equal(0.5f, project.Props.BackgroundImageOpacity, 3);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void InterpolatePropertyKeyframes_lerps_track_Transp()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProjectWithSong();
                project.TrackViews[1].TrackProps.MaterialProps.Transp = 0f;
                project.PropertyKeyframes.Add("track/1/Transp", 0, KfInterpolation.Linear,
                    new ScalarKfValue(0));
                project.PropertyKeyframes.Add("track/1/Transp", 100, KfInterpolation.Linear,
                    new ScalarKfValue(1));

                project.NormSongPos = 0.5; // SongPosT == 50
                project.InterpolatePropertyKeyframes();

                Assert.Equal(0.5f, project.TrackViews[1].TrackProps.MaterialProps.Transp!.Value, 3);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void InterpolatePropertyKeyframes_holds_bool_AudioVisLeft()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var project = BuildProjectWithSong();
                project.Props.AudioVisLeft = false;
                project.PropertyKeyframes.Add("proj/AudioVisLeft", 0, KfInterpolation.Linear,
                    new ScalarKfValue(1));
                project.PropertyKeyframes.Add("proj/AudioVisLeft", 100, KfInterpolation.Linear,
                    new ScalarKfValue(0));

                // Past midpoint: Linear would yield ~0.25 → false (v >= 0.5); Hold keeps 1 → true.
                project.NormSongPos = 0.75;
                project.InterpolatePropertyKeyframes();

                // Bool accessors force Hold regardless of keyframe interpolation mode.
                Assert.True(project.Props.AudioVisLeft);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void KeyframeService_AddKey_RemoveKey_and_ResolveIds()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            var previous = KeyframeService.Project;
            var previousIds = KeyframeService.SelectedTrackIds;
            var previousUndo = KeyframeService.RequestUndoSnapshot;
            KeyframeService.RequestUndoSnapshot = null;
            try
            {
                var project = BuildProjectWithSong();
                project.Props.BackgroundImageOpacity = 0.75f;
                project.NormSongPos = 0.25; // tick 25
                KeyframeService.Project = project;
                KeyframeService.SelectedTrackIds = Array.Empty<int>();

                Assert.Equal(new[] { "proj/BackgroundImageOpacity" },
                    KeyframeService.ResolveIds("BackgroundImageOpacity", KeyframeService.KfScope.Project).ToArray());

                KeyframeService.SelectedTrackIds = new[] { 1 };
                Assert.Equal(new[] { "track/1/Transp" },
                    KeyframeService.ResolveIds("Transp", KeyframeService.KfScope.Track).ToArray());

                KeyframeService.AddKey("BackgroundImageOpacity", KeyframeService.KfScope.Project);
                Assert.True(project.PropertyKeyframes.HasKeyAt("proj/BackgroundImageOpacity", 25));
                var added = project.PropertyKeyframes.Tracks["proj/BackgroundImageOpacity"].FindBrackets(25).Before;
                Assert.NotNull(added);
                Assert.Equal(0.75, ((ScalarKfValue)added.Value).V, 5);
                Assert.Equal(KfInterpolation.Smooth, added.Interpolation);

                KeyframeService.RemoveKey("BackgroundImageOpacity", KeyframeService.KfScope.Project);
                Assert.False(project.PropertyKeyframes.HasKeyAt("proj/BackgroundImageOpacity", 25));
            }
            finally
            {
                KeyframeService.Project = previous;
                KeyframeService.SelectedTrackIds = previousIds;
                KeyframeService.RequestUndoSnapshot = previousUndo;
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }
    }
}
