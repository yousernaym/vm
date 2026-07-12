using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Shown on first save / "Save As" of a non-MIDI project. Lets the user optionally export the
    /// converted MIDI and/or the generated WAV alongside the project. Both boxes default unchecked.
    /// A box is disabled when its source file isn't available (e.g. WAV when the user supplied audio).
    /// Caller reads <see cref="SaveMidi"/> / <see cref="SaveWav"/> after <c>ShowDialog() == true</c>.
    /// </summary>
    public partial class SaveSourceFilesWindow : MetroWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public bool MidiAvailable { get; }
        public bool WavAvailable { get; }
        public bool TrackWavsAvailable { get; }

        bool _saveMidi;
        public bool SaveMidi
        {
            get => _saveMidi;
            set
            {
                _saveMidi = value;
                Notify();
                // Saving the MIDI converts the project so the remuxer never re-runs; the WAVs must
                // then be saved alongside or their temp copies eventually go stale. Auto-check both
                // (the user can still uncheck manually).
                if (value)
                {
                    SaveWav = WavAvailable;
                    SaveTrackWavs = TrackWavsAvailable;
                }
            }
        }

        bool _saveWav;
        public bool SaveWav
        {
            get => _saveWav;
            set { _saveWav = value; Notify(); }
        }

        bool _saveTrackWavs;
        public bool SaveTrackWavs
        {
            get => _saveTrackWavs;
            set { _saveTrackWavs = value; Notify(); }
        }

        public SaveSourceFilesWindow(bool midiAvailable, bool wavAvailable, bool trackWavsAvailable)
        {
            MidiAvailable = midiAvailable;
            WavAvailable = wavAvailable;
            TrackWavsAvailable = trackWavsAvailable;

            DataContext = this;
            InitializeComponent();
        }

        void Ok_Click(object sender, RoutedEventArgs e) => DialogResult = true;

        void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
