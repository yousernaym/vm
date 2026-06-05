using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Unified progress window for any blocking background job — video rendering and URL
    /// downloads both use this.  Implements <see cref="IRenderProgressCallback"/> so the
    /// renderer can report progress without knowing about the UI type.
    ///
    /// Use the static factory methods <see cref="RunRender"/> and <see cref="RunDownload"/>
    /// rather than constructing the window directly.
    /// </summary>
    public partial class ProgressWindow : MetroWindow, IRenderProgressCallback, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- IRenderProgressCallback ----
        public bool   Cancel     { get; private set; }
        public object CancelLock { get; } = new object();

        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public CancellationToken CancelToken => _cts.Token;

        void IRenderProgressCallback.ShowMessage(string message)
        {
            // Block the job thread until the user dismisses the dialog (mirrors WinForms Invoke).
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

        readonly string _titlePrefix;   // fixed base, never includes the "%"

        string _titleText;
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

        bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set { _isIndeterminate = value; Notify(); }
        }

        // ---- Public result (non-null only for download jobs) ----

        public object Result { get; private set; }

        // ---- Timing / estimation (ported from WinForms ProgressForm) ----
        readonly Stopwatch _stopwatch = new Stopwatch();
        readonly ProgressAtTime[] _progressBuf = new ProgressAtTime[100];
        int _progressBufIndex0 = 1;   // trails index1 by ~100 frames → smoothed sliding window
        int _progressBufIndex1 = 0;

        struct ProgressAtTime { public double time; public double normProgress; }

        // ---- Job wiring ----
        readonly Func<IRenderProgressCallback, Task<object>> _job;
        readonly bool   _confirmCancel;
        readonly string _doneMessage;

        bool _finished;
        bool _cancelRequested;   // guard against re-entrancy in RequestCancel()

        // ---- Constructor ----

        /// <param name="titlePrefix">Window title before the percentage suffix is appended.</param>
        /// <param name="job">The background work; receives this window as the callback and returns an optional result.</param>
        /// <param name="confirmCancel">Show a "Stop?" confirmation before cancelling (use for rendering).</param>
        /// <param name="doneMessage">Message box text shown on success. Pass null to skip (use for downloads).</param>
        public ProgressWindow(string titlePrefix,
            Func<IRenderProgressCallback, Task<object>> job,
            bool confirmCancel = false,
            string doneMessage = null)
        {
            _titlePrefix   = titlePrefix;
            _titleText     = titlePrefix;
            _job           = job;
            _confirmCancel = confirmCancel;
            _doneMessage   = doneMessage;

            DataContext = this;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_job == null)
            {
                MessageBox.Show("No job provided.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                _finished = true;
                Close();
                return;
            }

            taskbarInfo.ProgressState = TaskbarItemProgressState.Normal;
            _stopwatch.Start();
            _job(this).ContinueWith(OnJobFinished);
        }

        // ---- Progress update (called on UI thread via IRenderProgressCallback.UpdateProgress) ----

        void UpdateProgressOnUi(float normProgress)
        {
            normProgress = Math.Max(0f, Math.Min(1f, normProgress));

            Progress = normProgress * 100.0;
            int percent = (int)(normProgress * 100.0 + 0.5);
            TitleText = $"{_titlePrefix}: {percent}%";
            IsIndeterminate = false;

            // Taskbar
            taskbarInfo.ProgressValue = normProgress;

            // Record sample in circular buffer
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

        // ---- Job completion ----

        void OnJobFinished(Task<object> task)
        {
            _finished = true;
            if (!Cancel && task.Status == TaskStatus.RanToCompletion)
                Result = task.Result;
            Dispatcher.InvokeAsync(CloseForm);
        }

        void CloseForm()
        {
            taskbarInfo.ProgressState = TaskbarItemProgressState.None;
            if (_doneMessage != null && !Cancel)
                MessageBox.Show(this, _doneMessage, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            _cts.Dispose();
            Close();
        }

        // ---- Cancellation ----

        void RequestCancel()
        {
            if (_finished || _cancelRequested) return;
            _cancelRequested = true;

            if (_confirmCancel)
            {
                var result = MessageBox.Show(this, "Stop?", Program.AppName,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    _cancelRequested = false;   // let the user try again later
                    return;
                }
            }

            lock (CancelLock) { Cancel = true; }
            _cts.Cancel();   // wake any job blocked on the token (e.g. a stalled download)
            // The job is responsible for detecting Cancel and returning; OnJobFinished then
            // dispatches CloseForm.  We set _finished=false guard so Window_Closing will try
            // to cancel again if the user closes before the job notices — but _cancelRequested
            // prevents a second confirm dialog.
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_finished) return;   // job done — allow close

            // Job still running — request cancel but keep window open until the job exits.
            e.Cancel = true;
            RequestCancel();
        }

        // ---- Static factories ----

        /// <summary>
        /// Shows the progress window for a video render job and blocks until done or cancelled.
        /// </summary>
        public static void RunRender(string file, VideoExportOptions opts,
            Action<string, IRenderProgressCallback, VideoExportOptions> render)
        {
            var w = new ProgressWindow("Rendering Video",
                cb => Task.Run(() => { render(file, cb, opts); return (object)null; }),
                confirmCancel: true,
                doneMessage: "Done!")
                { Owner = Application.Current?.MainWindow };
            w.ShowDialog();
        }

        /// <summary>
        /// Downloads <paramref name="url"/> to the temp dir with a progress dialog and returns
        /// the local file path, or null if the download was cancelled or failed.
        /// Must be called on (or marshals itself to) the UI thread.
        /// </summary>
        public static string RunDownload(string url)
        {
            var d = Application.Current?.Dispatcher;
            if (d != null && !d.CheckAccess()) return d.Invoke(() => RunDownload(url));

            var w = new ProgressWindow("Downloading",
                cb => DownloadJobAsync(url, cb),
                confirmCancel: false,
                doneMessage: null)
                { Owner = Application.Current?.MainWindow };
            w.ShowDialog();
            return w.Result as string;
        }

        /// <summary>Background download job wired to the progress/cancel callback.</summary>
        static async Task<object> DownloadJobAsync(string url, IRenderProgressCallback cb)
        {
            using var webClient = new WebClient();

            // Relay cancellation to WebClient the instant the user clicks Cancel. Registering on
            // the token (rather than polling cb.Cancel inside DownloadProgressChanged) means cancel
            // still works when the server is unresponsive and no progress events ever fire.
            using var cancelReg = cb.CancelToken.Register(webClient.CancelAsync);

            webClient.DownloadProgressChanged += (_, e) =>
            {
                bool indeterminate = e.TotalBytesToReceive < 0;
                if (indeterminate)
                {
                    // Signal the window to show indeterminate mode (no time estimates).
                    cb.UpdateProgress(0f);
                    if (cb is ProgressWindow pw)
                        pw.Dispatcher.InvokeAsync(() =>
                        {
                            pw.IsIndeterminate = true;
                            pw.ElapsedText   = "";
                            pw.RemainingText = "";
                        });
                }
                else
                {
                    cb.UpdateProgress((float)e.ProgressPercentage / 100f);
                }
            };

            try
            {
                byte[] data = await webClient.DownloadDataTaskAsync(new Uri(url));
                if (cb.Cancel) return null;

                // Grab response headers while the WebClient still holds them.
                string fileName = Download.GetDownloadFileName(webClient.ResponseHeaders, url);
                string path = Path.Combine(Program.TempDir, fileName);
                File.WriteAllBytes(path, data);
                return path;
            }
            catch
            {
                return null;   // cancelled or network error — caller converts null to FileImportException
            }
        }
    }
}
