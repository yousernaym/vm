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
	public enum MixdownType { None, Tparty, Internal }
	public abstract partial class ImportNotesWithAudioForm : SourceFileForm
    {
		//AutoResetEvent tpartyDoneEvent = new AutoResetEvent(false);
		static Process tpartyProcess;
		static FileSystemWatcher watcher;
		static string tpartyApp;
		static string tpartyArgs;
		static string tpartyOutputDir;
		static public string TpartyApp { get => tpartyApp; set => tpartyApp = value; }
		static public string TpartyArgs { get => tpartyArgs; set => tpartyArgs = value; }
		static public string TpartyOutputDir { get => tpartyOutputDir; set => tpartyOutputDir = value.ToLower(); }

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

        override public string AudioFilePath
        {
            get
            {
                //if (existingAudioRbtn.Checked)
                //{

                //}
                return existingAudioRbtn.Checked ? audioFilePath.Text : tpartyAudioTb.Text.Replace("%notefilename", Path.GetFileName(NoteFilePath));
            }

            set { audioFilePath.Text = value; }
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
			tpartyAudioDirDlg.InitialDirectory = tpartyAudioTb.Text;
			if (tpartyAudioDirDlg.ShowDialog() == CommonFileDialogResult.Ok)
                tpartyAudioTb.Text = tpartyAudioDirDlg.InitialDirectory = tpartyAudioDirDlg.FileName;
        }

        protected void importFiles(bool insTrack, bool internalMixdownSupported, bool xmPlayMixdownSupported, double songLengthS, Midi.FileType noteFileType)
        {
			if (!checkNoteFile())
				return;
			//insTrack = _insTrack;
            //internalMixdownSupported = _internalMixdownSupported;
            if (existingAudioRbtn.Checked)
            {   //No user-specified command line
                if (string.IsNullOrWhiteSpace(audioFilePath.Text))
                {   //No existing audio file, do mixdown
                    if (xmPlayMixdownSupported)
                    {   //Mixdown with xmplay
						//string folder = Application.StartupPath + "\\plugins\\xmplay";
						importUsingTpartyMixdown(insTrack, TpartyIntegrationForm.XmPlayPath,
								   "\"" + NoteFilePath + "\" -boost",
								   TpartyIntegrationForm.XmPlayOutputDir,
								   songLengthS,
								   noteFileType);
						
                    }
                    else
                    {   //Mixdown internally if possible, otherwise there will be no audio
                        base.importFiles(insTrack, internalMixdownSupported ? MixdownType.Internal : MixdownType.None, "", songLengthS, noteFileType);
                    }
                }
                else
                {   //Existing audio file specified
                    base.importFiles(insTrack, MixdownType.None, AudioFilePath, songLengthS, noteFileType);
                }
            }
            else 
            { //User-specified command line
				importUsingTpartyMixdown(insTrack, tpartyAppTb.Text,
					   tpartyArgsTb.Text.Replace("%notefilepath", "\"" + NoteFilePath + "\""),
					   tpartyAudioTb.Text,
					   songLengthS,
					   noteFileType);
            }
        }

		static public string runTpartyProcess()
		{
			if (!createTpartyProcess())
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
					Form1.showWarningMsgBox(null, "Couldn't find audio mixdown at " + tpartyOutputDir);
				else
				{
					File.Delete(Program.MixdownPath);
					File.Move(tpartyOutputFile, Program.MixdownPath);
					tpartyOutputFile = Program.MixdownPath;
				}
			}
			catch (Exception e)
			{
				Form1.showErrorMsgBox(null, e.Message, "Couldn't create audio file" + processName);
				return null;
			}
			finally
			{
				//tpartyProcess.Dispose();
			}
			return tpartyOutputFile;
		}
		void importUsingTpartyMixdown(bool insTrack, string appPath, string arguments, string outputDir, double songLengthS, Midi.FileType noteFileType)
		{
			tpartyApp = appPath;
			tpartyArgs = arguments;
			tpartyOutputDir = outputDir.TrimEnd('\\') + "\\";
			base.importFiles(insTrack, MixdownType.Tparty, "", songLengthS, noteFileType);
		}
		static bool createTpartyProcess()
        {
			if (!Directory.Exists(tpartyOutputDir))
			{
				MessageBox.Show("Couldn't find directory: \n" + tpartyOutputDir, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			try { watcher.Path = tpartyOutputDir; }
			catch (System.ArgumentException e)
			{
				MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			
			watcher.EnableRaisingEvents = true;

			if (tpartyProcess != null)
				tpartyProcess.Dispose();
			tpartyProcess = new Process();
			tpartyProcess.StartInfo.FileName = tpartyApp;
			tpartyProcess.StartInfo.Arguments = tpartyArgs;
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
				if (tpartyProcess.HasExited)
					return;
            }
			tpartyOutputFile = e.FullPath;
			//tpartyDoneEvent.Set();
			//Process[] components = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(tpartyProcess.StartInfo.FileName)));
			//if (components.Length > 0)
			//{
			if (!tpartyProcess.HasExited)
			{
				tpartyProcess.Kill();
			}
		}

		private void runTpartyBtn_Click(object sender, EventArgs e)
		{
			//if (!checkNoteFile())
				//return;
//			createTpartyProcess(tpartyAppTb.Text,
	//				   tpartyArgsTb.Text.Replace("%notefilepath", "\"" + noteFilePath.Text + "\""));

		}
	}
}
