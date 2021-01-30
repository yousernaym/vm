using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
	public partial class LocateFile : Form
	{
		public enum Reason { Missing, Corrupt };
		public string FilePath { get; private set; }
		string searchDir;

		public LocateFile()
		{
			InitializeComponent();
		}

 		public DialogResult ShowDialog(string filePath, string searchDir, Reason reason, bool criticalError, string optionalMsg = "")
		{
			string msg;
			if (reason == Reason.Missing)
			{
				msg = "File missing: ";
				findFileBtn.Text = "Find file";
				findInFolderBtn.Visible = true;
			}
			else
			{
				msg = "Invalid file format: ";
				findFileBtn.Text = "Select different file";
				findInFolderBtn.Visible = false; 
			}
			cancelBtn.Text = criticalError ? "Cancel" : "Ignore";
			msg += "\r\n" + filePath;
			msg += "\r\n\r\n" + optionalMsg;
			messageTb.Text = msg;
			FilePath = filePath;
			this.searchDir = searchDir;
			return base.ShowDialog();
		}

		private void findFileBtn_Click(object sender, EventArgs e)
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.InitialDirectory = FilePath;
			fileDialog.FileName = FilePath;
			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				FilePath = fileDialog.FileName;
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private void findInFolderBtn_Click(object sender, EventArgs e)
		{
			var folderDialog = new CommonOpenFileDialog();
			folderDialog.InitialDirectory = searchDir;
			folderDialog.IsFolderPicker = true;
			if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				searchDir = folderDialog.FileName;
				var filePaths = Directory.GetFiles(Path.GetDirectoryName(folderDialog.FileName), Path.GetFileName(FilePath), SearchOption.AllDirectories);
				if (filePaths.Length > 0)
				{
					FilePath = filePaths[0];
					DialogResult = DialogResult.OK;
					Close();
				}
				else
					MessageBox.Show("File not found");
			}
		}
	}
}
