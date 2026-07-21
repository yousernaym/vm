using System;
using System.IO;
using System.Text;
using Xunit;
using VmMidMix = VisualMusic.MidMix;

namespace VisualMusic.Tests
{
    [Collection("MidMixSequential")]
    public class MidMixIntegrationTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void Init_sfLoaded_and_mixdown_sequence()
        {
            TestFiles.EnsureNativeLoaded("MidMix.dll");
            string originalCwd = Directory.GetCurrentDirectory();
            using var emptyDir = TestFiles.TempPath.Directory("vm_midmix_empty_");
            using var sfDir = TestFiles.TempPath.Directory("vm_midmix_sf_");
            try
            {
                // 1) No soundfont
                Directory.SetCurrentDirectory(emptyDir.Path);
                VmMidMix.Init();
                Assert.False(VmMidMix.SfLoaded(), "expected no soundfont in empty cwd");
                VmMidMix.Close();

                // 2) With tiny.sf2 as soundfont.sf2
                File.Copy(TestFiles.PathTo("MidMix", Path.Combine("soundfont", "tiny.sf2")),
                    Path.Combine(sfDir.Path, "soundfont.sf2"));
                File.Copy(TestFiles.PathTo("MidMix", Path.Combine("soundfont", "sf-LICENSE.txt")),
                    Path.Combine(sfDir.Path, "sf-LICENSE.txt"));
                Directory.SetCurrentDirectory(sfDir.Path);
                VmMidMix.Init();
                try
                {
                    Assert.True(VmMidMix.SfLoaded(), "expected soundfont to load");

                    string midi = TestFiles.PathTo("midiLib", "minimal.mid");
                    string wavOut = Path.Combine(sfDir.Path, "mix.wav");
                    VmMidMix.Mixdown(midi, wavOut);
                    Assert.True(File.Exists(wavOut));
                    Assert.True(new FileInfo(wavOut).Length > 44);
                    byte[] hdr = new byte[12];
                    using (var fs = File.OpenRead(wavOut))
                        Assert.Equal(12, fs.Read(hdr, 0, 12));
                    Assert.Equal("RIFF", Encoding.ASCII.GetString(hdr, 0, 4));
                    Assert.Equal("WAVE", Encoding.ASCII.GetString(hdr, 8, 4));
                }
                finally
                {
                    VmMidMix.Close();
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(originalCwd);
            }
        }
    }

    [CollectionDefinition("MidMixSequential", DisableParallelization = true)]
    public class MidMixSequentialCollection { }
}
