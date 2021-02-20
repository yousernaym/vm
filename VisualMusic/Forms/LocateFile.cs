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
		CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
		OpenFileDialog fileDialog = new OpenFileDialog();
		
		public LocateFile()
		{
			InitializeComponent();
			folderDialog.IsFolderPicker = true;
		}

		public DialogResult ShowDialog(string filePath, string searchDir, Reason reason, bool criticalError, string optionalMsg = "")
		{
			bool isUrl = filePath.ToLower().StartsWith("http");
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
				//var filePaths = Directory.GetFiles(searchDir, Path.GetFileName(FilePath), SearchOption.AllDirectories);
				var filePath = findFile(searchDir, Path.GetFileName(FilePath));
				if (filePath != null)
				{
					FilePath = filePath;
					DialogResult = DialogResult.OK;
				}
				else
					MessageBox.Show("File not found");
			}
		}

		//Search dir recursively
		//Return path to file if found, otherwise null
		string findFile(string searchDir, string fileName)
		{
			// Exclude some directories according to their attributes
			string[] files = null;
			string skipReason = null;
			var dirInfo = new DirectoryInfo(searchDir);
			var isroot = dirInfo.Root.FullName.Equals(dirInfo.FullName);

			// as root dirs (e.g. "C:\") apparently have the system + hidden flags set, we must check whether it's a root dir, if it is, we do NOT skip it even though those attributes are present
			// We must not access such folders/files, or this crashes with UnauthorizedAccessException on folders like $RECYCLE.BIN
			if (dirInfo.Attributes.HasFlag(FileAttributes.System) && !isroot)
			{
				skipReason = "system file/folder, no access";
			}

			if (null == skipReason)
			{
				try
				{
					files = Directory.GetFiles(searchDir);
				}
				catch (UnauthorizedAccessException ex)
				{
					skipReason = ex.Message;
				}
				catch (PathTooLongException ex)
				{
					skipReason = ex.Message;
				}
			}

			if (null != skipReason)
			{
				return null; // we skip this directory
			}

			foreach (var path in files)
			{
				var attr = File.GetAttributes(path);
				if ((attr & FileAttributes.Directory) != FileAttributes.Directory && Path.GetFileName(path) == fileName)
					return path;
			}

			try
			{
				var dirs = Directory.GetDirectories(searchDir);
				foreach (var d in dirs)
				{
					var path = findFile(d, fileName); // recursive call
					if (path != null)
						return path; 
				}
			}
			catch (PathTooLongException ex)
			{
				Form1.showErrorMsgBox(ex.Message);
			}
			return null;
		}

		private void retryBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
