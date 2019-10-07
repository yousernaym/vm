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
	public partial class LocateFile : Form
	{
		public enum Reason { Missing, Corrupt };
		public string Path { get; private set; }
		public LocateFile()
		{
			InitializeComponent();
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			 
		}
		public DialogResult ShowDialog(string path, Reason reason, bool critical, string optionalMsg = "")
		{
			string msg;
			if (reason == Reason.Missing)
			{
				msg = "File missing: ";
				okBtn.Text = "Locate file";
			}
			else
			{
				msg = "Invalid file format: ";
				okBtn.Text = "Select different file";
			}
			cancelBtn.Text = critical ? "Cancel" : "Ignore";
			msg += path;
			msg += "\n\n" + optionalMsg;
			messageLabel.Text = msg;
			Path = path;
			return base.ShowDialog();
		}

		private void OkBtn_Click_1(object sender, EventArgs e)
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.InitialDirectory = Path;
			fileDialog.FileName = Path;
			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				Path = fileDialog.FileName;
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
