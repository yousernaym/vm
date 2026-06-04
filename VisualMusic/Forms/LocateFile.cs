using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class LocateFile : Form
    {
        public string FilePath { get; private set; }
        CommonOpenFileDialog _folderDialog = new CommonOpenFileDialog();
        OpenFileDialog _fileDialog = new OpenFileDialog();
        WaitForFileSearchForm _waitForFileSearchForm = new WaitForFileSearchForm();

        public LocateFile()
        {
            InitializeComponent();
            _folderDialog.IsFolderPicker = true;
        }

        public DialogResult ShowDialog(string filePath, string searchDir, ImportError error, ImportFileType fileType, bool criticalError)
        {
            bool isUrl = filePath.ToLower().StartsWith("http");
            findInFolderBtn.Visible = !isUrl;
            _folderDialog.InitialDirectory = searchDir;

            string fileTypeString = fileType == ImportFileType.Note ? "Note" : "Audio";
            if (error == ImportError.Missing)
                errorLabel.Text = $"{fileTypeString} file missing: ";
            else
                errorLabel.Text = $"Invalid {fileTypeString.ToLower()} file format: ";

            cancelBtn.Text = criticalError ? "Cancel" : "Ignore";
            filePathTb.Text = filePath;
            FilePath = filePath;

            if (isUrl)
            {
                _fileDialog.InitialDirectory = searchDir;
                _fileDialog.FileName = "";
            }
            else
            {
                _fileDialog.InitialDirectory = Path.GetDirectoryName(FilePath);
                _fileDialog.FileName = FilePath;
            }
            return base.ShowDialog();
        }

        private void SelectFileBtn_Click(object sender, EventArgs e)
        {
            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = _fileDialog.FileName;
                DialogResult = DialogResult.OK;
            }
        }

        private void FindInFolderBtn_Click(object sender, EventArgs e)
        {
            if (_folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string searchDir = _folderDialog.FileName;
                _folderDialog.InitialDirectory = searchDir;
                var dlgRes = _waitForFileSearchForm.ShowDialog(searchDir, Path.GetFileName(FilePath));
                string filePath = (string)_waitForFileSearchForm.Result;
                if (filePath != null)
                {
                    FilePath = filePath;
                    DialogResult = DialogResult.OK;
                }
                else if (dlgRes != DialogResult.Cancel)
                    MessageBox.Show("File not found");
            }
        }

        private void RetryBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
