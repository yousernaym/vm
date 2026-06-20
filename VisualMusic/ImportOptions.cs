using System;
using System.IO;
using System.Runtime.Serialization;
using VisualMusic.Controls;

namespace VisualMusic
{
    public enum FileType
    {
        [EnumMember] Midi,
        [EnumMember] Mod,
        [EnumMember] Sid,
        [EnumMember] Hvl
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
        internal string GeneratedAudioPath { get; set; }
        public bool HasSuppliedAudio => !string.IsNullOrWhiteSpace(AudioPath);
        public bool InsTrack;
        public int SubSong;
        public int NumSubSongs;
        public float SongLengthS;

        public bool EraseCurrent { get; set; }
        public string MidiOutputPath { get; set; }
        public bool SavedMidi { get; set; }

        /// <summary>
        /// Human-readable source file name for window titles and progress dialogs.
        /// Prefers the resolved local file so URL imports show the downloaded name
        /// (e.g. "song.mod"), not the full download URL.
        /// </summary>
        public string DisplayName
        {
            get
            {
                string p = !string.IsNullOrWhiteSpace(NotePath) ? NotePath
                         : SavedMidi ? MidiOutputPath
                         : RawNotePath;
                if (string.IsNullOrWhiteSpace(p)) return "";
                if (p.IsUrl())
                {
                    try { return Path.GetFileName(new Uri(p).LocalPath); } catch { return p; }
                }
                return Path.GetFileName(p);
            }
        }

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
                else if (entry.Name == "midiOutputPath")
                    MidiOutputPath = (string)entry.Value;
                else if (entry.Name == "savedMidi")
                    SavedMidi = (bool)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("rawNotePath", RawNotePath);
            if (HasSuppliedAudio)
                info.AddValue("audioPath", AudioPath);
            info.AddValue("insTrack", InsTrack);
            info.AddValue("noteFileType", NoteFileType);
            info.AddValue("subSong", SubSong);
            info.AddValue("numSubSong", NumSubSongs);
            info.AddValue("songLengthS", SongLengthS);
            info.AddValue("midiOutputPath", MidiOutputPath);
            info.AddValue("savedMidi", SavedMidi);
        }

        public void UpdateImportForm()
        {
            ImportSongWindow.UpdateSession(
                NoteFileType,
                erase: EraseCurrent,
                notePath: RawNotePath,
                audioPath: AudioPath ?? "",
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
        }

        public HvlImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
