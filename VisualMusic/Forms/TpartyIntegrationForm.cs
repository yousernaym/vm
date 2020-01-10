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
using System.Text.RegularExpressions;

namespace VisualMusic
{
	public partial class TpartyIntegrationForm : Form
	{
		const string SongLengthsFileName = "songlengths.md5";
		static readonly public string TpartyDir = Path.Combine(Program.AppDataDir, "tparty");
		public readonly static string XmPlayDir = Path.Combine(TpartyDir, "xmplay");
		public readonly static string SidPlayDir = Path.Combine(TpartyDir, "sidplayfp");
		public const string XmPlayFileName = "xmplay.exe";
		public const string SidPlayFileName = "sidplayfp.exe";
		public readonly static string XmPlayPath = Path.Combine(XmPlayDir, XmPlayFileName);
		public readonly static string SidPlayPath = Path.Combine(SidPlayDir, SidPlayFileName);
		public readonly static string MixdownOutputDir = Program.TempDir;
		
		CommonOpenFileDialog hvscDirDialog = new CommonOpenFileDialog();
		string hvscDir = "";
		public string HvscDir
		{
			get => hvscDir;
			set
			{
				hvscDir = hvscDirDialog.InitialDirectory = hvscDirTb.Text = value; songLengthsCb.Enabled = HvscInstalled;
				setXmPlayIni_hvscDir();
			}
		}
		public string SongLengthsPath { get => Path.Combine(HvscDir, SongLengthsFileName); }
		bool XmPlayInstalled { get => File.Exists(XmPlayPath); }
		bool HvscInstalled { get => hvscInstalledAt(hvscDir); }
		public bool ModuleMixdown{ get => modulesCb.Checked && XmPlayInstalled; set => modulesCb.Checked = XmPlayInstalled ? value : false; }
		public bool HvscSongLengths { get => songLengthsCb.Checked && HvscInstalled; set => songLengthsCb.Checked = HvscInstalled ? value : false; }
		
		public TpartyIntegrationForm()
		{
			InitializeComponent();
			enableCheckboxes();
			hvscDirDialog.IsFolderPicker = true;
			hvscDirDialog.EnsurePathExists = true;
			//hvscDirDialog.FileOk += new System.ComponentModel.CancelEventHandler(hvscDirDialog_FileOk);
			hvscDirDialog.Title = @"Browse to C64Music\DOCUMENTS";
			Directory.CreateDirectory(MixdownOutputDir);
			setXmPlayIni_outputDir();
		}

		private void xmPlayLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://support.xmplay.com/");
		}

		private void sidLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://sourceforge.net/projects/sidplay-residfp/files/sidplayfp/1.4/");
		}

		private void hvscLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.hvsc.c64.org/#download");
		}

		void importZip(string zipPath, string checkForEntry, string extractionDir, CancelEventArgs e, bool keepDirStructure = false)
		{
			try
			{
				using (Stream stream = File.OpenRead(zipPath))
				{
					using (ZipArchive zipArchive = new ZipArchive(stream))
					{
						//if (zipArchive.GetEntry(checkForEntry) != null)
						//{
						bool entryFound = false;
						foreach (ZipArchiveEntry entry in zipArchive.Entries)
						{
							if (entry.Name == checkForEntry)
							{
								entryFound = true;
								break;
							}
						}
						if (!entryFound)
						{
							MessageBox.Show(this, checkForEntry + " could not be found.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
							e.Cancel = true;
						}
						foreach (ZipArchiveEntry entry in zipArchive.Entries)
						{
							string entryPath;
							entryPath = keepDirStructure ? entry.FullName : entry.Name;
							entryPath = Path.Combine(extractionDir, entryPath);
							string directory = Path.GetDirectoryName(entryPath);

							if (!Directory.Exists(directory))
								Directory.CreateDirectory(directory);

							if (entry.Name != "")
								entry.ExtractToFile(entryPath, true);
						}
						//zipArchive.ExtractToDirectory(extractionDir);
						//}
						//else
						//{
						//	MessageBox.Show(this, checkForEntry + " could not be found.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
						//	e.Cancel = true;
						//}
					}
				}
			}
			catch (Exception ex)
			{
				Form1.showErrorMsgBox(ex.Message);
				e.Cancel = true;
			}
			enableCheckboxes();
		}
		private void importXmPlayBtn_Click(object sender, EventArgs e)
		{
			if (openXmPlayDialog.ShowDialog() == DialogResult.OK)
			{
				setXmPlayIni_outputDir();
				setXmPlayIni_hvscDir();
				Directory.CreateDirectory(MixdownOutputDir);
			}
		}
		
		private void importSidBtn_Click(object sender, EventArgs e)
		{
			openSidPlayDialog.ShowDialog();
			//setXmPlayIni_hvscDir();
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
						Form1.showErrorMsgBox(SongLengthsFileName + " not found in specified folder.");
						close = false;
					}
					else
						HvscDir = newDir;
				}
				else
					close = true;
			}
			enableCheckboxes();
			Focus();
		}

		private void openXmPlayDialog_FileOk(object sender, CancelEventArgs e)
		{
			importZip(openXmPlayDialog.FileName, XmPlayFileName, XmPlayDir, e);
		}

		private void openXmPlaySidPluginDialog_FileOk(object sender, CancelEventArgs e)
		{
			importZip(openSidPlayDialog.FileName, SidPlayFileName, SidPlayDir, e);
		}

		private void hvscDirDialog_FileOk(object sender, CancelEventArgs e)
		{
			string oldDir = HvscDir;
			HvscDir = hvscDirDialog.FileName;
			if (!HvscInstalled)
			{
				Form1.showErrorMsgBox("songlengths.txt not found in specified folder.");
				HvscDir = oldDir;
				e.Cancel = true;
			}
			enableCheckboxes();
		}

		void enableCheckboxes()
		{
			modulesCb.Enabled = XmPlayInstalled;
			songLengthsCb.Enabled = HvscInstalled;
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
		
		void setXmPlayIni_hvscDir()
		{
			setXmPlayIniValue("SID_.*", "documents", HvscDir);
		}
		void setXmPlayIni_outputDir()
		{
			setXmPlayIniValue("", "WritePath", MixdownOutputDir + "\\");
		}
		static void setXmPlayIniValue(string section, string key, string value)
		{
			if (string.IsNullOrEmpty(section))
				section = "XMPlay";
			string file = XmPlayDir + "\\xmplay.ini";
			string[] iniLines = File.ReadAllLines(file);
			int sectionStart = -1;
			for (int i = 0; i < iniLines.Length; i++)
			{
				string pattern = "\\[" + section + "\\]";
				Regex regex = new Regex(pattern);
				if (Regex.IsMatch(iniLines[i], pattern))
				{
					Match m = regex.Match(iniLines[i]);
					bool b = m.Success;
					sectionStart = i;
					break;
				}
			}
			if (sectionStart < 0)
				throw new Exception("Couldn't find section " + section + " in " + file);
			bool keyFound = false;
			for (int i = sectionStart; i < iniLines.Length; i++)
			{
				int equalSignIndex = iniLines[i].IndexOf('=');
				if (equalSignIndex < 0)
					continue;
				string lineKey = iniLines[i].Substring(0, equalSignIndex).Trim();
				if (lineKey == key)
				{
					iniLines[i] = key + "=" + value;
					keyFound = true;
					break;
				}
			}
			if (!keyFound)
				throw new Exception("Couldn't find key " + key + " in " + file);
			File.WriteAllLines(file, iniLines);
		}
	}
}
