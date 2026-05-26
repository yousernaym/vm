using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls.Tabs
{
    public partial class MaterialTab : UserControl
    {
        public MaterialTab()
        {
            InitializeComponent();
        }

        TrackPropsViewModel VM => DataContext as TrackPropsViewModel;

        void LoadTexBtn_Click(object sender, RoutedEventArgs e)
        {
            var vm = VM;
            if (vm?.LoadTexture == null) return;
            var dlg = new OpenFileDialog
            {
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.dds|All files|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            vm.LoadTexture(dlg.FileName);
        }

        void UnloadTexBtn_Click(object sender, RoutedEventArgs e)
            => VM?.UnloadTexture?.Invoke();
    }
}
