using System.IO;
using System.Text;
using Xunit;
using VmMidMix = VisualMusic.MidMix;

namespace VisualMusic.Tests
{
    // MidMix owns process-global Fluidsynth state; serialize MidMix tests against each other.
    [Collection("MidMixSequential")]
    public class MidMixIntegrationTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void Init_sfLoaded_and_mixdown_sequence()
        {
            TestFiles.EnsureNativeLoaded("MidMix.dll");
            using var emptyDir = TestFiles.TempPath.Directory("vm_midmix_empty_");
            using var sfDir = TestFiles.TempPath.Directory("vm_midmix_sf_");

            // 1) Missing soundfont — pass an absolute path that does not exist (no cwd mutation)
            string missingSf = Path.Combine(emptyDir.Path, Program.SoundFontFileName);
            VmMidMix.Init(missingSf);
            Assert.False(VmMidMix.SfLoaded(), "expected no soundfont when path is missing");
            VmMidMix.Close();

            // 2) With tiny.sf2 at an absolute path
            string sfPath = Path.Combine(sfDir.Path, Program.SoundFontFileName);
            File.Copy(TestFiles.PathTo("MidMix", Path.Combine("soundfont", "tiny.sf2")), sfPath);
            File.Copy(TestFiles.PathTo("MidMix", Path.Combine("soundfont", "sf-LICENSE.txt")),
                Path.Combine(sfDir.Path, "sf-LICENSE.txt"));
            VmMidMix.Init(sfPath);
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
    }

    [CollectionDefinition("MidMixSequential", DisableParallelization = true)]
    public class MidMixSequentialCollection { }
}
