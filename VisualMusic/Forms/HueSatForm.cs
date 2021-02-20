using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
	public partial class HueSatForm : BaseDialog
	{
		public event EventHandler SelectionChanged;
		public float Hue
		{
			get => twoDHueSat1.Hue;
			set => twoDHueSat1.Hue = value;
		}
		public float Saturation
		{
			get => twoDHueSat1.Saturation;
			set => twoDHueSat1.Saturation = value;
		}
		public HueSatForm()
		{
			InitializeComponent();
		}

		private void twoDHueSat1_SelectionChanged(object sender, EventArgs e)
		{
			SelectionChanged?.Invoke(sender, e);
		}

		private void okBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
