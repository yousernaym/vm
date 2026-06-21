using CefSharp;
using CefSharp.Example;
using CefSharp.Wpf;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisualMusic.Controls
{
    public partial class SongWebBrowserWpf : UserControl
    {
        ChromiumWebBrowser _browser;
        string _initialUrl;

        public IImportService ImportService { get; set; }
        public Func<Project> GetProject { get; set; }

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
                RequestHandler = new DownloadNavigationHandler(TryImportKnownDownloadUrl)
            };

            var downloadHandler = new DownloadHandler { ShowDialog = false };
            downloadHandler.OnBeforeDownloadFired += OnBeforeDownload;
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
            e.IsCancelled = true;
            string url = e.Url;
            string fileName = e.SuggestedFileName;
            Dispatcher.InvokeAsync(() => ImportService?.ImportFromUrl(url, fileName));
        }

        bool TryImportKnownDownloadUrl(string url)
        {
            if (!TryGetKnownDownloadFileName(url, out string fileName))
                return false;

            Dispatcher.InvokeAsync(() => ImportService?.ImportFromUrl(url, fileName));
            return true;
        }

        static bool TryGetKnownDownloadFileName(string url, out string fileName)
        {
            fileName = null;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            bool isFreeMidi = uri.Host.Equals("freemidi.org", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals("www.freemidi.org", StringComparison.OrdinalIgnoreCase);
            if (!isFreeMidi)
                return false;

            var match = Regex.Match(uri.AbsolutePath.Trim('/'), @"^getter-(\d+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return false;

            fileName = "freemidi-" + match.Groups[1].Value + ".mid";
            return true;
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
            readonly Func<string, bool> _tryImport;

            public DownloadNavigationHandler(Func<string, bool> tryImport) => _tryImport = tryImport;

            protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser,
                IFrame frame, IRequest request, bool userGesture, bool isRedirect)
            {
                if (!isRedirect && frame.IsMain && string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase) &&
                    _tryImport(request.Url))
                {
                    return true;
                }

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
