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
			updateResoItems();
			setOptions(Options);
		}

		private void sphereCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.Sphere = sphereCb.Checked;
			vrMetadataCb.Enabled = vrMetadataCb.Checked = sphereCb.Checked;
			updateResoItems();
		}

		private void resoComboBox_TextChanged(object sender, EventArgs e)
		{
			Options.resoIndex = resoComboBox.SelectedIndex;
			parseReso(resoComboBox);
		}

		private void VideoExportForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				if (!parseReso(resoComboBox))
				{
					e.Cancel = Visible = true;
					MessageBox.Show(null, "Invalid resolution.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				if (!parseFps())
				{
					e.Cancel = Visible = true;
					MessageBox.Show(null, "Invalid FPS setting.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
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
			resoComboBox.SelectedIndex = Options.resoIndex;
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
			return true;
		}

		private void vrMetadataCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.VrMetadata = vrMetadataCb.Checked;
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
			vrMetadataCb.Checked = options.VrMetadata;
			StereoscopicCb.Checked = options.Stereo;
			fpsTb.Text = options.Fps.ToString();
			resoComboBox.SelectedIndex = options.resoIndex;
			ssFactorComboBox.SelectedIndex = options.ssaaIndex;
		}

		private void fpsTb_TextChanged(object sender, EventArgs e)
		{
			parseFps();
		}

		bool parseFps()
		{
			bool b = float.TryParse(fpsTb.Text, out Options.Fps);
			if (b)
				fpsTb.ForeColor = System.Drawing.Color.Black;
			else
				fpsTb.ForeColor = System.Drawing.Color.Red;
			return b;
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
		public bool EnableSSAA => ssaaFactor > 1;
		public bool Sphere;
		public bool Stereo;
		public bool VrMetadata;
		public float Fps = 60;
		internal int resoIndex
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
		internal int sphereResoIndex;
		internal int nonSphereResoIndex;
		internal int ssaaIndex => (int)Math.Log(ssaaFactor, 2);

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
					VrMetadata = (bool)entry.Value;
				else if (entry.Name == "vrStereo")
					Stereo = (bool)entry.Value;
				else if (entry.Name == "sphereResoIndex")
					sphereResoIndex = (int)entry.Value;
				else if (entry.Name == "nonSphereResoIndex")
					nonSphereResoIndex = (int)entry.Value;
				else if (entry.Name == "ssaaFactor")
					SSAAFactor = (int)entry.Value;
				else if (entry.Name == "fps")
					Fps = (float)entry.Value;
			}
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("sphere", Form1.VidExpForm.Options.Sphere);
			info.AddValue("vrMeta", Form1.VidExpForm.Options.VrMetadata);
			info.AddValue("vrStereo", Form1.VidExpForm.Options.Stereo);
			info.AddValue("sphereResoIndex", Math.Max(0, Form1.VidExpForm.Options.sphereResoIndex));
			info.AddValue("nonSphereResoIndex", Math.Max(0, Form1.VidExpForm.Options.nonSphereResoIndex));
			info.AddValue("ssaaFactor", Form1.VidExpForm.Options.SSAAFactor);
			info.AddValue("fps", Fps);
		}

		void updateSSAAReso()
		{
			SSAAWidth = Width * ssaaFactor;
			SSAAHeight = Height * ssaaFactor;
			while (SSAAWidth > 16384 || SSAAHeight > 16384)
			{
				SSAAWidth /= 2;
				SSAAHeight /= 2;
			}
		}
	}
}
