using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Visual_Music
{
    public partial class ImportNotesWithAudioForm : Visual_Music.ImportMidiForm
    {
        Process tpartyProcess;
        FileSystemWatcher watcher = new FileSystemWatcher();
        bool insTrack;
        bool internalMixdownSupported;
        public ImportNotesWithAudioForm()
        {
            InitializeComponent();
        }
        public ImportNotesWithAudioForm(Form1 _parent) : base(_parent)
        { 
            InitializeComponent();
            existingAudioRbtn.Checked = true;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
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
            insTrack = _insTrack;
            internalMixdownSupported = _internalMixdownSupported;
            if (existingAudioRbtn.Checked)
            {   //No user-specified command line
                if (string.IsNullOrWhiteSpace(audioFilePath.Text))
                {   //No existing audio file, do mixdown
                    if (xmPlayMixdownSupported)
                    {   //Mixdown with xmplay
                        string folder = Application.StartupPath + "\\plugins\\xmplay";
						runCmdLine(folder + "\\xmplay.exe",
								   "\"" + noteFilePath.Text + "\" -boost",
                                   folder,
                                   "*.wav");
                    }
                    else
                    {   //Mixdown internally if possible, otherwise there will be no audio
                        base.importFiles(insTrack, internalMixdownSupported, "");
                    }
                }
                else
                {   //Existing audio file specified
                    importFiles(insTrack, false, AudioFilePath);
                }
            }
            else 
            { //User-specified command line
                runCmdLine(tpartyAppTb.Text,
                           tpartyArgsTb.Text.Replace("%notefilepath", "\"" + noteFilePath.Text + "\""),
                           tpartyAudioTb.Text,
                           "*.wav|*.mp3");
            }
        }
        private void Ok_Click(object sender, EventArgs e)
        {
           
        }

        private void runCmdLine(string appPath, string arguments, string outputPath, string filter)
        {
            tpartyProcess = new Process();
            tpartyProcess.StartInfo.FileName = appPath;
            tpartyProcess.StartInfo.Arguments = arguments;
            watcher.Path = outputPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = filter;
            watcher.EnableRaisingEvents = true;
            tpartyProcess.Start();
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

            Process[] components = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(tpartyProcess.StartInfo.FileName)));
            if (components.Length > 0)
            //if (!tpartyProcess.HasExited)
            {
                tpartyProcess.CloseMainWindow();
                tpartyProcess.Close();
            }
            base.importFiles(insTrack, internalMixdownSupported, e.FullPath);
        }
    }
}
