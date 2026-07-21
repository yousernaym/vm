using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;
using VmMidMix = VisualMusic.MidMix;

namespace VisualMusic.Tests
{
    [Collection("MidMixSequential")]
    public class MidMixIntegrationTests
    {
        static void EnsureNativeLoaded()
        {
            string dll = Path.Combine(AppContext.BaseDirectory, "MidMix.dll");
            if (!File.Exists(dll))
                throw new FileNotFoundException(
                    "MidMix.dll not found beside the test assembly. Build VisualMusic.sln (Any CPU) so VisualMusic.Tests copies native runtime DLLs.");
            NativeLibrary.Load(dll);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Init_sfLoaded_and_mixdown_sequence()
        {
            EnsureNativeLoaded();
            string originalCwd = Directory.GetCurrentDirectory();
            string emptyDir = Path.Combine(Path.GetTempPath(), "vm_midmix_empty_" + Guid.NewGuid().ToString("N"));
            string sfDir = Path.Combine(Path.GetTempPath(), "vm_midmix_sf_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(emptyDir);
            Directory.CreateDirectory(sfDir);
            try
            {
                // 1) No soundfont
                Directory.SetCurrentDirectory(emptyDir);
                VmMidMix.Init();
                Assert.False(VmMidMix.SfLoaded(), "expected no soundfont in empty cwd");
                VmMidMix.Close();

                // 2) With tiny.sf2 as soundfont.sf2
                File.Copy(TestFiles.PathTo(Path.Combine("soundfont", "tiny.sf2")),
                    Path.Combine(sfDir, "soundfont.sf2"));
                File.Copy(TestFiles.PathTo(Path.Combine("soundfont", "sf-LICENSE.txt")),
                    Path.Combine(sfDir, "sf-LICENSE.txt"));
                Directory.SetCurrentDirectory(sfDir);
                VmMidMix.Init();
                try
                {
                    Assert.True(VmMidMix.SfLoaded(), "expected soundfont to load");

                    string midi = TestFiles.PathTo("minimal.mid");
                    string wavOut = Path.Combine(sfDir, "mix.wav");
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
                try { Directory.Delete(emptyDir, true); } catch { }
                try { Directory.Delete(sfDir, true); } catch { }
            }
        }
    }

    [CollectionDefinition("MidMixSequential", DisableParallelization = true)]
    public class MidMixSequentialCollection { }
}
