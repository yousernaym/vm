using System.Collections.Generic;
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
    }
}
