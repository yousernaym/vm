using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VisualMusic
{
    using GdiPoint = System.Drawing.Point;
    public enum SourceSongType { Midi, Mod, Sid };

    [Serializable()]
    public class Project : ISerializable, IDisposable
    {
        // ---- Per-property interpolation accessor registry (not serialized) ----
        // Populated by InitPropertyAccessors() after track views are created.

        private sealed class PropAccessor
        {
            public readonly Func<Keyframes.KfValue> Capture;
            public readonly Action<Keyframes.KfValue> Apply;
            public readonly Func<Keyframes.KfValue, Keyframes.KfValue, double, Keyframes.KfInterpolation, Keyframes.KfValue> Interp;
            public readonly bool NeedsRebuild;
            /// <summary>Enum/bool props that must step (never blend) regardless of the keyframe's mode.</summary>
            public readonly bool AlwaysHold;

            PropAccessor(Func<Keyframes.KfValue> capture, Action<Keyframes.KfValue> apply,
                         Func<Keyframes.KfValue, Keyframes.KfValue, double, Keyframes.KfInterpolation, Keyframes.KfValue> interp,
                         bool needsRebuild = false, bool alwaysHold = false)
            { Capture = capture; Apply = apply; Interp = interp; NeedsRebuild = needsRebuild; AlwaysHold = alwaysHold; }

            /// <summary>Factory for a scalar (double) property.  <paramref name="logScale"/> interpolates in log2 space.</summary>
            public static PropAccessor Scalar(Func<double> get, Action<double> set,
                                              bool logScale = false, bool needsRebuild = false, bool alwaysHold = false)
                => new PropAccessor(
                    () => new Keyframes.ScalarKfValue(get()),
                    v => set(((Keyframes.ScalarKfValue)v).V),
                    (a, b, t, mode) => new Keyframes.ScalarKfValue(
                        Keyframes.PropertyKeyframeTrack.InterpolateValue(
                            ((Keyframes.ScalarKfValue)a).V, ((Keyframes.ScalarKfValue)b).V, t, mode, logScale)),
                    needsRebuild, alwaysHold);

            /// <summary>Factory for an RGBA color property.  Always interpolated per-channel.</summary>
            public static PropAccessor Color(Func<Microsoft.Xna.Framework.Color> get,
                                             Action<Microsoft.Xna.Framework.Color> set,
                                             bool alwaysHold = false)
                => new PropAccessor(
                    () => new Keyframes.ColorKfValue(get()),
                    v => set(((Keyframes.ColorKfValue)v).C),
                    (a, b, t, mode) => Keyframes.ColorKfValue.Lerp(
                        (Keyframes.ColorKfValue)a, (Keyframes.ColorKfValue)b, t, mode),
                    false, alwaysHold);

            /// <summary>
            /// Factory for a camera (pos + orientation quaternion + FOV) property.
            /// Apply mutates the Camera object in-place (it is a reference type).
            /// </summary>
            public static PropAccessor Camera(Func<VisualMusic.Camera> get)
                => new PropAccessor(
                    () => { var c = get(); return new Keyframes.CameraKfValue(c.Pos, c.Orientation, c.Fov); },
                    v => { var c = get(); var cv = (Keyframes.CameraKfValue)v; c.Pos = cv.Pos; c.Orientation = cv.Orientation; c.Fov = cv.Fov; },
                    (a, b, t, mode) => Keyframes.CameraKfValue.Interpolate(
                        (Keyframes.CameraKfValue)a, (Keyframes.CameraKfValue)b, t, mode));

            /// <summary>
            /// Factory for a string (path) property. The value itself is not blended — interpolation
            /// returns <paramref name="b"/> for Smooth/Linear (the renderer drives the visual crossfade)
            /// and <paramref name="a"/> for Hold (snap/hard-cut).
            /// </summary>
            public static PropAccessor StringHold(Func<string> get, Action<string> set,
                                                  bool alwaysHold = false)
                => new PropAccessor(
                    () => new Keyframes.StringKfValue(get()),
                    v => set(((Keyframes.StringKfValue)v).S),
                    (a, b, t, mode) => mode == Keyframes.KfInterpolation.Hold ? a : b,
                    alwaysHold: alwaysHold);

            /// <summary>Factory for a bool property. Bool keyframes always step instead of blending.</summary>
            public static PropAccessor Bool(Func<bool?> get, Action<bool> set,
                                            bool needsRebuild = false)
                => Scalar(
                    () => get() == true ? 1.0 : 0.0,
                    v => set(v >= 0.5),
                    needsRebuild: needsRebuild,
                    alwaysHold: true);
        }

        readonly Dictionary<string, PropAccessor> _propAccessors = new Dictionary<string, PropAccessor>();

        // ---- Static mod-property table (name → factory that creates a bound PropAccessor for a given entry) ----
        // Evaluated once per class (not per project instance) so it's cheap.

        static readonly Dictionary<string, Func<NoteStyleMod, PropAccessor>> _modPropTable =
            new Dictionary<string, Func<NoteStyleMod, PropAccessor>>
            {
                ["ModXOrigin"] = m => PropAccessor.Scalar(() => m.XOrigin ?? 0.5f, v => m.XOrigin = (float)v),
                ["ModYOrigin"] = m => PropAccessor.Scalar(() => m.YOrigin ?? 0.5f, v => m.YOrigin = (float)v),
                ["ModStart"] = m => PropAccessor.Scalar(() => m.Start ?? 0f, v => m.Start = (float)v),
                ["ModStop"] = m => PropAccessor.Scalar(() => m.Stop ?? 1f, v => m.Stop = (float)v),
                ["ModFadeIn"] = m => PropAccessor.Scalar(() => m.FadeIn ?? 0f, v => m.FadeIn = (float)v),
                ["ModFadeOut"] = m => PropAccessor.Scalar(() => m.FadeOut ?? 0f, v => m.FadeOut = (float)v),
                ["ModPower"] = m => PropAccessor.Scalar(() => m.Power ?? 1f, v => m.Power = (float)v),
                ["ModAngleDest"] = m => PropAccessor.Scalar(() => m.AngleDest ?? 45, v => m.AngleDest = (int)Math.Round(v)),
                ["ModCombineIndex"] = m => PropAccessor.Scalar(() => m.CombineXY ?? 0, v => m.CombineXY = (int)Math.Round(v), alwaysHold: true),
                ["ModColorDest"] = m => PropAccessor.Color(
                    () => m.ColorDest ?? Microsoft.Xna.Framework.Color.White,
                    c => m.ColorDest = c),
                ["ModXOriginEnable"] = m => PropAccessor.Scalar(() => m.XOriginEnable == true ? 1.0 : 0.0, v => m.XOriginEnable = v >= 0.5, alwaysHold: true),
                ["ModYOriginEnable"] = m => PropAccessor.Scalar(() => m.YOriginEnable == true ? 1.0 : 0.0, v => m.YOriginEnable = v >= 0.5, alwaysHold: true),
                ["ModSquareAspect"] = m => PropAccessor.Scalar(() => m.SquareAspect == true ? 1.0 : 0.0, v => m.SquareAspect = v >= 0.5, alwaysHold: true),
                ["ModColorDestEnable"] = m => PropAccessor.Scalar(() => m.ColorDestEnable == true ? 1.0 : 0.0, v => m.ColorDestEnable = v >= 0.5, alwaysHold: true),
                ["ModAngleDestEnable"] = m => PropAccessor.Scalar(() => m.AngleDestEnable == true ? 1.0 : 0.0, v => m.AngleDestEnable = v >= 0.5, alwaysHold: true),
                ["ModDiscardAfterStop"] = m => PropAccessor.Scalar(() => m.DiscardAfterStop == true ? 1.0 : 0.0, v => m.DiscardAfterStop = v >= 0.5, alwaysHold: true),
                ["ModInvert"] = m => PropAccessor.Scalar(() => m.Invert == true ? 1.0 : 0.0, v => m.Invert = v >= 0.5, alwaysHold: true),
            };

        // ---- Fields ----

        public Keyframes.KeyframeSet PropertyKeyframes = new Keyframes.KeyframeSet();
        ProjProps _props = new ProjProps();
        public ProjProps Props
        {
            get => _props;
            set
            {
                _props = value;
                _props.OnPlaybackOffsetSChanged = OnPlaybackOffsetSChanged;
                Props.OnPlaybackOffsetSChanged();
            }
        }
        static ISongDrawHost s_drawHostOverride;
        public static void SetDrawHost(ISongDrawHost host) => s_drawHostOverride = host;
        public static ISongDrawHost StaticDrawHost => s_drawHostOverride;
        ISongDrawHost DrawHost => StaticDrawHost;

        TimeSpan _pbStartSysTime = new TimeSpan(0);
        double _pbStartSongTimeS;
        // Set while CreateGeos bakes a track so screen-position mapping uses that track's ref width.
        float? _geoWidthOverrideQn;

        public float GlobalViewWidthQn => _geoWidthOverrideQn
            ?? EffectiveViewWidthQn(_trackViews != null && _trackViews.Count > 0 ? GlobalTrackProps : null);

        /// <summary>Effective width (QN) for a track: own value, else global track's, else default.</summary>
        public float EffectiveViewWidthQn(TrackProps tp)
        {
            float? w = tp?.SpatialProps?.ViewWidthQn;
            if (w == null && _trackViews != null && _trackViews.Count > 0)
                w = GlobalTrackProps.SpatialProps?.ViewWidthQn;
            float v = w ?? ProjProps.DefaultViewWidthQn;
            return v > 0 ? v : ProjProps.DefaultViewWidthQn;   // guard div-by-zero
        }

        /// <summary>Effective silence threshold (s) for a track: own value, else global track's, else default.</summary>
        public float EffectiveSilenceThresholdS(TrackProps tp)
        {
            float? s = tp?.AudioProps?.SilenceThresholdS;
            if (s == null && _trackViews != null && _trackViews.Count > 0)
                s = GlobalTrackProps.AudioProps?.SilenceThresholdS;
            return s ?? AudioProps.DefaultSilenceThresholdS;
        }

        public float ViewWidthT => _notes == null ? 0 : GlobalViewWidthQn * _notes.TicksPerBeat; //Number of ticks that fits on screen
        public float TrackViewWidthT(TrackProps tp)
            => _notes == null ? 0 : EffectiveViewWidthQn(tp) * _notes.TicksPerBeat;

        /// <summary>Shader x-scale: width the track's geometry was baked at / current effective width.</summary>
        public float TrackViewWidthQnScale(TrackProps tp)
        {
            float eff = EffectiveViewWidthQn(tp);
            float refW = tp?.TrackView?.Geo?.RefWidthQn ?? 0;
            return refW > 0 ? refW / eff : 1f;
        }

        public ImportOptions ImportOptions { get; set; }

        List<TrackView> _trackViews;
        public List<TrackView> TrackViews
        {
            get { return _trackViews; }
            set
            {
                foreach (var tv in _trackViews)
                    tv.Geo?.Dispose();
                _trackViews = value;
            }
        }

        int _firstTempoEvent = 0;

        //Current playback position to seek from
        int _pbTempoEvent = 0;
        double _pbTimeT = 0;
        double _pbTimeS = 0;

        //public Camera DefaultCamera { get; } = new Camera();
        //----------------------------------------------------

        public TrackProps GlobalTrackProps
        {
            get { return _trackViews[0].TrackProps; }
            set { _trackViews[0].TrackProps = value; }
        }

        Midi.Song _notes;
        public Midi.Song Notes
        {
            get => _notes;
            set => _notes = value;
        }

        public double SongLengthT => (_notes != null ? _notes.SongLengthT : 0) + _playbackOffsetT; //Song length in ticks
        public double SongLengthS { get; private set; }
        public double SongPosT => (int)(_normSongPos * SongLengthT); //Current song position in ticks
        public double SongPosB => (float)SongPosT / Notes.TicksPerBeat; //Current song position in beats
        public float SongPosP => GetScreenPosX(SongPosT); //Current song position in pixels
        public double SongPosS => NormSongPosToSeconds(_normSongPos); //Current song position in seconds
        double _normSongPos; //Song position normalized to [0,1]
        public double NormSongPos
        {
            get => _normSongPos;
            set
            {
                if (_normSongPos != value)
                {
                    _normSongPos = value;
                    _normSongPos = Math.Max(0, _normSongPos);
                    _normSongPos = Math.Min(1, _normSongPos);
                    DrawHost?.Invalidate();
                    DrawHost?.NotifySongPosChanged();
                }
            }
        }
        float _playbackOffsetT = 0;
        public float PlaybackOffsetT => _playbackOffsetT;
        public float PlaybackOffsetP => GetScreenPosX(_playbackOffsetT);

        bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                _isPlaying = value;
                if (!value)
                    AudioHasStarted = false;
            }
        }
        bool _tempPausing;
        public bool AudioHasStarted { get; set; }

        public Project()
        {
            Props.OnPlaybackOffsetSChanged = OnPlaybackOffsetSChanged;
        }

        async public Task<bool> LoadContent(IProgress<float> progress = null, CancellationToken ct = default)
        {
            if (ImportOptions == null)
                return true;

            ImportOptions.SetNotePath();
            ImportOptions.EraseCurrent = false;
            ImportOptions.IsProjectLoad = true;
            return await ImportSong(ImportOptions, progress, ct);
        }

        public void NudgeSongPos(float stepFraction)
        {
            if (Notes == null) return;
            float newPos = (float)(NormSongPos - (double)ViewWidthT * stepFraction / SongLengthT);
            NormSongPos = Math.Max(0, Math.Min(1, newPos));
        }

        public Project(SerializationInfo info, StreamingContext ctxt) : this()
        {
            //	Props = new ProjProps();
            float? legacyWidth = null;
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "version")
                    SongFormat.readVersion = (int)entry.Value;
                else if (entry.Name == "importOptions")
                    ImportOptions = (ImportOptions)entry.Value;
                else if (entry.Name == "trackViews")
                {
                    _trackViews = (List<TrackView>)entry.Value;
                    TrackView.NumTracks = TrackViews.Count;
                    for (int i = 0; i < _trackViews.Count; i++)
                    {
                        var tv = _trackViews[i];
                        tv.TrackProps.TrackView = tv;
                        tv.TrackProps.GlobalProps = TrackViews[0].TrackProps;
                        if (i > 0)
                        {
                            tv.TrackProps.AudioProps.LineColor = tv.TrackProps.MaterialProps.GetSysColor(true, tv.TrackProps.GlobalProps.MaterialProps);
                        }
                        else if (tv.TrackProps.AudioProps.SilenceThresholdS == null)
                            tv.TrackProps.AudioProps.SilenceThresholdS = AudioProps.DefaultSilenceThresholdS;
                    }
                }

                else if (entry.Name == "propertyKeyframes")
                    PropertyKeyframes = (Keyframes.KeyframeSet)entry.Value ?? PropertyKeyframes;
                else if (entry.Name == "props")
                    Props = (ProjProps)entry.Value;

                //Compatibility
                else if (entry.Name == "qn_viewWidth")
                    legacyWidth = (float)entry.Value;
                else if (entry.Name == "audioOffset")
                    Props.AudioOffset = (double)entry.Value;
                else if (entry.Name == "fadeIn")
                    Props.FadeIn = (float)entry.Value;
                else if (entry.Name == "fadeOut")
                    Props.FadeOut = (float)entry.Value;
                else if (entry.Name == "maxPitch")
                    Props.MaxPitch = (int)entry.Value;
                else if (entry.Name == "minPitch")
                    Props.MinPitch = (int)entry.Value;
                else if (entry.Name == "camera")
                    Props.Camera = (Camera)entry.Value;
                else if (entry.Name == "userViewWidth")
                    Props.UserViewWidth = (float)entry.Value;
            }

            // Migrate legacy project-level width to the global track (idempotent, clone-safe:
            // new-format clones already carry the per-track value).
            if (_trackViews != null && _trackViews.Count > 0)
            {
                var gs = _trackViews[0].TrackProps.SpatialProps;
                if (gs.ViewWidthQn == null)
                    gs.ViewWidthQn = legacyWidth ?? Props.LegacyViewWidthQn ?? ProjProps.DefaultViewWidthQn;
            }
            PropertyKeyframes?.RenameProperty("proj/ViewWidthQn", "track/0/ViewWidthQn");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("version", SongFormat.writeVersion);
            info.AddValue("importOptions", ImportOptions);
            info.AddValue("trackViews", _trackViews);
            info.AddValue("propertyKeyframes", PropertyKeyframes);
            info.AddValue("props", Props);
        }

        // Matches the "Progress: N%" lines emitted by remuxer.exe (see Remuxer/Program.cs).
        static readonly Regex RemuxerProgressRegex = new Regex(@"^Progress:\s*(\d+)%", RegexOptions.Compiled);

        // Matches the "TrackAudio: <miditrack>|<path>" lines emitted by remuxer.exe after processing.
        static readonly Regex RemuxerTrackAudioRegex = new Regex(@"^TrackAudio:\s*(\d+)\|(.+)$", RegexOptions.Compiled);

        // Remuxer writes a 58-byte IEEE-float WAV header before the data chunk.
        const long EmptyGeneratedWavBytes = 58;

        static bool HasGeneratedAudio(string path)
        {
            try
            {
                return !string.IsNullOrEmpty(path)
                    && File.Exists(path)
                    && new FileInfo(path).Length > EmptyGeneratedWavBytes;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// True when every real track that carries a serialized per-track audio filename already has
        /// that file present on disk (and at least one such track exists). Used on project load to
        /// skip the extra remuxer track-audio render passes when the WAVs are already available
        /// (either the temp copies or ones saved next to the project).
        /// </summary>
        bool AllSerializedTrackAudioPresent()
        {
            if (_trackViews == null) return false;
            bool any = false;
            for (int i = 1; i < _trackViews.Count; i++)   // index 0 = global/MIDI track 0
            {
                string fn = _trackViews[i].TrackProps.AudioProps.Filename;
                if (string.IsNullOrEmpty(fn)) continue;
                any = true;
                if (!File.Exists(fn)) return false;
            }
            return any;
        }

        async public Task<bool> ImportSong(ImportOptions options, IProgress<float> progress = null, CancellationToken ct = default)
        { //`Open project` and `import files` meet here
            options.CheckSourceFile();
            //Convert mod/sid files to mid/wav
            if (options.NoteFileType == FileType.Audio)
            {
                // Audio-only project: there is no note file to convert. The master audio drives
                // everything and is opened below by OpenAudioFile.
            }
            else if (options.NoteFileType != FileType.Midi)
            {
                string noteFile = Path.GetFileName(options.NotePath);
                string midiPath = null, midiArg = null, generatedAudioPath = null, audioArg = null;

                //Should midi file be created?
                if (!options.SavedMidi)
                {
                    midiPath = Path.Combine(Program.TempDir, noteFile) + ".mid";
                    midiArg = $"-m\"{midiPath}\"";
                    File.Delete(midiPath);
                }
                else
                    midiPath = options.MidiOutputPath;

                // Generate audio unless the user supplied a separate audio file.
                if (!options.HasSuppliedAudio)
                {
                    generatedAudioPath = Path.Combine(Program.TempDir, noteFile) + ".wav";
                    audioArg = $"-a\"{generatedAudioPath}\"";
                    File.Delete(generatedAudioPath);
                }

                // Per-track WAVs, if requested. They live under the stable TempDirRoot (not the random
                // per-session TempDir) so a reload regenerates identical paths — keeping serialized
                // AudioProps.Filename values valid. On project load we skip the extra passes when the
                // WAVs are already present on disk.
                string trackAudioArg = null, trackAudioBase = null;
                if (options.TrackAudio && !(options.IsProjectLoad && AllSerializedTrackAudioPresent()))
                {
                    string trackAudioDir = Path.Combine(Program.TempDirRoot, "trackaudio",
                        $"{noteFile}-s{options.SubSong}-{(options.InsTrack ? "ins" : "chn")}");
                    Directory.CreateDirectory(trackAudioDir);
                    trackAudioBase = Path.Combine(trackAudioDir, noteFile);
                    trackAudioArg = $"-t\"{trackAudioBase}\"";
                }

                //Does either midi or audio (or per-track audio) need to be created?
                if (midiArg != null || audioArg != null || trackAudioArg != null)
                {
                    string insTrackFlag = options.InsTrack ? "-i" : "";
                    string songLengthsFlag = $"-l{options.SongLengthS.ToString()}";
                    string subSongFlag = $"-s{options.SubSong.ToString()}";
                    string supressErrorFlag = "-e";
                    string cancelSignalPath = Path.Combine(Program.TempDir, Path.GetRandomFileName() + ".cancel");
                    File.Delete(cancelSignalPath);
                    string cancelFlag = $"-c\"{cancelSignalPath}\"";
                    string cmdLine = $"\"{options.NotePath}\" {midiArg} {audioArg} {trackAudioArg} {insTrackFlag} {songLengthsFlag} {subSongFlag} {supressErrorFlag} {cancelFlag}";
                    var workingDir = Path.Combine(Program.Dir, "remuxer");
                    var startInfo = new ProcessStartInfo(Path.Combine(workingDir, "remuxer.exe"), cmdLine)
                    {
                        WorkingDirectory = workingDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    using var process = Process.Start(startInfo)
                        ?? throw new IOException("Couldn't start remuxer.");

                    // Collected per-track WAV lines ("TrackAudio: <miditrack>|<path>"). The stdout
                    // handler runs on a pool thread, so guard the list with a lock.
                    var trackAudioFiles = new List<(int Track, string Path)>();

                    // Parse the "Progress: N%" and "TrackAudio:" lines remuxer writes to stdout.
                    process.OutputDataReceived += (_, e) =>
                    {
                        if (e.Data == null)
                            return;
                        var m = RemuxerProgressRegex.Match(e.Data);
                        if (m.Success && int.TryParse(m.Groups[1].Value, out int percent))
                        {
                            progress?.Report(percent / 100f);
                            return;
                        }
                        var t = RemuxerTrackAudioRegex.Match(e.Data);
                        if (t.Success && int.TryParse(t.Groups[1].Value, out int midiTrack))
                        {
                            lock (trackAudioFiles)
                                trackAudioFiles.Add((midiTrack, t.Groups[2].Value.Trim()));
                        }
                    };
                    var errorOutput = new StringBuilder();
                    process.ErrorDataReceived += (_, e) => { if (e.Data != null) errorOutput.AppendLine(e.Data); };
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Signal the converter to stop gracefully if the user cancels. Remuxer will
                    // exit its processing loop and run EndProcessing(), which writes partial outputs.
                    using (ct.Register(() =>
                    {
                        try { File.WriteAllText(cancelSignalPath, ""); }
                        catch { /* best-effort cancel signal */ }
                    }))
                    {
                        await process.WaitForExitAsync();
                    }
                    try { File.Delete(cancelSignalPath); } catch { /* best-effort cleanup */ }

                    // Record the per-track WAVs that were actually produced (non-empty on disk).
                    List<(int Track, string Path)> collectedTrackAudio;
                    lock (trackAudioFiles)
                        collectedTrackAudio = trackAudioFiles.ToList();
                    options.GeneratedTrackAudioPaths = collectedTrackAudio
                        .Where(x => HasGeneratedAudio(x.Path)).ToList();

                    bool cancelled = ct.IsCancellationRequested;
                    bool midiExists = !string.IsNullOrEmpty(midiPath) && File.Exists(midiPath);
                    bool audioExists = HasGeneratedAudio(generatedAudioPath);
                    if (!audioExists)
                        generatedAudioPath = null;

                    if (cancelled)
                    {
                        if (!midiExists)
                            return false;
                    }
                    else if (process.ExitCode != 0)
                    {
                        throw new FileImportException(
                            errorOutput.Length > 0 ? errorOutput.ToString().Trim() : null,
                            ImportError.Corrupt, ImportFileType.Note, options.RawNotePath);
                    }

                    if (!midiExists && !audioExists)
                        return false;
                }
                options.MidiOutputPath = midiPath;
                options.GeneratedAudioPath = generatedAudioPath;
            }
            else if (!options.HasSuppliedAudio)
            {
                options.GeneratedAudioPath = null;
                if (MidMix.SfLoaded())
                {
                    string audioPath = Path.Combine(Program.TempDir, Path.GetFileName(options.NotePath)) + ".wav";
                    MidMix.Mixdown(options.NotePath, audioPath);
                    options.GeneratedAudioPath = audioPath;
                }
            }

            bool resetProject = options.EraseCurrent || (_notes == null && (_trackViews == null || _trackViews.Count == 0));

            if (options.NoteFileType == FileType.Audio)
                OpenSyntheticSong(resetProject);
            else
                OpenNoteFile(options, resetProject);
            OpenAudioFile(options);

            // Audio-only: OpenAudioFile set SongLengthT from the master audio length; propagate it to
            // every (note-less) track so their reported length matches the song.
            if (options.NoteFileType == FileType.Audio)
                foreach (var track in _notes.Tracks)
                    track.Length = _notes.SongLengthT;

            ImportOptions = options;
            if (resetProject)
            {
                // Audio-only projects have no note path; name the project from the master audio.
                string sourceName = options.NoteFileType == FileType.Audio
                    ? ImportOptions.AudioPath : ImportOptions.NotePath;
                DefaultFileName = Path.GetFileName(sourceName) + "." + DefaultFileExt;
            }
            CreateTrackViews(_notes.Tracks.Count, resetProject);
            InitPropertyAccessors();
            return true;
        }

        /// <summary>
        /// Turn this (MOD/SID/HVL) project into a plain MIDI project that loads from a saved .mid,
        /// so future loads no longer run the remuxer. <paramref name="audioPath"/> (the saved WAV or a
        /// pre-existing supplied audio file, or null) becomes the project's supplied audio.
        /// </summary>
        public void ConvertToMidiProject(string midiPath, string audioPath)
        {
            var midiOpts = new MidiImportOptions
            {
                RawNotePath = midiPath,
                AudioPath = audioPath,   // saved WAV (or pre-existing supplied audio), else null
                InsTrack = true,         // saved .mid already carries the remuxer's track layout
                EraseCurrent = false,
            };
            midiOpts.SetNotePath();      // NotePath = midiPath (local file)
            ImportOptions = midiOpts;
            midiOpts.UpdateImportForm(); // pre-fill the MIDI import dialog session (UpdateSession)
        }

        public bool OpenNoteFile(ImportOptions options, bool? resetProjectOverride = null)
        {
            bool resetProject = resetProjectOverride ?? options.EraseCurrent;

            DrawHost?.Invalidate();
            StopPlayback();
            _pbTempoEvent = 0;
            _pbTimeT = 0;
            _pbTimeS = 0;

            Midi.Song newNotes = new Midi.Song();
            string path = options.MidiOutputPath;
            string errorPath = path;
            if (path == null)
            {
                path = options.NotePath;
                errorPath = options.RawNotePath;
            }

            try
            {
                newNotes.OpenFile(path);
            }
            catch (FileFormatException ex)
            {
                throw new FileImportException(ex.Message, ImportError.Corrupt, ImportFileType.Note, errorPath);
            }
            if (newNotes.Tracks == null || newNotes.Tracks.Count == 0 || newNotes.SongLengthT == 0)
                throw new FileImportException("No notes found.", ImportError.Corrupt, ImportFileType.Note, errorPath);

            _notes = newNotes;
            if (options.NoteFileType == FileType.Midi && !options.InsTrack)
                SplitTracksByChannel(_notes);
            _notes.CreateNoteBsp();

            if (resetProject)
                ResetProjectStateForNewSong();
            //viewWidthT = (int)(ViewWidthQn * notes.TicksPerBeat);
            return true;
        }

        /// <summary>
        /// Clears per-project state (keyframes, lyrics, offsets, position, pitch limits) when a new
        /// song replaces the current one. Assumes <see cref="_notes"/> is already assigned, because
        /// setting Props.PlaybackOffsetS fires a callback that reads <c>_notes.TempoEvents[0]</c>.
        /// </summary>
        void ResetProjectStateForNewSong()
        {
            PropertyKeyframes = new Keyframes.KeyframeSet();
            Props.LyricsSegments.Clear();
            Keyframes.KeyframeService.RaiseKeyframesChanged();
            Props.AudioOffset = Props.PlaybackOffsetS = Props.FadeIn = Props.FadeOut = 0;
            NormSongPos = 0;
            ResetPitchLimits();
        }

        /// <summary>
        /// Builds a synthetic, note-less <see cref="Midi.Song"/> for a pure-audio project. The song
        /// carries only a tempo map (so timing works) and one empty <see cref="Midi.Track"/> per
        /// project track; track names follow their audio filenames. On a fresh import (or erase) only
        /// the global track exists; on reload the deserialized <see cref="_trackViews"/> drive the
        /// track count and names.
        /// </summary>
        void OpenSyntheticSong(bool resetProject)
        {
            DrawHost?.Invalidate();
            StopPlayback();
            _pbTempoEvent = 0;
            _pbTimeT = 0;
            _pbTimeS = 0;

            // Fresh Midi.Song leaves Tracks/TempoEvents null, so assign new lists explicitly (the
            // collection-initializer form would NRE). Use the (int, double) tempo ctor: the (int, int)
            // overload never assigns Tempo, which would make SecondsToTicks loop forever (bps == 0).
            var song = new Midi.Song
            {
                TicksPerBeat = 480,
                TempoEvents = new List<Midi.TempoEvent> { new Midi.TempoEvent(0, 120.0) },
                Tracks = new List<Midi.Track>(),
            };

            // Track 0 is the empty global/master track (not rendered).
            song.Tracks.Add(new Midi.Track { Name = "" });

            // On reload, rebuild one track per deserialized track view. Name each by its TrackNumber
            // (the track list is reorderable, so list position isn't the track index), using the
            // saved audio filename; fall back to a generic name so names are never null.
            if (!resetProject && _trackViews != null && _trackViews.Count > 0)
            {
                int trackCount = _trackViews.Max(tv => tv.TrackNumber) + 1;
                while (song.Tracks.Count < trackCount)
                    song.Tracks.Add(new Midi.Track());

                foreach (var tv in _trackViews)
                {
                    if (tv.TrackNumber <= 0) continue;
                    string fn = tv.TrackProps.AudioProps.Filename;
                    song.Tracks[tv.TrackNumber].Name = !string.IsNullOrWhiteSpace(fn)
                        ? Path.GetFileNameWithoutExtension(fn)
                        : $"Track {tv.TrackNumber}";
                }
            }

            _notes = song;
            _notes.CreateNoteBsp();

            if (resetProject)
                ResetProjectStateForNewSong();
        }

        public void OpenAudioFile(ImportOptions options)
        {
            Media.CloseAudioFile();
            AudioFilePath = "";
            string file = options.HasSuppliedAudio ? options.AudioPath : options.GeneratedAudioPath;

            if (string.IsNullOrWhiteSpace(file))
                return;

            if (!Media.OpenAudioFile(file))
                throw new IOException("Unexpected error while opening audio file:\r\n" + file);

            if (_notes != null)
                _notes.SongLengthT = (int)SecondsToTicks((float)(Media.GetAudioLength() + Props.AudioOffset));
            AudioFilePath = file;
        }

        /// <summary>
        /// Regroup all MIDI notes by channel so each MIDI channel becomes its own app track.
        /// Track 0 is left empty (the global/master track — <see cref="CreateTrackViews"/> starts
        /// assigning visuals from index 1 and <see cref="AddTrackView"/> sets GlobalProps from
        /// Tracks[0], mirroring the MOD/SID Remuxer convention).
        /// </summary>
        static void SplitTracksByChannel(Midi.Song song)
        {
            var byChannel = new SortedDictionary<int, List<Midi.Note>>();
            foreach (var track in song.Tracks)
                foreach (var note in track.Notes)
                {
                    if (!byChannel.TryGetValue(note.channel, out var list))
                        byChannel[note.channel] = list = new List<Midi.Note>();
                    list.Add(note);
                }

            // Index 0: empty global/master track (not rendered)
            var newTracks = new List<Midi.Track> { new Midi.Track { Length = song.SongLengthT } };
            foreach (var kv in byChannel)
            {
                var t = new Midi.Track
                {
                    Length = song.SongLengthT,
                    Name = $"Channel {kv.Key + 1}"
                };
                kv.Value.Sort((a, b) => a.start.CompareTo(b.start));
                t.Notes = kv.Value;
                newTracks.Add(t);
            }
            song.Tracks = newTracks;
        }

        public void ResetPitchLimits()
        {
            Props.MaxPitch = Notes.MaxPitch;
            Props.MinPitch = Notes.MinPitch;
        }

        public void ShowNoteInfo(GdiPoint location)
        {
            //if (noteMap != null)
            //{
            //    GdiPoint clientP = PointToClient(location);
            //    int t = 0;
            //    float noteHeight = ClientRectangle.Height / (notes.NumPitches + 1);
            //    GdiPoint songP = new GdiPoint((int)(clientP.X * viewWidthT / ClientRectangle.Width - viewWidthT / 2 + normSongPos * notes.SongLengthInTicks), (int)((ClientRectangle.Height - (clientP.Y + noteHeight)) * (notes.NumPitches + 1) / ClientRectangle.Height));
            //    //Point songP2 = new Point((int)((clientP.X - panel1.AutoScrollPosition.X) * notes.SongLengthInTicks / (panel1.AutoScrollMinSize.Width + panel1.ClientRectangle.Width) - viewWidthT / 2), (int)((panel1.ClientRectangle.Height - (clientP.Y + noteHeight)) * (notes.NumPitches + 1) / panel1.ClientRectangle.Height));
            //    if (songP.X < 0)
            //        return;
            //    int x = songP.X;
            //    while (t < noteMap.GetLength(0) && !noteMap[t, x, songP.Y])
            //        t++;
            //    if (t == noteMap.GetLength(0))
            //        return;
            //    while (x > 0 && noteMap[t, x, songP.Y])
            //        x--;
            //    int noteStart = x + 1;
            //    x = songP.X;
            //    while (x < noteMap.GetLength(1) && noteMap[t, x, songP.Y])
            //        x++;
            //    MessageBox.Show("Start: " + noteStart + "    End: " + x + "\nPitch: "+songP.Y);
            //}
        }

        public void CreateTrackViews(int numTracks, bool eraseCurrent)
        {
            TrackView.NumTracks = numTracks;
            int startTrack; //At which index to start creating new (default) track props
            if (eraseCurrent || _trackViews == null)
            {
                startTrack = 0;
                _trackViews = new List<TrackView>(numTracks);
                DrawHost?.WaveformPanel?.ClearChannels();
            }
            else
            {
                startTrack = _trackViews.Count; //Keep current props but add new props if the new imported note file has more tracks than the current song. Start assigning default track props at current song's track count and up.
            }

            for (int i = 0; i < _trackViews.Count; i++)
            {
                //No need to update visual props
                // Update notes

                if (_trackViews[i].TrackNumber >= _notes.Tracks.Count) //The new note file has fewer tracks than the currently loaded
                {
                    DrawHost?.WaveformPanel?.RemoveChannel(_trackViews[i].TrackProps.AudioProps.SidWizChannel);
                    continue;
                }

                // Update note information
                _trackViews[i].MidiTrack = _notes.Tracks[_trackViews[i].TrackNumber];
                _trackViews[i].CreateCurve();

                // If a project file is being loaded, track views was deserialized, and further init involving the graphics device is needed here, because it was not initialized at the time of deserialization.
                _trackViews[i].TrackProps.StyleProps.LoadFx();
            }
            for (int i = startTrack; i < numTracks; i++)
            {
                //New note file has more tracks than current project or we're creating a new project. Create new track props for the new tracks.
                TrackView view = new TrackView(i, numTracks, _notes);
                AddTrackView(view);
            }
            //if (startTrack >= numTracks && numTracks > 0)  //New note file has fewer tracks than current song. Remove the extra trackViews.
            //trackViews.RemoveRange(numTracks, startTrack - numTracks);
            List<TrackView> tvCopy = new List<TrackView>();
            for (int i = 0; i < _trackViews.Count; i++)
            {
                if (_trackViews[i].TrackNumber < numTracks)
                    tvCopy.Add(_trackViews[i]);
            }
            _trackViews = tvCopy;
            CreateGeos(false);
        }

        void AddTrackView(TrackView view)
        {
            _trackViews.Add(view);
            view.TrackProps.GlobalProps = TrackViews[0].TrackProps;
            if (view.TrackNumber == 0 && view.TrackProps.AudioProps.SilenceThresholdS == null)
                view.TrackProps.AudioProps.SilenceThresholdS = AudioProps.DefaultSilenceThresholdS;
            view.TrackProps.AudioProps.LineColor = view.TrackProps.MaterialProps.GetSysColor(true, view.TrackProps.GlobalProps.MaterialProps);
            view.TrackProps.AudioProps.SidWizChannel.Filename = "";
            DrawHost?.WaveformPanel?.AddChannel(view.TrackProps.AudioProps.SidWizChannel);
            if (view.TrackNumber == 1)
                view.TrackProps.AudioProps.SidWizChannel.LoadDataAsync();
        }

        /// <summary>
        /// Appends one new project track per audio file, naming each track after its filename and
        /// assigning it as the track's audio. Returns the created track views (caller loads their
        /// audio and refreshes the UI). Creating tracks changes the track count, so the caller must
        /// reset the undo stack afterward (undo snapshots can't restore across track counts).
        /// </summary>
        public List<TrackView> AddAudioTracks(IReadOnlyList<string> files)
        {
            var created = new List<TrackView>();
            if (_notes == null || files == null || files.Count == 0)
                return created;

            int start = _notes.Tracks.Count;
            foreach (var file in files)
                _notes.Tracks.Add(new Midi.Track
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Length = _notes.SongLengthT,
                });

            // Preferred over manual AddTrackView: updates TrackView.NumTracks (default hue spread),
            // wires GlobalProps, and adds SidWiz channels to the live WaveformPanel.
            CreateTrackViews(_notes.Tracks.Count, false);

            for (int i = start; i < _notes.Tracks.Count; i++)
            {
                var tv = _trackViews.First(v => v.TrackNumber == i);
                tv.TrackProps.AudioProps.Filename = files[i - start];
                created.Add(tv);
            }

            InitPropertyAccessors();
            return created;
        }

        public void CreateGeos(bool resetVertScale = true)
        {
            if (_trackViews == null || Notes == null)
                return;
            for (int i = 1; i < _trackViews.Count; i++)
            {
                var tv = _trackViews[i];
                // resetVertScale: bake fresh at the current effective width (scale 1). Otherwise re-bake at
                // the track's existing ref width so the shader scale (ref/effective) is preserved.
                _geoWidthOverrideQn = (resetVertScale ? null : (tv.Geo?.RefWidthQn > 0 ? tv.Geo.RefWidthQn : (float?)null))
                    ?? EffectiveViewWidthQn(tv.TrackProps);
                try
                {
                    tv.CreateGeo(this, GlobalTrackProps);
                }
                finally { _geoWidthOverrideQn = null; }
            }
        }

        public void DrawSong()
        {
            var host = DrawHost;
            if (host == null) return;

            host.DrawBackground();

            if (_notes == null || _trackViews == null)
                return;

            host.InitFrame();
            DepthStencilState oldDss = host.GraphicsDevice.DepthStencilState;
            DepthStencilState dss = new DepthStencilState();
            dss.StencilEnable = true;
            dss.StencilFunction = CompareFunction.Greater;
            dss.StencilPass = StencilOperation.Replace;
            dss.ReferenceStencil = 1;
            host.GraphicsDevice.DepthStencilState = dss;
            for (int t = 1; t < _trackViews.Count; t++)
            {
                host.GraphicsDevice.Clear(ClearOptions.Stencil | ClearOptions.DepthBuffer, Color.AliceBlue, 1, 0);
                _trackViews[t].DrawTrack(GlobalTrackProps, host.ForceDefaultNoteStyle);
            }
            host.GraphicsDevice.DepthStencilState = oldDss;

            DrawLyrics(host);
            RefreshSidWizChannels();
            host.WaveformPanel?.Draw(SongPosS - Props.PlaybackOffsetS, GetSongFade(),
                Props.AudioVisLeft, Props.AudioVisRight, Props.AudioVisWidth);
        }

        /// <summary>
        /// Pushes each track's current highlighted material color into its SidWiz channel so the
        /// waveform overlay follows hue changes from keyframes (and live edits), including changes
        /// to the global track's hue. Also pushes the effective silence threshold (per-track
        /// override, else the global track's value).
        /// </summary>
        void RefreshSidWizChannels()
        {
            float globalThreshold = GlobalTrackProps.AudioProps.SilenceThresholdS
                ?? AudioProps.DefaultSilenceThresholdS;
            for (int t = 1; t < _trackViews.Count; t++)
            {
                var tp = _trackViews[t].TrackProps;
                var color = tp.MaterialProps.GetSysColor(true, GlobalTrackProps.MaterialProps);
                if (tp.AudioProps.LineColor.ToArgb() != color.ToArgb())
                    tp.AudioProps.LineColor = color;
                if (tp.AudioProps.SidWizChannel.LineWidth != Props.AudioVisLineWidth)
                    tp.AudioProps.SidWizChannel.LineWidth = Props.AudioVisLineWidth;
                tp.AudioProps.SidWizChannel.ActivityLookaheadSeconds =
                    tp.AudioProps.SilenceThresholdS ?? globalThreshold;
            }
        }

        /// <summary>
        /// Song-wide fade factor [0,1] from the project's fade in/out props, matching the
        /// SongFade computation used by the note shader (see NoteStyle.DrawTrack).
        /// </summary>
        float GetSongFade()
        {
            float songFade = 1;
            float songPosS = (float)SongPosS;
            float songLength = (float)SongLengthS;
            if (Props.FadeIn > 0 && songPosS < Props.FadeIn)
                songFade = songPosS / Props.FadeIn;
            else if (Props.FadeOut > 0 && songLength - songPosS < Props.FadeOut)
                songFade = (songLength - songPosS) / Props.FadeOut;
            return Math.Clamp(songFade, 0, 1);
        }

        void DrawLyrics(ISongDrawHost host)
        {
            if (Props.LyricsSegments.Count == 0 || ViewWidthT <= 0 || host.LyricsFont == null)
                return;

            var viewport = host.GraphicsDevice.Viewport;
            float width = Math.Max(1, viewport.Width);
            float height = Math.Max(1, viewport.Height);
            var oldDepth = host.GraphicsDevice.DepthStencilState;

            host.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            host.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            foreach (var lyricsSegment in Props.LyricsSegments)
            {
                string lyrics = lyricsSegment.Lyrics;
                if (string.IsNullOrWhiteSpace(lyrics))
                    continue;

                double tick = LyricsBeatToTimelineTick(lyricsSegment.Beat);
                float x = (float)(width * 0.5 + (tick - SongPosT) / ViewWidthT * width);
                Vector2 size = host.LyricsFont.MeasureString(lyrics);
                if (x > width || x + size.X < 0)
                    continue;

                float y = Math.Max(0, height - size.Y - 4);
                host.SpriteBatch.DrawString(host.LyricsFont, lyrics, new Vector2(x, y), Color.White);
            }

            host.SpriteBatch.End();
            host.GraphicsDevice.DepthStencilState = oldDepth;
        }

        public int ScreenPosToSongPos(float normScreenPos)
        {
            return (int)(NormSongPos * SongLengthT + (double)normScreenPos * ViewWidthT * 0.5f);
        }
        Point GetVisibleSongPortionT(double normPos)
        {
            double posT = normPos * SongLengthT;
            return new Point((int)(posT - ViewWidthT), (int)(posT + ViewWidthT));
        }
        public int GetPitch(float normPosY)
        {
            normPosY = 1 - normPosY;
            float height = 1 - ProjProps.NormPitchMargin * 2;
            float noteHeight = height / Notes.NumPitches;
            float pos = normPosY - ProjProps.NormPitchMargin;
            return Props.MinPitch + (int)(pos / noteHeight);
        }
        public TrackProps MergeTrackProps(IEnumerable<int> indices)
        {
            var list = indices.ToList();
            if (list.Count == 0) return null;
            TrackProps outProps = TrackViews[list[0]].TrackProps;
            if (list.Count == 1) return outProps;
            outProps = outProps.Clone(DrawHost);
            outProps.AudioProps = new AudioProps
            {
                Filename = outProps.AudioProps.Filename,
                LineColor = outProps.AudioProps.LineColor,
                SilenceThresholdS = outProps.AudioProps.SilenceThresholdS
            };
            for (int i = 1; i < list.Count; i++)
                outProps = (TrackProps)MergeObjects(outProps, TrackViews[list[i]].TrackProps);
            return outProps;
        }

        public object MergeObjects(object first, object second)
        {
            if (first == null || second == null)
                return null;
            PropertyInfo[] props = first.GetType().GetProperties();
            bool hasSuitableProp = false;
            if (props.Length > 0 && !(first is Color))
            {
                foreach (PropertyInfo propertyInfo in props)
                {
                    if (!propertyInfo.CanRead || !propertyInfo.CanWrite || propertyInfo.GetMethod.IsStatic || propertyInfo.GetType() is IEnumerable)
                        continue;
                    hasSuitableProp = true;

                    object firstValue, secondValue;
                    firstValue = propertyInfo.GetValue(first, null);
                    secondValue = propertyInfo.GetValue(second, null);

                    object subMerge = MergeObjects(firstValue, secondValue);
                    propertyInfo.SetValue(first, subMerge);
                    if (propertyInfo.Name == "Type" && subMerge != null && (NoteStyleType)subMerge != NoteStyleType.Default)
                    {
                        if (propertyInfo.DeclaringType == typeof(StyleProps)) //parent type
                        {
                            NoteStyle firstStyle = ((StyleProps)first).SelectedStyle;
                            NoteStyle secondStyle = ((StyleProps)second).SelectedStyle;
                            if (firstStyle.ModEntries != null && secondStyle.ModEntries != null && firstStyle.ModEntries.Count != secondStyle.ModEntries.Count)
                                firstStyle.ModEntries = null;
                        }
                    }
                    else if (propertyInfo.Name == "SelectedModEntryIndex" && subMerge != null && (int)subMerge != -1)
                        ((NoteStyle)first).SelectedModEntry = (NoteStyleMod)MergeObjects(((NoteStyle)first).SelectedModEntry, ((NoteStyle)second).SelectedModEntry);
                }
            }

            if (hasSuitableProp || object.Equals(first, second))
                return first;
            else
                return null;
        }

        public double TicksToSeconds(double ticks)
        {
            if (_pbTimeT > ticks)
            {
                _pbTimeT = _pbTimeS = 0;
                _pbTempoEvent = _firstTempoEvent;
            }
            else if (_pbTimeT == ticks)
                return _pbTimeS;

            int nextTempoEvent = _pbTempoEvent;
            while (_pbTimeT < ticks)
            {
                double nextTimeStepT;
                double currentBps = _notes.TempoEvents[_pbTempoEvent].Tempo / 60; //beats per seconds
                if (_pbTempoEvent + 1 >= _notes.TempoEvents.Count)
                    nextTimeStepT = ticks;
                else
                {
                    nextTempoEvent++;
                    nextTimeStepT = _notes.TempoEvents[nextTempoEvent].Time + _playbackOffsetT;
                    //if (nextTimeStepT < pbTimeT || nextTimeStepT == pbTimeT && bLastTempoEvent)
                    //	throw new Exception("nextTimeStepT < pbTimeT || nextTimeStepT == pbTimeT && bLastTempoEvent");
                    if (nextTimeStepT > ticks)
                        nextTimeStepT = ticks; //always causes loop to exit
                    else
                        _pbTempoEvent = nextTempoEvent;
                }
                _pbTimeS += (nextTimeStepT - _pbTimeT) / (_notes.TicksPerBeat * currentBps);
                _pbTimeT = nextTimeStepT;
            }
            return _pbTimeS;
        }
        double SecondsToTicks(double seconds)
        {
            if (_pbTimeS > seconds) //Reset seek position
            {
                _pbTimeS = _pbTimeT = 0;
                _pbTempoEvent = _firstTempoEvent;
            }

            int nextTempoEvent = _pbTempoEvent;
            while (_pbTimeS < seconds)
            {
                double nextTimeStepS;
                double currentBps = _notes.TempoEvents[_pbTempoEvent].Tempo / 60; //beats per seconds
                if (_pbTempoEvent + 1 >= _notes.TempoEvents.Count)
                    nextTimeStepS = seconds;
                else
                {
                    nextTempoEvent++;
                    double nextTempoTimeS = (_notes.TempoEvents[nextTempoEvent].Time + _playbackOffsetT - _pbTimeT) / (_notes.TicksPerBeat * currentBps) + _pbTimeS;
                    nextTimeStepS = nextTempoTimeS;
                    if (nextTimeStepS > seconds)
                        nextTimeStepS = seconds; //always causes loop to exit
                    else
                        _pbTempoEvent = nextTempoEvent;
                }
                _pbTimeT += (nextTimeStepS - _pbTimeS) * currentBps * _notes.TicksPerBeat;
                _pbTimeS = nextTimeStepS;
            }
            return _pbTimeT;
        }

        public void SetSongPosS(double newTimeS, bool updateScreen)
        {
            double offsetS = 0, offsetT = 0;
            if (Props.PlaybackOffsetS < 0)
            {
                offsetS = -Props.PlaybackOffsetS;
                offsetT = -_playbackOffsetT;
            }
            _pbTimeT = SecondsToTicks(newTimeS);
            double newSongPos = _pbTimeT / (double)SongLengthT;
            if (updateScreen)
                NormSongPos = newSongPos;
            else
                _normSongPos = newSongPos;
        }

        //Converts normalized song pos to seconds
        //0 as input returns 0 seconds, 1 returns song length in seconds
        public double NormSongPosToSeconds(double norm)
        {
            if (_notes == null)
                return 0;

            return TicksToSeconds(norm * SongLengthT);
        }

        public void Update(double deltaTimeS)
        {
            // Integrate on the live render camera; per-property camera keyframes are synced via SyncLiveCameraEdit.
            Props.Camera.Update(deltaTimeS);
            // Only drive property values from the new keyframe model during playback. When stopped the
            // user authors values directly through the controls (editing pauses playback), so overriding
            // every frame would fight the slider and prevent capturing a distinct value for a 2nd keyframe.
            if (IsPlaying)
                InterpolatePropertyKeyframes();

            //Scroll song depending on user input or playback position.
            if (IsPlaying)
            {
                double timeS;
                if (!AudioHasStarted)
                {
                    timeS = ((DrawHost?.TotalTimeElapsed ?? TimeSpan.Zero) - _pbStartSysTime).TotalSeconds + _pbStartSongTimeS;
                    if (timeS > Props.AudioOffset + Props.PlaybackOffsetS)
                    {
                        AudioHasStarted = true;
                        Media.StartPlaybackAtTime(0);
                    }
                }
                else
                {
                    if (!Media.PlaybackIsRunning()) //playback reached end of song
                    {
                        IsPlaying = false;
                        timeS = SongPosS;
                    }
                    else
                        timeS = Media.GetPlaybackPos() + Props.AudioOffset + Props.PlaybackOffsetS;
                }

                SetSongPosS(timeS, true);
                if (NormSongPos > 1)
                    TogglePlayback();
            }
            //else

            if (NormSongPos < 0)
                NormSongPos = 0;
            if (NormSongPos > 1)
                NormSongPos = 1;
        }
        public void TogglePlayback()
        {
            // Note: playback may be toggled while mouse-look is active (Ctrl+Space works when locked).
            if (Media.GetAudioLength() == 0)
                return;
            IsPlaying = !IsPlaying;
            //bAudioPlayback = !bAudioPlayback;
            if (!IsPlaying)
            {
                Media.PausePlayback();
                //MessageBox.Show("An error occured while pausing playback.");
            }
            else
            {
                double songPosS = SongPosS;
                double startTime = songPosS - Props.AudioOffset - Props.PlaybackOffsetS;
                if (startTime >= 0)
                {
                    Media.StartPlaybackAtTime(startTime);
                    //MessageBox.Show("An error occured while starting playback.");
                    if (!Media.PlaybackIsRunning()) //Assuming this is because we tried to start playback after end of audio
                        IsPlaying = false;
                    else
                        AudioHasStarted = true;
                }
                else
                {
                    _pbStartSysTime = DrawHost?.TotalTimeElapsed ?? TimeSpan.Zero;
                    _pbStartSongTimeS = songPosS;
                    AudioHasStarted = false;
                }
            }
        }

        public void PausePlayback()
        {
            if (_isPlaying)
                TogglePlayback();
        }

        public void StopPlayback()
        {
            IsPlaying = false;
            //			bAudioPlayback = false;
            Media.StopPlayback();
            // MessageBox.Show("An error occured while stopping playback.");
            NormSongPos = 0;
        }

        public Vector2 GetScreenPos(int timeT, int pitch)
        {
            Vector2 p = new Vector2();
            p.X = GetScreenPosX(timeT);
            p.Y = GetScreenPosY((float)pitch);
            return p;
        }
        public float GetScreenPosX(double timeT)
        {
            return (float)((timeT / ViewWidthT) * (Props.Camera.ViewportSize.X));
        }

        // Per-track overloads: map using the track's own effective width.
        public float GetScreenPosX(double timeT, TrackProps tp)
        {
            return (float)((timeT / TrackViewWidthT(tp)) * (Props.Camera.ViewportSize.X));
        }
        public Vector2 GetScreenPos(int timeT, int pitch, TrackProps tp)
        {
            Vector2 p = new Vector2();
            p.X = GetScreenPosX(timeT, tp);
            p.Y = GetScreenPosY((float)pitch);
            return p;
        }
        public float GetSongPosP(TrackProps tp) => GetScreenPosX(SongPosT - PlaybackOffsetT, tp);

        public float GetScreenPosY(float pitch)
        {
            return (pitch - Props.MinPitch) * Props.NoteHeight + Props.NoteHeight / 2.0f + Props.PitchMargin - Props.Camera.ViewportSize.Y / 2;
        }
        public double PixelsToTicks(double screenX)
        { //Returns time in ticks
            return screenX / Props.Camera.ViewportSize.X * ViewWidthT; //Far right -> screenX = viewPortSize / 2
        }
        public double PixelsToTicks(double screenX, TrackProps tp)
        { //Returns time in ticks using the track's own effective width
            return screenX / Props.Camera.ViewportSize.X * TrackViewWidthT(tp);
        }

        public float SongLengthP => (float)(SongLengthT * Props.Camera.ViewportSize.X) / ViewWidthT;

        public int SmallScrollStepT => (int)(ViewWidthT / 16f);   // 1/16 of visible width
        public int LargeScrollStepT => (int)ViewWidthT;            // one full visible width

        public string DefaultFileName { get; set; }
        public string AudioFilePath { get; private set; }

        public const string DefaultFileExt = "vmp";

        public float GetCurveScreenY(float x, Curve curve)
        {
            //float pitch = curve.EvaluateCurvature((float)getTimeT(x));
            //return pitch / 100;
            float pitch = curve.Evaluate((float)PixelsToTicks(x));
            return GetScreenPosY(pitch);
        }

        public void TempPausePlayback()
        {
            if (IsPlaying)
            {
                _tempPausing = true;
                TogglePlayback();
            }
        }

        public void ResumeTempPausedPlayback()
        {
            if (!IsPlaying && _tempPausing)
            {
                _tempPausing = false;
                TogglePlayback();
            }
        }

        public Vector3 GetSpatialNormPosOffset(TrackProps trackProps)
        {
            Vector3 offset = NormalizeVpVector(GlobalTrackProps.SpatialProps.PosOffset + trackProps.SpatialProps.PosOffset);
            // Pitch offset is in note rows, so it scales with NoteHeight (already in viewport units)
            // rather than with NormalizeVpVector.
            float pitchOffset = (GlobalTrackProps.SpatialProps.PitchOffset ?? 0) + (trackProps.SpatialProps.PitchOffset ?? 0);
            offset.Y += pitchOffset * Props.NoteHeight;
            return offset;
        }

        public float NormalizeVpScalar(float value)
        {
            return value * Props.Camera.ViewportSize.X / Props.UserViewWidth;
        }
        public Vector3 NormalizeVpVector(Vector3 value)
        {
            return value * Props.Camera.ViewportSize.X / Props.UserViewWidth;
        }

        public double CurrentLyricsBeat
        {
            get
            {
                if (Notes == null || Notes.TicksPerBeat <= 0)
                    return 0;
                return Math.Max(0, (SongPosT - PlaybackOffsetT) / Notes.TicksPerBeat);
            }
        }

        public int LyricsBeatToTimelineTick(double beat)
        {
            double tpb = Notes?.TicksPerBeat ?? 480;
            return (int)Math.Round(Math.Max(0, beat) * tpb + PlaybackOffsetT);
        }

        public double TimelineTickToLyricsBeat(double tick)
        {
            double tpb = Notes?.TicksPerBeat ?? 480;
            if (tpb <= 0) return 0;
            return Math.Max(0, (tick - PlaybackOffsetT) / tpb);
        }

        public void SortLyrics()
        {
            if (Props.LyricsSegments.Count <= 1)
                return;

            var sorted = Props.LyricsSegments
                .OrderBy(s => s.Beat)
                .ToList();

            Props.LyricsSegments.RaiseListChangedEvents = false;
            try
            {
                Props.LyricsSegments.Clear();
                foreach (var segment in sorted)
                    Props.LyricsSegments.Add(segment);
            }
            finally
            {
                Props.LyricsSegments.RaiseListChangedEvents = true;
                Props.LyricsSegments.ResetBindings();
            }
        }

        public int InsertLyrics()
        {
            float beat = (float)CurrentLyricsBeat;
            for (int i = 0; i < Props.LyricsSegments.Count; i++)
            {
                if (Props.LyricsSegments[i].Beat >= beat)
                {
                    Props.LyricsSegments.Insert(i, new LyricsSegment(beat));
                    DrawHost?.Invalidate();
                    return i;
                }
            }
            Props.LyricsSegments.Add(new LyricsSegment(beat));
            DrawHost?.Invalidate();
            return Props.LyricsSegments.Count - 1;
        }

        /// <summary>
        /// Seeks to <paramref name="tick"/> (absolute song position in ticks).
        /// Used by the new per-property keyframe GUI.  Callers are responsible for
        /// calling ResyncPlaybackPosition when audio is already playing.
        /// </summary>
        public void GoToTick(int tick)
        {
            if (SongLengthT > 0)
                NormSongPos = Math.Max(0, Math.Min(1, (tick + 0.5) / SongLengthT));
        }

        // ---- Property accessor registry for per-property keyframe interpolation ----

        /// <summary>
        /// Builds the map from property IDs to live getter/setter pairs so that
        /// <see cref="InterpolatePropertyKeyframes"/> can apply interpolated values every frame.
        /// Must be called after <c>_trackViews</c> is populated (end of ImportSong /
        /// InitAfterDeserialization).
        /// </summary>
        public void InitPropertyAccessors()
        {
            _propAccessors.Clear();

            // Project-scope
            _propAccessors["proj/Camera"] = PropAccessor.Camera(() => Props.Camera);
            _propAccessors["proj/MaxPitch"] = PropAccessor.Scalar(
                () => Props.MaxPitch,
                v => Props.MaxPitch = (int)Math.Round(v),
                needsRebuild: true);
            _propAccessors["proj/MinPitch"] = PropAccessor.Scalar(
                () => Props.MinPitch,
                v => Props.MinPitch = (int)Math.Round(v),
                needsRebuild: true);
            _propAccessors["proj/BackgroundImageOpacity"] = PropAccessor.Scalar(
                () => Props.BackgroundImageOpacity,
                v => Props.BackgroundImageOpacity = (float)v);
            _propAccessors["proj/BackgroundImageSaturation"] = PropAccessor.Scalar(
                () => Props.BackgroundImageSaturation,
                v => Props.BackgroundImageSaturation = (float)v);
            _propAccessors["proj/BackgroundImagePath"] = PropAccessor.StringHold(
                () => Props.BackgroundImagePath,
                v => Props.BackgroundImagePath = v);
            _propAccessors["proj/AudioVisLeft"] = PropAccessor.Bool(
                () => Props.AudioVisLeft,
                v => Props.AudioVisLeft = v);
            _propAccessors["proj/AudioVisRight"] = PropAccessor.Bool(
                () => Props.AudioVisRight,
                v => Props.AudioVisRight = v);
            _propAccessors["proj/AudioVisWidth"] = PropAccessor.Scalar(
                () => Props.AudioVisWidth,
                v => Props.AudioVisWidth = (float)v);
            _propAccessors["proj/AudioVisLineWidth"] = PropAccessor.Scalar(
                () => Props.AudioVisLineWidth,
                v => Props.AudioVisLineWidth = (float)v);

            // Track-scope — one entry per track view.
            // Key by TrackNumber (the MIDI track index, stable across list reorder) and capture the
            // TrackView reference directly so lambdas remain bound after a reorder.
            if (_trackViews == null) return;
            for (int i = 0; i < _trackViews.Count; i++)
            {
                var tv = _trackViews[i];     // capture reference, not index
                int tn = tv.TrackNumber;     // stable id, never changes
                string prefix = $"track/{tn}";

                // Style
                _propAccessors[$"track/{tn}/StyleTypeIndex"] = PropAccessor.Scalar(
                    () => { var t = tv.TrackProps.StyleProps.Type; return t == null ? 0 : (double)(int)t; },
                    v => { tv.TrackProps.StyleProps.Type = (NoteStyleType)(int)Math.Round(v); },
                    needsRebuild: true, alwaysHold: true);
                _propAccessors[$"track/{tn}/LineWidth"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().LineWidth ?? 0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().LineWidth = (float)v; },
                    needsRebuild: true);
                _propAccessors[$"track/{tn}/QnGapThreshold"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().Qn_gapThreshold ?? 0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().Qn_gapThreshold = (float)v; },
                    needsRebuild: true);
                _propAccessors[$"track/{tn}/HlSize"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().HlSize ?? 0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().HlSize = (float)v; });
                _propAccessors[$"track/{tn}/HlMovementPow"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().HlMovementPow ?? 0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().HlMovementPow = (float)v; });
                _propAccessors[$"track/{tn}/LineTypeIndex"] = PropAccessor.Scalar(
                    () => { var t = tv.TrackProps.StyleProps.GetLineStyle().LineType; return t == null ? 0 : (double)(int)t; },
                    v => { tv.TrackProps.StyleProps.GetLineStyle().LineType = (LineType)(int)Math.Round(v); },
                    needsRebuild: true, alwaysHold: true);
                _propAccessors[$"track/{tn}/LineHlTypeIndex"] = PropAccessor.Scalar(
                    () => { var t = tv.TrackProps.StyleProps.GetLineStyle().HlType; return t == null ? 0 : (double)(int)t; },
                    v => { tv.TrackProps.StyleProps.GetLineStyle().HlType = (LineHlType)(int)Math.Round(v); },
                    alwaysHold: true);
                _propAccessors[$"track/{tn}/Continuous"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().Continuous == true ? 1.0 : 0.0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().Continuous = v >= 0.5; },
                    needsRebuild: true, alwaysHold: true);
                _propAccessors[$"track/{tn}/MovingHl"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().MovingHl == true ? 1.0 : 0.0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().MovingHl = v >= 0.5; },
                    alwaysHold: true);
                _propAccessors[$"track/{tn}/ShrinkingHl"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().ShrinkingHl == true ? 1.0 : 0.0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().ShrinkingHl = v >= 0.5; },
                    alwaysHold: true);
                _propAccessors[$"track/{tn}/HlBorder"] = PropAccessor.Scalar(
                    () => tv.TrackProps.StyleProps.GetLineStyle().HlBorder == true ? 1.0 : 0.0,
                    v => { tv.TrackProps.StyleProps.GetLineStyle().HlBorder = v >= 0.5; },
                    alwaysHold: true);

                // Material
                _propAccessors[$"{prefix}/Transp"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.Transp ?? 0,
                    v => tv.TrackProps.MaterialProps.Transp = (float)v);
                _propAccessors[$"{prefix}/MaterialHue"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.Hue ?? 0,
                    v => tv.TrackProps.MaterialProps.Hue = (float)v);
                _propAccessors[$"{prefix}/NormalSat"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.Normal.Sat ?? 0,
                    v => tv.TrackProps.MaterialProps.Normal.Sat = (float)v);
                _propAccessors[$"{prefix}/NormalLum"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.Normal.Lum ?? 0,
                    v => tv.TrackProps.MaterialProps.Normal.Lum = (float)v);
                _propAccessors[$"{prefix}/HiliteSat"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.Hilited.Sat ?? 0,
                    v => tv.TrackProps.MaterialProps.Hilited.Sat = (float)v);
                _propAccessors[$"{prefix}/HiliteLum"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.Hilited.Lum ?? 0,
                    v => tv.TrackProps.MaterialProps.Hilited.Lum = (float)v);
                _propAccessors[$"{prefix}/TexturePath"] = PropAccessor.StringHold(
                    () => tv.TrackProps.MaterialProps.TexProps.Path ?? "",
                    v => ApplyTrackTexturePath(tv, v));
                _propAccessors[$"{prefix}/DisableTexture"] = PropAccessor.Bool(
                    () => tv.TrackProps.MaterialProps.TexProps.DisableTexture,
                    v => tv.TrackProps.MaterialProps.TexProps.DisableTexture = v);
                _propAccessors[$"{prefix}/PointSmp"] = PropAccessor.Bool(
                    () => tv.TrackProps.MaterialProps.TexProps.PointSmp,
                    v => tv.TrackProps.MaterialProps.TexProps.PointSmp = v);
                _propAccessors[$"{prefix}/TexColBlend"] = PropAccessor.Bool(
                    () => tv.TrackProps.MaterialProps.TexProps.TexColBlend,
                    v => tv.TrackProps.MaterialProps.TexProps.TexColBlend = v);
                _propAccessors[$"{prefix}/UTile"] = PropAccessor.Bool(
                    () => tv.TrackProps.MaterialProps.TexProps.UTile,
                    v => tv.TrackProps.MaterialProps.TexProps.UTile = v,
                    needsRebuild: true);
                _propAccessors[$"{prefix}/VTile"] = PropAccessor.Bool(
                    () => tv.TrackProps.MaterialProps.TexProps.VTile,
                    v => tv.TrackProps.MaterialProps.TexProps.VTile = v,
                    needsRebuild: true);
                _propAccessors[$"{prefix}/KeepAspect"] = PropAccessor.Bool(
                    () => tv.TrackProps.MaterialProps.TexProps.KeepAspect,
                    v => tv.TrackProps.MaterialProps.TexProps.KeepAspect = v,
                    needsRebuild: true);
                _propAccessors[$"{prefix}/UAnchorIndex"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.TexProps.UAnchor == null
                        ? 0 : (double)(int)tv.TrackProps.MaterialProps.TexProps.UAnchor.Value,
                    v => tv.TrackProps.MaterialProps.TexProps.UAnchor =
                        (TexAnchorEnum)Math.Clamp((int)Math.Round(v), 0, 2),
                    needsRebuild: true, alwaysHold: true);
                _propAccessors[$"{prefix}/VAnchorIndex"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.TexProps.VAnchor == null
                        ? 0 : (double)(int)tv.TrackProps.MaterialProps.TexProps.VAnchor.Value,
                    v => tv.TrackProps.MaterialProps.TexProps.VAnchor =
                        (TexAnchorEnum)Math.Clamp((int)Math.Round(v), 0, 1),
                    needsRebuild: true, alwaysHold: true);
                _propAccessors[$"{prefix}/UScroll"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.TexProps.UScroll ?? 0,
                    v => tv.TrackProps.MaterialProps.TexProps.UScroll = (float)v);
                _propAccessors[$"{prefix}/VScroll"] = PropAccessor.Scalar(
                    () => tv.TrackProps.MaterialProps.TexProps.VScroll ?? 0,
                    v => tv.TrackProps.MaterialProps.TexProps.VScroll = (float)v);

                // Light
                _propAccessors[$"{prefix}/UseGlobalLight"] = PropAccessor.Bool(
                    () => tv.TrackProps.LightProps.UseGlobalLight,
                    v => tv.TrackProps.LightProps.UseGlobalLight = v);
                _propAccessors[$"{prefix}/LightDirX"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.DirX ?? 0,
                    v => tv.TrackProps.LightProps.DirX = (float)v);
                _propAccessors[$"{prefix}/LightDirY"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.DirY ?? 0,
                    v => tv.TrackProps.LightProps.DirY = (float)v);
                _propAccessors[$"{prefix}/LightDirZ"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.DirZ ?? 0,
                    v => tv.TrackProps.LightProps.DirZ = (float)v);
                _propAccessors[$"{prefix}/AmbientAmount"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.AmbientAmount ?? 0,
                    v => tv.TrackProps.LightProps.AmbientAmount = (float)v);
                _propAccessors[$"{prefix}/AmbientColor"] = PropAccessor.Color(
                    () => tv.TrackProps.LightProps.AmbientColor ?? Color.White,
                    v => tv.TrackProps.LightProps.AmbientColor = v);
                _propAccessors[$"{prefix}/DiffuseAmount"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.DiffuseAmount ?? 0,
                    v => tv.TrackProps.LightProps.DiffuseAmount = (float)v);
                _propAccessors[$"{prefix}/DiffuseColor"] = PropAccessor.Color(
                    () => tv.TrackProps.LightProps.DiffuseColor ?? Color.White,
                    v => tv.TrackProps.LightProps.DiffuseColor = v);
                _propAccessors[$"{prefix}/SpecAmount"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.SpecAmount ?? 0,
                    v => tv.TrackProps.LightProps.SpecAmount = (float)v);
                _propAccessors[$"{prefix}/SpecColor"] = PropAccessor.Color(
                    () => tv.TrackProps.LightProps.SpecColor ?? Color.White,
                    v => tv.TrackProps.LightProps.SpecColor = v);
                _propAccessors[$"{prefix}/SpecPower"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.SpecPower ?? 1,
                    v => tv.TrackProps.LightProps.SpecPower = (float)v);
                _propAccessors[$"{prefix}/MasterAmount"] = PropAccessor.Scalar(
                    () => tv.TrackProps.LightProps.MasterAmount ?? 0,
                    v => tv.TrackProps.LightProps.MasterAmount = (float)v);
                _propAccessors[$"{prefix}/MasterColor"] = PropAccessor.Color(
                    () => tv.TrackProps.LightProps.MasterColor ?? Color.White,
                    v => tv.TrackProps.LightProps.MasterColor = v);

                // Spatial
                _propAccessors[$"{prefix}/XOffset"] = PropAccessor.Scalar(
                    () => tv.TrackProps.SpatialProps.XOffset ?? 0,
                    v => tv.TrackProps.SpatialProps.XOffset = (float)v);
                _propAccessors[$"{prefix}/YOffset"] = PropAccessor.Scalar(
                    () => tv.TrackProps.SpatialProps.YOffset ?? 0,
                    v => tv.TrackProps.SpatialProps.YOffset = (float)v);
                _propAccessors[$"{prefix}/ZOffset"] = PropAccessor.Scalar(
                    () => tv.TrackProps.SpatialProps.ZOffset ?? 0,
                    v => tv.TrackProps.SpatialProps.ZOffset = (float)v);
                _propAccessors[$"{prefix}/PitchOffset"] = PropAccessor.Scalar(
                    () => tv.TrackProps.SpatialProps.PitchOffset ?? 0,
                    v => tv.TrackProps.SpatialProps.PitchOffset = (float)v);
                _propAccessors[$"{prefix}/ViewWidthQn"] = PropAccessor.Scalar(
                    () => EffectiveViewWidthQn(tv.TrackProps),
                    v => tv.TrackProps.SpatialProps.ViewWidthQn = (float)v,
                    logScale: true);    // no needsRebuild — width renders via shader scale, as before

                // Audio
                _propAccessors[$"{prefix}/SilenceThreshold"] = PropAccessor.Scalar(
                    () => EffectiveSilenceThresholdS(tv.TrackProps),
                    v => tv.TrackProps.AudioProps.SilenceThresholdS = (float)v);
                    // no needsRebuild — the effective threshold feeds the waveform via RefreshSidWizChannels
            }
        }

        bool ApplyTrackTexturePath(TrackView tv, string path, bool rebuild = true)
        {
            if (tv == null) return false;
            var texProps = tv.TrackProps.MaterialProps.TexProps;
            path ??= "";
            if (string.Equals(texProps.Path ?? "", path, StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                if (string.IsNullOrEmpty(path))
                    texProps.UnloadTexture();
                else if (DrawHost != null)
                    texProps.LoadTexture(path, DrawHost);
                else
                    texProps.Path = path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply texture keyframe '{path}': {ex.Message}");
                texProps.UnloadTexture();
                texProps.Path = path;
            }

            if (rebuild)
                CreateGeos(resetVertScale: false);
            return true;
        }

        TrackView FindTrackViewByTrackNumber(int trackNumber)
        {
            if (_trackViews == null) return null;
            foreach (var t in _trackViews)
                if (t.TrackNumber == trackNumber) return t;
            return null;
        }

        bool ApplyMaterialTexturePathKeyframes(string id, Keyframes.PropertyKeyframeTrack track)
        {
            var parts = id.Split('/');
            if (parts.Length != 3 || parts[0] != "track" || parts[2] != "TexturePath")
                return false;
            if (!int.TryParse(parts[1], out int tn)) return false;

            var tv = FindTrackViewByTrackNumber(tn);
            if (tv == null) return false;

            var (before, after, t) = track.FindBrackets((int)SongPosT);
            if (before?.Value == null && after?.Value == null) return false;

            string pathA = (before?.Value as Keyframes.StringKfValue)?.S ?? "";
            string pathB = (after?.Value as Keyframes.StringKfValue)?.S ?? "";
            var texProps = tv.TrackProps.MaterialProps.TexProps;

            bool changed = false;
            if (before == null || before.Value == null)
            {
                changed |= ApplyTrackTexturePath(tv, pathB, rebuild: false);
                changed |= texProps.SetTextureTransition("", 0f, DrawHost);
            }
            else if (after == null || after.Value == null
                     || before.Interpolation == Keyframes.KfInterpolation.Hold)
            {
                changed |= ApplyTrackTexturePath(tv, pathA, rebuild: false);
                changed |= texProps.SetTextureTransition("", 0f, DrawHost);
            }
            else
            {
                float blend = before.Interpolation == Keyframes.KfInterpolation.Smooth
                    ? (float)(t * t * (3.0 - 2.0 * t))
                    : (float)t;
                changed |= ApplyTrackTexturePath(tv, pathA, rebuild: false);
                changed |= texProps.SetTextureTransition(pathB, blend, DrawHost);
            }

            return changed;
        }

        /// <summary>
        /// Resolves a full property id to a live accessor.  Pre-registered ids (proj/*, non-mod track/*)
        /// are returned directly.  Modulation ids of the form <c>track/{tn}/mod/{eid}/{name}</c> are
        /// resolved dynamically by finding the TrackView and entry on demand (robust to add/delete/reorder).
        /// Returns null if the id is unresolvable (orphan, wrong track type, etc.).
        /// </summary>
        PropAccessor ResolveAccessor(string id)
        {
            if (_propAccessors.TryGetValue(id, out var a)) return a;

            // Modulation: track/{tn}/mod/{eid}/{name}
            var parts = id.Split('/');
            if (parts.Length != 5 || parts[0] != "track" || parts[2] != "mod") return null;
            if (!int.TryParse(parts[1], out int tn)) return null;
            string eid = parts[3];
            string name = parts[4];

            if (!_modPropTable.TryGetValue(name, out var factory)) return null;

            // Find the TrackView by stable TrackNumber
            TrackView tv = FindTrackViewByTrackNumber(tn);
            if (tv == null) return null;

            // Find the mod entry by stable Id
            NoteStyleMod entry = null;
            var modEntries = tv.TrackProps.ActiveNoteStyle?.ModEntries;
            if (modEntries != null)
                foreach (var m in modEntries)
                    if (m.Id == eid) { entry = m; break; }
            if (entry == null) return null;

            return factory(entry);
        }

        /// <summary>Returns the live value for a full property id, or null if unresolvable.</summary>
        public Keyframes.KfValue GetCurrentValue(string fullId)
            => ResolveAccessor(fullId)?.Capture();

        /// <summary>True when the new keyframe set has any track-scoped keyframes.</summary>
        public bool HasTrackKeyframes
            => PropertyKeyframes?.AllTicks().Any() == true
            && PropertyKeyframes.Tracks.Keys.Any(k => k.StartsWith("track/"));

        /// <summary>
        /// Applies the new per-property keyframe set by interpolating each tracked property to its
        /// value at the current song position.  Called inside <see cref="Update"/> (during playback)
        /// and from the video-export loop so exports animate too.
        /// </summary>
        public void InterpolatePropertyKeyframes()
        {
            if (PropertyKeyframes == null) return;
            bool anyRebuildNeeded = false;

            // ---- Background-image crossfade — special-cased because the crossfade needs both
            // bracket values and the interpolant simultaneously, which the generic Apply can't express.
            if (PropertyKeyframes.Tracks.TryGetValue("proj/BackgroundImagePath", out var bkgTrack))
            {
                var (bkgBefore, bkgAfter, bkgT) = bkgTrack.FindBrackets((int)SongPosT);
                string pathA = (bkgBefore?.Value as Keyframes.StringKfValue)?.S ?? "";
                string pathB = (bkgAfter?.Value as Keyframes.StringKfValue)?.S ?? "";
                bool hold = bkgBefore == null || bkgAfter == null
                               || bkgBefore.Interpolation == Keyframes.KfInterpolation.Hold;
                float blend = hold ? 0f
                               : bkgBefore.Interpolation == Keyframes.KfInterpolation.Smooth
                                   ? (float)(bkgT * bkgT * (3.0 - 2.0 * bkgT))
                                   : (float)bkgT;
                string effectiveA = bkgBefore?.Value != null ? pathA : pathB;
                string effectiveB = hold ? null : pathB;
                DrawHost?.SetBackgroundCrossfade(effectiveA, effectiveB, blend);
            }

            foreach (var kv in PropertyKeyframes.Tracks)
            {
                string id = kv.Key;
                var track = kv.Value;

                // Background path is handled by the special-case block above (crossfade needs the brackets);
                // skip it here to avoid overwriting Props.BackgroundImagePath with just the "after" value.
                if (id == "proj/BackgroundImagePath") continue;

                if (id.StartsWith("track/", StringComparison.Ordinal)
                    && id.EndsWith("/TexturePath", StringComparison.Ordinal))
                {
                    if (ApplyMaterialTexturePathKeyframes(id, track))
                        anyRebuildNeeded = true;
                    continue;
                }

                var acc = ResolveAccessor(id);
                if (acc == null) continue;

                var (before, after, t) = track.FindBrackets((int)SongPosT);

                // Skip if neither surrounding keyframe carries a captured value
                if (before?.Value == null && after?.Value == null) continue;

                Keyframes.KfValue value;
                if (before == null || before.Value == null)
                    value = after.Value;
                else if (after == null || after.Value == null)
                    value = before.Value;
                else
                {
                    // Enum/bool props must step at the keyframe, never blend to a fractional value.
                    var mode = acc.AlwaysHold ? Keyframes.KfInterpolation.Hold : before.Interpolation;
                    value = acc.Interp(before.Value, after.Value, t, mode);
                }

                if (acc.NeedsRebuild && value is Keyframes.ScalarKfValue sv)
                {
                    var prevKv = acc.Capture() as Keyframes.ScalarKfValue;
                    acc.Apply(value);
                    if (prevKv == null || Math.Abs(sv.V - prevKv.V) > 1e-6)
                        anyRebuildNeeded = true;
                }
                else
                {
                    acc.Apply(value);
                }
            }

            if (anyRebuildNeeded) CreateGeos(resetVertScale: false);
        }

        // ---- End property-keyframe interpolation ----

        /// <summary>
        /// Writes the live <see cref="Props.Camera"/> into the per-property camera keyframe at the
        /// current tick after user-driven movement (WASD, mouse-look).
        /// </summary>
        public void SyncLiveCameraEdit()
        {
            var cam = Props.Camera;
            if (PropertyKeyframes?.HasAny("proj/Camera") == true
                && Keyframes.KeyframeService.HasKeyHereForAll("Camera", Keyframes.KeyframeService.KfScope.Project))
            {
                Keyframes.KeyframeService.SyncEditedValue("Camera", Keyframes.KeyframeService.KfScope.Project,
                    new Keyframes.CameraKfValue(cam.Pos, cam.Orientation, cam.Fov));
            }
        }

        void OnPlaybackOffsetSChanged()
        {
            if (_notes == null)
                return;
            _firstTempoEvent = _pbTempoEvent = 0;
            _pbTimeS = _pbTimeT = _playbackOffsetT = 0;

            //Set playbackOffsetT
            if (Props.PlaybackOffsetS >= 0)
                _playbackOffsetT = (float)(Props.PlaybackOffsetS * _notes.TempoEvents[0].Tempo / 60 * _notes.TicksPerBeat);
            else
                _playbackOffsetT = (float)-SecondsToTicks((double)-Props.PlaybackOffsetS);

            //Set firstTempoEvent (if playback offset is negative, playback may start after second event in which firstTempoEvent shouldn't be zero)
            if (Props.PlaybackOffsetS < 0)
            {
                double offsetT = -_playbackOffsetT;
                for (int i = 0; i < _notes.TempoEvents.Count; i++)
                {
                    if (_notes.TempoEvents[i].Time <= offsetT)
                        _firstTempoEvent = i;
                    else
                        break;
                }
            }

            _pbTimeS = _pbTimeT = 0;
            _pbTempoEvent = _firstTempoEvent;
            SongLengthS = NormSongPosToSeconds(1);
        }

        /// <summary>
        /// Deep-clones the project. By default the clone shares each track's live AudioProps
        /// (and thus the loaded SidWizChannel sample buffers) so e.g. the video-export clone can
        /// render waveforms without reloading audio. Pass <paramref name="shareAudioProps"/> =
        /// false for undo snapshots: the clone then keeps its own deserialized AudioProps
        /// (correct Filename + SilenceThresholdS, fresh empty channel) so later edits to the live
        /// project don't retroactively mutate the snapshot.
        /// </summary>
        public Project Clone(bool shareAudioProps = true)
        {
            Project dest = Cloning.Clone(this);

            for (int i = 0; i < _trackViews.Count; i++)
            {
                //dest.trackViews[i] = trackViews[i].clone();
                //dest.TrackViews[i].TrackProps.GlobalProps = dest.TrackViews[0].TrackProps;
                dest._trackViews[i].MidiTrack = _trackViews[i].MidiTrack;
                dest._trackViews[i].Geo = _trackViews[i].Geo;
                dest._trackViews[i].Curve = _trackViews[i].Curve;
                if (shareAudioProps)
                    dest._trackViews[i].TrackProps.AudioProps = _trackViews[i].TrackProps.AudioProps;
            }

            dest._notes = _notes;
            //dest.Props = Props.clone();
            //dest.vertViewWidthQn = vertViewWidthQn;
            dest.Props.OnPlaybackOffsetSChanged = dest.OnPlaybackOffsetSChanged;
            dest.Props.OnPlaybackOffsetSChanged();
            dest.InitPropertyAccessors();
            return dest;
        }

        public void Dispose()
        {
            // Only called on undo snapshots, which share AudioProps with the live project via clone().
            // Disposing AudioProps here would dispose the live SampleBuffer/AudioFileReader and cause
            // NREs the next time a chunk is loaded (e.g. after seeking).
            foreach (var tv in _trackViews)
            {
                tv.Geo?.Dispose();
            }
        }

        public void CopyPropsFrom(Project project)
        {
            var source = project.Clone();
            Props = source.Props;
            //TrackViews = source.TrackViews;
            for (int i = 0; i < TrackViews.Count; i++)
            {
                var destProps = TrackViews[i].TrackProps;
                var sourceProps = source.TrackViews[i].TrackProps;
                destProps.StyleProps = sourceProps.StyleProps;
                destProps.MaterialProps = sourceProps.MaterialProps;
                destProps.LightProps = sourceProps.LightProps;
                destProps.SpatialProps = sourceProps.SpatialProps;
                // The live AudioProps object is kept (WaveformPanel holds its SidWizChannel), but
                // its undoable values are restored from the snapshot. The effective silence
                // threshold is re-pushed by RefreshSidWizChannels.
                destProps.AudioProps.SilenceThresholdS = sourceProps.AudioProps.SilenceThresholdS;
                // Reload audio only for tracks whose filename actually changed, so ordinary
                // undo/redo steps stay cheap.
                string oldFn = destProps.AudioProps.Filename ?? "";
                string newFn = sourceProps.AudioProps.Filename ?? "";
                if (!string.Equals(oldFn, newFn, StringComparison.OrdinalIgnoreCase))
                {
                    destProps.AudioProps.Filename = newFn;
                    _ = destProps.AudioProps.LoadAudioAsync();
                }
            }
            PropertyKeyframes = source.PropertyKeyframes;
            //source.Dispose();
        }

        internal void InitAfterDeserialization(WaveformPanel waveformPanel = null)
        {
            if (PropertyKeyframes == null)
                PropertyKeyframes = new Keyframes.KeyframeSet();
            ImportOptions.UpdateImportForm();
            var wp = waveformPanel ?? DrawHost?.WaveformPanel;
            wp.ClearChannels();

            for (int i = 0; i < TrackViews.Count; i++)
            {
                var tv = TrackViews[i];
                tv.TrackProps.LoadContent();
                if (i > 0)
                {
                    _ = tv.TrackProps.AudioProps.LoadAudioAsync();
                    wp.AddChannel(tv.TrackProps.AudioProps.SidWizChannel);
                }
            }

            // Force recalculation of derived state (SongLengthS, playbackOffsetT, etc).
            // During deserialization, Props was set BEFORE notes were loaded, so the
            // playback-offset callback fired with notes==null and returned early.
            // Do this explicitly so loaded projects get the correct SongLengthS.
            OnPlaybackOffsetSChanged();
            InitPropertyAccessors();
        }
    }

    static class SongFormat
    {
        public const int writeVersion = 1;
        public static int readVersion;
    }

    [Serializable]
    public class LyricsSegment : ISerializable
    {
        public float Beat { get; set; }
        public string Lyrics { get; set; } = "";

        public LyricsSegment(float beat)
        {
            Beat = beat;
        }

        public LyricsSegment(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "beat")
                    Beat = (float)entry.Value;
                else if (entry.Name == "lyrics")
                    Lyrics = (string)entry.Value ?? "";
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("beat", Beat);
            info.AddValue("lyrics", Lyrics ?? "");
        }
    }
}

