using System.Windows;
using System.Windows.Controls;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls.Tabs
{
    public partial class SpatialTab : UserControl
    {
        public SpatialTab()
        {
            InitializeComponent();
        }

        TrackPropsViewModel VM => DataContext as TrackPropsViewModel;

        void DefaultSpatialBtn_Click(object sender, RoutedEventArgs e)
            => VM?.ResetSpatial?.Invoke();
    }
}
