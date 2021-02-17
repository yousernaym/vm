using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace VisualMusic
{
	public partial class VideoExportForm : BaseDialog
	{
		readonly System.Drawing.Color validColor = System.Drawing.Color.Black;
		readonly System.Drawing.Color errorColor = System.Drawing.Color.Red;
		public VideoExportOptions Options { get; set; } = new VideoExportOptions();
		public VideoExportForm()
		{
			InitializeComponent();
			updateResoItems();
			
			videoQualityLossCombo.Items.Add("0 - lossless");
			for (int i = 1; i < 10; i++)
				videoQualityLossCombo.Items.Add(i);
			videoQualityLossCombo.Items.Add("10 - smallest file");

			setOptions(Options);
		}

		private void sphereCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.Sphere = sphereCb.Checked;
			sphericalMetadataCb.Enabled = sphericalMetadataCb.Checked = sphereCb.Checked;
			updateResoItems();
		}

		private void resoComboBox_TextChanged(object sender, EventArgs e)
		{
			Options.ResoIndex = resoCombo.SelectedIndex;
			updateResoTextColor();
		}

		private void StereoscopicCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.SphericalStereo = stereoscopicCb.Checked;
			updateResoItems();
		}

		void updateResoItems()
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

		bool parseReso()
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

		void updateResoTextColor()
		{
			resoCombo.ForeColor = parseReso() ? validColor : errorColor;
		}

		private void vrMetadataCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.SphericalMetadata = sphericalMetadataCb.Checked;
		}

		private void ssFactorComboBox_TextChanged(object sender, EventArgs e)
		{
			updateSsFactor();
		}

		void updateSsFactor()
		{
			if (string.IsNullOrEmpty(ssFactorComboBox.Text))
				return;
			char lastChar = ssFactorComboBox.Text[ssFactorComboBox.Text.Length - 1];
			if (lastChar == 'x' || lastChar == 'X')
			{
				string numberString = ssFactorComboBox.Text.Substring(0, ssFactorComboBox.Text.Length - 1);
				Options.SSAAFactor  = int.Parse(numberString);
			}
			else
				Options.SSAAFactor = 1;
		}

		internal void setOptions(VideoExportOptions options)
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

		private void fpsUd_ValueChanged(object sender, EventArgs e)
		{
			Options.Fps = (float)fpsUd.Value;
		}

		private void videoQualityLossCombo_SelectedIndexChanged(object sender, EventArgs e)
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

		private void resoComboBox_DropDown(object sender, EventArgs e)
		{
			resoCombo.ForeColor = validColor;
		}

		private void resoCombo_DropDownClosed(object sender, EventArgs e)
		{
			updateResoTextColor();
		}

		private void okBtn_Click(object sender, EventArgs e)
		{
			if (!parseReso())
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
		int width;
		public int Width
		{
			get => width;
			set
			{
				width = value;
				updateSSAAReso();
			}
		}
		int height;
		public int Height
		{
			get => height;
			set
			{
				height = value;
				updateSSAAReso();
			}
		}
	
		public int SSAAWidth { get; private set; }
		public int SSAAHeight { get; private set; }

		int ssaaFactor = 4;
		public int SSAAFactor
		{
			get => ssaaFactor;
			set
			{
				ssaaFactor = value;
				updateSSAAReso();
			}
		}
		public bool SSAAEnabled => ssaaFactor > 1 && SSAAWidth > Width && SSAAHeight > Height;
		public bool Sphere;
		public bool SphericalStereo;
		public bool SphericalMetadata;
		public int VideoQualityLoss = 1;
		public string VideoCrf => VideoQualityLoss <= 1 ? VideoQualityLoss.ToString() : (VideoQualityLoss * 2).ToString(); //Maximum crf of 20
		public AVCodecID VideoCodec = AVCodecID.AV_CODEC_ID_H264;
		public float Fps = 60;
		public int ResoIndex
		{
			get => Sphere ? sphereResoIndex : nonSphereResoIndex;
			set
			{
				if (Sphere)
					sphereResoIndex = value;
				else
					nonSphereResoIndex = value;
			}
		}
		int sphereResoIndex = 1;
		int nonSphereResoIndex = 1;
		public int SsaaIndex => (int)Math.Log(ssaaFactor, 2);

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
					sphereResoIndex = (int)entry.Value;
				else if (entry.Name == "nonSphereResoIndex")
					nonSphereResoIndex = (int)entry.Value;
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
			info.AddValue("sphereResoIndex", Math.Max(0, Form1.VidExpForm.Options.sphereResoIndex));
			info.AddValue("nonSphereResoIndex", Math.Max(0, Form1.VidExpForm.Options.nonSphereResoIndex));
			info.AddValue("ssaaFactor", Form1.VidExpForm.Options.SSAAFactor);
			info.AddValue("videoQualityLoss", VideoQualityLoss);
			//info.AddValue("videoCodec", VideoCodec);
			info.AddValue("fps", Fps);
		}

		void updateSSAAReso()
		{
			SSAAWidth = Width * ssaaFactor;
			SSAAHeight = Height * ssaaFactor;
			while ((SSAAWidth > 16384 || SSAAHeight > 16384))
			{
				SSAAWidth /= 2;
				SSAAHeight /= 2;
			}
		}
	}
}
