using System.Collections.Generic;
using Midi;
using VisualMusic.ViewModels;
using Xunit;

namespace VisualMusic.Tests
{
    public class TrackPropsViewModelTests
    {
        static Song EmptySong(int trackCount)
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track>(),
                SongLengthT = 480,
            };
            for (int i = 0; i < trackCount; i++)
                song.Tracks.Add(new Track { Length = 480 });
            return song;
        }

        /// <summary>
        /// Mirrors MainViewModel: several selected tracks bind to a detached merge clone, while
        /// ApplyAndRebuild mutates the live TrackProps.
        /// </summary>
        static TrackPropsViewModel MultiSelectVm(NoteStyleType firstType, NoteStyleType secondType,
            out TrackProps[] live)
        {
            var song = EmptySong(3);
            var t1 = new TrackView(1, 3, song);
            var t2 = new TrackView(2, 3, song);
            t1.TrackProps.StyleProps.Type = firstType;
            t2.TrackProps.StyleProps.Type = secondType;
            var selected = new[] { t1.TrackProps, t2.TrackProps };
            live = selected;

            var merged = t1.TrackProps.Clone();
            if (firstType != secondType)
                merged.StyleProps.Type = null;

            return new TrackPropsViewModel
            {
                SelectedTrackCount = 2,
                ApplyAndRebuild = fn => { foreach (var tp in selected) fn(tp); },
                MergedProps = merged,
            };
        }

        [Fact]
        public void StyleTypeIndex_multi_select_mixed_to_line_shows_line_controls()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var vm = MultiSelectVm(NoteStyleType.Default, NoteStyleType.Bar, out var live);
                Assert.False(vm.LineStyleVisible);
                Assert.False(vm.ModulationVisible);

                vm.StyleTypeIndex = (int)NoteStyleType.Line;

                Assert.Equal((int)NoteStyleType.Line, vm.StyleTypeIndex);
                Assert.True(vm.LineStyleVisible);
                Assert.True(vm.ModulationVisible);
                Assert.Equal(NoteStyleType.Line, live[0].StyleProps.Type);
                Assert.Equal(NoteStyleType.Line, live[1].StyleProps.Type);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void StyleTypeIndex_multi_select_same_default_to_line_shows_line_controls()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var vm = MultiSelectVm(NoteStyleType.Default, NoteStyleType.Default, out var live);
                Assert.False(vm.LineStyleVisible);

                vm.StyleTypeIndex = (int)NoteStyleType.Line;

                Assert.True(vm.LineStyleVisible);
                Assert.True(vm.ModulationVisible);
                Assert.Equal(NoteStyleType.Line, live[0].StyleProps.Type);
                Assert.Equal(NoteStyleType.Line, live[1].StyleProps.Type);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void StyleTypeIndex_single_select_to_line_shows_line_controls()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var song = EmptySong(2);
                var tv = new TrackView(1, 2, song);
                var vm = new TrackPropsViewModel
                {
                    SelectedTrackCount = 1,
                    ApplyAndRebuild = fn => fn(tv.TrackProps),
                    MergedProps = tv.TrackProps,
                };

                vm.StyleTypeIndex = (int)NoteStyleType.Line;

                Assert.Equal((int)NoteStyleType.Line, vm.StyleTypeIndex);
                Assert.True(vm.LineStyleVisible);
                Assert.True(vm.ModulationVisible);
                Assert.Equal(NoteStyleType.Line, tv.TrackProps.StyleProps.Type);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }
    }
}
