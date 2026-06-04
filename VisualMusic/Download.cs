using CefSharp;
//using CefSharp.Handler;
using CefSharp.Example;
using CefSharp.WinForms;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace VisualMusic
{
    public static class Download
    {
        // Lazily created by init() — only the legacy WinForms/Form1 path uses the embedded
        // CefSharp browser. The WPF app never calls init(), so this stays null there and the
        // CefSharp.WinForms control is never instantiated inside the WPF (CefSharp.Wpf) process.
        static Client s_client;

        public static void Init(Form form)
        {
            s_client = new Client();
            form.Controls.Add(s_client);
        }

        /// <summary>
        /// Downloads <paramref name="url"/> to a file in the temp dir and returns the local path
        /// (or null on failure/cancellation).  In the WPF build this shows a progress dialog; in
        /// the legacy WinForms build the dispatcher is not available and it falls back to a
        /// synchronous WebClient download.
        /// </summary>
        public static string DownloadFile(this string url)
        {
            // WPF path: show the unified progress window (runs download async off the UI thread).
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
                return Controls.ProgressWindow.RunDownload(url);

            // Legacy WinForms fallback (Form1 path — no WPF Application).
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
                return null;   // setNotePath() turns this into a FileImportException
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
                catch { /* malformed header — fall back to the URL */ }
            }
            string urlName = Path.GetFileName(new Uri(url).LocalPath);
            return string.IsNullOrWhiteSpace(urlName) ? Path.GetRandomFileName() : urlName;
        }

        public static bool IsUrl(this string path)
        {
            return Uri.IsWellFormedUriString(path, UriKind.Absolute);
        }

        public static SourceFileForm GetImportFormFromFileType(this Form1 mainForm, string fileName)
        {
            string ext = fileName.Split('.').Last().ToLower();
            SourceFileForm importForm = null;

            if (ImportMidiForm.Formats.Contains(ext))
                importForm = Form1.ImportMidiForm;
            else if (ImportModForm.Formats.Contains(ext))
                importForm = Form1.ImportModForm;
            else if (ImportSidForm.Formats.Contains(ext))
                importForm = Form1.ImportSidForm;
            return importForm;
        }

        static void ExecuteBlockingTask()
        {
            s_client.Active = true;
        }
    }

    class Client : ChromiumWebBrowser
    {
        string savePath { get; set; }
        ProgressForm _progressForm = new ProgressForm();
        DownloadHandler _downloadHandler;

        public bool Active { set => this.InvokeOnUiThreadIfRequired(() => Visible = value); }

        string _url;
        AutoResetEvent _checkFileNameEvent = new AutoResetEvent(false);

        public Client() : base("")
        {
            Width = Height = 0;
            _downloadHandler = new DownloadHandler();
            _downloadHandler.OnBeforeDownloadFired += OnBeforeDownload;
            _downloadHandler.OnDownloadUpdatedFired += OnDownloadUpdated;
            _downloadHandler.ShowDialog = false;
            this.DownloadHandler = _downloadHandler;
            LoadError += OnLoadError;
            AddressChanged += OnAddressChanged;
            _progressForm.ProgressText = "Download progress";
            Active = false;
        }

        private void OnAddressChanged(object sender, AddressChangedEventArgs e)
        {
            _progressForm.DialogResult = DialogResult.Abort;
        }

        private void OnLoadError(object sender, LoadErrorEventArgs e)
        {
            if (e.ErrorCode != CefErrorCode.Aborted) //aborted means download started instead of a page loading
                _progressForm.DialogResult = DialogResult.Cancel;
        }

        private void OnBeforeDownload(object sender, DownloadItem e)
        {
            savePath = e.SuggestedFileName = Path.Combine(VisualMusic.Program.TempDir, e.SuggestedFileName);
        }

        private void OnDownloadUpdated(object sender, DownloadItem e)
        {
            _progressForm.InvokeOnUiThreadIfRequired(() => _progressForm.UpdateProgress(e.PercentComplete / 100.0f));
            if (e.IsComplete)
            {
                _progressForm.InvokeOnUiThreadIfRequired(delegate ()
                {
                    _progressForm.DialogResult = DialogResult.OK;
                    _progressForm.Hide();
                });
                Active = false;
            }
        }

        public new string Load(string url)
        {
            DialogResult dlgRes = DialogResult.Abort;
            this._url = url;
            try
            {
                Active = true;
                base.Load(url);
                dlgRes = _progressForm.ShowDialog();
            }
            finally
            {
                if (dlgRes != DialogResult.OK)
                {
                    savePath = null;
                    if (_downloadHandler.UpdateCallback != null && !_downloadHandler.UpdateCallback.IsDisposed)
                        _downloadHandler.UpdateCallback.Cancel();
                }
                Active = false;
            }
            //if (dlgRes == DialogResult.Abort)
            //throw new IOException("Unexpected error while downloading from url: " + url);

            return savePath;
        }
    }
}

