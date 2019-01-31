using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Visual_Music
{
	public partial class VideoExportForm : BaseDialog
	{
		public VideoExportOptions Options { get; set; } = new VideoExportOptions();
		public VideoExportForm()
		{
			InitializeComponent();
			ssResoComboBox.SelectedIndex = 2;
			updateResoItems();
		}

		private void sphereCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.Sphere = sphereCb.Checked;
			vrMetadataCb.Enabled = vrMetadataCb.Checked = sphereCb.Checked;
			updateResoItems();
		}

		private void resoComboBox_TextChanged(object sender, EventArgs e)
		{
			parseReso(resoComboBox);
		}

		private void VideoExportForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK && !parseReso(resoComboBox))
			{
				e.Cancel = Visible = true;
				MessageBox.Show(null, "Invalid resolution.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void StereoscopicCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.Stereo = StereoscopicCb.Checked;
			updateResoItems();
		}

		void updateResoItems()
		{
			//Video reso
			resoComboBox.Items.Clear();
			if (Options.Sphere)
			{
				if (Options.Stereo)
					resoComboBox.Items.Add("3060 x 3060");

				else
					resoComboBox.Items.Add("4320 x 2160");
			}
			else
			{
				resoComboBox.Items.Add("3840 x 2160");
				resoComboBox.Items.Add("1920 x 1080");
			}
			resoComboBox.SelectedIndex = 0;
		}

		bool parseReso(ComboBox resoBox)
		{
			resoBox.ForeColor = System.Drawing.Color.Red;

			string[] xy = resoBox.Text.Split('x', 'X');
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

			resoBox.ForeColor = System.Drawing.Color.Black;
			updateSsReso();
			return true;
		}

		private void vrMetadataCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.VrMetadata = vrMetadataCb.Checked;
		}

		private void ssResoComboBox_TextChanged(object sender, EventArgs e)
		{
			updateSsReso();
		}

		void updateSsReso()
		{
			char lastChar = ssResoComboBox.Text[ssResoComboBox.Text.Length - 1];
			if (lastChar == 'x' || lastChar == 'X')
			{
				string numberString = ssResoComboBox.Text.Substring(0, ssResoComboBox.Text.Length - 1);
				int multiplier = int.Parse(numberString);
				Options.SSAAWidth = Options.Width * multiplier;
				Options.SSAAHeight = Options.Height * multiplier;
				while (Options.SSAAWidth > 16384 || Options.SSAAHeight > 16384)
				{
					Options.SSAAWidth /= 2;
					Options.SSAAHeight /= 2;
				}
				Options.EnableSSAA = true;
			}
			else
			{
				Options.EnableSSAA = false;
				Options.SSAAWidth = Options.Width;
				Options.SSAAHeight = Options.Height;
			}
		}

		internal void updateControls(VideoExportOptions options)
		{
			sphereCb.Checked = options.Sphere;
			vrMetadataCb.Checked = options.VrMetadata;
			StereoscopicCb.Checked = options.Stereo;
		}
	}

	[Serializable]
	public class VideoExportOptions : ISerializable
	{
		public int Width;
		public int Height;
		public int SSAAWidth; 
		public int SSAAHeight;
		public bool EnableSSAA;
		public bool Sphere;
		public bool Stereo;
		public bool VrMetadata;

		public VideoExportOptions()
		{

		}

		public VideoExportOptions(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "videoSphere")
					Sphere = (bool)entry.Value;
				else if (entry.Name == "videoVrMeta")
					VrMetadata = (bool)entry.Value;
				else if (entry.Name == "videoVrStereo")
					Stereo = (bool)entry.Value;
				else if (entry.Name == "videoWidth")
					Width = (int)entry.Value;
				else if (entry.Name == "videoHeight")
					Height = (int)entry.Value;
				else if (entry.Name == "videoSSAAWidth")
					SSAAWidth = (int)entry.Value;
				else if (entry.Name == "videoSSAAHeight")
					SSAAHeight = (int)entry.Value;
				else if (entry.Name == "videoEnableSSAA")
					EnableSSAA = (bool)entry.Value;
			}
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("videoSphere", Form1.VidExpForm.Options.Sphere);
			info.AddValue("videoVrMeta", Form1.VidExpForm.Options.VrMetadata);
			info.AddValue("videoVrStereo", Form1.VidExpForm.Options.Stereo);
			info.AddValue("videoWidth", Form1.VidExpForm.Options.Width);
			info.AddValue("videoHeight", Form1.VidExpForm.Options.Height);
			info.AddValue("videoSSAAWidth", Form1.VidExpForm.Options.SSAAWidth);
			info.AddValue("videoSSAAHeight", Form1.VidExpForm.Options.SSAAHeight);
			info.AddValue("videoEnableSSAA", Form1.VidExpForm.Options.EnableSSAA);
		}

		
	}
}
