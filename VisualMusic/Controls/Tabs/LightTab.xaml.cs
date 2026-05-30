using System.Windows;
using System.Windows.Controls;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls.Tabs
{
    public partial class LightTab : UserControl
    {
        public LightTab()
        {
            InitializeComponent();
        }

        TrackPropsViewModel VM => DataContext as TrackPropsViewModel;

        void DefaultLightBtn_Click(object sender, RoutedEventArgs e)
            => VM?.ResetLight?.Invoke();
    }
}
