using CefSharp;
using CefSharp.Example;
using System.IO;
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

        [Fact]
        public void Browser_download_handler_continues_the_existing_browser_request()
        {
            string expectedPath = Path.Combine(Path.GetTempPath(), "song.mid");
            var item = new DownloadItem { SuggestedFileName = "song.mid" };
            var callback = new RecordingBeforeDownloadCallback();
            var handler = new DownloadHandler
            {
                ShowDialog = false,
                DownloadPathResolver = _ => expectedPath
            };

            bool handled = handler.OnBeforeDownload(null, null, item, callback);

            Assert.True(handled);
            Assert.Equal(expectedPath, callback.DownloadPath);
            Assert.False(callback.ShowDialog);
            Assert.True(callback.IsDisposed);
        }

        [Fact]
        public void Browser_downloaded_note_path_retains_the_source_url()
        {
            string path = Path.GetTempFileName();
            try
            {
                var options = new ImportOptions(FileType.Midi)
                {
                    RawNotePath = "https://example.test/getter-1"
                };

                options.SetBrowserDownloadedNotePath(path);

                Assert.Equal("https://example.test/getter-1", options.RawNotePath);
                Assert.Equal(path, options.NotePath);
            }
            finally
            {
                File.Delete(path);
            }
        }

        sealed class RecordingBeforeDownloadCallback : IBeforeDownloadCallback
        {
            public string DownloadPath { get; private set; }
            public bool ShowDialog { get; private set; }
            public bool IsDisposed { get; private set; }

            public void Continue(string downloadPath, bool showDialog)
            {
                DownloadPath = downloadPath;
                ShowDialog = showDialog;
            }

            public void Dispose() => IsDisposed = true;
        }
    }
}
