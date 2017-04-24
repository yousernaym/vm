using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Visual_Music
{
	public partial class SourceFileForm : Form
	{
		public string NoteFilePath
		{
			get { return noteFilePath.Text; }
			set { noteFilePath.Text = value; }
		}
		public string AudioFilePath
		{
			get { return audioFilePath.Text; }
			set { audioFilePath.Text = value; }
		}
		
		Form1 parent;
		public string NoteFolder
		{
			get { return openNoteFileDlg.InitialDirectory; }
			set{ openNoteFileDlg.InitialDirectory = value; }
		}
		public string AudioFolder
		{
			get { return openAudioFileDlg.InitialDirectory; }
			set { openAudioFileDlg.InitialDirectory = value; }
		}
		public bool EraseCurrent
		{
			get { return eraseCurrent.Checked; }
		}
        public SourceFileForm()
        {
            InitializeComponent();
        }
        public SourceFileForm(Form1 _parent)
		{
			parent = _parent;
			InitializeComponent();
		}

		private void browseNoteBtn_Click(object sender, EventArgs e)
		{
			openNoteFileDlg.ShowDialog();
			Ok.Focus();
		}

		private void BrowseAudioBtn_Click(object sender, EventArgs e)
		{
			openAudioFileDlg.ShowDialog();
			Ok.Focus();
		}

		private void openNoteFileDlg_FileOk(object sender, CancelEventArgs e)
		{
			noteFilePath.Text = openNoteFileDlg.FileName;
			NoteFolder = Path.GetDirectoryName(noteFilePath.Text);
		}

		private void openAudioFileDlg_FileOk(object sender, CancelEventArgs e)
		{
			audioFilePath.Text = openAudioFileDlg.FileName;
			AudioFolder = Path.GetDirectoryName(audioFilePath.Text);
		}

		private void noteFilePath_TextChanged(object sender, EventArgs e)
		{
			//NoteFilePath = noteFilePath.Text;
		}

		private void audioFilePath_TextChanged(object sender, EventArgs e)
		{
			//AudioFilePath = audioFilePath.Text;
		}

        protected void importFiles(bool modInsTrack)
        {
            if (string.IsNullOrEmpty(NoteFilePath))
            {
                MessageBox.Show("Song file path required.");
                return;
            }
            if (!File.Exists(NoteFilePath))
            {
                MessageBox.Show("Song file note found.");
                return;
            }
            if (parent.openSourceFiles(NoteFilePath, AudioFilePath, eraseCurrent.Checked, modInsTrack))
            {
                DialogResult = DialogResult.OK;
                Hide();
            }
        }

		private void SourceFileForm_VisibleChanged(object sender, EventArgs e)
		{
			if (Visible)
			{
			}
		}

		private void SourceFileForm_Load(object sender, EventArgs e)
		{
			Ok.Focus();
		}
	}
}
