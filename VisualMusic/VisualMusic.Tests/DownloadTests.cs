using Xunit;

namespace VisualMusic.Tests
{
    public class DownloadTests
    {
        [Fact]
        public void GetDownloadFileName_from_content_disposition()
        {
            string name = Download.GetDownloadFileName(
                "attachment; filename=\"song.mod\"",
                "http://example.com/dl");
            Assert.Equal("song.mod", name);
        }

        [Fact]
        public void GetDownloadFileName_falls_back_to_url()
        {
            string name = Download.GetDownloadFileName((string)null, "http://example.com/files/tune.sid");
            Assert.Equal("tune.sid", name);
        }

        [Theory]
        [InlineData("http://example.com/a", true)]
        [InlineData("https://x.test/y", true)]
        [InlineData(@"C:\local\file.mid", false)]
        [InlineData("not a url", false)]
        public void IsUrl(string path, bool expected)
        {
            Assert.Equal(expected, path.IsUrl());
        }
    }
}
