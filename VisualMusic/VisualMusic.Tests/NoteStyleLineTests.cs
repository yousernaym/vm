using System.Collections.Generic;
using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class NoteStyleLineTests
    {
        static Track TrackWithSeparatedNotes() => new Track
        {
            Notes = new List<Note>
            {
                new Note { start = 100, stop = 200, pitch = 60 },
                new Note { start = 300, stop = 400, pitch = 62 },
            },
        };

        [Theory]
        [InlineData(99.0, -1)]
        [InlineData(100.0, 0)]
        [InlineData(199.999, 0)]
        [InlineData(200.0, -1)]
        [InlineData(250.0, -1)]
        [InlineData(300.0, 1)]
        [InlineData(400.0, -1)]
        [InlineData(500.0, -1)]
        public void Highlight_exists_only_while_selected_note_is_playing(double playbackTick, int expectedIndex)
        {
            Assert.Equal(expectedIndex,
                NoteStyle_Line.GetHighlightedNoteIndex(TrackWithSeparatedNotes(), playbackTick));
        }
    }
}
