using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Visual_Music
{
	public partial class ImportNotesWithAudioForm : SourceFileForm
    {
		//AutoResetEvent tpartyDoneEvent = new AutoResetEvent(false);
		static Process tpartyProcess;
		static FileSystemWatcher watcher;
		static string tpartyOutputFile;
		
		CommonOpenFileDialog tpartyAudioDirDlg = new CommonOpenFileDialog();

		public ImportNotesWithAudioForm()
        {
            InitializeComponent();
        }
        public ImportNotesWithAudioForm(Form1 _parent) : base(_parent)
        { 
            InitializeComponent();
            existingAudioRbtn.Checked = true;

			if (watcher != null)
				watcher.Dispose();
			watcher = new FileSystemWatcher();
			watcher.Changed += new FileSystemEventHandler(OnChanged);
			
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Filter = "*.wav";

			tpartyAudioDirDlg.IsFolderPicker = true;
			tpartyAudioDirDlg.EnsurePathExists = true;
		}

        private void existingAudioRbtn_CheckedChanged(object sender, EventArgs e)
        {
            audioFilePath.Enabled = BrowseAudioBtn.Enabled = existingAudioRbtn.Checked;
            tPartyMixdownPnl.Enabled = !existingAudioRbtn.Checked;
        }

        private void browseTpartyExeBtn_Click(object sender, EventArgs e)
        {
            if (openTpartyExeDlg.ShowDialog() == DialogResult.OK)
                tpartyAppTb.Text = openTpartyExeDlg.FileName;
        }

        private void browseTpartyOutputBtn_Click(object sender, EventArgs e)
        {
			tpartyAudioDirDlg.InitialDirectory = tpartyOutputTb.Text;
			if (tpartyAudioDirDlg.ShowDialog() == CommonFileDialogResult.Ok)
                tpartyOutputTb.Text = tpartyAudioDirDlg.InitialDirectory = tpartyAudioDirDlg.FileName;
        }

        new protected void importFiles(ImportOptions options)
        {
			if (existingAudioRbtn.Checked)
            {   //No user-specified command line
				if (!String.IsNullOrWhiteSpace(options.AudioPath))
					options.MixdownType = Midi.MixdownType.None;
				if (options.MixdownType != Midi.MixdownType.None)
                {   //No existing audio file, do mixdown
                    if (options.MixdownType == Midi.MixdownType.Tparty)
                    {   //Mixdown with xmplay
						options.MixdownOutputDir = TpartyIntegrationForm.MixdownOutputDir;
						base.importFiles(options);
                    }
                    else
                    {   //Mixdown internally
                        base.importFiles(options);
                    }
                }
                else
                {   //No mixdown
                    base.importFiles(options);
                }
            }
            else 
            { //User-specified command line
				options.MixdownType = Midi.MixdownType.Tparty;
				options.MixdownAppPath = tpartyAppTb.Text;
				options.MixdownAppArgs = tpartyArgsTb.Text;
				options.MixdownOutputDir = tpartyOutputTb.Text;
				base.importFiles(options);
            }
        }

		static public string runTpartyProcess(ImportOptions options)
		{
			if (!createTpartyProcess(options))
				return null;
			string processName = "";
			try
			{
				tpartyOutputFile = null;
				tpartyProcess.Start();
				processName = tpartyProcess.ProcessName;
				tpartyProcess.WaitForExit();
				watcher.EnableRaisingEvents = false;
				Program.form1.Activate();
				if (tpartyOutputFile == null)
					Form1.showWarningMsgBox("Couldn't find audio mixdown at " + options.MixdownOutputDir);
				else
				{
					File.Delete(Program.MixdownPath);
					File.Move(tpartyOutputFile, Program.MixdownPath);
					tpartyOutputFile = Program.MixdownPath;
				}
			}
			catch (Exception e)
			{
				Form1.showWarningMsgBox(e.Message, $"An unexpected error occurred with process: {tpartyProcess.StartInfo.FileName}");
				return null;
			}
		
			return tpartyOutputFile;
		}
		
		static bool createTpartyProcess(ImportOptions options)
        {
			if (!Directory.Exists(options.MixdownOutputDir))
			{
				Form1.showWarningMsgBox("Couldn't find mixdown directory: \n" + options.MixdownOutputDir);
				return false;
			}
			try { watcher.Path = options.MixdownOutputDir; }
			catch (System.ArgumentException e)
			{
				Form1.showWarningMsgBox(e.Message);
				return false;
			}
			
			watcher.EnableRaisingEvents = true;

			if (tpartyProcess != null)
				tpartyProcess.Dispose();
			tpartyProcess = new Process();
			tpartyProcess.StartInfo.FileName = options.MixdownAppPath;
			tpartyProcess.StartInfo.Arguments = options.MixdownAppArgs?.Replace("%notefilepath", options.NotePath);
			return true;
		}
       
        static private void OnChanged(object sender, FileSystemEventArgs e)
        {
			//Console.WriteLine("OnChanged");
			watcher.EnableRaisingEvents = false;
			var file = new FileInfo(e.FullPath);
            FileStream stream = null;
            while (stream == null)
            {
                try
                {
                    stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException)
				{
					//Console.WriteLine("Filenotready");
				}
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
				if (tpartyProcess.HasExited && stream == null)
					return;
			}
			tpartyOutputFile = e.FullPath;
			//tpartyDoneEvent.Set();
			//Process[] components = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(tpartyProcess.StartInfo.FileName)));
			//if (components.Length > 0)R
			//{
			if (!tpartyProcess.HasExited)
			{
				tpartyProcess.Kill();
			}
		}
	}
}
