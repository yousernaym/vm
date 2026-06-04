using System;
using System.Windows;
using System.Windows.Controls;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls
{
    public partial class SongPropsPanel : UserControl
    {
        public SongPropsPanel()
        {
            InitializeComponent();
        }

        void VpWidth_CommitChanges(object sender, EventArgs e)
            => (DataContext as SongPropsViewModel)?.CommitViewWidth?.Invoke();

        void ResetPitches_Click(object sender, RoutedEventArgs e)
            => (DataContext as SongPropsViewModel)?.ResetPitches?.Invoke();

        void LoadBkg_Click(object sender, RoutedEventArgs e)
            => (DataContext as SongPropsViewModel)?.BrowseBackground?.Invoke();

        void UnloadBkg_Click(object sender, RoutedEventArgs e)
            => (DataContext as SongPropsViewModel)?.UnloadBackground?.Invoke();
    }
}
