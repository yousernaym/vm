using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
	public partial class BaseDialog : Form
	{
		public BaseDialog()
		{
			InitializeComponent();
		}

		private void BaseDialog_Load(object sender, EventArgs e)
		{
			okBtn.Focus();
		}
	}
}
