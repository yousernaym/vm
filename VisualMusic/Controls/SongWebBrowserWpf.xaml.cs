using CefSharp;
using CefSharp.Example;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisualMusic.Controls
{
    public partial class SongWebBrowserWpf : UserControl
    {
        ChromiumWebBrowser _browser;
        string _initialUrl;
        readonly HashSet<string> _completedDownloadIds = new HashSet<string>();
        readonly string _downloadFolder = Path.Combine(
            Program.TempDir, "browser-downloads", Path.GetRandomFileName());

        public IImportService ImportService { get; set; }
        public Func<Project> GetProject { get; set; }

        /// <summary>Import type for downloads from this browser (Mod / SID / MIDI).</summary>
        public FileType SourceFileType { get; set; }

        public string Url
        {
            set
            {
                string url = value.Contains("://") ? value : "https://" + value;
                if (_browser == null)
                {
                    _initialUrl = url;
                    return;
                }
                urlTextBox.Text = url;
                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                    _browser.Load(url);
            }
        }

        public SongWebBrowserWpf()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            _browser = new ChromiumWebBrowser(_initialUrl ?? "")
            {
                KeyboardHandler = new WpfBrowserKeyboardHandler(() => GetProject?.Invoke()),
                RequestHandler = new DownloadNavigationHandler()
            };

            var downloadHandler = new DownloadHandler
            {
                ShowDialog = false,
                DownloadPathResolver = GetBrowserDownloadPath
            };
            downloadHandler.OnBeforeDownloadFired += OnBeforeDownload;
            downloadHandler.OnDownloadUpdatedFired += OnDownloadUpdated;
            _browser.DownloadHandler = downloadHandler;

            _browser.LoadingStateChanged += OnLoadingStateChanged;
            _browser.StatusMessage += OnBrowserStatusMessage;
            // AddressChanged is a DependencyPropertyChangedEventHandler in CefSharp.Wpf
            _browser.AddressChanged += (_, e) => Dispatcher.InvokeAsync(() => urlTextBox.Text = (string)e.NewValue);

            browserContainer.Children.Add(_browser);

            if (_initialUrl != null)
                urlTextBox.Text = _initialUrl;
        }

        void OnBeforeDownload(object sender, DownloadItem e)
        {
            Dispatcher.InvokeAsync(() => statusLabel.Text = $"Downloading {e.SuggestedFileName}...");
        }

        void OnDownloadUpdated(object sender, DownloadItem e)
        {
            if (e.IsCancelled)
            {
                Dispatcher.InvokeAsync(() => statusLabel.Text = "Download failed.");
                return;
            }

            if (!e.IsComplete)
            {
                if (e.PercentComplete >= 0)
                    Dispatcher.InvokeAsync(() =>
                        statusLabel.Text = $"Downloading {e.SuggestedFileName}... {e.PercentComplete}%");
                return;
            }

            // CefSharp can report the terminal state more than once. Import each completed
            // browser transaction once, after Chromium has closed the destination file.
            lock (_completedDownloadIds)
            {
                if (!_completedDownloadIds.Add(e.Id.ToString()))
                    return;
            }

            string url = string.IsNullOrWhiteSpace(e.OriginalUrl) ? e.Url : e.OriginalUrl;
            string fileName = string.IsNullOrWhiteSpace(e.SuggestedFileName)
                ? Path.GetFileName(e.FullPath)
                : e.SuggestedFileName;
            string downloadedFilePath = e.FullPath;

            Dispatcher.InvokeAsync(() =>
            {
                statusLabel.Text = "";
                ImportService?.ImportBrowserDownload(
                    url, downloadedFilePath, fileName, SourceFileType);
            });
        }

        string GetBrowserDownloadPath(DownloadItem item)
        {
            string fileName = Path.GetFileName(item.SuggestedFileName);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = Path.GetRandomFileName();

            // A per-download directory avoids collisions while retaining the suggested file name,
            // which is used for type detection and project titles.
            string dir = Path.Combine(_downloadFolder, item.Id.ToString());
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, fileName);
        }

        void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args) =>
            Dispatcher.InvokeAsync(() => statusLabel.Text = args.Value);

        void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args) =>
            Dispatcher.InvokeAsync(() =>
            {
                backButton.IsEnabled = args.CanGoBack;
                forwardButton.IsEnabled = args.CanGoForward;
                goButton.Content = args.IsLoading ? "Stop" : "Go";
            });

        void GoButtonClick(object sender, RoutedEventArgs e) => LoadUrl(urlTextBox.Text);
        void BackButtonClick(object sender, RoutedEventArgs e) => _browser?.Back();
        void ForwardButtonClick(object sender, RoutedEventArgs e) => _browser?.Forward();

        void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoadUrl(urlTextBox.Text);
        }

        void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                _browser?.Load(url);
        }

        sealed class DownloadNavigationHandler : CefSharp.Handler.RequestHandler
        {
            protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser,
                IFrame frame, IRequest request, bool userGesture, bool isRedirect)
            {
                return false;
            }
        }

        sealed class WpfBrowserKeyboardHandler : IKeyboardHandler
        {
            readonly Func<Project> _getProject;
            public WpfBrowserKeyboardHandler(Func<Project> getProject) => _getProject = getProject;

            public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser b, KeyType type,
                int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers,
                bool isSystemKey, ref bool isKeyboardShortcut)
            {
                if (windowsKeyCode == 179 && type == KeyType.RawKeyDown)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => _getProject()?.TogglePlayback());
                    return true;
                }
                return false;
            }

            public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser b, KeyType type,
                int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
                => false;
        }
    }
}
