using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace VisualMusic
{
    public static class Download
    {
        const int MaxRedirects = 10;

        // Some hosts (e.g. the exotica.org.uk SID browser) gate the first request behind a
        // cookie-setting "Verifying..." HTML interstitial that returns 200 OK. The shared HttpClient
        // stores the cookie it sets, so re-requesting a bounded number of times fetches the real file.
        const int MaxVerifyRetries = 2;

        static readonly HttpClient Http = CreateHttpClient();

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
                return DownloadToTempFileAsync(url, null, CancellationToken.None)
                    .GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

        internal static async Task<string> DownloadToTempFileAsync(
            string url,
            Action<long?, long> progress,
            CancellationToken cancelToken)
        {
            Uri uri = new Uri(url);
            Uri referrer = new Uri(uri.GetLeftPart(UriPartial.Authority) + "/");

            int verifyRetries = 0;
            for (int redirects = 0; redirects <= MaxRedirects; redirects++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Referrer = referrer;

                using HttpResponseMessage response = await Http.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancelToken);

                if (response.IsSuccessStatusCode || HasAttachmentDisposition(response))
                {
                    // A cookie-gate interstitial (see MaxVerifyRetries) returns an HTML body instead of
                    // the file. Music downloads are always binary, so an HTML response means the file
                    // wasn't served yet; the interstitial has now set its cookie (stored in the shared
                    // HttpClient), so re-request the same URL rather than saving the HTML as the file.
                    if (IsHtmlResponse(response) && verifyRetries++ < MaxVerifyRetries)
                    {
                        redirects--;   // a verification hop isn't a redirect; don't spend the budget
                        continue;
                    }

                    string fileName = GetDownloadFileName(response, uri.ToString());
                    string path = Path.Combine(Program.TempDir, fileName);
                    await WriteContentAsync(response, path, progress, cancelToken);
                    return path;
                }

                if (IsRedirect(response) && response.Headers.Location != null)
                {
                    uri = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(uri, response.Headers.Location);
                    continue;
                }

                response.EnsureSuccessStatusCode();
            }

            return null;
        }

        internal static string GetDownloadFileName(WebHeaderCollection headers, string url)
            => GetDownloadFileName(headers?["Content-Disposition"], url);

        internal static string GetDownloadFileName(string contentDisposition, string url)
        {
            if (!string.IsNullOrEmpty(contentDisposition))
            {
                try
                {
                    string name = null;
                    if (ContentDispositionHeaderValue.TryParse(contentDisposition, out var parsed))
                        name = parsed.FileNameStar ?? parsed.FileName;

                    name ??= new System.Net.Mime.ContentDisposition(contentDisposition).FileName;
                    if (!string.IsNullOrWhiteSpace(name))
                        return Path.GetFileName(name.Trim('"'));
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

        static HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/120.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            return client;
        }

        static string GetDownloadFileName(HttpResponseMessage response, string url)
        {
            string contentDisposition = response.Content.Headers.ContentDisposition?.ToString();
            if (string.IsNullOrEmpty(contentDisposition) &&
                response.Headers.TryGetValues("Content-Disposition", out var values))
            {
                contentDisposition = values.FirstOrDefault();
            }

            return GetDownloadFileName(contentDisposition, url);
        }

        static bool HasAttachmentDisposition(HttpResponseMessage response)
        {
            string contentDisposition = response.Content.Headers.ContentDisposition?.ToString();
            if (string.IsNullOrEmpty(contentDisposition) &&
                response.Headers.TryGetValues("Content-Disposition", out var values))
            {
                contentDisposition = values.FirstOrDefault();
            }

            return contentDisposition?.IndexOf("attachment", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsHtmlResponse(HttpResponseMessage response)
        {
            string mediaType = response.Content.Headers.ContentType?.MediaType;
            return mediaType != null &&
                   mediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase);
        }

        static bool IsRedirect(HttpResponseMessage response)
        {
            int status = (int)response.StatusCode;
            return status >= 300 && status < 400;
        }

        static async Task WriteContentAsync(
            HttpResponseMessage response,
            string path,
            Action<long?, long> progress,
            CancellationToken cancelToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            long? total = response.Content.Headers.ContentLength;
            long downloaded = 0;
            progress?.Invoke(total, downloaded);

            await using Stream source = await response.Content.ReadAsStreamAsync(cancelToken);
            await using var dest = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            var buffer = new byte[81920];
            int read;
            while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancelToken)) > 0)
            {
                await dest.WriteAsync(buffer.AsMemory(0, read), cancelToken);
                downloaded += read;
                progress?.Invoke(total, downloaded);
            }
        }
    }
}
