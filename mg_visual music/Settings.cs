using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace Visual_Music
{
	[Serializable]
	class Settings : ISerializable
	{
		enum Keys { MidiNoteFolder, ModNoteFolder, SidNoteFolder, MidiAudioFolder, ModAudioFolder, SidAudioFolder, VideoFolder, TextureFolder, ProjectFolder, ModTpartyApp, ModTpartyArgs, ModTpartyOutput, SidTpartyApp, SidTpartyArgs, SidTpartyOutput, HvscDir, TpartyModuleMixdown, TpartySidMixdown, HvscSongLengths }
		public const string Filename = "settings.xml";
		public static Type[] Types = { typeof(string) };
		string getKeyName(Keys key)
		{
			return Enum.GetName(typeof(Keys), key);
		}
		public Settings()
		{
		}
		public Settings(SerializationInfo info, StreamingContext context)
		{
			Form1 form = Program.form1;
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == getKeyName(Keys.MidiNoteFolder))
					form.importMidiForm.NoteFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModNoteFolder))
					form.importModForm.NoteFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidNoteFolder))
					form.importSidForm.NoteFolder = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.MidiAudioFolder))
					form.importMidiForm.AudioFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModAudioFolder))
					form.importModForm.AudioFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidAudioFolder))
					form.importSidForm.AudioFolder = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.VideoFolder))
					form.saveVideoDlg.InitialDirectory = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.TextureFolder))
					form.openTextureDlg.InitialDirectory = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ProjectFolder))
					form.ProjectFolder = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.ModTpartyApp))
					form.importModForm.tpartyAppTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModTpartyArgs))
					form.importModForm.tpartyArgsTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModTpartyOutput))
					form.importModForm.tpartyAudioTb.Text = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.SidTpartyApp))
					form.importSidForm.tpartyAppTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidTpartyArgs))
					form.importSidForm.tpartyArgsTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidTpartyOutput))
					form.importSidForm.tpartyAudioTb.Text = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.HvscDir))
					form.tpartyIntegrationForm.HvscDir = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.TpartyModuleMixdown))
					form.tpartyIntegrationForm.ModuleMixdown = (bool)entry.Value;
				else if (entry.Name == getKeyName(Keys.TpartySidMixdown))
					form.tpartyIntegrationForm.SidMixdown = (bool)entry.Value;
				else if (entry.Name == getKeyName(Keys.HvscSongLengths))
					form.tpartyIntegrationForm.HvscSongLengths = (bool)entry.Value;
			}

		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			Form1 form = Program.form1;
			info.AddValue(getKeyName(Keys.MidiNoteFolder), form.importMidiForm.NoteFolder);
			info.AddValue(getKeyName(Keys.ModNoteFolder), form.importModForm.NoteFolder);
			info.AddValue(getKeyName(Keys.SidNoteFolder), form.importSidForm.NoteFolder);
			info.AddValue(getKeyName(Keys.MidiAudioFolder), form.importMidiForm.AudioFolder);
			info.AddValue(getKeyName(Keys.ModAudioFolder), form.importModForm.AudioFolder);
			info.AddValue(getKeyName(Keys.SidAudioFolder), form.importSidForm.AudioFolder);

			info.AddValue(getKeyName(Keys.VideoFolder), form.saveVideoDlg.InitialDirectory);
			info.AddValue(getKeyName(Keys.TextureFolder), form.openTextureDlg.InitialDirectory);
			info.AddValue(getKeyName(Keys.ProjectFolder), form.ProjectFolder);

			info.AddValue(getKeyName(Keys.ModTpartyApp), form.importModForm.tpartyAppTb.Text);
			info.AddValue(getKeyName(Keys.ModTpartyArgs), form.importModForm.tpartyArgsTb.Text);
			info.AddValue(getKeyName(Keys.ModTpartyOutput), form.importModForm.tpartyAudioTb.Text);
			info.AddValue(getKeyName(Keys.SidTpartyApp), form.importSidForm.tpartyAppTb.Text);
			info.AddValue(getKeyName(Keys.SidTpartyArgs), form.importSidForm.tpartyArgsTb.Text);
			info.AddValue(getKeyName(Keys.SidTpartyOutput), form.importSidForm.tpartyAudioTb.Text);

			info.AddValue(getKeyName(Keys.HvscDir), form.tpartyIntegrationForm.HvscDir);
			info.AddValue(getKeyName(Keys.TpartyModuleMixdown), form.tpartyIntegrationForm.ModuleMixdown);
			info.AddValue(getKeyName(Keys.TpartySidMixdown), form.tpartyIntegrationForm.SidMixdown);
			info.AddValue(getKeyName(Keys.HvscSongLengths), form.tpartyIntegrationForm.HvscSongLengths);
		}
	}
}
