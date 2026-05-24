using MahApps.Metro.Controls;
using System.Windows;
using VisualMusic.ViewModels;

namespace VisualMusic
{
    public partial class MainWindow : MetroWindow
    {
        MainViewModel vm;

        public MainWindow(string[] args = null)
        {
            vm = new MainViewModel();
            DataContext = vm;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            modBrowser.ImportService  = vm;
            sidBrowser.ImportService  = vm;
            midiBrowser.ImportService = vm;

            modBrowser.GetProject  = () => monoGameHost.Renderer?.Project;
            sidBrowser.GetProject  = () => monoGameHost.Renderer?.Project;
            midiBrowser.GetProject = () => monoGameHost.Renderer?.Project;

            modBrowser.Url  = "https://modarchive.org/index.php?request=view_searchbox";
            sidBrowser.Url  = "https://www.exotica.org.uk/wiki/Special:HVSC";
            midiBrowser.Url = "https://bitmidi.com/";

            vm.GetDrawHost = () => monoGameHost.Renderer;

            vm.OnProjectLoaded = project =>
            {
                if (monoGameHost.Renderer != null)
                    monoGameHost.Renderer.Project = project;
            };

            vm.OnLoadBackgroundImage = path =>
                monoGameHost.Renderer?.LoadBackgroundImage(path);

            vm.GetRendererWaveformPanel = () =>
                monoGameHost.Renderer?.WaveformPanel;
        }
    }
}
