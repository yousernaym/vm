using System;
using System.Windows;
using System.Windows.Controls;
using VisualMusic.Keyframes;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls
{
    public partial class SongPropsPanel : UserControl
    {
        public SongPropsPanel()
        {
            InitializeComponent();
        }

        void AudioOffset_CommitChanges(object sender, EventArgs e)
            => KeyframeService.RaiseUndoSnapshot("Edit Audio offset");

        void PlaybackOffset_CommitChanges(object sender, EventArgs e)
            => KeyframeService.RaiseUndoSnapshot("Edit Playback offset");

        void FadeIn_CommitChanges(object sender, EventArgs e)
            => KeyframeService.RaiseUndoSnapshot("Edit Fade in");

        void FadeOut_CommitChanges(object sender, EventArgs e)
            => KeyframeService.RaiseUndoSnapshot("Edit Fade out");

        void AudioVisSide_Click(object sender, RoutedEventArgs e)
            => KeyframeService.RaiseUndoSnapshot("Edit Audio visualization sides");

        void AudioVisWidth_CommitChanges(object sender, EventArgs e)
            => KeyframeService.RaiseUndoSnapshot("Edit Audio visualization width");

        void AudioVisLineWidth_CommitChanges(object sender, EventArgs e)
            => KeyframeService.RaiseUndoSnapshot("Edit Audio visualization line width");

        void ResetPitches_Click(object sender, RoutedEventArgs e)
            => (DataContext as SongPropsViewModel)?.ResetPitches?.Invoke();

        void LoadBkg_Click(object sender, RoutedEventArgs e)
            => (DataContext as SongPropsViewModel)?.BrowseBackground?.Invoke();

        void UnloadBkg_Click(object sender, RoutedEventArgs e)
            => (DataContext as SongPropsViewModel)?.UnloadBackground?.Invoke();
    }
}
