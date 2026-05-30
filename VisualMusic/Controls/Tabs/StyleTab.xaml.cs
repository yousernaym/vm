using System.Windows;
using System.Windows.Controls;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls.Tabs
{
    public partial class StyleTab : UserControl
    {
        public StyleTab()
        {
            InitializeComponent();
        }

        TrackPropsViewModel VM => DataContext as TrackPropsViewModel;

        void DefaultStyleBtn_Click(object sender, RoutedEventArgs e) =>
            VM?.ResetStyle?.Invoke();

        void NewModEntry_Click(object sender, RoutedEventArgs e) =>
            VM?.AddModEntry?.Invoke();

        void CloneModEntry_Click(object sender, RoutedEventArgs e) =>
            VM?.CloneModEntry?.Invoke();

        void DeleteModEntry_Click(object sender, RoutedEventArgs e) =>
            VM?.DeleteModEntry?.Invoke();
    }
}
