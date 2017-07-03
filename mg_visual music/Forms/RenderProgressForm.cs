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

namespace Visual_Music
{
	public partial class RenderProgressForm : Form
	{
		delegate void Delegate_renderVideo(string file, RenderProgressForm progressForm, VideoExportForm options);
		delegate void Delegate_updateProgress(double progress);
		delegate void Delegate_void_noparams();
		delegate void Delegate_void_string(string str);

		public bool Cancel { get; set; }
		public object cancelLock = new object();
		bool finished = false;
		SongPanel songPanel;

		Stopwatch stopWatch = new Stopwatch();
		//TimeSpan startTime;
		Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskBarProgress = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
		StopRenderingMb cancelMb = new StopRenderingMb();
		static int progressBufIndex0 = 1;
		static int progressBufIndex1 = 0;
		static ProgressAtTime[] progressBuf = new ProgressAtTime[100];
		static bool timeBufFull = false;
		static int frame = 0;
		static float totalTime = 0;
				
		void _updateProgress(double progress)
		{
			if (frame % 1 == 0)
			{
				progress = Math.Min(1, progress);
				progress = Math.Max(0, progress);
				int value = (int)(progress * songPanel.SongLengthT);
				progressBar1.Value = value;
				int percent = (int)(100.0 * progress + 0.5);
				if (percent > 100)
					percent = 100;
				Text = "Render progress: " + percent.ToString() + "%";

				ProgressAtTime pat = new ProgressAtTime();
				pat.time = stopWatch.Elapsed.TotalSeconds;
				pat.progress = progress;
				progressBuf[progressBufIndex1] = pat;
				
				//Task bar
				taskBarProgress.SetProgressValue(progressBar1.Value, progressBar1.Maximum);

				double deltaTime = progressBuf[progressBufIndex1].time - progressBuf[progressBufIndex0].time;
				double deltaProgress = progressBuf[progressBufIndex1].progress - progressBuf[progressBufIndex0].progress;
				double progressLeft = 1.0 - progressBuf[progressBufIndex1].progress;
				TimeSpan timeLeft = TimeSpan.FromSeconds(deltaTime * progressLeft / deltaProgress);

				estimatedTimeLabel.Text = string.Format("Estimated time remaining: {0:d2}:{1:d2}:{2:d2}", new object[] { timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds });
				elapsedTimeLabel.Text = string.Format("Elapsed time: {0:d2}:{1:d2}:{2:d2}", new object[] { stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds });

				if (++progressBufIndex0 >= progressBuf.Length)
					progressBufIndex0 = 0;
				if (++progressBufIndex1 >= progressBuf.Length)
					progressBufIndex1 = 0;
			}
			frame++;
		}
		public void updateProgress(double progress)
		{
			Invoke(new Delegate_updateProgress(_updateProgress), progress);
		}
				
		public RenderProgressForm(SongPanel _songPanel, string file, VideoExportForm options)
		{
			InitializeComponent();

			songPanel = _songPanel;
			//Application.Idle -= delegate { songPanel.Invalidate(); };
			Cancel = false;
			progressBar1.Maximum = songPanel.SongLengthT;// getSongPosInSeconds(1);
			taskBarProgress.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
			
			//Delegate_updateProgress delestimatedTimeestimatedTimeegate_updateProgress = new Delegate_updateProgress(updateProgress);
			Delegate_renderVideo delegate_renderVideo = new Delegate_renderVideo(songPanel.renderVideo);
			IAsyncResult result = delegate_renderVideo.BeginInvoke(file, this, options, renderingFinished, delegate_renderVideo);
			
			stopWatch.Start();
			//startTime = stopWatch.Elapsed;
		}

		void renderingFinished(IAsyncResult result)
		{
			finished = true;
			Delegate_renderVideo rv = (Delegate_renderVideo)result.AsyncState;
			rv.EndInvoke(result);
			Invoke(new Delegate_void_noparams(closeForm));
			//Application.Idle += delegate { songPanel.Invalidate(); };
		}
		void closeForm()
		{
			if (Cancel)
				DialogResult = DialogResult.Cancel;
			else
			{
				DialogResult = DialogResult.OK;
				MessageBox.Show(this, "Done!");
			}
			cancelMb.Close();
			taskBarProgress.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress);
			Close();
		}
		
		private void RenderProgressForm_VisibleChanged(object sender, EventArgs e)
		{
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void RenderProgressForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (finished != true)
			{
				e.Cancel = true;
				//if (DialogResult.Yes == MessageBox.Show(this, "Are you sure you want to stop rendering?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
				//new Thread(delegate(){
					if (DialogResult.Yes == cancelMb.ShowDialog(this))
					{
						lock (cancelLock)
						Cancel = true; //Will cause rendering loop in songPanel to finish so that renderingFinished gets called by delegate_renderVideo.BeginInvoke. renderingFinished will call closeForm which will call Close which will take us back here again, but this time finished will be true.
					}					
				//}).Start();
			}
		}
		
		public void showMessage(string message)
		{
			Invoke(new Delegate_void_string(_showMessage), message);
		}
		void _showMessage(string message)
		{
			MessageBox.Show(this, message);
		}

		private void RenderProgressForm_Load(object sender, EventArgs e)
		{
		}
		
	}
	struct ProgressAtTime
	{
		public double time;
		public double progress;
	}
}
