using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
	public partial class LocateFile : Form
	{
		public string FilePath { get; private set; }
		CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
		OpenFileDialog fileDialog = new OpenFileDialog();
		WaitForFileSearchForm waitForFileSearchForm = new WaitForFileSearchForm();

		public LocateFile()
		{
			InitializeComponent();
			folderDialog.IsFolderPicker = true;
		}

		public DialogResult ShowDialog(string filePath, string searchDir, ImportError reason, bool criticalError, string optionalMsg = "")
		{
			bool isUrl = filePath.ToLower().StartsWith("http");
			findInFolderBtn.Visible = !isUrl;
			folderDialog.InitialDirectory = searchDir;
			string msg;
			
			if (reason == ImportError.Missing)
				msg = "File missing: ";
			else
				msg = "Invalid file format: ";

			cancelBtn.Text = criticalError ? "Cancel" : "Ignore";
			msg += "\r\n" + filePath;
			msg += "\r\n\r\n" + optionalMsg;
			messageTb.Text = msg;
			FilePath = filePath;
			
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
			return base.ShowDialog();
		}

		private void selectFileBtn_Click(object sender, EventArgs e)
		{
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
				string searchDir = folderDialog.FileName;
				folderDialog.InitialDirectory = searchDir;
				var dlgRes = waitForFileSearchForm.ShowDialog(searchDir, Path.GetFileName(FilePath));
				string filePath = (string)waitForFileSearchForm.Result;
				if (filePath != null)
				{
					FilePath = filePath;
					DialogResult = DialogResult.OK;
				}
				else if (dlgRes != DialogResult.Cancel)
					MessageBox.Show("File not found");
			}
		}

		private void retryBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
