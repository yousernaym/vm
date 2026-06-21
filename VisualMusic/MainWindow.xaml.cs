using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using VisualMusic.ViewModels;

namespace VisualMusic
{
    public partial class MainWindow : MetroWindow
    {
        MainViewModel _vm;

        public MainWindow(string[] args = null)
        {
            _vm = new MainViewModel();
            DataContext = _vm;
            InitializeComponent();
            if (AppSettings.Instance.WindowMaximizedOrDefault)
                WindowState = WindowState.Maximized;
            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            if (!_vm.ConfirmSaveChangesBefore("exiting"))
            {
                e.Cancel = true;
                return;
            }

            AppSettings.Instance.WindowMaximized = WindowState == WindowState.Maximized;
            AppSettings.Instance.Save();
        }

        // Tunneling PreviewKeyDown ensures Ctrl+Space reaches us before focusable panel controls
        // (CheckBox, RadioButton, ListView items) can swallow the Space activation key.
        void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+Space or the keyboard's media play/pause key toggles playback.
            bool ctrlSpace = e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control;
            bool mediaPlayPause = e.Key == Key.MediaPlayPause;
            if (ctrlSpace || mediaPlayPause)
            {
                if (_vm.TogglePlaybackCommand.CanExecute(null))
                    _vm.TogglePlaybackCommand.Execute(null);
                e.Handled = true;
            }

            // Escape exits mouse-look mode even when WPF focus is outside the song panel.
            if (e.Key == Key.Escape && _vm.IsMouseLookMode)
            {
                monoGameHost.Renderer?.ToggleMouseLook();
                e.Handled = true;
            }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            modBrowser.ImportService = _vm;
            sidBrowser.ImportService = _vm;
            midiBrowser.ImportService = _vm;

            modBrowser.GetProject = () => monoGameHost.Renderer?.Project;
            sidBrowser.GetProject = () => monoGameHost.Renderer?.Project;
            midiBrowser.GetProject = () => monoGameHost.Renderer?.Project;

            modBrowser.Url = AppSettings.Instance.ModBrowserUrlOrDefault;
            sidBrowser.Url = AppSettings.Instance.SidBrowserUrlOrDefault;
            midiBrowser.Url = AppSettings.Instance.MidiBrowserUrlOrDefault;

            _vm.GetDrawHost = () => monoGameHost.Renderer;
            monoGameHost.Renderer?.SetTrackSelectionService(trackListControl);

            // Wire mouse-look mode toggle to the view-model flag so the yellow label updates.
            if (monoGameHost.Renderer != null)
            {
                monoGameHost.Renderer.OnCameraControlModeChanged =
                    on => Dispatcher.InvokeAsync(() => _vm.IsMouseLookMode = on);
            }

            _vm.OnProjectLoaded = project =>
            {
                if (monoGameHost.Renderer != null)
                {
                    monoGameHost.Renderer.Project = project;
                    monoGameHost.Renderer.OnSongPosChanged = () =>
                        Dispatcher.InvokeAsync(() => _vm.NotifyScrollPositionChanged());
                }
                keyframeListView.SetProject(project);
            };

            _vm.OnLoadBackgroundImage = path =>
                monoGameHost.Renderer?.LoadBackgroundImage(path);

            _vm.OnUnloadBackgroundImage = () =>
                monoGameHost.Renderer?.UnloadBackgroundImage();

            _vm.GetRendererWaveformPanel = () =>
                monoGameHost.Renderer?.WaveformPanel;

            _vm.RenderVideo = (file, cb, opts) =>
                monoGameHost.Renderer?.RenderVideo(file, cb, opts);
        }
    }
}
