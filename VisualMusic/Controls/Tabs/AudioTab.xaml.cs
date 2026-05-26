using System.Windows;
using System.Windows.Controls;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls.Tabs
{
    public partial class AudioTab : UserControl
    {
        public AudioTab()
        {
            InitializeComponent();
        }

        void BrowseBtn_Click(object sender, RoutedEventArgs e)
            => (DataContext as TrackPropsViewModel)?.BrowseAudioFile?.Invoke();
    }
}
