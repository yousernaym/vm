using System.Collections.Generic;
using System.Linq;
using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class ProjectTrackLayoutTests
    {
        static Song EmptySong(int trackCount, int lengthT = 960)
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>(),
                SongLengthT = lengthT,
            };
            for (int i = 0; i < trackCount; i++)
                song.Tracks.Add(new Track { Length = lengthT });
            return song;
        }

        [Fact]
        public void SplitTracksByChannel_groups_notes_and_leaves_track0_empty()
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>
                {
                    new Track
                    {
                        Length = 1000,
                        Notes = new List<Note>
                        {
                            new Note { start = 200, stop = 300, channel = 2, pitch = 60, velocity = 80 },
                            new Note { start = 50, stop = 100, channel = 0, pitch = 64, velocity = 90 },
                            new Note { start = 10, stop = 20, channel = 0, pitch = 67, velocity = 70 },
                        },
                    },
                },
                SongLengthT = 1000,
            };

            Project.SplitTracksByChannel(song);

            Assert.Equal(3, song.Tracks.Count);
            Assert.Empty(song.Tracks[0].Notes);
            Assert.Equal(1000, song.Tracks[0].Length);

            Assert.Equal("Channel 1", song.Tracks[1].Name);
            Assert.Equal(2, song.Tracks[1].Notes.Count);
            Assert.Equal(10, song.Tracks[1].Notes[0].start);
            Assert.Equal(50, song.Tracks[1].Notes[1].start);

            Assert.Equal("Channel 3", song.Tracks[2].Name);
            Assert.Single(song.Tracks[2].Notes);
            Assert.Equal(200, song.Tracks[2].Notes[0].start);
            Assert.Equal(1000, song.Tracks[2].Length);
        }

        [Fact]
        public void CreateTrackViews_eraseCurrent_builds_fresh_views()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var song = EmptySong(3);
                var project = new Project();
                project.Notes = song;
                project.TrackViews = new List<TrackView> { new TrackView(0, 1, EmptySong(1)) };

                project.CreateTrackViews(3, eraseCurrent: true);

                Assert.Equal(3, project.TrackViews.Count);
                Assert.Equal(new[] { 0, 1, 2 }, project.TrackViews.Select(v => v.TrackNumber).ToArray());
                Assert.Equal(3, TrackView.NumTracks);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void CreateTrackViews_preserveTrackSet_keeps_sparse_saved_set()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var song = EmptySong(4);
                var project = new Project();
                project.Notes = song;
                project.TrackViews = new List<TrackView>
                {
                    new TrackView(0, 4, song),
                    new TrackView(2, 4, song),
                };
                project.TrackViews[1].TrackProps.GlobalProps = project.TrackViews[0].TrackProps;

                project.CreateTrackViews(4, eraseCurrent: false, preserveTrackSet: true);

                Assert.Equal(2, project.TrackViews.Count);
                Assert.Equal(new[] { 0, 2 }, project.TrackViews.Select(v => v.TrackNumber).ToArray());
                Assert.Equal(2, TrackView.NumTracks);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void CreateTrackViews_without_preserve_appends_missing_tracks()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var song = EmptySong(3);
                var project = new Project();
                project.Notes = song;
                project.TrackViews = new List<TrackView> { new TrackView(0, 1, song) };

                project.CreateTrackViews(3, eraseCurrent: false, preserveTrackSet: false);

                Assert.Equal(3, project.TrackViews.Count);
                Assert.Equal(new[] { 0, 1, 2 }, project.TrackViews.Select(v => v.TrackNumber).ToArray());
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }
    }
}
