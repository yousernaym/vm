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
        static Client client;

        public static void init(Form form)
        {
            client = new Client();
            form.Controls.Add(client);
        }

        /// <summary>
        /// Downloads <paramref name="url"/> to a file in the temp dir and returns the local path
        /// (or null on failure). Uses WebClient rather than the embedded CefSharp browser so it
        /// works in the WPF app, where the CefSharp.WinForms download client is never initialized.
        /// The filename is taken from the server's Content-Disposition header when present
        /// (matching the old CefSharp behavior), else derived from the URL.
        /// </summary>
        public static string downloadFile(this string url)
        {
            try
            {
                using var webClient = new WebClient();
                byte[] data = webClient.DownloadData(url);
                string fileName = getDownloadFileName(webClient.ResponseHeaders, url);
                string path = Path.Combine(Program.TempDir, fileName);
                File.WriteAllBytes(path, data);
                return path;
            }
            catch
            {
                return null;   // setNotePath() turns this into a FileImportException
            }
        }

        static string getDownloadFileName(WebHeaderCollection headers, string url)
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

        public static SourceFileForm getImportFormFromFileType(this Form1 mainForm, string fileName)
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
            client.Active = true;
        }
    }

    class Client : ChromiumWebBrowser
    {
        string savePath { get; set; }
        ProgressForm progressForm = new ProgressForm();
        DownloadHandler downloadHandler;

        public bool Active { set => this.InvokeOnUiThreadIfRequired(() => Visible = value); }

        string url;
        AutoResetEvent checkFileNameEvent = new AutoResetEvent(false);

        public Client() : base("")
        {
            Width = Height = 0;
            downloadHandler = new DownloadHandler();
            downloadHandler.OnBeforeDownloadFired += OnBeforeDownload;
            downloadHandler.OnDownloadUpdatedFired += OnDownloadUpdated;
            downloadHandler.ShowDialog = false;
            this.DownloadHandler = downloadHandler;
            LoadError += OnLoadError;
            AddressChanged += OnAddressChanged;
            progressForm.ProgressText = "Download progress";
            Active = false;
        }

        private void OnAddressChanged(object sender, AddressChangedEventArgs e)
        {
            progressForm.DialogResult = DialogResult.Abort;
        }

        private void OnLoadError(object sender, LoadErrorEventArgs e)
        {
            if (e.ErrorCode != CefErrorCode.Aborted) //aborted means download started instead of a page loading
                progressForm.DialogResult = DialogResult.Cancel;
        }

        private void OnBeforeDownload(object sender, DownloadItem e)
        {
            savePath = e.SuggestedFileName = Path.Combine(VisualMusic.Program.TempDir, e.SuggestedFileName);
        }

        private void OnDownloadUpdated(object sender, DownloadItem e)
        {
            progressForm.InvokeOnUiThreadIfRequired(() => progressForm.updateProgress(e.PercentComplete / 100.0f));
            if (e.IsComplete)
            {
                progressForm.InvokeOnUiThreadIfRequired(delegate ()
                {
                    progressForm.DialogResult = DialogResult.OK;
                    progressForm.Hide();
                });
                Active = false;
            }
        }

        public new string Load(string url)
        {
            DialogResult dlgRes = DialogResult.Abort;
            this.url = url;
            try
            {
                Active = true;
                base.Load(url);
                dlgRes = progressForm.ShowDialog();
            }
            finally
            {
                if (dlgRes != DialogResult.OK)
                {
                    savePath = null;
                    if (downloadHandler.UpdateCallback != null && !downloadHandler.UpdateCallback.IsDisposed)
                        downloadHandler.UpdateCallback.Cancel();
                }
                Active = false;
            }
            //if (dlgRes == DialogResult.Abort)
            //throw new IOException("Unexpected error while downloading from url: " + url);

            return savePath;
        }
    }
}

