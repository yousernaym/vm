using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace VisualMusic
{
    /// <summary>
    /// Central, non-UI helper for HVSC (High Voltage SID Collection) data.
    /// Owns paths, the default URL, the song-length DB lookup, and the async download.
    /// </summary>
    internal static class Hvsc
    {
        // ---- Paths ----

        public static readonly string TpartyDir      = Path.Combine(Program.AppDataDir, "tparty");
        public static readonly string HvscDir        = Path.Combine(TpartyDir, "hvsc");

        /// <summary>Canonical target path for the downloaded song-lengths DB.</summary>
        public static readonly string SongLengthsPath = Path.Combine(HvscDir, "Songlengths.md5");

        // ---- Default URL ----

        public const string DefaultSongLengthsUrl =
            "https://www.hvsc.c64.org/download/C64Music/DOCUMENTS/Songlengths.md5";

        // ---- Shared HttpClient (reuse across downloads) ----

        static readonly HttpClient s_http = new();

        // ---- Public API ----

        /// <summary>
        /// Returns the per-subsong M:SS length strings from the HVSC song-length DB
        /// for the given SID file, or null if the DB is absent or the file isn't listed.
        /// </summary>
        public static string[] GetSongLengths(string sidPath)
        {
            string dbPath = ResolveDbPath();
            if (dbPath == null) return null;

            string hash;
            using (var stream = File.OpenRead(sidPath))
            {
                byte[] bytes = MD5.HashData(stream);
                hash = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }

            using var reader = new StreamReader(dbPath);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line) || line[0] == ';') continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                if (line.Substring(0, eq).ToLowerInvariant() == hash)
                    return line.Substring(eq + 1).Split(' ');
            }
            return null;
        }

        /// <summary>
        /// Downloads the song-lengths DB from <paramref name="url"/> to
        /// <see cref="SongLengthsPath"/>, reporting download percent via
        /// <paramref name="progress"/> (may be null). Returns false on cancellation,
        /// true on success, throws on network/IO errors.
        /// </summary>
        public static async Task<bool> DownloadSonglengthsAsync(
            string url,
            IProgress<int> progress,
            CancellationToken ct)
        {
            EnsureHvscDir();
            string tempPath = SongLengthsPath + "_";
            try
            {
                using var response = await s_http.GetAsync(
                    url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                using (var src = await response.Content.ReadAsStreamAsync(ct))
                using (var dst = new FileStream(
                           tempPath, FileMode.Create, FileAccess.Write,
                           FileShare.None, 65536, useAsync: true))
                {
                    byte[] buf = new byte[65536];
                    long downloaded = 0;
                    int lastPct = -1;
                    int read;
                    while ((read = await src.ReadAsync(buf, 0, buf.Length, ct)) > 0)
                    {
                        await dst.WriteAsync(buf, 0, read, ct);
                        downloaded += read;
                        if (totalBytes > 0)
                        {
                            int pct = (int)(downloaded * 100 / totalBytes.Value);
                            if (pct != lastPct)
                            {
                                progress?.Report(pct);
                                lastPct = pct;
                            }
                        }
                    }
                }

                TryDelete(SongLengthsPath);
                File.Move(tempPath, SongLengthsPath);
                return true;
            }
            catch (OperationCanceledException)
            {
                TryDelete(tempPath);
                return false;
            }
            catch
            {
                TryDelete(tempPath);
                throw;
            }
        }

        /// <summary>
        /// Silent startup download: skips if the local DB is less than 30 days old,
        /// otherwise downloads from the persisted URL. All exceptions are swallowed.
        /// </summary>
        public static async Task EnsureRecentAsync()
        {
            try
            {
                string existing = ResolveDbPath();
                if (existing != null)
                {
                    double days = (DateTime.Now - new FileInfo(existing).LastWriteTime).TotalDays;
                    if (days < 30) return;
                }
                await DownloadSonglengthsAsync(
                    AppSettings.Instance.SongLengthsUrlOrDefault,
                    progress: null,
                    ct: CancellationToken.None);
            }
            catch { /* best-effort; never crash startup */ }
        }

        /// <summary>
        /// Returns the last-write time of the local DB, or null if not present.
        /// </summary>
        public static DateTime? GetLastUpdatedTime()
        {
            string path = ResolveDbPath();
            return path != null ? new FileInfo(path).LastWriteTime : (DateTime?)null;
        }

        // ---- Internal helpers ----

        /// <summary>
        /// Finds whichever local DB file exists: the canonical name first, then the
        /// legacy misspelled name used by old WinForms builds ("songlenghts.md5").
        /// Returns null if neither exists.
        /// </summary>
        static string ResolveDbPath()
        {
            if (File.Exists(SongLengthsPath)) return SongLengthsPath;
            // Backward compat: legacy builds wrote a misspelled filename
            string legacy = Path.Combine(HvscDir, "songlenghts.md5");
            if (File.Exists(legacy)) return legacy;
            return null;
        }

        static void EnsureHvscDir()
        {
            Directory.CreateDirectory(HvscDir);
        }

        static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }
    }
}
