using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
	public partial class StopRenderingMb : Form
	{
		public StopRenderingMb()
		{
			InitializeComponent();
		}

		private void StopRenderingMb_Load(object sender, EventArgs e)
		{

		}

		private void yesBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
		}

		private void noBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
		}
	}
}
