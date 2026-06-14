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
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.dds|All files|*.*"
            };
            var texDir = AppSettings.Instance.TextureFolder;
            if (!string.IsNullOrEmpty(texDir)) dlg.InitialDirectory = texDir;
            if (dlg.ShowDialog() != true) return;
            AppSettings.Instance.RememberFolder(dlg.FileName, dir => AppSettings.Instance.TextureFolder = dir);
            vm.LoadTexture(dlg.FileName);
        }

        void UnloadTexBtn_Click(object sender, RoutedEventArgs e)
            => VM?.UnloadTexture?.Invoke();

        void DefaultMaterialBtn_Click(object sender, RoutedEventArgs e)
            => VM?.ResetMaterial?.Invoke();
    }
}
