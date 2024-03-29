﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class LocateFile : Form
    {
        public string FilePath { get; private set; }
        CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
        OpenFileDialog fileDialog = new OpenFileDialog();
        WaitForFileSearchForm waitForFileSearchForm = new WaitForFileSearchForm();

        public LocateFile()
        {
            InitializeComponent();
            folderDialog.IsFolderPicker = true;
        }

        public DialogResult ShowDialog(string filePath, string searchDir, ImportError error, ImportFileType fileType, bool criticalError)
        {
            bool isUrl = filePath.ToLower().StartsWith("http");
            findInFolderBtn.Visible = !isUrl;
            folderDialog.InitialDirectory = searchDir;

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
                fileDialog.InitialDirectory = searchDir;
                fileDialog.FileName = "";
            }
            else
            {
                fileDialog.InitialDirectory = Path.GetDirectoryName(FilePath);
                fileDialog.FileName = FilePath;
            }
            return base.ShowDialog();
        }

        private void selectFileBtn_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = fileDialog.FileName;
                DialogResult = DialogResult.OK;
            }
        }

        private void findInFolderBtn_Click(object sender, EventArgs e)
        {
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string searchDir = folderDialog.FileName;
                folderDialog.InitialDirectory = searchDir;
                var dlgRes = waitForFileSearchForm.ShowDialog(searchDir, Path.GetFileName(FilePath));
                string filePath = (string)waitForFileSearchForm.Result;
                if (filePath != null)
                {
                    FilePath = filePath;
                    DialogResult = DialogResult.OK;
                }
                else if (dlgRes != DialogResult.Cancel)
                    MessageBox.Show("File not found");
            }
        }

        private void retryBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
