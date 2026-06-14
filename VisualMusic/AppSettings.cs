using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace VisualMusic
{
    /// <summary>
    /// Persistent settings (file-dialog folders, etc.).
    /// Serialized to %AppData%\Visual Music\appsettings.xml via DataContractSerializer.
    /// </summary>
    [DataContract]
    public class AppSettings
    {
        static readonly string s_filePath = Path.Combine(Program.AppDataDir, "appsettings.xml");
        
        // ---- Default theme - Also change in app.xaml.
        const string DefaultThemeBaseColor = "Dark";
        const string DefaultThemeColorScheme = "Teal";

        // ---- Singleton ----
        static AppSettings s_instance;
        public static AppSettings Instance => s_instance ??= Load();

        // ---- Scalar dialog folders ----
        [DataMember] public string ProjectFolder  { get; set; }
        [DataMember] public string CamFolder      { get; set; }
        [DataMember] public string TrackPropsFolder { get; set; }
        [DataMember] public string TextureFolder  { get; set; }
        [DataMember] public string TrackAudioFolder { get; set; }
        [DataMember] public string BackgroundFolder { get; set; }

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

        // ---- Theme ----

        [DataMember] public string ThemeBaseColor   { get; set; }  // e.g. "Dark" or "Light"
        [DataMember] public string ThemeColorScheme { get; set; }  // e.g. "Steel", "Blue", ...

        public string ThemeBaseColorOrDefault
            => string.IsNullOrEmpty(ThemeBaseColor)   ? DefaultThemeBaseColor  : ThemeBaseColor;
        public string ThemeColorSchemeOrDefault
            => string.IsNullOrEmpty(ThemeColorScheme) ? DefaultThemeColorScheme : ThemeColorScheme;

        // ---- HVSC song-length DB ----

        /// <summary>Persisted URL for the HVSC Songlengths.md5 database download.</summary>
        [DataMember] public string SongLengthsUrl { get; set; }

        /// <summary>Returns the persisted URL, falling back to the built-in default.</summary>
        public string SongLengthsUrlOrDefault
            => string.IsNullOrEmpty(SongLengthsUrl) ? Hvsc.DefaultSongLengthsUrl : SongLengthsUrl;

        // ---- Video export ----

        [DataMember] public string VideoFolder         { get; set; }
        [DataMember] public bool   VideoSphere         { get; set; }
        [DataMember] public bool   VideoSphericalMetadata { get; set; }
        [DataMember] public bool   VideoSphericalStereo   { get; set; }
        [DataMember] public int    VideoSphereResoIndex    { get; set; }
        [DataMember] public int    VideoNonSphereResoIndex { get; set; }
        [DataMember] public int    VideoSsaaFactor    { get; set; }
        [DataMember] public int?   VideoQualityLoss   { get; set; }
        [DataMember] public float  VideoFps           { get; set; }

        public string VideoFolderOrDefault
            => string.IsNullOrEmpty(VideoFolder)
               ? Path.Combine(Program.DefaultUserFilesDir, "Videos")
               : VideoFolder;

        /// <summary>Reconstruct a <see cref="VideoExportOptions"/> from persisted scalars.</summary>
        public VideoExportOptions LoadVideoExportOptions()
        {
            var o = new VideoExportOptions();
            // ResoIndex setter routes by Sphere, so set the non-sphere index first.
            o.Sphere = false; o.ResoIndex = VideoNonSphereResoIndex;
            o.Sphere = true;  o.ResoIndex = VideoSphereResoIndex;
            o.Sphere            = VideoSphere;
            o.SphericalMetadata = VideoSphericalMetadata;
            o.SphericalStereo   = VideoSphericalStereo;
            o.SSAAFactor        = VideoSsaaFactor > 0 ? VideoSsaaFactor : 4;
            o.VideoQualityLoss  = VideoQualityLoss ?? 1;
            o.Fps               = VideoFps > 0 ? VideoFps : 60f;
            return o;
        }

        /// <summary>Persist <paramref name="o"/> as scalars and save to disk.</summary>
        public void SaveVideoExportOptions(VideoExportOptions o)
        {
            VideoSphere             = o.Sphere;
            VideoSphericalMetadata  = o.SphericalMetadata;
            VideoSphericalStereo    = o.SphericalStereo;
            bool origSphere = o.Sphere;
            o.Sphere = true;  VideoSphereResoIndex    = o.ResoIndex;
            o.Sphere = false; VideoNonSphereResoIndex = o.ResoIndex;
            o.Sphere = origSphere;
            VideoSsaaFactor    = o.SSAAFactor;
            VideoQualityLoss   = o.VideoQualityLoss;
            VideoFps           = o.Fps;
            Save();
        }

        // ---- Per-file-type track-split preference ----

        // true = one track per instrument (MIDI: per track chunk).
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
            if (File.Exists(s_filePath))
            {
                try
                {
                    var dcs = new DataContractSerializer(typeof(AppSettings));
                    using var stream = File.Open(s_filePath, FileMode.Open);
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
                using var stream = File.Open(s_filePath, FileMode.Create);
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
