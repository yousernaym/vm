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
		delegate void Delegate_updateProgress(int progress);
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
		static int timeBufIndex = 0;
		static float[] timeBuf = new float[100];
		static bool timeBufFull = false;
		static int frame = 0;
		static float totalTime = 0;

		void _updateProgress(int progress)
		{
			if (frame % 40 == 0 || !timeBufFull)
			{
				progressBar1.Value = progress;
				float normProgress = (float)progress / (float)progressBar1.Maximum;
				Text = "Render progress: " + ((int)(100.0f * normProgress)).ToString() + "%";
				
				if (normProgress > 0)
					timeBuf[timeBufIndex++] = (float)stopWatch.Elapsed.TotalSeconds / normProgress;
				if (timeBufIndex >= timeBuf.Length)
				{
					timeBufFull = true;
					timeBufIndex = 0;
				}
				foreach (float t in timeBuf)
					totalTime += t;
				totalTime /= timeBuf.Length;
				//Task bar
				taskBarProgress.SetProgressValue(progress, progressBar1.Maximum);
			}
			TimeSpan estimatedTime;
			if (totalTime > 0)
			{
				float seconds = totalTime - (float)stopWatch.Elapsed.TotalSeconds;
				if (seconds < 0)
					seconds = 0;
				estimatedTime = TimeSpan.FromSeconds(seconds);
			}
			else
				estimatedTime = new TimeSpan(0);
			estimatedTimeLabel.Text = string.Format("Estimated time remaining: {0:d2}:{1:d2}:{2:d2}", new object[] { estimatedTime.Hours, estimatedTime.Minutes, estimatedTime.Seconds });
			elapsedTimeLabel.Text = string.Format("Elapsed time: {0:d2}:{1:d2}:{2:d2}", new object[] { stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds });
			frame++;
		}
		public void updateProgress(int progress)
		{
			Invoke(new Delegate_updateProgress(_updateProgress), progress);
		}
				
		public RenderProgressForm(SongPanel _songPanel, string file, VideoExportForm options)
		{
			InitializeComponent();

			songPanel = _songPanel;
			//Application.Idle -= delegate { songPanel.Invalidate(); };
			Cancel = false;
			progressBar1.Maximum = (int)songPanel.getSongPosInSeconds(1);
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
}
