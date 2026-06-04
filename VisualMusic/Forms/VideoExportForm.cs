using System;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class VideoExportForm : BaseDialog
    {
        readonly System.Drawing.Color _validColor = System.Drawing.Color.Black;
        readonly System.Drawing.Color _errorColor = System.Drawing.Color.Red;
        public VideoExportOptions Options { get; set; } = new VideoExportOptions();
        public VideoExportForm()
        {
            InitializeComponent();
            UpdateResoItems();

            videoQualityLossCombo.Items.Add("0 - lossless");
            for (int i = 1; i < 10; i++)
                videoQualityLossCombo.Items.Add(i);
            videoQualityLossCombo.Items.Add("10 - smallest file");

            SetOptions(Options);
        }

        private void SphereCb_CheckedChanged(object sender, EventArgs e)
        {
            Options.Sphere = sphereCb.Checked;
            sphericalMetadataCb.Enabled = sphericalMetadataCb.Checked = sphereCb.Checked;
            UpdateResoItems();
        }

        private void ResoComboBox_TextChanged(object sender, EventArgs e)
        {
            Options.ResoIndex = resoCombo.SelectedIndex;
            UpdateResoTextColor();
        }

        private void StereoscopicCb_CheckedChanged(object sender, EventArgs e)
        {
            Options.SphericalStereo = stereoscopicCb.Checked;
            UpdateResoItems();
        }

        void UpdateResoItems()
        {
            //Video reso
            resoCombo.Items.Clear();
            if (Options.Sphere)
            {
                if (Options.SphericalStereo)
                {
                    resoCombo.Items.Add("4096 x 4096");
                    resoCombo.Items.Add("8192 x 8192");
                }
                else
                {
                    resoCombo.Items.Add("4096 x 2048");
                    resoCombo.Items.Add("8192 x 4096");
                }
            }
            else
            {
                resoCombo.Items.Add("1920 x 1080");
                resoCombo.Items.Add("3840 x 2160");
                resoCombo.Items.Add("7680 x 4320");
            }
            resoCombo.SelectedIndex = Options.ResoIndex;
        }

        bool ParseReso()
        {
            string[] xy = resoCombo.Text.Split('x', 'X');
            if (xy.Length != 2)
                return false;

            try
            {
                Options.Width = int.Parse(xy[0]);
                Options.Height = int.Parse(xy[1]);
            }
            catch (System.FormatException)
            {
                return false;
            }

            if (Options.Width <= 0 || Options.Height <= 0)
                return false;

            return true;
        }

        void UpdateResoTextColor()
        {
            resoCombo.ForeColor = ParseReso() ? _validColor : _errorColor;
        }

        private void VrMetadataCb_CheckedChanged(object sender, EventArgs e)
        {
            Options.SphericalMetadata = sphericalMetadataCb.Checked;
        }

        private void SsFactorComboBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSsFactor();
        }

        void UpdateSsFactor()
        {
            if (string.IsNullOrEmpty(ssFactorComboBox.Text))
                return;
            char lastChar = ssFactorComboBox.Text[ssFactorComboBox.Text.Length - 1];
            if (lastChar == 'x' || lastChar == 'X')
            {
                string numberString = ssFactorComboBox.Text.Substring(0, ssFactorComboBox.Text.Length - 1);
                Options.SSAAFactor = int.Parse(numberString);
            }
            else
                Options.SSAAFactor = 1;
        }

        internal void SetOptions(VideoExportOptions options)
        {
            Options = options;
            sphereCb.Checked = options.Sphere;
            sphericalMetadataCb.Checked = options.SphericalMetadata;
            stereoscopicCb.Checked = options.SphericalStereo;
            resoCombo.SelectedIndex = options.ResoIndex;
            ssFactorComboBox.SelectedIndex = options.SsaaIndex;
            videoQualityLossCombo.SelectedIndex = options.VideoQualityLoss;
            //if (options.VideoCodec == AVCodecID.AV_CODEC_ID_H264)
            //	videoCodecCombo.SelectedIndex = 0;
            //else 
            //	videoCodecCombo.SelectedIndex = 1;
            fpsUd.Value = (decimal)options.Fps;

        }

        private void FpsUd_ValueChanged(object sender, EventArgs e)
        {
            Options.Fps = (float)fpsUd.Value;
        }

        private void VideoQualityLossCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.VideoQualityLoss = videoQualityLossCombo.SelectedIndex;
        }

        //private void videoCodecCombo_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //	if (videoCodecCombo.SelectedIndex == 0)
        //		Options.VideoCodec = AVCodecID.AV_CODEC_ID_H264;
        //	else
        //		Options.VideoCodec = AVCodecID.AV_CODEC_ID_VP9;
        //}

        private void ResoComboBox_DropDown(object sender, EventArgs e)
        {
            resoCombo.ForeColor = _validColor;
        }

        private void ResoCombo_DropDownClosed(object sender, EventArgs e)
        {
            UpdateResoTextColor();
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            if (!ParseReso())
                MessageBox.Show(null, "Invalid resolution format", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (Options.Width % 2 != 0 || Options.Height % 2 != 0)
                MessageBox.Show(null, "Resolution width and height must be even numbers", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                DialogResult = DialogResult.OK;
        }
    }

    [Serializable]
    public class VideoExportOptions : ISerializable
    {
        int _width;
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                UpdateSSAAReso();
            }
        }
        int _height;
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                UpdateSSAAReso();
            }
        }

        public int SSAAWidth { get; private set; }
        public int SSAAHeight { get; private set; }

        int _ssaaFactor = 4;
        public int SSAAFactor
        {
            get => _ssaaFactor;
            set
            {
                _ssaaFactor = value;
                UpdateSSAAReso();
            }
        }
        public bool SSAAEnabled => _ssaaFactor > 1 && SSAAWidth > Width && SSAAHeight > Height;
        public bool Sphere;
        public bool SphericalStereo;
        public bool SphericalMetadata;
        public int VideoQualityLoss = 1;
        public string VideoCrf => VideoQualityLoss <= 1 ? VideoQualityLoss.ToString() : (VideoQualityLoss * 2).ToString(); //Maximum crf of 20
        public AVCodecID VideoCodec = AVCodecID.AV_CODEC_ID_H264;
        public float Fps = 60;
        public int ResoIndex
        {
            get => Sphere ? _sphereResoIndex : _nonSphereResoIndex;
            set
            {
                if (Sphere)
                    _sphereResoIndex = value;
                else
                    _nonSphereResoIndex = value;
            }
        }
        int _sphereResoIndex = 1;
        int _nonSphereResoIndex = 1;
        public int SsaaIndex => (int)Math.Log(_ssaaFactor, 2);

        public VideoExportOptions()
        {

        }

        public VideoExportOptions(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "sphere")
                    Sphere = (bool)entry.Value;
                else if (entry.Name == "vrMeta")
                    SphericalMetadata = (bool)entry.Value;
                else if (entry.Name == "vrStereo")
                    SphericalStereo = (bool)entry.Value;
                else if (entry.Name == "sphereResoIndex")
                    _sphereResoIndex = (int)entry.Value;
                else if (entry.Name == "nonSphereResoIndex")
                    _nonSphereResoIndex = (int)entry.Value;
                else if (entry.Name == "ssaaFactor")
                    SSAAFactor = (int)entry.Value;
                else if (entry.Name == "videoQualityLoss")
                    VideoQualityLoss = (int)entry.Value;
                //else if (entry.Name == "videoCodec")
                //	VideoCodec = (AVCodecID)entry.Value;
                else if (entry.Name == "fps")
                    Fps = (float)entry.Value;
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sphere", Form1.VidExpForm.Options.Sphere);
            info.AddValue("vrMeta", Form1.VidExpForm.Options.SphericalMetadata);
            info.AddValue("vrStereo", Form1.VidExpForm.Options.SphericalStereo);
            info.AddValue("sphereResoIndex", Math.Max(0, Form1.VidExpForm.Options._sphereResoIndex));
            info.AddValue("nonSphereResoIndex", Math.Max(0, Form1.VidExpForm.Options._nonSphereResoIndex));
            info.AddValue("ssaaFactor", Form1.VidExpForm.Options.SSAAFactor);
            info.AddValue("videoQualityLoss", VideoQualityLoss);
            //info.AddValue("videoCodec", VideoCodec);
            info.AddValue("fps", Fps);
        }

        void UpdateSSAAReso()
        {
            SSAAWidth = Width * _ssaaFactor;
            SSAAHeight = Height * _ssaaFactor;
            while ((SSAAWidth > 16384 || SSAAHeight > 16384))
            {
                SSAAWidth /= 2;
                SSAAHeight /= 2;
            }
        }
    }
}
