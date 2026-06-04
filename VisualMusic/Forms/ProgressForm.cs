using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class ProgressForm : Form
    {
        Stopwatch _stopwatch = new Stopwatch();
        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager _taskBarProgress = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
        int _progressBufIndex0 = 1;
        int _progressBufIndex1 = 0;
        ProgressAtTime[] _progressBuf = new ProgressAtTime[100];
        int _frame = 0;
        public string ProgressText { get; set; } = "Progress";

        public ProgressForm()
        {
            InitializeComponent();
            progressBar1.Maximum = 1000;
            _stopwatch.Start();
        }

        public void UpdateProgress(double normProgress)
        {
            if (_frame % 1 == 0)
            {
                normProgress = Math.Min(1, normProgress);
                normProgress = Math.Max(0, normProgress);
                progressBar1.Value = (int)(normProgress * progressBar1.Maximum);
                int percent = (int)(100.0 * normProgress + 0.5);
                if (percent > 100)
                    percent = 100;
                Text = $"{ProgressText}: {percent.ToString()}%";
                ProgressAtTime pat = new ProgressAtTime();
                pat.time = _stopwatch.Elapsed.TotalSeconds;
                pat.normProgress = normProgress;
                _progressBuf[_progressBufIndex1] = pat;

                //Task bar
                _taskBarProgress.SetProgressValue(progressBar1.Value, progressBar1.Maximum);

                double deltaTime = _progressBuf[_progressBufIndex1].time - _progressBuf[_progressBufIndex0].time;
                double deltaProgress = _progressBuf[_progressBufIndex1].normProgress - _progressBuf[_progressBufIndex0].normProgress;
                double progressLeft = 1.0 - _progressBuf[_progressBufIndex1].normProgress;
                if (deltaProgress > 0)
                {
                    TimeSpan timeLeft = TimeSpan.FromSeconds(deltaTime * progressLeft / deltaProgress);
                    estimatedTimeLabel.Text = string.Format("Estimated time remaining: {0:d2}:{1:d2}:{2:d2}", new object[] { timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds });
                    elapsedTimeLabel.Text = string.Format("Elapsed time: {0:d2}:{1:d2}:{2:d2}", new object[] { _stopwatch.Elapsed.Hours, _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds });
                }
                else
                {
                    estimatedTimeLabel.Text = "";
                    elapsedTimeLabel.Text = "";
                }

                if (++_progressBufIndex0 >= _progressBuf.Length)
                    _progressBufIndex0 = 0;
                if (++_progressBufIndex1 >= _progressBuf.Length)
                    _progressBufIndex1 = 0;
            }
            _frame++;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ProgressForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void ProgressForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                _taskBarProgress.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
                _stopwatch.Reset();
                _stopwatch.Start();
            }
            else
            {
                _taskBarProgress.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress);
                UpdateProgress(0);
            }
        }
    }
    public struct ProgressAtTime
    {
        public double time;
        public double normProgress;
    }
}
