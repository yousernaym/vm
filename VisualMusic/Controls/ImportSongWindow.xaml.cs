using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Dialog for importing MIDI, module, SID, and HVL files.
    /// Parameterised by file type; title is set in the constructor switch (not via binding).
    /// Caller reads NoteFilePath, AudioFilePath, EraseCurrent, InsTrack.
    /// InsTrack: true = one track per instrument (MIDI: per track chunk), false = per channel.
    /// </summary>
    public partial class ImportSongWindow : MetroWindow, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        FileType _fileType;

        // ---- In-memory session cache (resets to defaults after app restart) ----
        static readonly Dictionary<FileType, (bool Erase, string NotePath, string AudioPath, bool InsTrack, bool TrackAudio)> s_session = new();

        /// <summary>
        /// Update the session cache for <paramref name="type"/> without opening the dialog.
        /// Call this after a silent import or project load so the dialog is pre-filled correctly
        /// if the user opens it manually afterward.
        /// </summary>
        internal static void UpdateSession(FileType type, bool erase, string notePath, string audioPath, bool insTrack, bool trackAudio = false)
            => s_session[type] = (erase, notePath, audioPath, insTrack, trackAudio);

        /// <summary>
        /// Uncheck "generate audio file for each track" in the session cache for <paramref name="type"/>
        /// (leaves the persisted per-type preference alone). Called after the per-track WAVs are saved
        /// next to the project, when regeneration is no longer wanted.
        /// </summary>
        internal static void ClearSessionTrackAudio(FileType type)
        {
            if (s_session.TryGetValue(type, out var s))
                s_session[type] = (s.Erase, s.NotePath, s.AudioPath, s.InsTrack, false);
        }

        // ---- Bound properties ----

        string _noteFilePath = "";
        public string NoteFilePath
        {
            get => _noteFilePath;
            set { _noteFilePath = value; Notify(); }
        }

        string _audioFilePath = "";
        public string AudioFilePath
        {
            get => _audioFilePath;
            set { _audioFilePath = value; Notify(); }
        }

        string _audioLabel = "Audio file:";
        public string AudioLabel
        {
            get => _audioLabel;
            set { _audioLabel = value; Notify(); }
        }

        bool _eraseCurrent = true;
        public bool EraseCurrent
        {
            get => _eraseCurrent;
            set { _eraseCurrent = value; Notify(); }
        }

        bool _insTrack = true;
        /// <summary>true = one track per instrument (MIDI: per track chunk).</summary>
        public bool InsTrack
        {
            get => _insTrack;
            set { _insTrack = value; Notify(); Notify(nameof(PerChannel)); }
        }

        /// <summary>Mirror of !InsTrack for the "per channel" radio button.</summary>
        public bool PerChannel
        {
            get => !InsTrack;
            set { if (value) InsTrack = false; }
        }

        bool _trackAudio;
        /// <summary>true = render one WAV per track (in addition to the mixdown). Not offered for MIDI.</summary>
        public bool TrackAudio
        {
            get => _trackAudio;
            set { _trackAudio = value; Notify(); }
        }

        /// <summary>The per-track audio option is disabled for MIDI imports.</summary>
        public bool TrackAudioEnabled => _fileType != FileType.Midi;

        string _perInstrumentLabel = "One track per instrument";
        public string PerInstrumentLabel
        {
            get => _perInstrumentLabel;
            set { _perInstrumentLabel = value; Notify(); }
        }

        string _perChannelLabel = "One track per channel";
        public string PerChannelLabel
        {
            get => _perChannelLabel;
            set { _perChannelLabel = value; Notify(); }
        }

        // ---- File-dialog filters ----
        readonly string _noteFilter;
        readonly string _audioFilter = "Wave files (*.wav)|*.wav|All files (*.*)|*.*";

        string _noteFolder;
        string _audioFolder;

        // ---- Constructor ----

        public ImportSongWindow(FileType fileType)
        {
            _fileType = fileType;
            _noteFolder = AppSettings.Instance.GetNoteFolder(fileType);
            _audioFolder = AppSettings.Instance.GetAudioFolder(fileType);

            DataContext = this;
            InitializeComponent();

            // Restore persisted preferences, then let the session override if present
            InsTrack = AppSettings.Instance.GetInsTrack(fileType);
            TrackAudio = AppSettings.Instance.GetTrackAudio(fileType);

            // Restore in-memory session values (resets to defaults after app restart)
            if (s_session.TryGetValue(fileType, out var s))
            {
                EraseCurrent = s.Erase;
                NoteFilePath = s.NotePath;
                AudioFilePath = s.AudioPath;
                InsTrack = s.InsTrack;
                TrackAudio = s.TrackAudio;
            }

            switch (fileType)
            {
                case FileType.Midi:
                    Title = "Import MIDI Song";
                    _noteFilter = BuildFilter("MIDI files", ImportFileFormats.Midi);
                    PerInstrumentLabel = "One track per MIDI track";
                    PerChannelLabel = "One track per MIDI channel";
                    break;
                case FileType.Mod:
                    Title = "Import Module";
                    _noteFilter = BuildFilter("Module files", ImportFileFormats.Mod);
                    break;
                case FileType.Sid:
                    Title = "Import SID Song";
                    _noteFilter = BuildFilter("SID files", ImportFileFormats.Sid);
                    AudioLabel = "Audio file (leave empty for SID audio):";
                    break;
                case FileType.Hvl:
                    Title = "Import HVL Song";
                    _noteFilter = BuildFilter("HVL files", ImportFileFormats.Hvl);
                    break;
                default:
                    Title = "Import Song";
                    _noteFilter = "All files (*.*)|*.*";
                    break;
            }
        }

        static string BuildFilter(string description, string[] exts)
        {
            string list = string.Join("; ", System.Array.ConvertAll(exts, e => $"*.{e}"));
            return $"{description} ({list})|{list}|All files (*.*)|*.*";
        }

        // ---- Browse handlers ----

        void BrowseNote_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = _noteFilter };
            if (!string.IsNullOrEmpty(_noteFolder)) dlg.InitialDirectory = _noteFolder;
            if (dlg.ShowDialog(this) != true) return;
            NoteFilePath = dlg.FileName;
            _noteFolder = Path.GetDirectoryName(dlg.FileName);
            AppSettings.Instance.SetNoteFolder(_fileType, _noteFolder);
            AppSettings.Instance.Save();
        }

        void BrowseAudio_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = _audioFilter };
            if (!string.IsNullOrEmpty(_audioFolder)) dlg.InitialDirectory = _audioFolder;
            if (dlg.ShowDialog(this) != true) return;
            AudioFilePath = dlg.FileName;
            _audioFolder = Path.GetDirectoryName(dlg.FileName);
            AppSettings.Instance.SetAudioFolder(_fileType, _audioFolder);
            AppSettings.Instance.Save();
        }

        // ---- OK / Cancel ----

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoteFilePath))
            {
                MetroMessageBox.Show("Please specify a song file.", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                noteFileBox.Focus();
                return;
            }

            // Persist the track-split and per-track-audio choices across app restarts
            AppSettings.Instance.SetInsTrack(_fileType, InsTrack);
            AppSettings.Instance.SetTrackAudio(_fileType, TrackAudio);
            AppSettings.Instance.Save();

            // Remember all fields for the rest of this session
            s_session[_fileType] = (EraseCurrent, NoteFilePath, AudioFilePath, InsTrack, TrackAudio);

            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
