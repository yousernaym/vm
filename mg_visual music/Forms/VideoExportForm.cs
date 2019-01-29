using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Visual_Music
{
	public partial class VideoExportForm : BaseDialog
	{
		public VideoExportOptions Options { get; set; } = new VideoExportOptions();
		public VideoExportForm()
		{
			InitializeComponent();
			updateResoItems();
		}

		private void sphereCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.Sphere = sphereCb.Checked;
			vrMetadataCb.Enabled = vrMetadataCb.Checked = sphereCb.Checked;
			updateResoItems();
		}

		private void resoCb_TextChanged(object sender, EventArgs e)
		{
			parseReso();
		}

		private void VideoExportForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!parseReso() && DialogResult == DialogResult.OK)
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

		bool parseReso()
		{
			resoComboBox.ForeColor = System.Drawing.Color.Red;

			string[] xy = resoComboBox.Text.Split('x', 'X');
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

			resoComboBox.ForeColor = System.Drawing.Color.Black;
			return true;
		}

		private void vrMetadataCb_CheckedChanged(object sender, EventArgs e)
		{
			Options.VrMetadata = vrMetadataCb.Checked;
		}
	}

	public class VideoExportOptions
	{
		public int Width;
		public int Height;
		public int SuperSamplingWidth; 
		public int SuperSamplingHeight;
		public bool Sphere;
		public bool Stereo;
		public bool VrMetadata;
	}
}
