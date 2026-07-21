using System.IO;
using Xunit;
using VmMedia = VisualMusic.Media;

namespace VisualMusic.Tests
{
    [Collection("MediaSequential")]
    public class MediaIntegrationTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void Playback_lifecycle()
        {
            TestFiles.EnsureNativeLoaded("media.dll");
            string wav = TestFiles.PathTo("silence.wav");
            Assert.True(VmMedia.InitMF());
            try
            {
                Assert.True(VmMedia.OpenAudioFile(wav));
                Assert.True(VmMedia.GetAudioLength() > 0);

                // StartPlayback needs a live MF audio render endpoint. Headless CI / RDP
                // without audio fails cleanly once waitForEvent surfaces MEError — exercise
                // open/length (above) and return; pause/stop only run when start succeeds.
                if (!VmMedia.StartPlayback())
                {
                    Assert.True(VmMedia.CloseAudioFile());
                    return;
                }

                VmMedia.PausePlayback();
                VmMedia.StopPlayback();
                Assert.True(VmMedia.CloseAudioFile());
            }
            finally
            {
                VmMedia.CloseMF();
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Encode_smoke_writes_mkv()
        {
            TestFiles.EnsureNativeLoaded("media.dll");
            using var outFile = TestFiles.TempPath.File("vm_enc_", ".mkv");
            Assert.True(VmMedia.InitMF());
            try
            {
                var fmt = new VideoFormat(64, 64, 10f);
                Assert.True(VmMedia.BeginVideoEnc(
                    outFile.Path,
                    audioFile: null,
                    fmt,
                    audioOffsetSeconds: 0,
                    spherical: false,
                    sphericalStereo: false,
                    AVCodecID.AV_CODEC_ID_H264,
                    crf: "28"));
                try
                {
                    uint[] frame = new uint[64 * 64];
                    for (int i = 0; i < frame.Length; i++)
                        frame[i] = 0xFF0080FF; // BGRA-ish solid
                    for (int f = 0; f < 5; f++)
                        Assert.True(VmMedia.WriteFrame(frame));
                }
                finally
                {
                    VmMedia.EndVideoEnc();
                }

                Assert.True(File.Exists(outFile.Path));
                Assert.True(new FileInfo(outFile.Path).Length > 0);
            }
            finally
            {
                VmMedia.CloseMF();
            }
        }
    }

    [CollectionDefinition("MediaSequential", DisableParallelization = true)]
    public class MediaSequentialCollection { }
}
