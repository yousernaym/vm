using CefSharp;
using CefSharp.Example;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace VisualMusic.Tests
{
    public class DownloadTests
    {
        [Theory]
        [InlineData("attachment; filename=\"song.mod\"", "song.mod")]
        [InlineData("attachment; filename=ThePenis(Eek!).mid", "ThePenis(Eek!).mid")]
        [InlineData("attachment; filename=\"song (live).mid\"", "song (live).mid")]
        [InlineData("attachment; filename*=UTF-8''song%20name.mid", "song name.mid")]
        public void GetDownloadFileName_from_content_disposition(string header, string expected)
        {
            string name = Download.GetDownloadFileName(header, "http://example.com/getter-1");
            Assert.Equal(expected, name);
        }

        [Fact]
        public void GetDownloadFileName_falls_back_to_url()
        {
            string name = Download.GetDownloadFileName((string)null, "http://example.com/files/tune.sid");
            Assert.Equal("tune.sid", name);
        }

        [Fact]
        public void GetDownloadFileName_reads_unparsed_content_disposition_header()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.Found)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };
            Assert.True(response.Content.Headers.TryAddWithoutValidation(
                "Content-Disposition", "attachment; filename=ThePenis(Eek!).mid"));
            Assert.Null(response.Content.Headers.ContentDisposition);

            string name = Download.GetDownloadFileName(response, "http://example.com/getter-29691");
            Assert.Equal("ThePenis(Eek!).mid", name);
        }

        [Fact]
        public async Task DownloadToTempFile_saves_redirect_body_when_attachment_filename_is_unquoted()
        {
            byte[] midi = Encoding.ASCII.GetBytes("MThd" + new string('\0', 12));
            using var server = new OneShotAttachmentRedirectServer(midi, "ThePenis(Eek!).mid");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            string path = await Download.DownloadToTempFileAsync(server.GetterUrl, null, cts.Token);

            Assert.NotNull(path);
            Assert.Equal("ThePenis(Eek!).mid", Path.GetFileName(path));
            Assert.Equal(midi, File.ReadAllBytes(path));
            Assert.Equal(0, server.FollowedRedirects);
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

        /// <summary>
        /// Mimics FreeMIDI getter URLs: 302 + attachment body, Location that 500s.
        /// </summary>
        sealed class OneShotAttachmentRedirectServer : IDisposable
        {
            readonly TcpListener _listener;
            readonly CancellationTokenSource _cts = new();
            readonly Task _acceptLoop;
            int _followedRedirects;

            public string GetterUrl { get; }
            public int FollowedRedirects => Volatile.Read(ref _followedRedirects);

            public OneShotAttachmentRedirectServer(byte[] fileBytes, string fileName)
            {
                _listener = new TcpListener(IPAddress.Loopback, 0);
                _listener.Start();
                int port = ((IPEndPoint)_listener.LocalEndpoint).Port;
                GetterUrl = $"http://127.0.0.1:{port}/getter-29691";
                string downloadPath = "/download-29691";

                byte[] attachment = Encoding.ASCII.GetBytes(
                    "HTTP/1.1 302 Found\r\n" +
                    $"Location: http://127.0.0.1:{port}{downloadPath}\r\n" +
                    $"Content-Disposition: attachment; filename={fileName}\r\n" +
                    "Content-Type: application/octet-stream\r\n" +
                    $"Content-Length: {fileBytes.Length}\r\n" +
                    "Connection: close\r\n\r\n");
                byte[] error = Encoding.ASCII.GetBytes(
                    "HTTP/1.1 500 Internal Server Error\r\n" +
                    "Content-Type: text/html; charset=UTF-8\r\n" +
                    "Content-Length: 0\r\n" +
                    "Connection: close\r\n\r\n");

                _acceptLoop = Task.Run(async () =>
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        TcpClient client;
                        try { client = await _listener.AcceptTcpClientAsync(_cts.Token); }
                        catch (OperationCanceledException) { return; }
                        catch (ObjectDisposedException) { return; }
                        catch (SocketException) { return; }

                        _ = Task.Run(() => Serve(client, attachment, fileBytes, error, downloadPath));
                    }
                });
            }

            void Serve(TcpClient client, byte[] attachmentHead, byte[] fileBytes, byte[] error, string downloadPath)
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    string request = ReadHeaders(stream);
                    bool follow = request.Contains(downloadPath, StringComparison.Ordinal);
                    if (follow)
                        Interlocked.Increment(ref _followedRedirects);

                    if (follow)
                    {
                        stream.Write(error);
                    }
                    else
                    {
                        stream.Write(attachmentHead);
                        stream.Write(fileBytes);
                    }
                }
            }

            static string ReadHeaders(NetworkStream stream)
            {
                var buffer = new byte[4096];
                var ms = new MemoryStream();
                while (ms.Length < 65536)
                {
                    int n = stream.Read(buffer, 0, buffer.Length);
                    if (n <= 0)
                        break;
                    ms.Write(buffer, 0, n);
                    string text = Encoding.ASCII.GetString(ms.ToArray());
                    if (text.Contains("\r\n\r\n", StringComparison.Ordinal))
                        return text;
                }
                return Encoding.ASCII.GetString(ms.ToArray());
            }

            public void Dispose()
            {
                _cts.Cancel();
                try { _listener.Stop(); } catch { /* already stopped */ }
                try { _acceptLoop.Wait(TimeSpan.FromSeconds(2)); } catch { /* shutting down */ }
                _cts.Dispose();
            }
        }
    }
}
