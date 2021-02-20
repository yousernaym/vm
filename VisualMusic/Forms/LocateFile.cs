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
		CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
		OpenFileDialog fileDialog = new OpenFileDialog();
		private bool isUrl;

		public LocateFile()
		{
			InitializeComponent();
			folderDialog.IsFolderPicker = true;

		}

		public DialogResult ShowDialog(string filePath, string searchDir, Reason reason, bool criticalError, string optionalMsg = "")
		{
			isUrl = filePath.ToLower().StartsWith("http");
			findInFolderBtn.Visible = !isUrl;
			folderDialog.InitialDirectory = searchDir;
			string msg;
			
			if (reason == Reason.Missing)
				msg = "File missing: ";
			else
				msg = "Invalid file format: ";

			cancelBtn.Text = criticalError ? "Cancel" : "Ignore";
			msg += "\r\n" + filePath;
			msg += "\r\n\r\n" + optionalMsg;
			messageTb.Text = msg;
			FilePath = filePath;
			this.searchDir = searchDir;
			return base.ShowDialog();
		}

		private void selectFileBtn_Click(object sender, EventArgs e)
		{
			if (isUrl)
			{
				fileDialog.InitialDirectory = searchDir;
				fileDialog.FileName = "";
			}
			else
			{
				fileDialog.InitialDirectory = Path.GetDirectoryName(FilePath);
				fileDialog.FileName = FilePath;
			}
			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				FilePath = fileDialog.FileName;
				DialogResult = DialogResult.OK;
			}
		}

		private void findInFolderBtn_Click(object sender, EventArgs e)
		{
			if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				searchDir = folderDialog.FileName;
				var filePaths = Directory.GetFiles(searchDir, Path.GetFileName(FilePath), SearchOption.AllDirectories);
				if (filePaths.Length > 0)
				{
					FilePath = filePaths[0];
					DialogResult = DialogResult.OK;
				}
				else
					MessageBox.Show("File not found");
			}
		}

		private void retryBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
