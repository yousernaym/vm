using Midi;
using Xunit;

namespace VisualMusic.Tests
{
    public class ProjectTempoTests
    {
        [Fact]
        public void TicksToSeconds_and_SetSongPosS_round_trip()
        {
            var host = new FakeSongDrawHost();
            Project.SetDrawHost(host);
            try
            {
                var song = new Song();
                song.OpenMidiFile(TestFiles.PathTo("minimal.mid"));

                var project = new Project();
                project.TrackViews = new System.Collections.Generic.List<TrackView>();
                project.Notes = song;
                // Fire tempo-map init (default offset is 0; nudge then restore)
                project.Props.PlaybackOffsetS = 0.001f;
                project.Props.PlaybackOffsetS = 0f;

                // 480 ticks at 120 bpm = 1 beat = 0.5 seconds
                double seconds = project.TicksToSeconds(480);
                Assert.Equal(0.5, seconds, 3);

                project.SetSongPosS(0.25, updateScreen: true);
                Assert.True(project.NormSongPos > 0);
                Assert.Equal(0.25, project.SongPosS, 2);
            }
            finally
            {
                Project.SetDrawHost(null);
            }
        }
    }
}
