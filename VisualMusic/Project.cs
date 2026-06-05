using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
    using GdiPoint = System.Drawing.Point;
    public enum SourceSongType { Midi, Mod, Sid };

    [Serializable()]
    public class Project : ISerializable, IDisposable
    {
        public KeyFrames KeyFrames;
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
        public static ISongDrawHost StaticDrawHost => s_drawHostOverride ?? Form1.SongPanel;
        ISongDrawHost DrawHost => StaticDrawHost;

        TimeSpan _pbStartSysTime = new TimeSpan(0);
        double _pbStartSongTimeS;
        public float ViewWidthT => _notes == null ? 0 : Props.ViewWidthQn * _notes.TicksPerBeat; //Number of ticks that fits on screen
        float _vertViewWidthQn;
        public float ViewWidthQnScale => _vertViewWidthQn / Props.ViewWidthQn;

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
            KeyFrames = new KeyFrames();
            Props.ViewWidthQn = KeyFrames[0].ProjProps.ViewWidthQn;
            Props.OnPlaybackOffsetSChanged = OnPlaybackOffsetSChanged;
        }


        public Task LoadContent() => LoadContent(null);

        async public Task LoadContent(Form parentForm)
        {
            if (ImportOptions == null)
                return;

            ImportOptions.SetNotePath();
            ImportOptions.EraseCurrent = false;
            await ImportSong(ImportOptions, parentForm);
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
                    }
                }

                else if (entry.Name == "keyFrames")
                    KeyFrames = (KeyFrames)entry.Value;
                else if (entry.Name == "propertyKeyframes")
                    PropertyKeyframes = (Keyframes.KeyframeSet)entry.Value ?? PropertyKeyframes;
                else if (entry.Name == "props")
                    Props = (ProjProps)entry.Value;
                else if (entry.Name == "vertWidthQn")
                    _vertViewWidthQn = (float)entry.Value;

                //Compatibility
                else if (entry.Name == "qn_viewWidth")
                {
                    Props.ViewWidthQn = (float)entry.Value;
                    _vertViewWidthQn = Props.ViewWidthQn;
                }
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
                    KeyFrames[0].ProjProps.Camera = (Camera)entry.Value;
                else if (entry.Name == "userViewWidth")
                    Props.UserViewWidth = (float)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("version", SongFormat.writeVersion);
            info.AddValue("importOptions", ImportOptions);
            info.AddValue("trackViews", _trackViews);
            info.AddValue("keyFrames", KeyFrames);
            info.AddValue("propertyKeyframes", PropertyKeyframes);
            info.AddValue("props", Props);
            info.AddValue("vertWidthQn", _vertViewWidthQn);
        }

        async public Task<bool> ImportSong(ImportOptions options, Form parentForm)
        { //`Open project` and `import files` meet here
            options.CheckSourceFile();
            //Convert mod/sid files to mid/wav
            if (options.NoteFileType != Midi.FileType.Midi)
            {
                string noteFile = Path.GetFileName(options.NotePath);
                string midiPath = null, midiArg = null, audioPath = null, audioArg = null;

                //Should midi file be created?
                if (!options.SavedMidi)
                {
                    midiPath = Path.Combine(Program.TempDir, noteFile) + ".mid";
                    midiArg = $"-m\"{midiPath}\"";
                    File.Delete(midiPath);
                }
                else
                    midiPath = options.MidiOutputPath;

                //Should audio file be created?
                if (options.MixdownType == Midi.MixdownType.Internal)
                {
                    audioPath = Path.Combine(Program.TempDir, noteFile) + ".wav";
                    audioArg = $"-a\"{audioPath}\"";
                    File.Delete(audioPath);
                }
                else if (options.MixdownType == Midi.MixdownType.None)
                    audioPath = options.AudioPath;

                //Does either midi or audio need to be created?
                if (midiArg != null || audioArg != null)
                {
                    string insTrackFlag = options.InsTrack ? "-i" : "";
                    string songLengthsFlag = $"-l{options.SongLengthS.ToString()}";
                    string subSongFlag = $"-s{options.SubSong.ToString()}";
                    string supressErrorFlag = "-e";
                    string cmdLine = $"\"{options.NotePath}\" {midiArg} {audioArg} {insTrackFlag} {songLengthsFlag} {subSongFlag} {supressErrorFlag}";
                    var workingDir = Path.Combine(Program.Dir, "remuxer");
                    var startInfo = new ProcessStartInfo(Path.Combine(workingDir, "remuxer.exe"), cmdLine);
                    startInfo.WorkingDirectory = workingDir;
                    var process = Process.Start(startInfo);
                    Form1.RemuxerProcess = process;
                    if (parentForm != null)
                        parentForm.Enabled = false;
                    try
                    {
                        FormWindowState initialWindowState = Program.form1?.WindowState ?? FormWindowState.Normal;
                        await Task.Run(() =>
                        {
                            bool wasMinimized = false;
                            while (!process.HasExited)
                            {
                                if (Program.form1 != null)
                                {
                                    bool isMinimized = IsIconic(process.MainWindowHandle);
                                    if (isMinimized && !wasMinimized)
                                        Program.form1.Invoke(new Action(() => Program.form1.WindowState = FormWindowState.Minimized));
                                    else if (!isMinimized && wasMinimized)
                                    {
                                        Program.form1.Invoke(new Action(() => Program.form1.WindowState = initialWindowState));
                                        Form1.RegainFocus(process);
                                    }
                                    wasMinimized = isMinimized;
                                }
                                Thread.Sleep(200);
                            }
                        });
                    }
                    finally
                    {
                        if (parentForm != null)
                            parentForm.Enabled = true;
                        Program.form1?.Activate();
                    }
                    if (process.ExitCode != 0)
                        throw new FileImportException(null, ImportError.Corrupt, ImportFileType.Note, options.RawNotePath);

                    if (!File.Exists(midiPath) && !File.Exists(audioPath))
                        return false;
                }
                options.MidiOutputPath = midiPath;
                options.AudioPath = audioPath;
            }
            else if (options.MixdownType == Midi.MixdownType.Internal)
            {
                string audioPath = Path.Combine(Program.TempDir, Path.GetFileName(options.NotePath)) + ".wav";
                MidMix.Mixdown(options.NotePath, audioPath);
                options.AudioPath = audioPath;
            }

            OpenNoteFile(options);
            OpenAudioFile(options);

            ImportOptions = options;
            if (options.EraseCurrent)
            {
                DefaultFileName = Path.GetFileName(ImportOptions.NotePath) + "." + DefaultFileExt;
                Props.ViewWidthQn = _vertViewWidthQn = ProjProps.DefaultViewWidthQn;
            }
            CreateTrackViews(_notes.Tracks.Count, options.EraseCurrent);
            return true;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        public bool OpenNoteFile(ImportOptions options)
        {
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
            if (options.NoteFileType == Midi.FileType.Midi && !options.InsTrack)
                SplitTracksByChannel(_notes);
            _notes.CreateNoteBsp();

            if (options.EraseCurrent)
            {
                KeyFrames = new KeyFrames();
                Props.AudioOffset = Props.PlaybackOffsetS = Props.FadeIn = Props.FadeOut = 0;
                NormSongPos = 0;
                ResetPitchLimits();
            }
            //viewWidthT = (int)(ViewWidthQn * notes.TicksPerBeat);
            return true;
        }

        public void InterpolateFrames()
        {
            var interpolatedFrame = KeyFrames.CreateInterpolatedFrame((int)SongPosT);
            Props.ViewWidthQn = interpolatedFrame.ProjProps.ViewWidthQn;
            Props.Camera = interpolatedFrame.ProjProps.Camera;
            Props.BackgroundImageOpacity = interpolatedFrame.ProjProps.BackgroundImageOpacity;
            _props.BackgroundImageSaturation = interpolatedFrame.ProjProps.BackgroundImageSaturation;
            //Props = interpolatedFrame.ProjProps;
        }

        public void OpenAudioFile(ImportOptions options)
        {
            Media.CloseAudioFile();
            string file = options.AudioPath;

            //Third-party mixdown needed?
            if (options.MixdownType == Midi.MixdownType.Tparty)
                file = ImportNotesWithAudioForm.RunTpartyProcess(options);

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
                    Name   = $"Channel {kv.Key + 1}"
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
            view.TrackProps.AudioProps.LineColor = view.TrackProps.MaterialProps.GetSysColor(true, view.TrackProps.GlobalProps.MaterialProps);
            view.TrackProps.AudioProps.SidWizChannel.Filename = "";
            DrawHost?.WaveformPanel?.AddChannel(view.TrackProps.AudioProps.SidWizChannel);
            if (view.TrackNumber == 1)
                view.TrackProps.AudioProps.SidWizChannel.LoadDataAsync();
        }

        public void CreateGeos(bool resetVertScale = true)
        {
            if (_trackViews == null || Props.ViewWidthQn == 0 || Notes == null)
                return;
            float viewWidthQnBackup = Props.ViewWidthQn;
            if (resetVertScale)
                _vertViewWidthQn = Props.ViewWidthQn;
            else
                Props.ViewWidthQn = _vertViewWidthQn;
            for (int i = 1; i < _trackViews.Count; i++)
                TrackViews[i].CreateGeo(this, GlobalTrackProps);
            Props.ViewWidthQn = viewWidthQnBackup;
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

            var effect = new BasicEffect(host.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };
            Viewport viewport = host.GraphicsDevice.Viewport;
            effect.Projection = Props.Camera.ProjMat;
            effect.View = Matrix.CreateTranslation(new Vector3(0, 0, -Props.Camera.ProjMat.M11 / 2));
            Vector2 songPanelSize = new Vector2(host.ClientWidth, host.ClientHeight);
            Vector2 scale = new Vector2(Props.Camera.ViewportSize.X / songPanelSize.X, -Props.Camera.ViewportSize.Y / songPanelSize.Y);

            host.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, RasterizerState.CullNone, effect, null);
            foreach (var lyricsSegment in Props.LyricsSegments)
            {
                if (string.IsNullOrWhiteSpace(lyricsSegment.Lyrics))
                    continue;
                float textHeight = -host.LyricsFont.MeasureString(lyricsSegment.Lyrics).Y * scale.Y;
                host.SpriteBatch.DrawString(host.LyricsFont, lyricsSegment.Lyrics, new Vector2(-SongPosP + GetScreenPosX(SecondsToTicks(lyricsSegment.Time) + PlaybackOffsetT), -Props.Camera.ViewportSize.Y / 2 + textHeight), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }

            host.SpriteBatch.End();
            host.WaveformPanel?.Draw(SongPosS - Props.PlaybackOffsetS);
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
            outProps.AudioProps = new AudioProps { Filename = outProps.AudioProps.Filename, LineColor = outProps.AudioProps.LineColor };
            for (int i = 1; i < list.Count; i++)
                outProps = (TrackProps)MergeObjects(outProps, TrackViews[list[i]].TrackProps);
            return outProps;
        }

        public TrackProps MergeTrackProps(ListView.SelectedIndexCollection listIndices)
        {
            if (listIndices.Count == 0)
                return null;
            TrackProps outProps = TrackViews[listIndices[0]].TrackProps;
            if (listIndices.Count == 1)
                return outProps;
            outProps = outProps.Clone(DrawHost);
            // TrackProps.cloneFrom shares the AudioProps reference with the source track.
            // Detach it here so mergeObjects can null out Filename without corrupting the source.
            outProps.AudioProps = new AudioProps { Filename = outProps.AudioProps.Filename, LineColor = outProps.AudioProps.LineColor };
            for (int i = 1; i < listIndices.Count; i++)
                outProps = (TrackProps)MergeObjects(outProps, TrackViews[listIndices[i]].TrackProps);
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

        public void ResetTrackProps(ListView.SelectedIndexCollection indices)
        {
            if (indices != null)
            {
                foreach (int index in indices)
                    _trackViews[index].TrackProps.ResetProps();
            }
            else
                _trackViews[0].TrackProps.ResetProps();
            CreateGeos();
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
            foreach (var keyFrame in KeyFrames.Values)
            {
                if (keyFrame.Selected)
                    keyFrame.ProjProps.Camera.Update(deltaTimeS);
            }
            InterpolateFrames();

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

        public float GetScreenPosY(float pitch)
        {
            return (pitch - Props.MinPitch) * Props.NoteHeight + Props.NoteHeight / 2.0f + Props.PitchMargin - Props.Camera.ViewportSize.Y / 2;
        }
        public double PixelsToTicks(double screenX)
        { //Returns time in ticks
            return screenX / Props.Camera.ViewportSize.X * ViewWidthT; //Far right -> screenX = viewPortSize / 2
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
            return NormalizeVpVector(GlobalTrackProps.SpatialProps.PosOffset + trackProps.SpatialProps.PosOffset);
        }

        public float NormalizeVpScalar(float value)
        {
            return value * Props.Camera.ViewportSize.X / Props.UserViewWidth;
        }
        public Vector3 NormalizeVpVector(Vector3 value)
        {
            return value * Props.Camera.ViewportSize.X / Props.UserViewWidth;
        }

        public int InsertLyrics()
        {
            for (int i = 0; i < Props.LyricsSegments.Count; i++)
            {
                if (Props.LyricsSegments[i].Time >= SongPosS)
                {
                    Props.LyricsSegments.Insert(i, new LyricsSegment((float)SongPosS));
                    return i;
                }
            }
            Props.LyricsSegments.Add(new LyricsSegment((float)SongPosS));
            return Props.LyricsSegments.Count - 1;
        }

        public int InsertKeyFrameAtSongPos()
        {
            return KeyFrames.Insert((int)SongPosT);
        }

        public void GoToKeyFrame(int index)
        {
            int newPosT = KeyFrames.KeyAtIndex(index);
            if (SongLengthT > 0 && newPosT >= 0)
                NormSongPos = (newPosT + 0.5) / SongLengthT;
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

        public KeyFrame GetKeyFrameAtSongPos()
        {
            return KeyFrames[(int)SongPosT];
        }

        /// <summary>
        /// Selects only the keyframe at the current song position, deselecting all others.
        /// The WPF build has no keyframe-selection grid (unlike the legacy WinForms UI), so camera
        /// movement (<see cref="update"/> integrates only selected keyframes) and reset need the
        /// keyframe under the playhead to be marked selected. Safe to call every frame.
        /// </summary>
        public void SelectKeyFrameAtSongPos()
        {
            var current = GetKeyFrameAtSongPos();
            foreach (var keyFrame in KeyFrames.Values)
                keyFrame.Selected = (keyFrame == current);
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

        public Project Clone()
        {
            Project dest = Cloning.Clone(this);

            for (int i = 0; i < _trackViews.Count; i++)
            {
                //dest.trackViews[i] = trackViews[i].clone();
                //dest.TrackViews[i].TrackProps.GlobalProps = dest.TrackViews[0].TrackProps;
                dest._trackViews[i].MidiTrack = _trackViews[i].MidiTrack;
                dest._trackViews[i].Geo = _trackViews[i].Geo;
                dest._trackViews[i].Curve = _trackViews[i].Curve;
                dest._trackViews[i].TrackProps.AudioProps = _trackViews[i].TrackProps.AudioProps;
            }

            dest._notes = _notes;
            //dest.Props = Props.clone();
            //dest.vertViewWidthQn = vertViewWidthQn;
            dest.Props.OnPlaybackOffsetSChanged = dest.OnPlaybackOffsetSChanged;
            dest.Props.OnPlaybackOffsetSChanged();
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
            _vertViewWidthQn = Props.ViewWidthQn;
            //TrackViews = source.TrackViews;
            for (int i = 0; i < TrackViews.Count; i++)
            {
                var destProps = TrackViews[i].TrackProps;
                var sourceProps = TrackViews[i].TrackProps;
                destProps.StyleProps = sourceProps.StyleProps;
                destProps.MaterialProps = sourceProps.MaterialProps;
                destProps.LightProps = sourceProps.LightProps;
                destProps.SpatialProps = sourceProps.SpatialProps;
                //Skip AudioProps for more lightweight redos
            }
            KeyFrames = source.KeyFrames;
            PropertyKeyframes = source.PropertyKeyframes;
            //source.Dispose();
        }

        internal void InitAfterDeserialization(WaveformPanel waveformPanel = null)
        {
            if (KeyFrames == null) //Old project file format
                KeyFrames = new KeyFrames();
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
            // In WinForms this is done indirectly via UpdateProjPropsControls() doing
            // `Props.PlaybackOffsetS = Props.PlaybackOffsetS`; do it explicitly here so
            // both WPF and WinForms paths get correct SongLengthS.
            OnPlaybackOffsetSChanged();
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
        public float Time { get; set; }
        public string Lyrics { get; set; }
        public LyricsSegment(float time)
        {
            Time = time;
        }

        public LyricsSegment(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "time")
                    Time = (float)entry.Value;
                else if (entry.Name == "lyrics")
                    Lyrics = (string)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("time", Time);
            info.AddValue("lyrics", Lyrics);
        }
    }
}

