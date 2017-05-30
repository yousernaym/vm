using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Visual_Music
{
	public partial class TpartyIntegrationForm : Form
	{
		CommonOpenFileDialog hvscDirDialog = new CommonOpenFileDialog();
		const string SongLengthsFileName = "songlengths.txt";
		string hvscDir;
		public string HvscDir { get => hvscDir; set { hvscDir = hvscDirDialog.InitialDirectory = hvscDirTb.Text = value; songLengthCb.Enabled = HvscInstalled; } }
		string SongLengthsPath { get => HvscDir + "\\" + SongLengthsFileName; }
		const string XmPlaySidPluginFileName = "xmp-sid.dll";
		bool XmPlayInstalled { get => File.Exists(Program.XmPlayPath); }
		bool XmPlaySidPluginInstalled { get => XmPlayInstalled && File.Exists(Program.XmPlayDir + "\\" + XmPlaySidPluginFileName); }
		bool HvscInstalled { get => File.Exists(SongLengthsPath); }
		public bool ModuleMixdown{ get => modulesCb.Checked && XmPlayInstalled; set => modulesCb.Checked = XmPlayInstalled ? value : false; }
		public bool SidMixdown{ get => sidsCb.Checked && XmPlaySidPluginInstalled; set => sidsCb.Checked = XmPlaySidPluginInstalled ? value : false; }
		public bool HvscSongLengths { get => songLengthCb.Checked && HvscInstalled  ; set => songLengthCb.Checked = HvscInstalled ? value : false; }
		

		public TpartyIntegrationForm()
		{
			InitializeComponent();
			enableCheckboxes();
			hvscDirDialog.IsFolderPicker = true;
			hvscDirDialog.EnsurePathExists = true;
			//hvscDirDialog.FileOk += new System.ComponentModel.CancelEventHandler(hvscDirDialog_FileOk);
			hvscDirDialog.Title = "Browae to <HVSC root>\\C64Music\\DOCUMENTS";
		}

		private void xmPlayLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://support.xmplay.com/");
		}

		private void sidLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://support.xmplay.com/files_view.php?file_id=504");
		}

		private void hvscLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.hvsc.c64.org/#download");
		}

		void importZip(string zipPath, string checkForEntry, string extractionDir, CancelEventArgs e)
		{
			try
			{
				using (Stream stream = File.OpenRead(zipPath))
				{
					using (ZipArchive zipArchive = new ZipArchive(stream))
					{
						if (zipArchive.GetEntry(checkForEntry) != null)
						{
							foreach (ZipArchiveEntry entry in zipArchive.Entries)
							{
								string completeFileName = Path.Combine(extractionDir, entry.FullName);
								string directory = Path.GetDirectoryName(completeFileName);

								if (!Directory.Exists(directory))
									Directory.CreateDirectory(directory);

								if (entry.Name != "")
									entry.ExtractToFile(completeFileName, true);
							}
							//zipArchive.ExtractToDirectory(extractionDir);
						}
						else
						{
							MessageBox.Show(this, checkForEntry + " could not be found.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
							e.Cancel = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Form1.showErrorMsgBox(this, ex.Message);
				e.Cancel = true;
			}
			enableCheckboxes();
		}
		private void importXmPlayBtn_Click(object sender, EventArgs e)
		{
			openXmPlayDialog.ShowDialog();
			Directory.CreateDirectory(Program.XmPlayOutputDir);
		}

		private void importSidBtn_Click(object sender, EventArgs e)
		{
			openXmPlaySidPluginDialog.ShowDialog();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
			{
				this.Close();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		private void browseHvscBtn_Click(object sender, EventArgs e)
		{
			bool close = false;
			while (!close)
			{
				if (hvscDirDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					close = true;
					string newDir = hvscDirDialog.InitialDirectory = hvscDirDialog.FileName;
					if (!hvscInstalledAt(newDir))
					{
						Form1.showErrorMsgBox(this, SongLengthsFileName + " not found in specified folder.");
						close = false;
					}
					else
					{
						HvscDir = newDir;
					}
				}
				else
					close = true;
			}
			enableCheckboxes();
			Focus();
		}

		private void openXmPlayDialog_FileOk(object sender, CancelEventArgs e)
		{
			importZip(openXmPlayDialog.FileName, Program.XmPlayFileName, Program.XmPlayDir, e);
		}

		private void openXmPlaySidPluginDialog_FileOk(object sender, CancelEventArgs e)
		{
			importZip(openXmPlaySidPluginDialog.FileName, XmPlaySidPluginFileName, Program.XmPlayDir, e);
		}

		private void hvscDirDialog_FileOk(object sender, CancelEventArgs e)
		{
			string oldDir = HvscDir;
			HvscDir = hvscDirDialog.FileName;
			if (!HvscInstalled)
			{
				Form1.showErrorMsgBox(this, "songlengths.txt not found in specified folder.");
				HvscDir = oldDir;
				e.Cancel = true;
			}
			enableCheckboxes();
		}

		void enableCheckboxes()
		{
			modulesCb.Enabled = XmPlayInstalled;
			sidsCb.Enabled = XmPlaySidPluginInstalled;
			songLengthCb.Enabled = HvscInstalled;
		}

		private void TpartyIntegrationForm_Load(object sender, EventArgs e)
		{
			enableCheckboxes();
		}

		private void modulesCb_EnabledChanged(object sender, EventArgs e)
		{
			CheckBox cb = (CheckBox)sender;
			cb.Checked = cb.Enabled;
		}

		private void sidsCb_EnabledChanged(object sender, EventArgs e)
		{
			CheckBox cb = (CheckBox)sender;
			cb.Checked = cb.Enabled;
		}

		private void songLengthCb_EnabledChanged(object sender, EventArgs e)
		{
			CheckBox cb = (CheckBox)sender;
			cb.Checked = cb.Enabled;
		}

		bool hvscInstalledAt(string dir)
		{
			return File.Exists(Path.Combine(dir,SongLengthsFileName));
		}
	}
}
