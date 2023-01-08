using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class TpartyIntegrationForm : Form
    {
        public readonly static string TpartyDir = Path.Combine(Program.AppDataDir, "tparty");
        public readonly static string XmPlayDir = Path.Combine(TpartyDir, "xmplay");
        public readonly static string SidPlayDir = Path.Combine(TpartyDir, "sidplayfp");
        public const string XmPlayFileName = "xmplay.exe";
        public const string XmPlayIniFileName = "xmplay.ini";
        public const string SidPlayFileName = "sidplayfp.exe";
        public readonly static string XmPlayPath = Path.Combine(XmPlayDir, XmPlayFileName);
        public readonly static string XmPlayIniPath = Path.Combine(XmPlayDir, XmPlayIniFileName);
        public readonly static string SidPlayPath = Path.Combine(SidPlayDir, SidPlayFileName);
        public readonly static string MixdownOutputDir = Program.TempDir;

        const string DefaultSongLengthsUrl = "https://www.hvsc.c64.org/download/C64Music/DOCUMENTS/Songlengths.md5";
        public readonly string SongLengthsPath = Path.Combine(TpartyDir, "hvsc", "songlenghts.md5");
        public string SongLengthsUrl
        {
            get => songLengthsUrlTb.Text;
            set => songLengthsUrlTb.Text = value;
        }

        ProgressForm songLengthsDownloadForm;
        bool silentSongLengthsDownload;
        readonly string tempSongLengthDownloadPath;

        bool XmPlayInstalled { get => File.Exists(XmPlayPath); }
        public bool ModuleMixdown { get => modulesCb.Checked && XmPlayInstalled; set => modulesCb.Checked = XmPlayInstalled ? value : false; }

        public TpartyIntegrationForm()
        {
            InitializeComponent();
            enableCheckboxes();
            Directory.CreateDirectory(MixdownOutputDir);
            if (!File.Exists(XmPlayIniPath))
            {
                Directory.CreateDirectory(XmPlayDir);
                File.Copy(Path.Combine(Program.Dir, XmPlayIniFileName), XmPlayIniPath);
            }
            if (!Directory.Exists(Path.GetDirectoryName(SongLengthsPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(SongLengthsPath));
            setXmPlayIni_outputDir();
            SongLengthsUrl = DefaultSongLengthsUrl;
            tempSongLengthDownloadPath = SongLengthsPath + "_";
        }

        private void xmPlayLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://support.xmplay.com/");
        }

        private void sidLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://sourceforge.net/projects/sidplay-residfp/files/sidplayfp/1.4/");
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
                Directory.CreateDirectory(MixdownOutputDir);
            }
        }

        private void importSidBtn_Click(object sender, EventArgs e)
        {
            openSidPlayDialog.ShowDialog();
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

        private void openXmPlayDialog_FileOk(object sender, CancelEventArgs e)
        {
            importZip(openXmPlayDialog.FileName, XmPlayFileName, XmPlayDir, e);
        }

        private void openXmPlaySidPluginDialog_FileOk(object sender, CancelEventArgs e)
        {
            importZip(openSidPlayDialog.FileName, SidPlayFileName, SidPlayDir, e);
        }

        void enableCheckboxes()
        {
            modulesCb.Enabled = XmPlayInstalled;
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

        void setXmPlayIni_outputDir()
        {
            setXmPlayIniValue("", "WritePath", MixdownOutputDir + "\\");
        }
        static void setXmPlayIniValue(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section))
                section = "XMPlay";
            string[] iniLines = File.ReadAllLines(XmPlayIniPath);
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
                throw new Exception("Couldn't find section " + section + " in " + XmPlayIniPath);
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
                throw new Exception("Couldn't find key " + key + " in " + XmPlayIniPath);
            File.WriteAllLines(XmPlayIniPath, iniLines);
        }

        private void updateSongLengthsBtn_Click(object sender, EventArgs e)
        {
            downloadSonglengths(false);
        }

        public void downloadSonglengths(bool silent)
        {
            if (silent && File.Exists(SongLengthsPath))
            {
                UpdateLastUpdatedLabel();
                var fi = new FileInfo(SongLengthsPath);
                var daysSinceDownload = (DateTime.Now - fi.LastWriteTime).TotalDays;
                if (daysSinceDownload < 30)
                    return;
            }
            silentSongLengthsDownload = silent;
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(WebClient_SongLengthsDownloadCompleted);
            if (!silent)
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            try
            {
                webClient.DownloadFileAsync(new Uri(SongLengthsUrl), tempSongLengthDownloadPath);
            }
            catch (UriFormatException)
            {
                if (!silent)
                    Form1.showErrorMsgBox("Invalid url.");
                return;
            }

            if (!silent)
            {
                songLengthsDownloadForm = new ProgressForm();
                if (songLengthsDownloadForm.ShowDialog() == DialogResult.Cancel)
                    webClient.CancelAsync();
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            songLengthsDownloadForm.updateProgress(e.ProgressPercentage);
        }

        private void WebClient_SongLengthsDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (!silentSongLengthsDownload)
                songLengthsDownloadForm.DialogResult = DialogResult.OK;
            if (e.Cancelled)
            {
                File.Delete(tempSongLengthDownloadPath);
                if (!silentSongLengthsDownload)
                    MessageBox.Show("Update cancelled.");
                return;
            }
            else if (e.Error != null)
            {
                File.Delete(tempSongLengthDownloadPath);
                if (!silentSongLengthsDownload)
                    Form1.showErrorMsgBox("Couldn't download file from the specified url.");
                return;
            }

            File.Delete(SongLengthsPath);
            File.Move(tempSongLengthDownloadPath, SongLengthsPath);
            UpdateLastUpdatedLabel();
        }

        private void UpdateLastUpdatedLabel()
        {
            var fi = new FileInfo(SongLengthsPath);
            lastUpdatedLabel.Text = "Last updated: " + fi.LastWriteTime.ToString();
        }

        private void defaultSongLengthsBtn_Click(object sender, EventArgs e)
        {
            SongLengthsUrl = songLengthsUrlTb.Text = DefaultSongLengthsUrl;
        }
    }
}
