using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// WPF replacement for the WinForms RenderProgressForm.
    /// Implements <see cref="IRenderProgressCallback"/> so <see cref="Controls.SongRenderer.renderVideo"/>
    /// can report progress and errors without knowing about the UI type.
    /// The render task is started in <see cref="OnLoaded"/>; the window is modal (ShowDialog).
    /// Close the window to cancel — a confirmation prompt is shown while rendering is in progress.
    /// </summary>
    public partial class RenderProgressWindow : MetroWindow, IRenderProgressCallback, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- IRenderProgressCallback ----
        public bool   Cancel    { get; private set; }
        public object CancelLock { get; } = new object();

        void IRenderProgressCallback.ShowMessage(string message)
        {
            // Block the render thread until the user dismisses the dialog (mirrors WinForms Invoke).
            Dispatcher.Invoke(() =>
                MessageBox.Show(this, message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Warning));
        }

        void IRenderProgressCallback.UpdateProgress(float normProgress)
            => Dispatcher.InvokeAsync(() => Progress = normProgress * 100.0);

        // ---- Bound properties ----
        double _progress;
        public double Progress
        {
            get => _progress;
            private set { _progress = value; Notify(); Notify(nameof(ProgressText)); }
        }

        public string ProgressText => $"Render progress: {(int)_progress}%";

        // ---- Internal state ----
        bool _finished;
        readonly string _file;
        readonly VideoExportOptions _options;
        readonly Action<string, IRenderProgressCallback, VideoExportOptions> _render;

        public RenderProgressWindow(string file, VideoExportOptions options,
            Action<string, IRenderProgressCallback, VideoExportOptions> render)
        {
            _file    = file;
            _options = options;
            _render  = render;

            DataContext = this;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_render == null)
            {
                MessageBox.Show("Renderer not available.", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _finished = true;
                Close();
                return;
            }

            Task.Run(() => _render(_file, this, _options))
                .ContinueWith(OnRenderFinished);
        }

        void OnRenderFinished(Task _)
        {
            _finished = true;
            Dispatcher.InvokeAsync(CloseForm);
        }

        void CloseForm()
        {
            // Show "Done!" only when not cancelled.
            if (!Cancel)
                MessageBox.Show("Done!", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        // ---- Closing handler ----

        void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_finished) return;   // render task completed — allow close

            // Render is still in progress — confirm before cancelling.
            e.Cancel = true;
            var result = MessageBox.Show(
                "Stop rendering?", Program.AppName,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                lock (CancelLock) { Cancel = true; }
            // The render loop will exit on the next Cancel check, call OnRenderFinished,
            // which dispatches CloseForm → Close(), allowing the window to close.
        }
    }
}
