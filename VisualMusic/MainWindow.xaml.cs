using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Input;
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

        // Tunneling PreviewKeyDown ensures Ctrl+Space reaches us before focusable panel controls
        // (CheckBox, RadioButton, ListView items) can swallow the Space activation key — mirrors
        // the old WinForms KeyPreview = true behavior.
        void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (vm.TogglePlaybackCommand.CanExecute(null))
                    vm.TogglePlaybackCommand.Execute(null);
                e.Handled = true;
            }

            // Escape exits mouse-look mode even when WPF focus is outside the song panel.
            if (e.Key == Key.Escape && vm.IsMouseLookMode)
            {
                monoGameHost.Renderer?.ToggleMouseLook();
                e.Handled = true;
            }
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
            monoGameHost.Renderer?.SetTrackSelectionService(trackListControl);

            // Wire mouse-look mode toggle to the view-model flag so the yellow label updates.
            if (monoGameHost.Renderer != null)
            {
                monoGameHost.Renderer.OnCameraControlModeChanged =
                    on => Dispatcher.InvokeAsync(() => vm.IsMouseLookMode = on);
            }

            vm.OnProjectLoaded = project =>
            {
                if (monoGameHost.Renderer != null)
                {
                    monoGameHost.Renderer.Project = project;
                    monoGameHost.Renderer.OnSongPosChanged = () =>
                        Dispatcher.InvokeAsync(() => vm.NotifyScrollPositionChanged());
                }
            };

            vm.OnLoadBackgroundImage = path =>
                monoGameHost.Renderer?.LoadBackgroundImage(path);

            vm.GetRendererWaveformPanel = () =>
                monoGameHost.Renderer?.WaveformPanel;
        }
    }
}
