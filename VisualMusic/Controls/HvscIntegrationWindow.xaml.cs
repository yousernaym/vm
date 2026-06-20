using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// WPF dialog for HVSC (High Voltage SID Collection) integration.
    /// Lets the user update the HVSC song-lengths database used for per-subsong
    /// duration lookup when importing SID files.
    /// </summary>
    public partial class HvscIntegrationWindow : MetroWindow, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- Bindable properties ----

        string _url;
        public string Url
        {
            get => _url;
            set { _url = value; Notify(); }
        }

        bool _isDownloading;
        public bool IsDownloading
        {
            get => _isDownloading;
            set { _isDownloading = value; Notify(); Notify(nameof(IsIdle)); }
        }

        /// <summary>True when no download is in progress; used to enable/disable controls.</summary>
        public bool IsIdle => !_isDownloading;

        int _downloadProgress;
        public int DownloadProgress
        {
            get => _downloadProgress;
            set { _downloadProgress = value; Notify(); }
        }

        string _lastUpdated;
        public string LastUpdated
        {
            get => _lastUpdated;
            set { _lastUpdated = value; Notify(); }
        }

        // ---- State ----

        CancellationTokenSource _cts;

        // ---- Constructor ----

        public HvscIntegrationWindow()
        {
            DataContext = this;
            InitializeComponent();
            Url = AppSettings.Instance.SongLengthsUrlOrDefault;
            RefreshLastUpdated();
        }

        // ---- Helpers ----

        void RefreshLastUpdated()
        {
            DateTime? t = Hvsc.GetLastUpdatedTime();
            LastUpdated = t.HasValue
                ? "Last updated: " + t.Value.ToString()
                : "Not downloaded yet.";
        }

        void PersistUrl()
        {
            AppSettings.Instance.SongLengthsUrl = Url;
            AppSettings.Instance.Save();
        }

        // ---- Event handlers ----

        async void Update_Click(object sender, RoutedEventArgs e)
        {
            PersistUrl();

            _cts = new CancellationTokenSource();
            IsDownloading = true;
            DownloadProgress = 0;

            bool cancelled = false;
            try
            {
                var progressReporter = new Progress<int>(p => DownloadProgress = p);
                bool success = await Hvsc.DownloadSonglengthsAsync(Url, progressReporter, _cts.Token);
                cancelled = !success;
                if (success)
                {
                    DownloadProgress = 100;
                    RefreshLastUpdated();
                }
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(
                    "Couldn't download the file from the specified URL.\n" + ex.Message,
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsDownloading = false;
                _cts?.Dispose();
                _cts = null;
            }

            if (cancelled)
                MetroMessageBox.Show("Download cancelled.", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void CancelDownload_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        void DefaultUrl_Click(object sender, RoutedEventArgs e)
        {
            Url = Hvsc.DefaultSongLengthsUrl;
        }

        void Close_Click(object sender, RoutedEventArgs e)
        {
            PersistUrl();
            Close();
        }
    }
}
