using System.Collections.Generic;
using Midi;
using VisualMusic.ViewModels;
using Xunit;

namespace VisualMusic.Tests
{
    public class TimelineScrollingTests
    {
        [Fact]
        public void Scroll_changes_are_relative_to_visible_song_width()
        {
            const int ticksPerBeat = 480;
            const int songLengthT = 16 * ticksPerBeat;
            var song = new Song
            {
                TicksPerBeat = ticksPerBeat,
                SongLengthT = songLengthT,
                Tracks = new List<Track>
                {
                    new Track { Length = songLengthT },
                    new Track { Length = songLengthT },
                },
            };
            var project = new Project
            {
                Notes = song,
                TrackViews = new List<TrackView>
                {
                    new TrackView(0, 2, song),
                    new TrackView(1, 2, song),
                },
            };
            project.GlobalTrackProps.SpatialProps.ViewWidthQn = 4;

            var vm = new MainViewModel { Project = project };

            Assert.Equal(0.25, vm.ScrollPageChange, 6);
            Assert.Equal(0.25 / 16, vm.ScrollLineChange, 6);
        }

        [Fact]
        public void Scroll_page_change_is_capped_when_entire_song_is_visible()
        {
            const int ticksPerBeat = 480;
            const int songLengthT = 4 * ticksPerBeat;
            var song = new Song
            {
                TicksPerBeat = ticksPerBeat,
                SongLengthT = songLengthT,
                Tracks = new List<Track> { new Track { Length = songLengthT } },
            };
            var project = new Project
            {
                Notes = song,
                TrackViews = new List<TrackView> { new TrackView(0, 1, song) },
            };
            project.GlobalTrackProps.SpatialProps.ViewWidthQn = 8;

            var vm = new MainViewModel { Project = project };

            Assert.Equal(1, vm.ScrollPageChange);
        }
    }
}
