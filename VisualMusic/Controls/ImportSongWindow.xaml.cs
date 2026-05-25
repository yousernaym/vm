using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// WPF replacement for the WinForms SourceFileForm / ImportNotesWithAudioForm hierarchy.
    /// Parameterised by file type; caller reads NoteFilePath, AudioFilePath, EraseCurrent, InsTrack.
    /// </summary>
    public partial class ImportSongWindow : MetroWindow, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- Bound properties ----

        public string Title { get; }

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

        bool _insTrack;
        public bool InsTrack
        {
            get => _insTrack;
            set { _insTrack = value; Notify(); }
        }

        // ---- File-dialog filters ----
        readonly string _noteFilter;
        readonly string _audioFilter = "Wave files (*.wav)|*.wav|All files (*.*)|*.*";

        string _noteFolder = "";
        string _audioFolder = "";

        // ---- Constructor ----

        public ImportSongWindow(Midi.FileType fileType)
        {
            DataContext = this;
            InitializeComponent();

            switch (fileType)
            {
                case Midi.FileType.Midi:
                    Title = "Import MIDI Song";
                    _noteFilter = BuildFilter("MIDI files", ImportMidiForm.Formats);
                    break;
                case Midi.FileType.Mod:
                    Title = "Import Module";
                    _noteFilter = BuildFilter("Module files", ImportModForm.Formats);
                    break;
                case Midi.FileType.Sid:
                    Title = "Import SID Song";
                    _noteFilter = BuildFilter("SID files", ImportSidForm.Formats);
                    AudioLabel = "Audio file (leave empty for SID audio):";
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
        }

        void BrowseAudio_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = _audioFilter };
            if (!string.IsNullOrEmpty(_audioFolder)) dlg.InitialDirectory = _audioFolder;
            if (dlg.ShowDialog(this) != true) return;
            AudioFilePath = dlg.FileName;
            _audioFolder = Path.GetDirectoryName(dlg.FileName);
        }

        // ---- OK / Cancel ----

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoteFilePath))
            {
                MessageBox.Show("Please specify a song file.", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                noteFileBox.Focus();
                return;
            }
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
