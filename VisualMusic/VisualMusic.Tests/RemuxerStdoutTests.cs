using System.Globalization;
using System.Threading;
using Xunit;

namespace VisualMusic.Tests
{
    public class RemuxerStdoutTests
    {
        [Theory]
        [InlineData("Progress: 0%", 0)]
        [InlineData("Progress: 42%", 42)]
        [InlineData("Progress: 100%", 100)]
        public void Progress_regex(string line, int expected)
        {
            var m = Project.RemuxerProgressRegex.Match(line);
            Assert.True(m.Success);
            Assert.Equal(expected, int.Parse(m.Groups[1].Value));
        }

        [Fact]
        public void SongLength_flag_uses_invariant_culture()
        {
            var prev = CultureInfo.CurrentCulture;
            try
            {
                // Comma-decimal UI culture must still emit a dotted -l value for Remuxer.
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                Assert.Equal("-l23.079", Project.FormatRemuxerSongLengthFlag(23.079f));
                Assert.Equal("-l5.5", Project.FormatRemuxerSongLengthFlag(5.5f));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = prev;
            }
        }

        [Fact]
        public void TrackAudio_regex()
        {
            var m = Project.RemuxerTrackAudioRegex.Match(@"TrackAudio: 3|C:\tmp\track-ch01.wav");
            Assert.True(m.Success);
            Assert.Equal("3", m.Groups[1].Value);
            Assert.Equal(@"C:\tmp\track-ch01.wav", m.Groups[2].Value);
        }

        [Fact]
        public void TrackVoiceAudio_regex()
        {
            var m = Project.RemuxerTrackVoiceAudioRegex.Match(@"TrackVoiceAudio: 2|0|D:\a-ch00.wav");
            Assert.True(m.Success);
            Assert.Equal("2", m.Groups[1].Value);
            Assert.Equal("0", m.Groups[2].Value);
            Assert.Equal(@"D:\a-ch00.wav", m.Groups[3].Value);
        }
    }
}
