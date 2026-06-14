using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using VisualMusic.Controls;

namespace VisualMusic
{
    [DataContract(Name = "FileType", Namespace = "http://schemas.datacontract.org/2004/07/Midi")]
    public enum FileType
    {
        [EnumMember] Midi,
        [EnumMember] Mod,
        [EnumMember] Sid,
        [EnumMember] Hvl
    }

    [DataContract(Name = "MixdownType", Namespace = "http://schemas.datacontract.org/2004/07/Midi")]
    public enum MixdownType
    {
        [EnumMember] None,
        [EnumMember] Tparty,
        [EnumMember] Internal
    }

    [Serializable]
    public class ImportOptions : ISerializable
    {
        public FileType NoteFileType { get; set; }

        string _rawNotePath;
        public string RawNotePath
        {
            get => _rawNotePath;
            set => _rawNotePath = value;
        }

        public string NotePath
        {
            get;
            private set;
        }

        public string AudioPath;
        public MixdownType MixdownType;
        public bool InsTrack;
        public int SubSong;
        public int NumSubSongs;
        public float SongLengthS;

        public bool EraseCurrent { get; set; }
        public string MixdownAppPath { get; set; }
        public string MixdownAppArgs { get; set; }
        public string MixdownOutputDir { get; set; }
        public string MidiOutputPath { get; set; }
        public bool SavedMidi { get; set; }

        public ImportOptions(FileType noteFileType)
        {
            NoteFileType = noteFileType;
        }

        public ImportOptions(SerializationInfo info, StreamingContext context)
        {
            foreach (var entry in info)
            {
                if (entry.Name == "rawNotePath")
                    RawNotePath = (string)entry.Value;
                else if (entry.Name == "audioPath")
                    AudioPath = (string)entry.Value;
                else if (entry.Name == "mixdownType")
                    MixdownType = (MixdownType)entry.Value;
                else if (entry.Name == "insTrack")
                    InsTrack = (bool)entry.Value;
                else if (entry.Name == "noteFileType")
                    NoteFileType = (FileType)entry.Value;
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
                    string dir = (string)entry.Value;
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        dir = dir.ToLowerInvariant();
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
            if (MixdownType == MixdownType.None)
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

        public void UpdateImportForm()
        {
            ImportSongWindow.UpdateSession(
                NoteFileType,
                erase: false,
                notePath: RawNotePath,
                audioPath: MixdownType == MixdownType.None ? AudioPath : "",
                insTrack: InsTrack);
        }

        public void CheckSourceFile()
        {
            if (!SavedMidi)
            {
                if (string.IsNullOrWhiteSpace(NotePath) || !File.Exists(NotePath))
                    throw new FileImportException("Note file not found", ImportError.Missing, ImportFileType.Note, NotePath);
            }
            else if (!File.Exists(MidiOutputPath))
            {
                throw new FileImportException("Note file not found", ImportError.Missing, ImportFileType.Note, MidiOutputPath);
            }

            if (!string.IsNullOrWhiteSpace(AudioPath))
            {
                if (!File.Exists(AudioPath))
                    throw new FileImportException("Audio file not found", ImportError.Missing, ImportFileType.Audio, AudioPath);
                if (!Media.OpenAudioFile(AudioPath))
                    throw new FileImportException("Couldn't read audio file", ImportError.Corrupt, ImportFileType.Audio, AudioPath);
            }
        }

        public void SetNotePath()
        {
            if (SavedMidi)
                return;

            if (string.IsNullOrWhiteSpace(_rawNotePath))
            {
                NotePath = _rawNotePath;
                return;
            }

            if (_rawNotePath.IsUrl())
            {
                NotePath = _rawNotePath.DownloadFile();
                if (string.IsNullOrEmpty(NotePath))
                    throw new FileImportException("", ImportError.Missing, ImportFileType.Note, _rawNotePath);
            }
            else
            {
                NotePath = _rawNotePath;
            }
        }
    }

    [Serializable]
    class MidiImportOptions : ImportOptions
    {
        public MidiImportOptions() : base(FileType.Midi)
        {
            MixdownType = MidMix.SfLoaded() ? MixdownType.Internal : MixdownType.None;
        }

        public MidiImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    class ModImportOptions : ImportOptions
    {
        public ModImportOptions() : base(FileType.Mod)
        {
            MixdownType = MixdownType.Internal;
        }

        public ModImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    class SidImportOptions : ImportOptions
    {
        public SidImportOptions() : base(FileType.Sid)
        {
            MixdownType = MixdownType.Internal;
        }

        public SidImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    class HvlImportOptions : ImportOptions
    {
        public HvlImportOptions() : base(FileType.Hvl)
        {
            MixdownType = MixdownType.Internal;
        }

        public HvlImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    static class ExternalMixdown
    {
        public static string Run(ImportOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.MixdownAppPath)
                || string.IsNullOrWhiteSpace(options.MixdownOutputDir)
                || !Directory.Exists(options.MixdownOutputDir))
            {
                MessageBox.Show("The configured audio mixdown application or output folder could not be found.",
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            DateTime startedAt = DateTime.UtcNow;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = options.MixdownAppPath,
                    Arguments = options.MixdownAppArgs?.Replace("%notefilepath", options.NotePath),
                    UseShellExecute = false,
                };

                using var process = Process.Start(startInfo);
                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            string outputFile = Directory.GetFiles(options.MixdownOutputDir, "*.wav")
                .Select(path => new FileInfo(path))
                .Where(file => file.LastWriteTimeUtc >= startedAt)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Select(file => file.FullName)
                .FirstOrDefault();

            if (outputFile == null)
            {
                MessageBox.Show("Couldn't find audio mixdown at " + options.MixdownOutputDir,
                    Program.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            File.Delete(Program.MixdownPath);
            File.Move(outputFile, Program.MixdownPath);
            return Program.MixdownPath;
        }
    }
}
