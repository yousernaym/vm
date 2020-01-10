using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace VisualMusic
{
	public partial class ProgressForm : Form
	{
		Stopwatch stopwatch = new Stopwatch();
		Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskBarProgress = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
		int progressBufIndex0 = 1;
		int progressBufIndex1 = 0;
		ProgressAtTime[] progressBuf = new ProgressAtTime[100];
		int frame = 0;
		public string ProgressText { get; set; } = "Progress";
						
		public ProgressForm()
		{
			InitializeComponent();
			progressBar1.Maximum = 1000;
			stopwatch.Start();
		}

		public void updateProgress(double normProgress)
		{
			if (frame % 1 == 0)
			{
				normProgress = Math.Min(1, normProgress);
				normProgress = Math.Max(0, normProgress);
				progressBar1.Value = (int)(normProgress * progressBar1.Maximum);
				int percent = (int)(100.0 * normProgress + 0.5);
				if (percent > 100)
					percent = 100;
				Text =  $"{ProgressText}: {percent.ToString()}%";

				ProgressAtTime pat = new ProgressAtTime();
				pat.time = stopwatch.Elapsed.TotalSeconds;
				pat.normProgress = normProgress;
				progressBuf[progressBufIndex1] = pat;

				//Task bar
				taskBarProgress.SetProgressValue(progressBar1.Value, progressBar1.Maximum);

				double deltaTime = progressBuf[progressBufIndex1].time - progressBuf[progressBufIndex0].time;
				double deltaProgress = progressBuf[progressBufIndex1].normProgress - progressBuf[progressBufIndex0].normProgress;
				double progressLeft = 1.0 - progressBuf[progressBufIndex1].normProgress;
				if (deltaProgress > 0)
				{
					TimeSpan timeLeft = TimeSpan.FromSeconds(deltaTime * progressLeft / deltaProgress);
					estimatedTimeLabel.Text = string.Format("Estimated time remaining: {0:d2}:{1:d2}:{2:d2}", new object[] { timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds });
					elapsedTimeLabel.Text = string.Format("Elapsed time: {0:d2}:{1:d2}:{2:d2}", new object[] { stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds });
				}
				else
				{
					estimatedTimeLabel.Text = "";
					elapsedTimeLabel.Text = "";
				}

				if (++progressBufIndex0 >= progressBuf.Length)
					progressBufIndex0 = 0;
				if (++progressBufIndex1 >= progressBuf.Length)
					progressBufIndex1 = 0;
			}
			frame++;
		}

		private void button1_Click(object sender, EventArgs e)
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
				taskBarProgress.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
				stopwatch.Reset();
				stopwatch.Start();
			}
			else
			{
				taskBarProgress.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress);
				updateProgress(0);
			}
		}
	}
	public struct ProgressAtTime
	{
		public double time;
		public double normProgress;
	}
}
