using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace VisualMusic
{
    /// <summary>
    /// WPF-path persistent settings (file-dialog folders, etc.).
    /// Serialized to %AppData%\Visual Music\appsettings.xml via DataContractSerializer.
    /// Leave legacy Settings.cs / Form1 untouched.
    /// </summary>
    [DataContract]
    public class AppSettings
    {
        static readonly string FilePath = Path.Combine(Program.AppDataDir, "appsettings.xml");

        // ---- Singleton ----

        static AppSettings _instance;
        public static AppSettings Instance => _instance ??= Load();

        // ---- Scalar dialog folders ----

        [DataMember] public string ProjectFolder  { get; set; }
        [DataMember] public string CamFolder      { get; set; }
        [DataMember] public string TrackPropsFolder { get; set; }
        [DataMember] public string TextureFolder  { get; set; }
        [DataMember] public string TrackAudioFolder { get; set; }

        // ---- Per-file-type import folders ----

        [DataMember] Dictionary<Midi.FileType, string> _noteFolders  = new();
        [DataMember] Dictionary<Midi.FileType, string> _audioFolders = new();

        public string GetNoteFolder(Midi.FileType type)
            => _noteFolders.TryGetValue(type, out var v) ? v : "";

        public void SetNoteFolder(Midi.FileType type, string dir)
            => _noteFolders[type] = dir;

        public string GetAudioFolder(Midi.FileType type)
            => _audioFolders.TryGetValue(type, out var v) ? v : "";

        public void SetAudioFolder(Midi.FileType type, string dir)
            => _audioFolders[type] = dir;

        // ---- HVSC song-length DB ----

        /// <summary>Persisted URL for the HVSC Songlengths.md5 database download.</summary>
        [DataMember] public string SongLengthsUrl { get; set; }

        /// <summary>Returns the persisted URL, falling back to the built-in default.</summary>
        public string SongLengthsUrlOrDefault
            => string.IsNullOrEmpty(SongLengthsUrl) ? Hvsc.DefaultSongLengthsUrl : SongLengthsUrl;

        // ---- Per-file-type track-split preference ----

        // true = one track per instrument (MIDI: per track chunk) — the default, matching old WinForms
        // false = one track per channel (MIDI: per MIDI channel)
        [DataMember] Dictionary<Midi.FileType, bool> _insTrack = new();

        public bool GetInsTrack(Midi.FileType type) => _insTrack.TryGetValue(type, out var v) ? v : true;
        public void SetInsTrack(Midi.FileType type, bool v) => _insTrack[type] = v;

        // ---- Defaults (applied when a stored value is null/empty) ----

        public string ProjectFolderOrDefault
            => string.IsNullOrEmpty(ProjectFolder)
               ? Path.Combine(Program.DefaultUserFilesDir, "Projects")
               : ProjectFolder;

        public string CamFolderOrDefault
            => string.IsNullOrEmpty(CamFolder)
               ? Path.Combine(Program.DefaultUserFilesDir, "Props")
               : CamFolder;

        public string TrackPropsFolderOrDefault
            => string.IsNullOrEmpty(TrackPropsFolder)
               ? Path.Combine(Program.DefaultUserFilesDir, "Props")
               : TrackPropsFolder;

        // ---- Deserialization init ----
        // DataContractSerializer bypasses the constructor, so field initializers (= new()) don't run
        // for fields that are absent from the XML (e.g. newly added fields on an old file).
        [OnDeserialized]
        void OnDeserialized(StreamingContext _)
        {
            _noteFolders  ??= new();
            _audioFolders ??= new();
            _insTrack     ??= new();
        }

        // ---- Persistence ----

        static AppSettings Load()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    var dcs = new DataContractSerializer(typeof(AppSettings));
                    using var stream = File.Open(FilePath, FileMode.Open);
                    return (AppSettings)dcs.ReadObject(stream);
                }
                catch { /* corrupt / version mismatch — start fresh */ }
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Program.AppDataDir);
                var dcs = new DataContractSerializer(typeof(AppSettings));
                using var stream = File.Open(FilePath, FileMode.Create);
                dcs.WriteObject(stream, this);
            }
            catch { /* best-effort */ }
        }

        /// <summary>
        /// Convenience helper: extracts the directory from <paramref name="filePath"/>,
        /// calls <paramref name="setter"/> with it, then saves.
        /// </summary>
        public void RememberFolder(string filePath, Action<string> setter)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
            {
                setter(dir);
                Save();
            }
        }
    }
}
