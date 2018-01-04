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
		Point resolution;
		public Point Resolution { get => resolution; }
		public bool Sphere { get => sphereCb.Checked; }
		public bool Stereo { get => StereoscopicCb.Checked; }
		public bool VrMetadata { get => vrMetadataCb.Checked; }
		public VideoExportForm()
		{
			InitializeComponent();
			updateResoItems();
		}

		private void sphereCb_CheckedChanged(object sender, EventArgs e)
		{
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
				e.Cancel = true;
				MessageBox.Show(null, "Invalid resolution.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void StereoscopicCb_CheckedChanged(object sender, EventArgs e)
		{
			updateResoItems();
		}

		void updateResoItems()
		{
			resoCb.Items.Clear();
			if (Sphere)
			{
				if (Stereo)
					resoCb.Items.Add("3060 x 3060");

				else
					resoCb.Items.Add("4320 x 4320");
			}
			else
			{
				resoCb.Items.Add("3840 x 2160");
				resoCb.Items.Add("1920 x 1080");
			}
			resoCb.SelectedIndex = 0;
		}

		bool parseReso()
		{
			resoCb.ForeColor = System.Drawing.Color.Red;
			
			string[] xy = resoCb.Text.Split('x', 'X');
			if (xy.Length != 2)
				return false;
			
			try
			{
				resolution.X = int.Parse(xy[0]);
				resolution.Y = int.Parse(xy[1]);
			}
			catch (System.FormatException)
			{
				return false;
			}

			if (resolution.X <= 0 || resolution.Y <= 0)
				return false;

			resoCb.ForeColor = System.Drawing.Color.Black;
			return true;
		}
	}
}
