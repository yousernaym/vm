using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music
{
	public partial class HueSatForm : BaseDialog
	{
		public event EventHandler SelectionChanged;
		public HueSatForm()
		{
			InitializeComponent();
		}

		private void twoDHueSat1_SelectionChanged(object sender, EventArgs e)
		{
			SelectionChanged?.Invoke(sender, e);
		}
	}
}
