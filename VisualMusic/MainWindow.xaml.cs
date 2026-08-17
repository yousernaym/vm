using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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

            // Ctrl+F toggles the viewport FPS overlay on the song screen. Skip browser screens so
            // Chromium can keep Ctrl+F for Find. Repeat is ignored so holding the key doesn't flicker.
            // Skip when the MonoGame host has WPF focus — SongRenderer.HandleKeyDown owns that path
            // and both firing would toggle the overlay twice.
            if (_vm.CurrentScreen == AppScreen.Song
                && e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control && !e.IsRepeat
                && !ReferenceEquals(Keyboard.FocusedElement, monoGameHost))
            {
                _vm.ToggleFpsOverlayCommand.Execute(null);
                e.Handled = true;
            }

            // Ctrl+Shift+H switches between hardware-tessellated patches and the retained legacy
            // line buffers. The focused MonoGame HWND handles its own copy of this shortcut.
            if (_vm.CurrentScreen == AppScreen.Song
                && e.Key == Key.H
                && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)
                && !e.IsRepeat
                && !ReferenceEquals(Keyboard.FocusedElement, monoGameHost))
            {
                _vm.ToggleHardwareLineTessellationCommand.Execute(null);
                e.Handled = true;
            }
        }

        // If a track-prop control kept WPF focus, the song HWND still gets the key-down; the
        // matching key-up can land here instead. Stop that axis so the camera does not keep sliding.
        void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_vm.CurrentScreen != AppScreen.Song) return;
            if (ReferenceEquals(Keyboard.FocusedElement, monoGameHost)) return;

            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key is not (Key.W or Key.A or Key.S or Key.D or Key.R or Key.F or Key.Q or Key.E))
                return;

            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (vk != 0)
                monoGameHost.Renderer?.HandleKeyUp(vk);
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

                // Ctrl+wheel over the focused song panel adjusts the selected tracks' viewport width.
                monoGameHost.Renderer.OnCtrlWheelViewWidth =
                    notches => _vm.AdjustSelectedViewWidth(notches);

                // Ctrl+F in the focused song panel (WPF KeyBindings do not reach the HWND).
                monoGameHost.Renderer.OnToggleFpsOverlay =
                    () => Dispatcher.InvokeAsync(() => _vm.ToggleFpsOverlayCommand.Execute(null));

                monoGameHost.Renderer.OnHardwareLineTessellationChanged =
                    enabled => Dispatcher.InvokeAsync(
                        () => _vm.UseHardwareLineTessellation = enabled);
            }

            void WireRendererSongPosChanged()
            {
                if (monoGameHost.Renderer == null) return;
                monoGameHost.Renderer.OnSongPosChanged = () =>
                    // Position refresh can rebuild interpolated geometry. Queue it below input so
                    // mouse move/up remains responsive while right-drag scrolling through that work.
                    Dispatcher.InvokeAsync(
                        () => _vm.NotifyScrollPositionChanged(),
                        DispatcherPriority.Background);
            }

            void SyncRendererProject(Project project)
            {
                if (monoGameHost.Renderer == null) return;
                monoGameHost.Renderer.Project = project;
                WireRendererSongPosChanged();
            }

            void ForceSyncRendererProject(Project project)
            {
                if (monoGameHost.Renderer == null) return;
                // No NoteStyle bake — restore path rebinds NoteStyle separately when bake fails.
                monoGameHost.Renderer.ForceProjectReference(project);
                WireRendererSongPosChanged();
            }

            // Open binds the temp project here before LoadContent/Init so DrawSong and NoteStyle agree.
            _vm.SyncRendererProject = SyncRendererProject;
            _vm.ForceSyncRendererProject = ForceSyncRendererProject;

            _vm.OnProjectLoaded = project =>
            {
                SyncRendererProject(project);
                keyframeListView.SetProject(project);
            };

            _vm.OnLoadBackgroundImage = path =>
                monoGameHost.Renderer?.LoadBackgroundImage(path);

            _vm.OnUnloadBackgroundImage = () =>
                monoGameHost.Renderer?.UnloadBackgroundImage();

            _vm.GetRendererWaveformPanel = () =>
                monoGameHost.Renderer?.WaveformPanel;

            // Open/Import switch to Song then call this so a previously Collapsed HwndHost can
            // BuildWindowCore before InitAfterDeserialization requires WaveformPanel.
            _vm.EnsureSongHostReady = () =>
            {
                UpdateLayout();
                monoGameHost.UpdateLayout();
            };

            _vm.RenderVideo = (file, cb, opts) =>
                monoGameHost.Renderer?.RenderVideo(file, cb, opts);
        }
    }
}
