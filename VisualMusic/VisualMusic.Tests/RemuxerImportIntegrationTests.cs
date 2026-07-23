using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using VmMedia = VisualMusic.Media;

namespace VisualMusic.Tests
{
    /// <summary>
    /// End-to-end Remuxer launch from <see cref="Project.ImportSong"/> — command-line paths and
    /// UTF-8 stdout decoding for TrackAudio lines. Needs remuxer/ + media.dll beside the test
    /// assembly (prior VisualMusic.sln Any CPU build).
    /// </summary>
    [Collection("MediaSequential")]
    public class RemuxerImportIntegrationTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task ImportSong_track_audio_under_non_ascii_paths()
        {
            TestFiles.EnsureNativeLoaded("media.dll");
            TestFiles.EnsureRemuxerAvailable();
            Program.InitTempDir();
            Project.SetDrawHost(new FakeSongDrawHost());
            var previousNumTracks = TrackView.NumTracks;

            using var dir = TestFiles.TempPath.NonAsciiDirectory("vm_import_utf8_");
            string input = Path.Combine(dir.Path, "minimal.ahx");
            File.Copy(TestFiles.PathTo("libRemuxer", "minimal.ahx"), input);
            string trackDir = Path.Combine(dir.Path, "tracks");
            Directory.CreateDirectory(trackDir);

            var options = new ImportOptions(FileType.Hvl)
            {
                RawNotePath = input,
                TrackAudio = true,
                TrackAudioOutputDir = trackDir,
                EraseCurrent = true,
            };
            options.SetNotePath();

            Assert.True(VmMedia.InitMF());
            try
            {
                var project = new Project();
                Assert.True(await project.ImportSong(options));

                Assert.NotNull(options.GeneratedTrackAudioPaths);
                Assert.NotEmpty(options.GeneratedTrackAudioPaths);
                foreach (var entry in options.GeneratedTrackAudioPaths)
                {
                    Assert.Contains("日本語", entry.Path, StringComparison.Ordinal);
                    Assert.True(File.Exists(entry.Path), "track wav missing: " + entry.Path);
                }

                Assert.NotNull(project.Notes);
                Assert.True(project.Notes.Tracks.Sum(t => t.Notes.Count) > 0);
            }
            finally
            {
                try { VmMedia.CloseAudioFile(); } catch { /* best-effort */ }
                VmMedia.CloseMF();
                TrackView.NumTracks = previousNumTracks;
                Project.SetDrawHost(null);
                Program.CloseTempDir();
            }
        }
    }
}
