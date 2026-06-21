using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Application settings dialog. Organized into side-menu sections:
    ///   * Themes — MahApps.Metro base color + accent color scheme (previewed live).
    ///   * Integrations — HVSC song-lengths database URL/download and the web-browser start URLs.
    /// OK persists all changes; Cancel reverts the live theme and discards URL edits.
    /// </summary>
    public partial class SettingsWindow : MetroWindow, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- Section selection ----

        int _selectedSectionIndex;
        public int SelectedSectionIndex
        {
            get => _selectedSectionIndex;
            set
            {
                if (_selectedSectionIndex == value) return;
                _selectedSectionIndex = value;
                Notify();
                Notify(nameof(IsThemesSelected));
                Notify(nameof(IsIntegrationsSelected));
            }
        }

        public bool IsThemesSelected => _selectedSectionIndex == 0;
        public bool IsIntegrationsSelected => _selectedSectionIndex == 1;

        // ---- Theme state ----

        readonly string _originalBase;
        readonly string _originalScheme;

        public IEnumerable<string> BaseColors => ThemeManager.Current.BaseColors;
        public IEnumerable<string> ColorSchemes => ThemeManager.Current.ColorSchemes;

        string _selectedBaseColor;
        public string SelectedBaseColor
        {
            get => _selectedBaseColor;
            set
            {
                if (_selectedBaseColor == value) return;
                _selectedBaseColor = value;
                Notify();
                ApplyTheme();
            }
        }

        string _selectedColorScheme;
        public string SelectedColorScheme
        {
            get => _selectedColorScheme;
            set
            {
                if (_selectedColorScheme == value) return;
                _selectedColorScheme = value;
                Notify();
                ApplyTheme();
            }
        }

        // ---- HVSC state ----

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

        CancellationTokenSource _cts;

        // ---- Browser URL state ----

        string _modBrowserUrl;
        public string ModBrowserUrl
        {
            get => _modBrowserUrl;
            set { _modBrowserUrl = value; Notify(); }
        }

        string _sidBrowserUrl;
        public string SidBrowserUrl
        {
            get => _sidBrowserUrl;
            set { _sidBrowserUrl = value; Notify(); }
        }

        string _midiBrowserUrl;
        public string MidiBrowserUrl
        {
            get => _midiBrowserUrl;
            set { _midiBrowserUrl = value; Notify(); }
        }

        // ---- Constructor ----

        public SettingsWindow()
        {
            DataContext = this;
            InitializeComponent();

            // Open the dropdowns on the currently-applied theme. DetectTheme reflects the live
            // theme; fall back to saved settings if detection fails.
            var current = ThemeManager.Current.DetectTheme(Application.Current);
            _originalBase = current?.BaseColorScheme ?? AppSettings.Instance.ThemeBaseColorOrDefault;
            _originalScheme = current?.ColorScheme ?? AppSettings.Instance.ThemeColorSchemeOrDefault;

            // Assign the backing fields directly (no ApplyTheme — the theme is already applied),
            // then notify so the bound ComboBoxes show the current selection.
            _selectedBaseColor = _originalBase;
            _selectedColorScheme = _originalScheme;
            Notify(nameof(SelectedBaseColor));
            Notify(nameof(SelectedColorScheme));

            // Integration settings (edited copies; committed to AppSettings on OK).
            Url = AppSettings.Instance.SongLengthsUrlOrDefault;
            ModBrowserUrl = AppSettings.Instance.ModBrowserUrlOrDefault;
            SidBrowserUrl = AppSettings.Instance.SidBrowserUrlOrDefault;
            MidiBrowserUrl = AppSettings.Instance.MidiBrowserUrlOrDefault;
            RefreshLastUpdated();
        }

        // ---- Helpers ----

        void ApplyTheme()
        {
            if (_selectedBaseColor == null || _selectedColorScheme == null) return;
            ThemeManager.Current.ChangeTheme(Application.Current, _selectedBaseColor, _selectedColorScheme);
        }

        void RefreshLastUpdated()
        {
            DateTime? t = Hvsc.GetLastUpdatedTime();
            LastUpdated = t.HasValue
                ? "Last updated: " + t.Value.ToString()
                : "Not downloaded yet.";
        }

        // ---- HVSC event handlers ----

        async void Update_Click(object sender, RoutedEventArgs e)
        {
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

        // ---- Browser URL "Default" handlers ----

        void DefaultModUrl_Click(object sender, RoutedEventArgs e)
            => ModBrowserUrl = AppSettings.DefaultModBrowserUrl;

        void DefaultSidUrl_Click(object sender, RoutedEventArgs e)
            => SidBrowserUrl = AppSettings.DefaultSidBrowserUrl;

        void DefaultMidiUrl_Click(object sender, RoutedEventArgs e)
            => MidiBrowserUrl = AppSettings.DefaultMidiBrowserUrl;

        // ---- OK / Cancel ----

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            var s = AppSettings.Instance;
            s.ThemeBaseColor = _selectedBaseColor;
            s.ThemeColorScheme = _selectedColorScheme;
            s.SongLengthsUrl = Url;
            s.ModBrowserUrl = ModBrowserUrl;
            s.SidBrowserUrl = SidBrowserUrl;
            s.MidiBrowserUrl = MidiBrowserUrl;
            s.Save();
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Revert to the theme that was active when the dialog opened.
            ThemeManager.Current.ChangeTheme(Application.Current, _originalBase, _originalScheme);
            DialogResult = false;
        }
    }
}
