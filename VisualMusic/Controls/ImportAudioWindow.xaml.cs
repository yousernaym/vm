using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Minimal dialog for importing a pure-audio project: a required master audio path (drives
    /// playback and song length) plus an "Erase current project" checkbox. Tracks are created
    /// afterward via the batch-assign flow (Audio tab → "Assign audio files…").
    /// Caller reads <see cref="AudioFilePath"/> and <see cref="EraseCurrent"/>.
    /// </summary>
    public partial class ImportAudioWindow : MetroWindow, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- In-memory session cache (resets to defaults after app restart) ----
        static (string AudioPath, bool Erase)? s_session;

        /// <summary>
        /// Update the session cache without opening the dialog. Call this after a project load so
        /// the dialog is pre-filled correctly if the user opens it manually afterward.
        /// </summary>
        internal static void UpdateSession(string audioPath, bool erase)
            => s_session = (audioPath, erase);

        // ---- Bound properties ----

        string _audioFilePath = "";
        public string AudioFilePath
        {
            get => _audioFilePath;
            set { _audioFilePath = value; Notify(); }
        }

        bool _eraseCurrent = true;
        public bool EraseCurrent
        {
            get => _eraseCurrent;
            set { _eraseCurrent = value; Notify(); }
        }

        // Master audio is decoded by Media Foundation (per-track filter with *.ogg is separate).
        readonly string _audioFilter = "Audio files|*.wav;*.mp3;*.flac;*.m4a;*.wma|All files (*.*)|*.*";
        string _audioFolder;

        // ---- Constructor ----

        public ImportAudioWindow()
        {
            _audioFolder = AppSettings.Instance.GetAudioFolder(FileType.Audio);

            DataContext = this;
            InitializeComponent();

            // Restore in-memory session values (resets to defaults after app restart)
            if (s_session is { } s)
            {
                AudioFilePath = s.AudioPath;
                EraseCurrent = s.Erase;
            }
        }

        // ---- Browse handler ----

        void BrowseAudio_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = _audioFilter };
            if (!string.IsNullOrEmpty(_audioFolder)) dlg.InitialDirectory = _audioFolder;
            if (dlg.ShowDialog(this) != true) return;
            AudioFilePath = dlg.FileName;
            _audioFolder = Path.GetDirectoryName(dlg.FileName);
            AppSettings.Instance.SetAudioFolder(FileType.Audio, _audioFolder);
            AppSettings.Instance.Save();
        }

        // ---- OK / Cancel ----

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AudioFilePath))
            {
                MetroMessageBox.Show("Please specify an audio file.", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                audioFileBox.Focus();
                return;
            }

            // Remember the fields for the rest of this session.
            s_session = (AudioFilePath, EraseCurrent);
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
