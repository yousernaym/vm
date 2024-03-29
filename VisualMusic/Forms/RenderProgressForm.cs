﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class RenderProgressForm : ProgressForm
    {
        delegate void Delegate_renderVideo(string videoFilePath, RenderProgressForm progressForm, VideoExportOptions options);
        delegate void Delegate_updateProgress(double progress);
        delegate void Delegate_void_noparams();
        delegate void Delegate_void_string(string str);

        public bool Cancel { get; set; }
        public object cancelLock = new object();
        bool finished = false;
        SongPanel songPanel;

        ProgressAtTime[] progressBuf = new ProgressAtTime[100];

        public new void updateProgress(double progress)
        {
            Invoke(new Delegate_updateProgress(base.updateProgress), progress);
        }

        public RenderProgressForm()
        {
            InitializeComponent();
        }
        public RenderProgressForm(SongPanel _songPanel, string file, VideoExportOptions options)
        {
            InitializeComponent();
            songPanel = _songPanel;
            Cancel = false;
            Task.Run(() => songPanel.renderVideo(file, this, options)).ContinueWith(renderingFinished);
            ProgressText = "Render progress";
        }

        void renderingFinished(IAsyncResult result)
        {
            finished = true;
            Delegate_renderVideo rv = (Delegate_renderVideo)result.AsyncState;
            Invoke(new Delegate_void_noparams(closeForm));
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
            Close();
        }

        private void RenderProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (finished != true)
            {
                e.Cancel = true;
                using (StopRenderingMb cancelMb = new StopRenderingMb())
                {
                    if (DialogResult.Yes == cancelMb.ShowDialog(this))
                    {
                        lock (cancelLock)
                            Cancel = true; //Will cause rendering loop in songPanel to finish so that renderingFinished gets called by delegate_renderVideo.BeginInvoke. renderingFinished will call closeForm which will call Close which will take us back here again, but this time finished will be true.
                    }
                }
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

        private void RenderProgressForm_VisibleChanged(object sender, EventArgs e)
        {


        }
    }
}

