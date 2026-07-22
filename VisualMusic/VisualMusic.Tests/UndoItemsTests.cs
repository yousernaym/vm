using System.Collections.Generic;
using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class UndoItemsTests
    {
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
    }
}
