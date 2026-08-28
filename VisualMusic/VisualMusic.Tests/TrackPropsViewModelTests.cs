using System;
using System.Collections.Generic;
using System.Windows.Media;
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

        static Project TwoLineTracks(Action<NoteStyle_Line, NoteStyle_Line> configure)
        {
            var song = EmptySong(3);
            var project = new Project
            {
                Notes = song,
                TrackViews = new List<TrackView>(),
            };
            for (int i = 0; i < 3; i++)
                project.TrackViews.Add(new TrackView(i, 3, song));
            var a = project.TrackViews[1].TrackProps.StyleProps;
            var b = project.TrackViews[2].TrackProps.StyleProps;
            a.Type = NoteStyleType.Line;
            b.Type = NoteStyleType.Line;
            configure(a.GetLineStyle(), b.GetLineStyle());
            return project;
        }

        [Fact]
        public void Multi_select_line_tracks_blanks_controls_that_differ()
        {
            var previousNumTracks = TrackView.NumTracks;
            Project.SetDrawHost(new FakeSongDrawHost());
            try
            {
                var project = TwoLineTracks((a, b) =>
                {
                    a.LineWidth = 5;
                    b.LineWidth = 20;
                    a.Qn_gapThreshold = 3;
                    b.Qn_gapThreshold = 3;
                    a.Continuous = true;
                    b.Continuous = false;
                    a.LineType = LineType.Ribbon;
                    b.LineType = LineType.Standard;
                    a.HlType = LineHlType.Arrow;
                    b.HlType = LineHlType.Circle;
                    a.HlSize = 20;
                    b.HlSize = 20;
                    a.HlMovementPow = 1;
                    b.HlMovementPow = 2;
                    a.MovingHl = false;
                    b.MovingHl = true;
                    a.ShrinkingHl = false;
                    b.ShrinkingHl = false;
                    a.HlBorder = true;
                    b.HlBorder = false;
                });

                var vm = new TrackPropsViewModel
                {
                    SelectedTrackCount = 2,
                    MergedProps = project.MergeTrackProps(new[] { 1, 2 }),
                };

                Assert.True(vm.LineStyleVisible);
                Assert.Null(vm.LineWidth);
                Assert.Equal(3, vm.QnGapThreshold);
                Assert.Null(vm.Continuous);
                Assert.Equal(-1, vm.LineTypeIndex);
                Assert.Equal(-1, vm.LineHlTypeIndex);
                Assert.Equal(20, vm.HlSize);
                Assert.Null(vm.HlMovementPow);
                Assert.Null(vm.MovingHl);
                Assert.False(vm.ShrinkingHl);
                Assert.Null(vm.HlBorder);

                // Live tracks must keep their own values.
                Assert.Equal(5f, project.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineWidth);
                Assert.Equal(20f, project.TrackViews[2].TrackProps.StyleProps.GetLineStyle().LineWidth);
                Assert.Equal(LineType.Ribbon, project.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineType);
            }
            finally
            {
                Project.SetDrawHost(null);
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void Multi_select_line_tracks_keeps_matching_line_type()
        {
            var previousNumTracks = TrackView.NumTracks;
            Project.SetDrawHost(new FakeSongDrawHost());
            try
            {
                var project = TwoLineTracks((a, b) =>
                {
                    a.LineType = LineType.Ribbon;
                    b.LineType = LineType.Ribbon;
                    a.LineWidth = 8;
                    b.LineWidth = 8;
                });

                var vm = new TrackPropsViewModel
                {
                    SelectedTrackCount = 2,
                    MergedProps = project.MergeTrackProps(new[] { 1, 2 }),
                };

                Assert.True(vm.LineStyleVisible);
                Assert.Equal((int)LineType.Ribbon, vm.LineTypeIndex);
                Assert.Equal(8, vm.LineWidth);
            }
            finally
            {
                Project.SetDrawHost(null);
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void TextureThumbnailScaling_follows_point_sampling()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var song = EmptySong(2);
                var tv = new TrackView(1, 2, song);
                var vm = new TrackPropsViewModel
                {
                    SelectedTrackCount = 1,
                    ApplyToSelected = fn => fn(tv.TrackProps),
                    MergedProps = tv.TrackProps,
                };

                Assert.False(vm.PointSmp);
                Assert.Equal(BitmapScalingMode.Linear, vm.TextureThumbnailScaling);

                vm.PointSmp = true;
                Assert.True(tv.TrackProps.MaterialProps.TexProps.PointSmp);
                Assert.Equal(BitmapScalingMode.NearestNeighbor, vm.TextureThumbnailScaling);

                vm.PointSmp = false;
                Assert.Equal(BitmapScalingMode.Linear, vm.TextureThumbnailScaling);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }
    }
}
