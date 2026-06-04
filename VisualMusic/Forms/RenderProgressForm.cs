using System;
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
        bool _finished = false;
        SongPanel _songPanel;

        ProgressAtTime[] _progressBuf = new ProgressAtTime[100];

        public new void UpdateProgress(double progress)
        {
            Invoke(new Delegate_updateProgress(base.UpdateProgress), progress);
        }

        public RenderProgressForm()
        {
            InitializeComponent();
        }
        public RenderProgressForm(SongPanel _songPanel, string file, VideoExportOptions options)
        {
            InitializeComponent();
            _songPanel = _songPanel;
            Cancel = false;
            Task.Run(() => _songPanel.RenderVideo(file, this, options)).ContinueWith(RenderingFinished);
            ProgressText = "Render progress";
        }

        void RenderingFinished(IAsyncResult result)
        {
            _finished = true;
            Delegate_renderVideo rv = (Delegate_renderVideo)result.AsyncState;
            Invoke(new Delegate_void_noparams(CloseForm));
        }

        void CloseForm()
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
            if (_finished != true)
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

        public void ShowMessage(string message)
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

