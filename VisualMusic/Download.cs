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

                // Some hosts (FreeMIDI getter URLs) return 302 with the file in the body and
                // Content-Disposition: attachment, while Location points at an HTML page that
                // 500s outside a browser session. Save that body instead of following Location.
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
            string name = TryDispositionFileName(contentDisposition);
            if (!string.IsNullOrWhiteSpace(name))
                return Path.GetFileName(name);

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

        // HttpClient's typed ContentDisposition parser rejects unquoted filenames with
        // parentheses (FreeMIDI: attachment; filename=ThePenis(Eek!).mid), leaving the
        // typed property null. The raw header is still on Content.Headers.
        internal static string GetDownloadFileName(HttpResponseMessage response, string url)
            => GetDownloadFileName(GetRawContentDisposition(response), url);

        static bool HasAttachmentDisposition(HttpResponseMessage response)
        {
            string contentDisposition = GetRawContentDisposition(response);
            return contentDisposition?.IndexOf("attachment", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static string GetRawContentDisposition(HttpResponseMessage response)
        {
            if (response.Content.Headers.TryGetValues("Content-Disposition", out var contentValues))
            {
                string raw = contentValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(raw))
                    return raw;
            }

            if (response.Headers.TryGetValues("Content-Disposition", out var values))
            {
                string raw = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(raw))
                    return raw;
            }

            return response.Content.Headers.ContentDisposition?.ToString();
        }

        static string TryDispositionFileName(string contentDisposition)
        {
            if (string.IsNullOrEmpty(contentDisposition))
                return null;

            try
            {
                if (ContentDispositionHeaderValue.TryParse(contentDisposition, out var parsed))
                {
                    string name = parsed.FileNameStar ?? parsed.FileName;
                    if (!string.IsNullOrWhiteSpace(name))
                        return name.Trim('"');
                }
            }
            catch { /* use lenient parse */ }

            try
            {
                string name = new System.Net.Mime.ContentDisposition(contentDisposition).FileName;
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
            catch { /* use lenient parse */ }

            return TryLenientDispositionFileName(contentDisposition);
        }

        /// <summary>
        /// RFC parsers reject unquoted filename tokens with parentheses/! (legal on disk,
        /// common on FreeMIDI). Split parameters and take filename* then filename.
        /// </summary>
        static string TryLenientDispositionFileName(string header)
        {
            string star = FindDispositionParam(header, "filename*");
            if (!string.IsNullOrWhiteSpace(star))
            {
                int sep = star.IndexOf("''", StringComparison.Ordinal);
                string value = sep >= 0 ? star.Substring(sep + 2) : star;
                try { value = Uri.UnescapeDataString(value); }
                catch { /* keep the raw value */ }
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim().Trim('"');
            }

            string name = FindDispositionParam(header, "filename");
            return string.IsNullOrWhiteSpace(name) ? null : name.Trim().Trim('"');
        }

        static string FindDispositionParam(string header, string name)
        {
            foreach (string rawPart in header.Split(';'))
            {
                string part = rawPart.Trim();
                int eq = part.IndexOf('=');
                if (eq <= 0)
                    continue;
                string key = part.Substring(0, eq).Trim();
                if (!key.Equals(name, StringComparison.OrdinalIgnoreCase))
                    continue;
                string value = part.Substring(eq + 1).Trim();
                if (value.Length >= 2 && value[0] == '"')
                {
                    int end = value.IndexOf('"', 1);
                    return end > 0 ? value.Substring(1, end - 1) : value.Trim('"');
                }
                return value;
            }
            return null;
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
