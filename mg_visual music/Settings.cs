using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace Visual_Music
{
	[Serializable]
	public class Settings : ISerializable
	{
		enum Keys { MidiNoteFolder, ModNoteFolder, SidNoteFolder, MidiAudioFolder, ModAudioFolder, SidAudioFolder, VideoFolder, TextureFolder, ProjectFolder, ModTpartyApp, ModTpartyArgs, ModTpartyOutput, SidTpartyApp, SidTpartyArgs, SidTpartyOutput, HvscDir, TpartyModuleMixdown, TpartySidMixdown, HvscSongLengths, DefaultInsTrack}
		public static readonly string FilePath = Path.Combine(Program.AppDataDir, "settings.xml");
		public static Type[] Types = { typeof(string), typeof(bool), typeof(VideoExportOptions) };

		//public bool DefaultInsTrack { get; set; } = true; //preserve latest import form setting instead

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
				if (entry.Name == getKeyName(Keys.DefaultInsTrack))
					Form1.ImportModForm.InsTrack = (bool)entry.Value;
				else if (entry.Name == getKeyName(Keys.MidiNoteFolder))
					Form1.ImportMidiForm.NoteFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModNoteFolder))
					Form1.ImportModForm.NoteFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidNoteFolder))
					Form1.ImportSidForm.NoteFolder = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.MidiAudioFolder))
					Form1.ImportMidiForm.AudioFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModAudioFolder))
					Form1.ImportModForm.AudioFolder = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidAudioFolder))
					Form1.ImportSidForm.AudioFolder = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.VideoFolder))
					form.saveVideoDlg.InitialDirectory = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.TextureFolder))
					form.openTextureDlg.InitialDirectory = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ProjectFolder))
					form.ProjectFolder = (string)entry.Value;
				else if (entry.Name == "trackPropsFolder")
					form.TrackPropsFolder = (string)entry.Value;
				else if (entry.Name == "camFolder")
					form.CamFolder = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.ModTpartyApp))
					Form1.ImportModForm.tpartyAppTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModTpartyArgs))
					Form1.ImportModForm.tpartyArgsTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.ModTpartyOutput))
					Form1.ImportModForm.tpartyAudioTb.Text = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.SidTpartyApp))
					Form1.ImportSidForm.tpartyAppTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidTpartyArgs))
					Form1.ImportSidForm.tpartyArgsTb.Text = (string)entry.Value;
				else if (entry.Name == getKeyName(Keys.SidTpartyOutput))
					Form1.ImportSidForm.tpartyAudioTb.Text = (string)entry.Value;

				else if (entry.Name == getKeyName(Keys.HvscDir))
				{
					string value = (string)entry.Value;
					if (string.IsNullOrWhiteSpace(value)) //set default dir
						value = Path.Combine(Program.AppDataDir, @"tparty\HVSC");
					Form1.TpartyIntegrationForm.HvscDir = value;
				}
				else if (entry.Name == getKeyName(Keys.TpartyModuleMixdown))
					Form1.TpartyIntegrationForm.ModuleMixdown = (bool)entry.Value;
				else if (entry.Name == getKeyName(Keys.HvscSongLengths))
					Form1.TpartyIntegrationForm.HvscSongLengths = (bool)entry.Value;

				else if (entry.Name == "videoExportOptions")
				{
					VideoExportOptions options = (VideoExportOptions)entry.Value;
					Form1.VidExpForm.updateControls(options);
				}
				else if (entry.Name == "videoExportFormResoIndex")
				{
					if (Form1.VidExpForm.resoComboBox.Items.Count > (int)entry.Value)
						Form1.VidExpForm.resoComboBox.SelectedIndex = (int)entry.Value;
				}
				else if (entry.Name == "videoExportFormSsIndex")
				{
					if (Form1.VidExpForm.ssResoComboBox.Items.Count > (int)entry.Value)
						Form1.VidExpForm.ssResoComboBox.SelectedIndex = (int)entry.Value;
				}
			}

		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			Form1 form = Program.form1;
			info.AddValue(getKeyName(Keys.DefaultInsTrack), Form1.ImportModForm.InsTrack);
			info.AddValue(getKeyName(Keys.MidiNoteFolder), Form1.ImportMidiForm.NoteFolder);
			info.AddValue(getKeyName(Keys.ModNoteFolder), Form1.ImportModForm.NoteFolder);
			info.AddValue(getKeyName(Keys.SidNoteFolder), Form1.ImportSidForm.NoteFolder);
			info.AddValue(getKeyName(Keys.MidiAudioFolder), Form1.ImportMidiForm.AudioFolder);
			info.AddValue(getKeyName(Keys.ModAudioFolder), Form1.ImportModForm.AudioFolder);
			info.AddValue(getKeyName(Keys.SidAudioFolder), Form1.ImportSidForm.AudioFolder);

			info.AddValue(getKeyName(Keys.VideoFolder), form.saveVideoDlg.InitialDirectory);
			info.AddValue(getKeyName(Keys.TextureFolder), form.openTextureDlg.InitialDirectory);
			info.AddValue(getKeyName(Keys.ProjectFolder), form.ProjectFolder);
			info.AddValue("trackPropsFolder", form.TrackPropsFolder);
			info.AddValue("camFolder", form.CamFolder);

			info.AddValue(getKeyName(Keys.ModTpartyApp), Form1.ImportModForm.tpartyAppTb.Text);
			info.AddValue(getKeyName(Keys.ModTpartyArgs), Form1.ImportModForm.tpartyArgsTb.Text);
			info.AddValue(getKeyName(Keys.ModTpartyOutput), Form1.ImportModForm.tpartyAudioTb.Text);
			info.AddValue(getKeyName(Keys.SidTpartyApp), Form1.ImportSidForm.tpartyAppTb.Text);
			info.AddValue(getKeyName(Keys.SidTpartyArgs), Form1.ImportSidForm.tpartyArgsTb.Text);
			info.AddValue(getKeyName(Keys.SidTpartyOutput), Form1.ImportSidForm.tpartyAudioTb.Text);

			info.AddValue(getKeyName(Keys.HvscDir), Form1.TpartyIntegrationForm.HvscDir);
			info.AddValue(getKeyName(Keys.TpartyModuleMixdown), Form1.TpartyIntegrationForm.ModuleMixdown);
			info.AddValue(getKeyName(Keys.HvscSongLengths), Form1.TpartyIntegrationForm.HvscSongLengths);

			info.AddValue("videoExportOptions", Form1.VidExpForm.Options);
			info.AddValue("videoExportFormResoIndex", Form1.VidExpForm.resoComboBox.SelectedIndex);
			info.AddValue("videoExportFormSsIndex", Form1.VidExpForm.ssResoComboBox.SelectedIndex);
		}
	}
}
