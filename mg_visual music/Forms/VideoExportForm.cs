using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
	public partial class VideoExportForm : BaseDialog
	{
		Point resolution;
		public Point Resolution { get => resolution; }
		public bool Sphere { get => sphereCb.Checked; }
		public VideoExportForm()
		{
			InitializeComponent();
		}

		private void sphereCb_CheckedChanged(object sender, EventArgs e)
		{
			youtubeCb.Enabled = sphereCb.Checked;
		}

		private void fourkRb_CheckedChanged(object sender, EventArgs e)
		{
			if (((RadioButton)sender).Checked)
				resolution = new Point(3840, 2160);
		}

		private void fullHdRb_CheckedChanged(object sender, EventArgs e)
		{
			if (((RadioButton)sender).Checked)
				resolution = new Point(1920, 1080);
		}
	}
}
