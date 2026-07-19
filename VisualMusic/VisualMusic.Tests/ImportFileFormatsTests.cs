using Xunit;

namespace VisualMusic.Tests
{
    public class ImportFileFormatsTests
    {
        [Theory]
        [InlineData("mid", FileType.Midi)]
        [InlineData(".MID", FileType.Midi)]
        [InlineData("mod", FileType.Mod)]
        [InlineData("xm", FileType.Mod)]
        [InlineData("hvl", FileType.Hvl)]
        [InlineData("ahx", FileType.Hvl)]
        [InlineData("sid", FileType.Sid)]
        public void FromExtension_maps_known_types(string ext, FileType expected)
        {
            Assert.Equal(expected, ImportFileFormats.FromExtension(ext));
        }

        [Fact]
        public void FromExtension_mus_prefers_context()
        {
            Assert.Equal(FileType.Mod, ImportFileFormats.FromExtension("mus", FileType.Mod));
            Assert.Equal(FileType.Sid, ImportFileFormats.FromExtension("mus", FileType.Sid));
            // Without preferred, Mod list is checked before Sid
            Assert.Equal(FileType.Mod, ImportFileFormats.FromExtension("mus"));
        }

        [Fact]
        public void FromExtension_unknown_returns_null()
        {
            Assert.Null(ImportFileFormats.FromExtension("xyz"));
            Assert.Null(ImportFileFormats.FromExtension(""));
            Assert.Null(ImportFileFormats.FromExtension(null));
        }
    }
}
