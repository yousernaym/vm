using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class ImportNotesWithAudioForm : SourceFileForm
    {
        //AutoResetEvent tpartyDoneEvent = new AutoResetEvent(false);
        static Process s_tpartyProcess;
        static FileSystemWatcher s_watcher;
        static string s_tpartyOutputFile;

        CommonOpenFileDialog _tpartyAudioDirDlg = new CommonOpenFileDialog();

        public ImportNotesWithAudioForm()
        {
            InitializeComponent();
        }
        public ImportNotesWithAudioForm(Form1 _parent) : base(_parent)
        {
            InitializeComponent();
            existingAudioRbtn.Checked = true;

            if (s_watcher != null)
                s_watcher.Dispose();
            s_watcher = new FileSystemWatcher();
            s_watcher.Changed += new FileSystemEventHandler(OnChanged);

            s_watcher.NotifyFilter = NotifyFilters.LastWrite;
            s_watcher.Filter = "*.wav";

            _tpartyAudioDirDlg.IsFolderPicker = true;
            _tpartyAudioDirDlg.EnsurePathExists = true;
        }

        private void ExistingAudioRbtn_CheckedChanged(object sender, EventArgs e)
        {
            audioFilePath.Enabled = BrowseAudioBtn.Enabled = existingAudioRbtn.Checked;
            tPartyMixdownPnl.Enabled = !existingAudioRbtn.Checked;
        }

        private void BrowseTpartyExeBtn_Click(object sender, EventArgs e)
        {
            if (openTpartyExeDlg.ShowDialog() == DialogResult.OK)
                tpartyAppTb.Text = openTpartyExeDlg.FileName;
        }

        private void BrowseTpartyOutputBtn_Click(object sender, EventArgs e)
        {
            _tpartyAudioDirDlg.InitialDirectory = tpartyOutputTb.Text;
            if (_tpartyAudioDirDlg.ShowDialog() == CommonFileDialogResult.Ok)
                tpartyOutputTb.Text = _tpartyAudioDirDlg.InitialDirectory = _tpartyAudioDirDlg.FileName;
        }

        new protected void ImportFiles(ImportOptions options)
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
                        base.ImportFiles(options);
                    }
                    else
                    {   //Mixdown internally
                        base.ImportFiles(options);
                    }
                }
                else
                {   //No mixdown
                    base.ImportFiles(options);
                }
            }
            else
            { //User-specified command line
                options.MixdownType = Midi.MixdownType.Tparty;
                options.MixdownAppPath = tpartyAppTb.Text;
                options.MixdownAppArgs = tpartyArgsTb.Text;
                options.MixdownOutputDir = tpartyOutputTb.Text;
                base.ImportFiles(options);
            }
        }

        static public string RunTpartyProcess(ImportOptions options)
        {
            if (!CreateTpartyProcess(options))
                return null;
            string processName = "";
            try
            {
                s_tpartyOutputFile = null;
                s_tpartyProcess.Start();
                processName = s_tpartyProcess.ProcessName;
                s_tpartyProcess.WaitForExit();
                s_watcher.EnableRaisingEvents = false;
                Program.form1.Activate();
                if (s_tpartyOutputFile == null)
                    Form1.ShowWarningMsgBox("Couldn't find audio mixdown at " + options.MixdownOutputDir);
                else
                {
                    File.Delete(Program.MixdownPath);
                    File.Move(s_tpartyOutputFile, Program.MixdownPath);
                    s_tpartyOutputFile = Program.MixdownPath;
                }
            }
            catch (Exception e)
            {
                Form1.ShowWarningMsgBox(e.Message, $"An unexpected error occurred with process: {s_tpartyProcess.StartInfo.FileName}");
                return null;
            }

            return s_tpartyOutputFile;
        }

        static bool CreateTpartyProcess(ImportOptions options)
        {
            if (!Directory.Exists(options.MixdownOutputDir))
            {
                Form1.ShowWarningMsgBox("Couldn't find mixdown directory: \n" + options.MixdownOutputDir);
                return false;
            }
            try { s_watcher.Path = options.MixdownOutputDir; }
            catch (System.ArgumentException e)
            {
                Form1.ShowWarningMsgBox(e.Message);
                return false;
            }

            s_watcher.EnableRaisingEvents = true;

            if (s_tpartyProcess != null)
                s_tpartyProcess.Dispose();
            s_tpartyProcess = new Process();
            s_tpartyProcess.StartInfo.FileName = options.MixdownAppPath;
            s_tpartyProcess.StartInfo.Arguments = options.MixdownAppArgs?.Replace("%notefilepath", options.NotePath);
            return true;
        }

        static private void OnChanged(object sender, FileSystemEventArgs e)
        {
            //Console.WriteLine("OnChanged");
            s_watcher.EnableRaisingEvents = false;
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
                if (s_tpartyProcess.HasExited && stream == null)
                    return;
            }
            s_tpartyOutputFile = e.FullPath;
            //tpartyDoneEvent.Set();
            //Process[] components = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(tpartyProcess.StartInfo.FileName)));
            //if (components.Length > 0)R
            //{
            if (!s_tpartyProcess.HasExited)
            {
                s_tpartyProcess.Kill();
            }
        }
    }
}
