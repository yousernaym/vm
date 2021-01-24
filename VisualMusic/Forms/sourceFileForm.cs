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

namespace VisualMusic
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
		public bool InsTrack
		{
			get => insTrackBtn.Checked;
			set
			{
				if (value)
					insTrackBtn.Checked = true;
				else
					chTrackBtn.Checked = true;
			}
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
			if (options.MixdownType == Midi.MixdownType.None && !string.IsNullOrWhiteSpace(options.AudioPath))
			{
				//Audio file was specified. Make sure the path is correct and the file is a valid audio file, otherwise abort import and let user try again.
				if (!File.Exists(options.AudioPath))
				{
					//Audio file not found
					Form1.showErrorMsgBox("Couldn't find audio file.");
					return;
				}
				if (!Media.openAudioFile(options.AudioPath))
				{
					//Not a valid audio file
					Form1.showErrorMsgBox("Couldn't read audio file.");
					return;
				}
			}
			try { options.checkNoteFile(); }
			catch (Exception ex) when (ex is FileNotFoundException || ex is ArgumentException) 
			{
				Form1.showErrorMsgBox(ex.Message);
				return;
			}
			try
			{
				if (!parent.openSourceFiles(options))
					return;
			}
			catch (FileFormatException ex)
			{
				Form1.showErrorMsgBox("Couldn't read note file.\n" + ex.Message);
				return;
			}
			DialogResult = DialogResult.OK;
			Hide();
		}

		private void SourceFileForm_VisibleChanged(object sender, EventArgs e)
		{
			
		}

		private void SourceFileForm_Load(object sender, EventArgs e)
		{
			Ok.Focus();
		}

		protected void createNoteFormatFilter(string description, string[] formatList)
		{
			string formatString = "";
			foreach (string format in formatList)
				formatString += $"*.{format}; ";
			formatString.Substring(0, formatString.Length - 3);
			openNoteFileDlg.Filter = $"{description} ({formatString})|{formatString}" +
				$"|All files (*.*)|*.*";
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
			set => rawNotePath = value;
		}

		new public string NotePath
		{
			get => base.NotePath;
			private set => base.NotePath = value;
		}
		//	set
		//	{
		//		if (value.IsUrl())
		//			NotePath = RawNotePath.downloadFile();
		//		else
		//			NotePath = RawNotePath;
		//	}
		//}
		
		public bool EraseCurrent { get; set; }
		public string MixdownAppPath { get; set; }
		public string MixdownAppArgs { get; set; }
		public string MixdownOutputDir { get; set; }
		public string MidiOutputPath { get; set; }
		public bool SavedMidi { get; set; }

		public ImportOptions(Midi.FileType noteFileType)
		{
			NoteFileType = noteFileType;
			RawNotePath = ImportForm.NoteFilePath;
			setNotePath();
			AudioPath = ImportForm.AudioFilePath;
			EraseCurrent = ImportForm.EraseCurrent;
			InsTrack = ImportForm.InsTrack;
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
				else if (entry.Name == "mixdownOutputDir")
				{
					string dir = ((string)entry.Value);
					if (!string.IsNullOrWhiteSpace(dir))
					{
						dir = dir.ToLower();
						if (dir.Contains(Program.TempDirRoot))
							dir = Program.TempDir;
						MixdownOutputDir = dir;
					}
				}
				else if (entry.Name == "midiOutputPath")
					MidiOutputPath = (string)entry.Value;
				else if (entry.Name == "savedMidi")
					SavedMidi = (bool)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("rawNotePath", RawNotePath);
			if (MixdownType == Midi.MixdownType.None)
				info.AddValue("audioPath", AudioPath);
			info.AddValue("mixdownType", MixdownType);
			info.AddValue("insTrack", InsTrack);
			info.AddValue("noteFileType", NoteFileType);
			info.AddValue("subSong", SubSong);
			info.AddValue("numSubSong", NumSubSongs);
			info.AddValue("songLengthS", SongLengthS);
			info.AddValue("mixdownAppPath", MixdownAppPath);
			info.AddValue("mixdownAppArgs", MixdownAppArgs);
			info.AddValue("mixdownOutputDir", MixdownOutputDir);
			info.AddValue("midiOutputPath", MidiOutputPath);
			info.AddValue("savedMidi", SavedMidi);
		}

		public void updateImportForm()
		{
			//Clear all forms before update
			Form1.ImportMidiForm.NoteFilePath = Form1.ImportMidiForm.AudioFilePath = Form1.ImportModForm.NoteFilePath = Form1.ImportModForm.AudioFilePath = Form1.ImportSidForm.NoteFilePath = Form1.ImportSidForm.AudioFilePath = null;

			string audioPath = MixdownType == Midi.MixdownType.None ? AudioPath : null;
			ImportForm.NoteFilePath = RawNotePath;
			ImportForm.AudioFilePath = audioPath;
			//if (ImportForm.GetType() == typeof(ImportModForm))
			//((ImportModForm)ImportForm).InsTrack = InsTrack;
			ImportForm.InsTrack = InsTrack;

			//If note file was not midi it was converted to midi before imported.
			//If the midi file was saved, it will be loaded next time the project loads, and the MidiImportForm should contain the mid/audio paths used in the import so that the user can modify the paths and re-import or whatever.
			if (SavedMidi)
			{
				Form1.ImportMidiForm.NoteFilePath = MidiOutputPath;
				Form1.ImportMidiForm.AudioFilePath = audioPath;
			}
		}

		public void checkNoteFile()
		{
			if (string.IsNullOrWhiteSpace(NotePath))
				throw (new ArgumentException("Note file path is empty."));
			else if (!File.Exists(NotePath))
				throw new FileNotFoundException("Couldn't find note file.", NotePath);
		}

		public void setNotePath()
		{
			if (rawNotePath.IsUrl())
				NotePath = rawNotePath.downloadFile();
			else
				NotePath = rawNotePath;
		}
	}
}
