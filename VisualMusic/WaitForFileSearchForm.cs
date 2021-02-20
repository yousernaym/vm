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
	public partial class WaitForFileSearchForm : WaitForTaskForm
	{
		public WaitForFileSearchForm()
		{
			InitializeComponent();
		}

		public DialogResult ShowDialog(string searchDir, string fileName)
		{
			dirTb.Text = searchDir;
			return base.ShowDialog(() => findFile(searchDir, fileName), "Searching in folder");
		}
		
		//Search dir recursively
		//Return path to file if found, otherwise null
		string findFile(string searchDir, string fileName)
		{
			// Exclude some directories according to their attributes
			WaitForTaskForm.CancellationToken.ThrowIfCancellationRequested();
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
	}
}
