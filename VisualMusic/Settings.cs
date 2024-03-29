﻿using System;
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
                    Form1.ImportModForm.tpartyOutputTb.Text = (string)entry.Value;

                else if (entry.Name == getKeyName(Keys.SidTpartyApp))
                    Form1.ImportSidForm.tpartyAppTb.Text = (string)entry.Value;
                else if (entry.Name == getKeyName(Keys.SidTpartyArgs))
                    Form1.ImportSidForm.tpartyArgsTb.Text = (string)entry.Value;
                else if (entry.Name == getKeyName(Keys.SidTpartyOutput))
                    Form1.ImportSidForm.tpartyOutputTb.Text = (string)entry.Value;

                else if (entry.Name == getKeyName(Keys.SongLengthsUrl))
                    Form1.TpartyIntegrationForm.SongLengthsUrl = (string)entry.Value;
                else if (entry.Name == getKeyName(Keys.TpartyModuleMixdown))
                    Form1.TpartyIntegrationForm.ModuleMixdown = (bool)entry.Value;

                else if (entry.Name == "videoExportOptions")
                    Form1.VidExpForm.setOptions((VideoExportOptions)entry.Value);
            }
            Form1.TpartyIntegrationForm.downloadSonglengths(true);
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
            info.AddValue(getKeyName(Keys.ModTpartyOutput), Form1.ImportModForm.tpartyOutputTb.Text);
            info.AddValue(getKeyName(Keys.SidTpartyApp), Form1.ImportSidForm.tpartyAppTb.Text);
            info.AddValue(getKeyName(Keys.SidTpartyArgs), Form1.ImportSidForm.tpartyArgsTb.Text);
            info.AddValue(getKeyName(Keys.SidTpartyOutput), Form1.ImportSidForm.tpartyOutputTb.Text);

            info.AddValue(getKeyName(Keys.SongLengthsUrl), Form1.TpartyIntegrationForm.SongLengthsUrl);
            info.AddValue(getKeyName(Keys.TpartyModuleMixdown), Form1.TpartyIntegrationForm.ModuleMixdown);

            info.AddValue("videoExportOptions", Form1.VidExpForm.Options);
        }
    }
}
