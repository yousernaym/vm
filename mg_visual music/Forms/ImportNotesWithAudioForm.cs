﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Visual_Music
{
	public enum MixdownType { None, Tparty, Internal }
	public partial class ImportNotesWithAudioForm : SourceFileForm
    {
		//AutoResetEvent tpartyDoneEvent = new AutoResetEvent(false);
		static string XmPlayPath = Application.StartupPath + "\\plugins\\xmplay";
		static string XmPlayOutputPath = XmPlayPath + "\\output";
		static Process tpartyProcess;
		FileSystemWatcher watcher = new FileSystemWatcher();
        bool insTrack;
        bool internalMixdownSupported;
		static string cmdLineOutputFile;
		public static string CmdLineOutputFile { get => cmdLineOutputFile; }
		public ImportNotesWithAudioForm()
        {
            InitializeComponent();
        }
        public ImportNotesWithAudioForm(Form1 _parent) : base(_parent)
        { 
            InitializeComponent();
            existingAudioRbtn.Checked = true;
			watcher.Changed += new FileSystemEventHandler(OnChanged);

			string xmPlayIniPath = Application.StartupPath + "\\plugins\\xmplay\\xmplay.ini";
			//FileStream xmPlayIni = File.Open(, FileMode.Open, FileAccess.Read);
			string[] iniLines = File.ReadAllLines(xmPlayIniPath);
			string findKey = "WritePath";
			for (int i=0;i<iniLines.Length;i++)
			{
				int equalSignIndex = iniLines[i].IndexOf('=');
				if (equalSignIndex < 0)
					continue;
				string key = iniLines[i].Substring(0, equalSignIndex).Trim();
				if (key == findKey)
					iniLines[i] = findKey + "=" + XmPlayOutputPath + "\\";
			}
			File.WriteAllLines(xmPlayIniPath, iniLines);


		}

        override public string AudioFilePath
        {
            get
            {
                //if (existingAudioRbtn.Checked)
                //{

                //}
                return existingAudioRbtn.Checked ? audioFilePath.Text : tpartyAudioTb.Text.Replace("%notefilename", Path.GetFileName(noteFilePath.Text));
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
            if (openTpartyAudioDlg.ShowDialog() == DialogResult.OK)
                tpartyAudioTb.Text = openTpartyAudioDlg.FileName;
        }

        protected void importFiles(bool _insTrack, bool _internalMixdownSupported, bool xmPlayMixdownSupported)
        {
			if (!checkNoteFile())
				return;
			insTrack = _insTrack;
            internalMixdownSupported = _internalMixdownSupported;
            if (existingAudioRbtn.Checked)
            {   //No user-specified command line
                if (string.IsNullOrWhiteSpace(audioFilePath.Text))
                {   //No existing audio file, do mixdown
                    if (xmPlayMixdownSupported)
                    {   //Mixdown with xmplay
						//string folder = Application.StartupPath + "\\plugins\\xmplay";
						if (createTpartyProcess(XmPlayPath + "\\xmplay.exe",
								   "\"" + noteFilePath.Text + "\" -boost",
								   XmPlayOutputPath))
						{
							//tpartyDoneEvent.WaitOne();
							
						}
                    }
                    else
                    {   //Mixdown internally if possible, otherwise there will be no audio
                        base.importFiles(insTrack, internalMixdownSupported ? MixdownType.Internal : MixdownType.None, "");
                    }
                }
                else
                {   //Existing audio file specified
                    base.importFiles(insTrack, MixdownType.None, AudioFilePath);
                }
            }
            else 
            { //User-specified command line
				if (createTpartyProcess(tpartyAppTb.Text,
					   tpartyArgsTb.Text.Replace("%notefilepath", "\"" + noteFilePath.Text + "\""),
					   tpartyAudioTb.Text))
				{
					//base.importFiles(insTrack, false, tpartyAudioTb.Text.Replace("%notefilename", Path.GetFileName(noteFilePath.Text)));
				}
            }
        }

		static public bool runTpartyProcess()
		{
			try
			{
				tpartyProcess.Start();
				tpartyProcess.WaitForExit();
			}
			catch (Exception)
			{
				MessageBox.Show("Couldn't load application " + tpartyProcess.ProcessName);
				return false;
			}
			return true;
		}
		private bool createTpartyProcess(string appPath, string arguments, string outputPath)
        {
			try { watcher.Path = outputPath; }
			catch (System.ArgumentException e)
			{
				MessageBox.Show(Parent, e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Filter = "*.wav";
			watcher.EnableRaisingEvents = true;

			tpartyProcess = new Process();
            tpartyProcess.StartInfo.FileName = appPath;
            tpartyProcess.StartInfo.Arguments = arguments;
			
			base.importFiles(insTrack, MixdownType.Tparty, "");
			//tpartyProcess.WaitForExit();
			return true;
		}
       
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            watcher.EnableRaisingEvents = false;
            var file = new FileInfo(e.FullPath);
            FileStream stream = null;
            while (stream == null)
            {
                try
                {
                    stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException) { }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
			cmdLineOutputFile = e.FullPath;
			//tpartyDoneEvent.Set();
			//Process[] components = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(tpartyProcess.StartInfo.FileName)));
			//if (components.Length > 0)
			//{
			if (!tpartyProcess.HasExited)
			{
				tpartyProcess.Kill();
				tpartyProcess.Dispose();
			}
				//tpartyProcess.CloseMainWindow();

				//}
			}

		private void runTpartyBtn_Click(object sender, EventArgs e)
		{
			if (!checkNoteFile())
				return;
//			createTpartyProcess(tpartyAppTb.Text,
	//				   tpartyArgsTb.Text.Replace("%notefilepath", "\"" + noteFilePath.Text + "\""));

		}
	}
}
