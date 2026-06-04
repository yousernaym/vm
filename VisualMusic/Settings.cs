using System;
using System.IO;
using System.Runtime.Serialization;

namespace VisualMusic
{
    [Serializable]
    public class Settings : ISerializable
    {
        enum Keys { MidiNoteFolder, ModNoteFolder, SidNoteFolder, MidiAudioFolder, ModAudioFolder, SidAudioFolder, VideoFolder, TextureFolder, ProjectFolder, ModTpartyApp, ModTpartyArgs, ModTpartyOutput, SidTpartyApp, SidTpartyArgs, SidTpartyOutput, TpartyModuleMixdown, TpartySidMixdown, SongLengthsUrl, DefaultInsTrack }
        public static readonly string FilePath = Path.Combine(Program.AppDataDir, "settings.xml");
        public static Type[] Types = { typeof(string), typeof(bool), typeof(VideoExportOptions), typeof(AVCodecID) };

        //public bool DefaultInsTrack { get; set; } = true; //preserve latest import form setting instead

        string GetKeyName(Keys key)
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
                if (entry.Name == GetKeyName(Keys.DefaultInsTrack))
                    Form1.ImportModForm.InsTrack = (bool)entry.Value;
                else if (entry.Name == GetKeyName(Keys.MidiNoteFolder))
                    Form1.ImportMidiForm.NoteFolder = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.ModNoteFolder))
                    Form1.ImportModForm.NoteFolder = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.SidNoteFolder))
                    Form1.ImportSidForm.NoteFolder = (string)entry.Value;

                else if (entry.Name == GetKeyName(Keys.MidiAudioFolder))
                    Form1.ImportMidiForm.AudioFolder = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.ModAudioFolder))
                    Form1.ImportModForm.AudioFolder = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.SidAudioFolder))
                    Form1.ImportSidForm.AudioFolder = (string)entry.Value;

                else if (entry.Name == GetKeyName(Keys.VideoFolder))
                    form.saveVideoDlg.InitialDirectory = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.TextureFolder))
                    form.openTextureDlg.InitialDirectory = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.ProjectFolder))
                    form.ProjectFolder = (string)entry.Value;
                else if (entry.Name == "trackPropsFolder")
                    form.TrackPropsFolder = (string)entry.Value;
                else if (entry.Name == "camFolder")
                    form.CamFolder = (string)entry.Value;

                else if (entry.Name == GetKeyName(Keys.ModTpartyApp))
                    Form1.ImportModForm.tpartyAppTb.Text = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.ModTpartyArgs))
                    Form1.ImportModForm.tpartyArgsTb.Text = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.ModTpartyOutput))
                    Form1.ImportModForm.tpartyOutputTb.Text = (string)entry.Value;

                else if (entry.Name == GetKeyName(Keys.SidTpartyApp))
                    Form1.ImportSidForm.tpartyAppTb.Text = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.SidTpartyArgs))
                    Form1.ImportSidForm.tpartyArgsTb.Text = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.SidTpartyOutput))
                    Form1.ImportSidForm.tpartyOutputTb.Text = (string)entry.Value;

                else if (entry.Name == GetKeyName(Keys.SongLengthsUrl))
                    Form1.TpartyIntegrationForm.SongLengthsUrl = (string)entry.Value;
                else if (entry.Name == GetKeyName(Keys.TpartyModuleMixdown))
                    Form1.TpartyIntegrationForm.ModuleMixdown = (bool)entry.Value;
                else if (entry.Name == "videoExportOptions")
                    Form1.VidExpForm.SetOptions((VideoExportOptions)entry.Value);
                else if (entry.Name == "backgroundImageFolder")
                    form.BackgroundImageFolder = (string)entry.Value;
                else if (entry.Name == "trackAudioFolder")
                    form.TrackAudioFolder = (string)entry.Value;
            }
            Form1.TpartyIntegrationForm.DownloadSonglengths(true);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Form1 form = Program.form1;
            info.AddValue(GetKeyName(Keys.DefaultInsTrack), Form1.ImportModForm.InsTrack);
            info.AddValue(GetKeyName(Keys.MidiNoteFolder), Form1.ImportMidiForm.NoteFolder);
            info.AddValue(GetKeyName(Keys.ModNoteFolder), Form1.ImportModForm.NoteFolder);
            info.AddValue(GetKeyName(Keys.SidNoteFolder), Form1.ImportSidForm.NoteFolder);
            info.AddValue(GetKeyName(Keys.MidiAudioFolder), Form1.ImportMidiForm.AudioFolder);
            info.AddValue(GetKeyName(Keys.ModAudioFolder), Form1.ImportModForm.AudioFolder);
            info.AddValue(GetKeyName(Keys.SidAudioFolder), Form1.ImportSidForm.AudioFolder);

            info.AddValue(GetKeyName(Keys.VideoFolder), form.saveVideoDlg.InitialDirectory);
            info.AddValue(GetKeyName(Keys.TextureFolder), form.openTextureDlg.InitialDirectory);
            info.AddValue(GetKeyName(Keys.ProjectFolder), form.ProjectFolder);
            info.AddValue("trackPropsFolder", form.TrackPropsFolder);
            info.AddValue("camFolder", form.CamFolder);

            info.AddValue(GetKeyName(Keys.ModTpartyApp), Form1.ImportModForm.tpartyAppTb.Text);
            info.AddValue(GetKeyName(Keys.ModTpartyArgs), Form1.ImportModForm.tpartyArgsTb.Text);
            info.AddValue(GetKeyName(Keys.ModTpartyOutput), Form1.ImportModForm.tpartyOutputTb.Text);
            info.AddValue(GetKeyName(Keys.SidTpartyApp), Form1.ImportSidForm.tpartyAppTb.Text);
            info.AddValue(GetKeyName(Keys.SidTpartyArgs), Form1.ImportSidForm.tpartyArgsTb.Text);
            info.AddValue(GetKeyName(Keys.SidTpartyOutput), Form1.ImportSidForm.tpartyOutputTb.Text);
            info.AddValue(GetKeyName(Keys.SongLengthsUrl), Form1.TpartyIntegrationForm.SongLengthsUrl);
            info.AddValue(GetKeyName(Keys.TpartyModuleMixdown), Form1.TpartyIntegrationForm.ModuleMixdown);
            info.AddValue("videoExportOptions", Form1.VidExpForm.Options);
            info.AddValue("backgroundImageFolder", form.BackgroundImageFolder);
            info.AddValue("trackAudioFolder", form.TrackAudioFolder);
        }
    }
}
