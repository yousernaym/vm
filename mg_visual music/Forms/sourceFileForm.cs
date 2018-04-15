using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace Visual_Music
{
	public partial class SourceFileForm : Form
	{
		WebClient client = new WebClient();
		public string DownloadedFilePath { get; private set; }
		ProgressForm progressForm = new ProgressForm();

		public string NoteFilePath
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(DownloadedFilePath))
					return DownloadedFilePath;
				else if (Uri.IsWellFormedUriString(noteFilePath.Text, UriKind.Absolute))
				{
					string fileName = "downloadedsong." + Path.GetFileName(noteFilePath.Text.Split('.').Last());
					Uri url = new Uri(noteFilePath.Text);
					DownloadedFilePath = Path.Combine(Program.TempDir, fileName);
					client.DownloadFileAsync(url, DownloadedFilePath);
					if (progressForm.ShowDialog() != DialogResult.OK)
						DownloadedFilePath = null;
					return DownloadedFilePath;
				}
				else
					return noteFilePath.Text;
			}
			set
			{
				if (noteFilePath.Text.Equals(value))
					return;
				noteFilePath.Text = value;
				DownloadedFilePath = null;
			}
		}
        virtual public string AudioFilePath
		{
			get { return audioFilePath.Text; }
			set { audioFilePath.Text = value; }
		}
		
		protected Form1 parent;
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
        public SourceFileForm() //For designer view
        {
            InitializeComponent();
        }
        public SourceFileForm(Form1 _parent)
		{
			parent = _parent;
			InitializeComponent();
			client.DownloadFileCompleted += OnDownloadCompleted;
			client.DownloadProgressChanged += OnDownloadProgressChanged;
			
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			progressForm.updateProgress(e.ProgressPercentage / 100.0f);
		}

		private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			//NoteFilePath = downloadedFilePath;
			progressForm.DialogResult = DialogResult.OK;
			progressForm.Hide();
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

		protected bool checkNoteFile()
		{
			if (string.IsNullOrWhiteSpace(NoteFilePath))
			{
				MessageBox.Show("Note file path required.");
				return false;
			}
			else if (!File.Exists(NoteFilePath))
			{
				MessageBox.Show("Note file not found.");
				return false;
			}
			return true;
		}
		protected void importFiles(bool modInsTrack, MixdownType mixdownType, string audioPath, double songLengthS, Midi.FileType noteFileType)
        {
			if (!checkNoteFile())
				return;
            if (parent.openSourceFiles(NoteFilePath, audioPath, eraseCurrent.Checked, modInsTrack, mixdownType, songLengthS, noteFileType))
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
