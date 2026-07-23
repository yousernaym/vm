using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Midi;
using VisualMusic.Keyframes;
using Xunit;

namespace VisualMusic.Tests
{
    public class UndoItemsTests
    {
        sealed class EmptyServices : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        static Project MinimalProject()
        {
            var p = new Project();
            p.TrackViews = new List<TrackView>();
            return p;
        }

        static Project ProjectWithAudioTrack(string filename)
        {
            var song = new Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<TempoEvent> { new TempoEvent(0, 120.0) },
                Tracks = new List<Track> { new Track { Length = 480 } },
                SongLengthT = 480,
            };
            var project = new Project();
            project.Notes = song;
            project.TrackViews = new List<TrackView> { new TrackView(0, 1, song) };
            project.TrackViews[0].TrackProps.AudioProps.Filename = filename;
            return project;
        }

        static Project ProjectWithNoteOnTrack1()
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
            project.TrackViews = new List<TrackView>
            {
                new TrackView(0, 2, song),
                new TrackView(1, 2, song),
            };
            project.TrackViews[1].TrackProps.GlobalProps = project.TrackViews[0].TrackProps;
            return project;
        }

        [Fact]
        public void Add_undo_redo_and_truncate()
        {
            var undo = new UndoItems();
            var live = MinimalProject();

            undo.Add("one", live);
            Assert.Equal("one", undo.UndoDesc);
            Assert.Equal("", undo.RedoDesc);

            undo.Add("two", live);
            Assert.Equal("two", undo.UndoDesc);

            undo--;
            Assert.Equal("one", undo.UndoDesc);
            Assert.Equal("two", undo.RedoDesc);

            undo++;
            Assert.Equal("two", undo.UndoDesc);

            // Branch: truncate redo
            undo--;
            undo.Add("three", live);
            Assert.Equal("three", undo.UndoDesc);
            Assert.Equal("", undo.RedoDesc);
        }

        [Fact]
        public void MarkSaved_and_IsCurrentSaved()
        {
            var undo = new UndoItems();
            var live = MinimalProject();
            undo.Add("a", live);
            Assert.False(undo.IsCurrentSaved);
            undo.MarkSaved();
            Assert.True(undo.IsCurrentSaved);
            undo.Add("b", live);
            Assert.False(undo.IsCurrentSaved);
        }

        [Fact]
        public void Snapshot_AudioProps_isolated_from_live_edits()
        {
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var undo = new UndoItems();
                var live = ProjectWithAudioTrack("a.wav");

                undo.Add("snap", live);
                live.TrackViews[0].TrackProps.AudioProps.Filename = "b.wav";

                Assert.Equal("b.wav", live.TrackViews[0].TrackProps.AudioProps.Filename);
                Assert.Equal("a.wav", undo.Current.Project.TrackViews[0].TrackProps.AudioProps.Filename);
                Assert.NotSame(
                    live.TrackViews[0].TrackProps.AudioProps,
                    undo.Current.Project.TrackViews[0].TrackProps.AudioProps);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
            }
        }

        [Fact]
        public void Undo_CopyPropsFrom_restores_proj_and_track_props()
        {
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var live = ProjectWithAudioTrack("a.wav");
                var tp = live.TrackViews[0].TrackProps;

                live.Props.FadeIn = 1.5f;
                tp.StyleProps.Type = NoteStyleType.Bar;
                tp.StyleProps.GetLineStyle().LineWidth = 2.5f;
                tp.MaterialProps.Transp = 0.25f;
                tp.MaterialProps.Hue = 0.4f;
                tp.LightProps.AmbientAmount = 0.3f;
                tp.SpatialProps.XOffset = 1.25f;
                live.PropertyKeyframes.Add("proj/BackgroundImageOpacity", 0, KfInterpolation.Linear,
                    new ScalarKfValue(0.2));
                live.PropertyKeyframes.Add("track/0/Transp", 100, KfInterpolation.Smooth,
                    new ScalarKfValue(0.5));

                var undo = new UndoItems();
                undo.Add("before edit", live);

                live.Props.FadeIn = 3f;
                tp.StyleProps.Type = NoteStyleType.Line;
                tp.StyleProps.GetLineStyle().LineWidth = 9f;
                tp.MaterialProps.Transp = 0.9f;
                tp.MaterialProps.Hue = 0.8f;
                tp.LightProps.AmbientAmount = 0.9f;
                tp.SpatialProps.XOffset = -4f;
                tp.AudioProps.Filename = "b.wav";
                live.PropertyKeyframes = new KeyframeSet();
                live.PropertyKeyframes.Add("proj/BackgroundImageOpacity", 50, KfInterpolation.Hold,
                    new ScalarKfValue(1));
                undo.Add("after edit", live);

                undo--;
                live.CopyPropsFrom(undo.Current.Project);
                tp = live.TrackViews[0].TrackProps;

                Assert.Equal(1.5f, live.Props.FadeIn);
                Assert.Equal(NoteStyleType.Bar, tp.StyleProps.Type);
                Assert.Equal(2.5f, tp.StyleProps.GetLineStyle().LineWidth!.Value);
                Assert.Equal(0.25f, tp.MaterialProps.Transp!.Value);
                Assert.Equal(0.4f, tp.MaterialProps.Hue!.Value);
                Assert.Equal(0.3f, tp.LightProps.AmbientAmount!.Value);
                Assert.Equal(1.25f, tp.SpatialProps.XOffset!.Value);
                Assert.Equal("a.wav", tp.AudioProps.Filename);
                Assert.True(live.PropertyKeyframes.HasKeyAt("proj/BackgroundImageOpacity", 0));
                Assert.True(live.PropertyKeyframes.HasKeyAt("track/0/Transp", 100));
                Assert.False(live.PropertyKeyframes.HasKeyAt("proj/BackgroundImageOpacity", 50));
                Assert.Equal(KfInterpolation.Linear,
                    live.PropertyKeyframes.GetInterpolation("proj/BackgroundImageOpacity", 0));
                Assert.Equal(0.2,
                    ((ScalarKfValue)live.PropertyKeyframes.Tracks["proj/BackgroundImageOpacity"]
                        .FindBrackets(0).Before.Value).V, 5);
            }
            finally
            {
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Undo_CopyPropsFrom_then_LoadStyleFxAndCreateGeos_attempts_fx_reload()
        {
            // Mirrors MainViewModel.ApplyUndoItem exactly: CopyPropsFrom then
            // LoadStyleFxAndCreateGeos(resetVertScale: true) (not CreateGeos alone / resetVertScale false).
            // Without Content, FX soft-skips and geo stays null; installing Content retries and hits
            // Content.Load (empty root → ContentLoadException).
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;
            try
            {
                var live = ProjectWithNoteOnTrack1();
                live.TrackViews[1].TrackProps.StyleProps.Type = NoteStyleType.Bar;
                live.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineWidth = 3.5f;
                live.TrackViews[1].TrackProps.MaterialProps.Transp = 0.25f;

                var undo = new UndoItems();
                undo.Add("before", live);

                live.TrackViews[1].TrackProps.StyleProps.Type = NoteStyleType.Line;
                live.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineWidth = 9f;
                live.TrackViews[1].TrackProps.MaterialProps.Transp = 0.9f;
                undo.Add("after", live);

                undo--;
                live.CopyPropsFrom(undo.Current.Project);
                Assert.Equal(NoteStyleType.Bar, live.TrackViews[1].TrackProps.StyleProps.Type);
                Assert.Equal(3.5f, live.TrackViews[1].TrackProps.StyleProps.GetLineStyle().LineWidth!.Value);
                Assert.Equal(0.25f, live.TrackViews[1].TrackProps.MaterialProps.Transp!.Value);

                live.LoadStyleFxAndCreateGeos(resetVertScale: true);
                Assert.Null(live.TrackViews[1].Geo);

                NoteStyle.SetProject(live);
                var cm = new ContentManager(new EmptyServices(), "Content-missing-for-test");
                Assert.Throws<ContentLoadException>(() => NoteStyle.SetContent(cm));
                Assert.False(NoteStyle.HasContent);
            }
            finally
            {
                NoteStyle.SetContent(null);
                NoteStyle.SetProject(null);
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
            }
        }
    }
}
