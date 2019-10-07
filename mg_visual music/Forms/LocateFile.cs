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
	public partial class LocateFile : BaseDialog
	{
		string Path => fileTb.Text;
		public LocateFile()
		{
			InitializeComponent();
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			 
		}
		new public DialogResult ShowDialog()
		{
			return base.ShowDialog();
		}

		private void browseBtn_Click(object sender, EventArgs e)
		{

		}
	}
}
