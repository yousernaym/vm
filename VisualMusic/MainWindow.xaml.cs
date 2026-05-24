using MahApps.Metro.Controls;
using System.Windows;
using VisualMusic.ViewModels;

namespace VisualMusic
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(string[] args = null)
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;

            modBrowser.ImportService  = vm;
            sidBrowser.ImportService  = vm;
            midiBrowser.ImportService = vm;

            modBrowser.GetProject  = () => monoGameHost.Renderer?.Project;
            sidBrowser.GetProject  = () => monoGameHost.Renderer?.Project;
            midiBrowser.GetProject = () => monoGameHost.Renderer?.Project;

            modBrowser.Url  = "https://modarchive.org/index.php?request=view_searchbox";
            sidBrowser.Url  = "https://www.exotica.org.uk/wiki/Special:HVSC";
            midiBrowser.Url = "https://bitmidi.com/";
        }
    }
}
