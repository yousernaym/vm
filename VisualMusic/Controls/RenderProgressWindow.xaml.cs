using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace VisualMusic.Controls
{
    /// <summary>
    /// WPF replacement for the WinForms RenderProgressForm.
    /// Implements <see cref="IRenderProgressCallback"/> so <see cref="SongRenderer.renderVideo"/>
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
        public bool   Cancel     { get; private set; }
        public object CancelLock { get; } = new object();

        void IRenderProgressCallback.ShowMessage(string message)
        {
            // Block the render thread until the user dismisses the dialog (mirrors WinForms Invoke).
            Dispatcher.Invoke(() =>
                MessageBox.Show(this, message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Warning));
        }

        void IRenderProgressCallback.UpdateProgress(float normProgress)
            => Dispatcher.InvokeAsync(() => UpdateProgressOnUi(normProgress));

        // ---- Bound properties ----

        double _progress;
        public double Progress
        {
            get => _progress;
            private set { _progress = value; Notify(); }
        }

        string _titleText = "Rendering Video";
        public string TitleText
        {
            get => _titleText;
            private set { _titleText = value; Notify(); }
        }

        string _elapsedText = "";
        public string ElapsedText
        {
            get => _elapsedText;
            private set { _elapsedText = value; Notify(); }
        }

        string _remainingText = "";
        public string RemainingText
        {
            get => _remainingText;
            private set { _remainingText = value; Notify(); }
        }

        // ---- Timing / estimation (ported from WinForms ProgressForm) ----
        readonly Stopwatch _stopwatch = new Stopwatch();
        readonly ProgressAtTime[] _progressBuf = new ProgressAtTime[100];
        int _progressBufIndex0 = 1;   // trails index1 by ~100 frames → smoothed sliding window
        int _progressBufIndex1 = 0;

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
                taskbarInfo.ProgressState = TaskbarItemProgressState.None;
                _finished = true;
                Close();
                return;
            }

            taskbarInfo.ProgressState = TaskbarItemProgressState.Normal;
            _stopwatch.Start();
            Task.Run(() => _render(_file, this, _options))
                .ContinueWith(OnRenderFinished);
        }

        // ---- Progress update (called on UI thread via InvokeAsync) ----

        void UpdateProgressOnUi(float normProgress)
        {
            normProgress = Math.Max(0f, Math.Min(1f, normProgress));

            Progress = normProgress * 100.0;
            int percent = (int)(normProgress * 100.0 + 0.5);
            TitleText = $"Rendering Video: {percent}%";

            // Taskbar
            taskbarInfo.ProgressValue = normProgress;

            // Record sample in circular buffer (same algorithm as WinForms ProgressForm)
            _progressBuf[_progressBufIndex1] = new ProgressAtTime
            {
                time         = _stopwatch.Elapsed.TotalSeconds,
                normProgress = normProgress
            };

            double deltaTime     = _progressBuf[_progressBufIndex1].time         - _progressBuf[_progressBufIndex0].time;
            double deltaProgress = _progressBuf[_progressBufIndex1].normProgress - _progressBuf[_progressBufIndex0].normProgress;
            double progressLeft  = 1.0 - normProgress;

            if (deltaProgress > 0)
            {
                var timeLeft = TimeSpan.FromSeconds(deltaTime * progressLeft / deltaProgress);
                var elapsed  = _stopwatch.Elapsed;
                ElapsedText   = $"Elapsed time: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                RemainingText = $"Estimated time remaining: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                ElapsedText   = "";
                RemainingText = "";
            }

            if (++_progressBufIndex0 >= _progressBuf.Length) _progressBufIndex0 = 0;
            if (++_progressBufIndex1 >= _progressBuf.Length) _progressBufIndex1 = 0;
        }

        // ---- Render-task completion ----

        void OnRenderFinished(Task _)
        {
            _finished = true;
            Dispatcher.InvokeAsync(CloseForm);
        }

        void CloseForm()
        {
            taskbarInfo.ProgressState = TaskbarItemProgressState.None;
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
            // which dispatches CloseForm → Close(), clearing the taskbar and closing the window.
        }
    }
}
