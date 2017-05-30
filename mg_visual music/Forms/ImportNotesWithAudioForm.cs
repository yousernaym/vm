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

namespace Visual_Music
{
	public enum MixdownType { None, Tparty, Internal }
	public partial class ImportNotesWithAudioForm : SourceFileForm
    {
		//AutoResetEvent tpartyDoneEvent = new AutoResetEvent(false);
		static Process tpartyProcess;
		static FileSystemWatcher watcher;
		static string tpartyApp;
		static string tpartyArgs;
		static string tpartyOutputDir;
		static public string TpartyApp { get => tpartyApp; set => tpartyApp = value; }
		static public string TpartyArgs { get => tpartyArgs; set => tpartyArgs = value; }
		static public string TpartyOutputDir { get => tpartyOutputDir; set => tpartyOutputDir = value; }

		static string tpartyOutputFile;
		public static string TpartyOutputFile { get => tpartyOutputFile; }
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

			string xmPlayIniPath = TpartyIntegrationForm.XmPlayDir + "\\xmplay.ini";
			string[] iniLines = File.ReadAllLines(xmPlayIniPath);
			string findKey = "WritePath";
			for (int i=0;i<iniLines.Length;i++)
			{
				int equalSignIndex = iniLines[i].IndexOf('=');
				if (equalSignIndex < 0)
					continue;
				string key = iniLines[i].Substring(0, equalSignIndex).Trim();
				if (key == findKey)
					iniLines[i] = findKey + "=" + TpartyIntegrationForm.XmPlayOutputDir + "\\";
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

        protected void importFiles(bool insTrack, bool internalMixdownSupported, bool xmPlayMixdownSupported)
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
								   "\"" + noteFilePath.Text + "\" -boost",
								   TpartyIntegrationForm.XmPlayOutputDir);
						
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
				importUsingTpartyMixdown(insTrack, tpartyAppTb.Text,
					   tpartyArgsTb.Text.Replace("%notefilepath", "\"" + noteFilePath.Text + "\""),
					   tpartyAudioTb.Text);
            }
        }

		static public string runTpartyProcess()
		{
			if (!createTpartyProcess())
				return null;
			try
			{
				tpartyProcess.Start();
				tpartyProcess.WaitForExit();
				Program.form1.Activate();
			}
			catch (Exception)
			{
				MessageBox.Show("Couldn't load application " + tpartyProcess.ProcessName);
				return null;
			}
			finally
			{
				//tpartyProcess.Dispose();
			}
			return TpartyOutputFile;
		}
		void importUsingTpartyMixdown(bool insTrack, string appPath, string arguments, string outputDir)
		{
			tpartyApp = appPath;
			tpartyArgs = arguments;
			tpartyOutputDir = outputDir;
			base.importFiles(insTrack, MixdownType.Tparty, "");
		}
		static bool createTpartyProcess()
        {
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
