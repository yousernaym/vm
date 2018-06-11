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
using System.Runtime.Serialization;

namespace Visual_Music
{
	public partial class SourceFileForm : Form
	{
		//public string DownloadedFilePath { get; private set; }
		
		//public string RawNoteFilePath => noteFilePath.Text;
		public string NoteFilePath
		{
			get => noteFilePath.Text;
			set => noteFilePath.Text = value;
		}
			//get
			//{
			//	//If a file has been previously downloaded to temp dir, return path to that file.
			//	if (!string.IsNullOrWhiteSpace(DownloadedFilePath))
			//	{
			//		if (!File.Exists(DownloadedFilePath))
			//			DownloadedFilePath = null;
			//		return DownloadedFilePath;
			//	}
			//	//If noteFilePath textbox is a URL, download file to temp dir and return path to that file, otherwise return
			//	else if (noteFilePath.Text.IsUrl())
			//		return DownloadedFilePath = noteFilePath.Text.downloadFile();
			//	//Return path as written in textbox
			//	else
			//		return noteFilePath.Text;
			//}
			//set
			//{
			//	if (noteFilePath.Text.Equals(value))
			//		return;
			//	noteFilePath.Text = value;
			//	DownloadedFilePath = null;
			//}
		//}
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
			get => eraseCurrent.Checked;
			set => eraseCurrent.Checked = value;
		}
		
        public SourceFileForm() //For designer view
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
			//DownloadedFilePath = null;
		}

		private void audioFilePath_TextChanged(object sender, EventArgs e)
		{
			//AudioFilePath = audioFilePath.Text;
		}

		public virtual void importFiles() //Can't be abstract because designer won't be able to show derived forms
		{
			throw new NotImplementedException();
		}

		protected void importFiles(ImportOptions options)
        {
			if (!options.checkNoteFile())
				return;
			if (options.MixdownType == Midi.MixdownType.None && !string.IsNullOrWhiteSpace(options.AudioPath) && !File.Exists(options.AudioPath))
			{
				Form1.showErrorMsgBox("Couldn't find audio file.");
				return;
			}
			try
			{
				parent.openSourceFiles(options);
				DialogResult = DialogResult.OK;
				Hide();
			}
			catch (FileFormatException ex)
			{
				Form1.showErrorMsgBox(ex.Message);
			}
		}

		private void SourceFileForm_VisibleChanged(object sender, EventArgs e)
		{
			
		}

		private void SourceFileForm_Load(object sender, EventArgs e)
		{
			Ok.Focus();
		}

		protected void createFormatFilter(string description, string[] formatList)
		{
			string formatString = "";
			foreach (string format in formatList)
				formatString += $"*.{format}; ";
			formatString.Substring(0, formatString.Length - 3);
			openNoteFileDlg.Filter = $"{description} ({formatString})|{formatString}";
		}
	}

	[Serializable()]
	public class ImportOptions : Midi.ImportOptions, ISerializable
	{
		SourceFileForm _importForm;
		public SourceFileForm ImportForm => _importForm;

		new public Midi.FileType NoteFileType
		{
			get => base.NoteFileType;
			set
			{
				base.NoteFileType = value;
				if (value == Midi.FileType.Midi)
					_importForm = Form1.ImportMidiForm;
				else if (value == Midi.FileType.Mod)
					_importForm = Form1.ImportModForm;
				else if (value == Midi.FileType.Sid)
					_importForm = Form1.ImportSidForm;
			}
		}

		string rawNotePath;
		public string RawNotePath
		{
			get => rawNotePath; //Note path as entered at import, either local file or url.
			set
			{
				rawNotePath = value;
				if (value.IsUrl())
					base.NotePath = value.downloadFile();
				else
					base.NotePath = value;
			}
		}

		new public string NotePath
		{
			get => base.NotePath;
		}

		public bool EraseCurrent;
		public string MixdownAppPath;
		public string MixdownAppArgs;

		public ImportOptions(Midi.FileType noteFileType)
		{
			NoteFileType = noteFileType;
			RawNotePath = ImportForm.NoteFilePath;
			AudioPath = ImportForm.AudioFilePath;
			EraseCurrent = ImportForm.EraseCurrent;
		}

		public ImportOptions(SerializationInfo info, StreamingContext context)
		{
			foreach (var entry in info)
			{
				if (entry.Name == "rawNotePath")
					RawNotePath = (string) entry.Value;
				else if (entry.Name == "audioPath")
					AudioPath = (string) entry.Value;
				else if (entry.Name == "mixdownType")
					MixdownType = (Midi.MixdownType)entry.Value;
				else if (entry.Name == "insTrack")
					InsTrack = (bool)entry.Value;
				else if (entry.Name == "noteFileType")
					NoteFileType = (Midi.FileType)entry.Value;
				else if (entry.Name == "subSong")
					SubSong = (int)entry.Value;
				else if (entry.Name == "numSubSong")
					NumSubSongs = (int)entry.Value;
				else if (entry.Name == "songLengthS")
					SongLengthS = (float)entry.Value;
				else if (entry.Name == "mixdownAppPath")
					MixdownAppPath = (string)entry.Value;
				else if (entry.Name == "mixdownAppArgs")
					MixdownAppArgs = (string)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("rawNotePath", RawNotePath);
			info.AddValue("audioPath", AudioPath);
			info.AddValue("mixdownType", MixdownType);
			info.AddValue("insTrack", InsTrack);
			info.AddValue("noteFileType", NoteFileType);
			info.AddValue("subSong", SubSong);
			info.AddValue("numSubSong", NumSubSongs);
			info.AddValue("songLengthS", SongLengthS);
			info.AddValue("mixdownAppPath", MixdownAppPath);
			info.AddValue("mixdownAppArgs", MixdownAppArgs);
		}

		public void updateImportForm()
		{
			ImportForm.NoteFilePath = RawNotePath;
			ImportForm.AudioFilePath = AudioPath;
			if (ImportForm.GetType() == typeof(ImportModForm))
				((ImportModForm)ImportForm).InsTrack = InsTrack;
		}

		public bool checkNoteFile()
		{
			if (string.IsNullOrWhiteSpace(NotePath))
			{
				MessageBox.Show("Note file path required.");
				return false;
			}
			else if (!File.Exists(NotePath))
			{
				MessageBox.Show("Note file not found.");
				return false;
			}
			return true;
		}
	}
}
