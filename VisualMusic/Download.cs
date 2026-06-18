using System;
using System.IO;
using System.Net;

namespace VisualMusic
{
    public static class Download
    {
        /// <summary>
        /// Downloads <paramref name="url"/> to a file in the temp dir and returns the local path
        /// (or null on failure/cancellation).
        /// </summary>
        public static string DownloadFile(this string url)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
                return Controls.ProgressWindow.RunDownload(url);

            try
            {
                using var webClient = new WebClient();
                byte[] data = webClient.DownloadData(url);
                string fileName = GetDownloadFileName(webClient.ResponseHeaders, url);
                string path = Path.Combine(Program.TempDir, fileName);
                File.WriteAllBytes(path, data);
                return path;
            }
            catch
            {
                return null;
            }
        }

        internal static string GetDownloadFileName(WebHeaderCollection headers, string url)
        {
            string contentDisposition = headers?["Content-Disposition"];
            if (!string.IsNullOrEmpty(contentDisposition))
            {
                try
                {
                    string name = new System.Net.Mime.ContentDisposition(contentDisposition).FileName;
                    if (!string.IsNullOrWhiteSpace(name))
                        return Path.GetFileName(name);
                }
                catch
                {
                    // Malformed header - fall back to the URL.
                }
            }

            string urlName = Path.GetFileName(new Uri(url).LocalPath);
            return string.IsNullOrWhiteSpace(urlName) ? Path.GetRandomFileName() : urlName;
        }

        public static bool IsUrl(this string path)
        {
            return Uri.IsWellFormedUriString(path, UriKind.Absolute);
        }
    }
}
