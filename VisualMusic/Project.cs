using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
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
        ProjProps props = new ProjProps();
        public ProjProps Props
        {
            get => props;
            set
            {
                props = value;
                props.OnPlaybackOffsetSChanged = onPlaybackOffsetSChanged;
                Props.OnPlaybackOffsetSChanged();
            }
        }
        SongPanel SongPanel => Form1.SongPanel;

        TimeSpan pbStartSysTime = new TimeSpan(0);
        double pbStartSongTimeS;
        public float ViewWidthT => notes == null ? 0 : Props.ViewWidthQn * notes.TicksPerBeat; //Number of ticks that fits on screen
        float vertViewWidthQn;
        public float ViewWidthQnScale => vertViewWidthQn / Props.ViewWidthQn;

        public ImportOptions ImportOptions { get; set; }

        List<TrackView> trackViews;
        public List<TrackView> TrackViews
        {
            get { return trackViews; }
            set
            {
                foreach (var tv in trackViews)
                    tv.OcTree?.Dispose();
                trackViews = value;
            }
        }

        int firstTempoEvent = 0;

        //Current playback position to seek from
        int pbTempoEvent = 0;
        double pbTimeT = 0;
        double pbTimeS = 0;

        //public Camera DefaultCamera { get; } = new Camera();
        //----------------------------------------------------

        public TrackProps GlobalTrackProps
        {
            get { return trackViews[0].TrackProps; }
            set { trackViews[0].TrackProps = value; }
        }

        Midi.Song notes;
        public Midi.Song Notes
        {
            get => notes;
            set => notes = value;
        }

        public double SongLengthT => (notes != null ? notes.SongLengthT : 0) + playbackOffsetT; //Song length in ticks
        public double SongLengthS { get; private set; }
        public double SongPosT => (int)(normSongPos * SongLengthT); //Current song position in ticks
        public double SongPosB => (float)SongPosT / Notes.TicksPerBeat; //Current song position in beats
        public float SongPosP => getScreenPosX(SongPosT); //Current song position in pixels
        public double SongPosS => normSongPosToSeconds(normSongPos); //Current song position in seconds
        double normSongPos; //Song position normalized to [0,1]
        public double NormSongPos
        {
            get => normSongPos;
            set
            {
                if (normSongPos != value)
                {
                    normSongPos = value;
                    normSongPos = Math.Max(0, normSongPos);
                    normSongPos = Math.Min(1, normSongPos);
                    //SongPanel.paint();
                    SongPanel.Invalidate();
                    if (SongPanel.OnSongPosChanged != null)
                        SongPanel.OnSongPosChanged();
                }
            }
        }
        float playbackOffsetT = 0;
        public float PlaybackOffsetT => playbackOffsetT;
        public float PlaybackOffsetP => getScreenPosX(playbackOffsetT);

        bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            private set
            {
                isPlaying = value;
                if (!value)
                    AudioHasStarted = false;
            }
        }
        bool tempPausing;
        public bool AudioHasStarted { get; set; }

        public Project()
        {
            KeyFrames = new KeyFrames();
            Props.ViewWidthQn = KeyFrames[0].ProjProps.ViewWidthQn;
            Props.OnPlaybackOffsetSChanged = onPlaybackOffsetSChanged;
        }


        async public Task loadContent(Form parentForm)
        {
            if (ImportOptions == null)
                return;

            ImportOptions.setNotePath();
            ImportOptions.EraseCurrent = false;
            await importSong(ImportOptions, parentForm);
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
                    trackViews = (List<TrackView>)entry.Value;
                    TrackView.NumTracks = TrackViews.Count;
                    foreach (var tv in trackViews)
                    {
                        tv.TrackProps.TrackView = tv;
                        tv.TrackProps.GlobalProps = TrackViews[0].TrackProps;
                    }
                }

                else if (entry.Name == "keyFrames")
                    KeyFrames = (KeyFrames)entry.Value;
                else if (entry.Name == "props")
                    Props = (ProjProps)entry.Value;
                else if (entry.Name == "vertWidthQn")
                    vertViewWidthQn = (float)entry.Value;

                //Compatibility
                else if (entry.Name == "qn_viewWidth")
                {
                    Props.ViewWidthQn = (float)entry.Value;
                    vertViewWidthQn = Props.ViewWidthQn;
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
            info.AddValue("trackViews", trackViews);
            info.AddValue("keyFrames", KeyFrames);
            info.AddValue("props", Props);
            info.AddValue("vertWidthQn", vertViewWidthQn);
        }

        async public Task<bool> importSong(ImportOptions options, Form parentForm)
        { //<Open project> and <import files> meet here
            options.checkSourceFile();
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
                    parentForm.Enabled = false;
                    try
                    {
                        FormWindowState initialWindowState = Program.form1.WindowState;
                        await Task.Run(() =>
                        {
                            //Minimize parent form when process form is minimized,
                            //and restore parent form when process form is restored
                            //Process form also needs to be restored if parent form is restored, which is done in Form1.Form1_Activated event handler
                            bool wasMinimized = false;
                            while (!process.HasExited)
                            {
                                bool isMinimized = IsIconic(process.MainWindowHandle);
                                if (isMinimized && !wasMinimized)
                                    Program.form1.Invoke(new Action(() => Program.form1.WindowState = FormWindowState.Minimized));
                                else if (!isMinimized && wasMinimized)
                                {
                                    Program.form1.Invoke(new Action(() => Program.form1.WindowState = initialWindowState));
                                    Form1.regainFocus(process); //Prevent parentForm to steal focus from process when its initial window state is restored
                                }
                                wasMinimized = isMinimized;
                                Thread.Sleep(200); //Free cpu cycles
                            }
                        });
                    }
                    finally
                    {
                        parentForm.Enabled = true;
                        Program.form1.Activate();
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
                MidMix.mixdown(options.NotePath, audioPath);
                options.AudioPath = audioPath;
            }

            openNoteFile(options);
            openAudioFile(options);

            ImportOptions = options;
            if (options.EraseCurrent)
            {
                DefaultFileName = Path.GetFileName(ImportOptions.NotePath) + "." + DefaultFileExt;
                Props.ViewWidthQn = vertViewWidthQn = ProjProps.DefaultViewWidthQn;
            }
            createTrackViews(notes.Tracks.Count, options.EraseCurrent);
            return true;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        public bool openNoteFile(ImportOptions options)
        {
            SongPanel.Invalidate();
            stopPlayback();
            pbTempoEvent = 0;
            pbTimeT = 0;
            pbTimeS = 0;

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
                newNotes.openFile(path);
            }
            catch (FileFormatException ex)
            {
                throw new FileImportException(ex.Message, ImportError.Corrupt, ImportFileType.Note, errorPath);
            }
            if (newNotes.Tracks == null || newNotes.Tracks.Count == 0 || newNotes.SongLengthT == 0)
                throw new FileImportException("No notes found.", ImportError.Corrupt, ImportFileType.Note, errorPath);

            notes = newNotes;
            notes.createNoteBsp();

            if (options.EraseCurrent)
            {
                KeyFrames = new KeyFrames();
                Props.AudioOffset = Props.PlaybackOffsetS = Props.FadeIn = Props.FadeOut = 0;
                NormSongPos = 0;
                resetPitchLimits();
            }
            //viewWidthT = (int)(ViewWidthQn * notes.TicksPerBeat);
            return true;
        }

        public void interpolateFrames()
        {
            var interpolatedFrame = KeyFrames.createInterpolatedFrame((int)SongPosT);
            //Props.ViewWidthQn = interpolatedFrame.ProjProps.ViewWidthQn;
            //Props.Camera = interpolatedFrame.ProjProps.Camera;
            Props = interpolatedFrame.ProjProps;
        }

        public void openAudioFile(ImportOptions options)
        {
            Media.closeAudioFile();
            string file = options.AudioPath;

            //Third-party mixdown needed?
            if (options.MixdownType == Midi.MixdownType.Tparty)
                file = ImportNotesWithAudioForm.runTpartyProcess(options);

            if (string.IsNullOrWhiteSpace(file))
                return;

            if (!Media.openAudioFile(file))
                throw new IOException("Unexpected error while opening audio file:\r\n" + file);

            if (notes != null)
                notes.SongLengthT = (int)secondsToTicks((float)(Media.getAudioLength() + Props.AudioOffset));
            AudioFilePath = file;
        }

        public void resetPitchLimits()
        {
            Props.MaxPitch = Notes.MaxPitch;
            Props.MinPitch = Notes.MinPitch;
        }

        public void showNoteInfo(GdiPoint location)
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

        public void createTrackViews(int numTracks, bool eraseCurrent)
        {
            TrackView.NumTracks = numTracks;
            int startTrack; //At which index to start creating new (default) track props
            if (eraseCurrent || trackViews == null)
            {
                startTrack = 0;
                trackViews = new List<TrackView>(numTracks);
            }
            else
            {
                startTrack = trackViews.Count; //Keep current props but add new props if the new imported note file has more tracks than the current song. Start assigning default track props at current song's track count and up.
            }

            for (int i = 0; i < trackViews.Count; i++)
            {
                //No need to update visual props
                // Update notes

                if (trackViews[i].TrackNumber >= notes.Tracks.Count) //The new note file has fewer tracks than the currently loaded
                    continue;

                trackViews[i].MidiTrack = notes.Tracks[trackViews[i].TrackNumber];
                trackViews[i].createCurve();
                //If a project file is being loaded, track views was deserialized, and further init involving the graphics device is needed here, because it was not initialized at the time of deserialization.
                trackViews[i].TrackProps.StyleProps.loadFx();
            }
            for (int i = startTrack; i < numTracks; i++)
            {
                //New note file has more tracks than current project or we're creating a new project. Create new track props for the new tracks.
                TrackView view = new TrackView(i, numTracks, notes);
                addTrackView(view);
            }
            //if (startTrack >= numTracks && numTracks > 0)  //New note file has fewer tracks than current song. Remove the extra trackViews.
            //trackViews.RemoveRange(numTracks, startTrack - numTracks);
            List<TrackView> tvCopy = new List<TrackView>();
            for (int i = 0; i < trackViews.Count; i++)
            {
                if (trackViews[i].TrackNumber < numTracks)
                    tvCopy.Add(trackViews[i]);
            }
            trackViews = tvCopy;
            createOcTrees(false);
        }

        private void addTrackView(TrackView view)
        {
            trackViews.Add(view);
            view.TrackProps.GlobalProps = TrackViews[0].TrackProps;
        }

        public void createOcTrees(bool resetVertScale = true)
        {
            if (trackViews == null || Props.ViewWidthQn == 0 || Notes == null)
                return;
            float viewWidthQnBackup = Props.ViewWidthQn;
            if (resetVertScale)
                vertViewWidthQn = Props.ViewWidthQn;
            else
                Props.ViewWidthQn = vertViewWidthQn;
            for (int i = 1; i < trackViews.Count; i++)
                TrackViews[i].createOcTree(this, GlobalTrackProps);
            Props.ViewWidthQn = viewWidthQnBackup;
        }

        public void drawSong()
        {
            if (notes == null || trackViews == null)
                return;

            DepthStencilState oldDss = SongPanel.GraphicsDevice.DepthStencilState;
            DepthStencilState dss = new DepthStencilState();
            dss.StencilEnable = true;
            dss.StencilFunction = CompareFunction.Greater;
            dss.StencilPass = StencilOperation.Replace;
            dss.ReferenceStencil = 1;
            SongPanel.GraphicsDevice.DepthStencilState = dss;
            for (int t = 1; t < trackViews.Count; t++)
            {
                SongPanel.GraphicsDevice.Clear(ClearOptions.Stencil | ClearOptions.DepthBuffer, Color.AliceBlue, 1, 0);
                trackViews[t].drawTrack(GlobalTrackProps, SongPanel.ForceDefaultNoteStyle);
            }
            SongPanel.GraphicsDevice.DepthStencilState = oldDss;

            var effect = new BasicEffect(SongPanel.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };
            Viewport viewport = SongPanel.GraphicsDevice.Viewport;
            //effect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            effect.Projection = Props.Camera.ProjMat;
            effect.View = Matrix.CreateTranslation(new Vector3(0, 0, -Props.Camera.ProjMat.M11 / 2));
            Vector2 songPanelSize = new Vector2(SongPanel.ClientRectangle.Width, SongPanel.ClientRectangle.Height);
            Vector2 scale = new Vector2(Props.Camera.ViewportSize.X / songPanelSize.X, -Props.Camera.ViewportSize.Y / songPanelSize.Y);

            SongPanel.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, RasterizerState.CullNone, effect, null);
            foreach (var lyricsSegment in Props.LyricsSegments)
            {
                if (string.IsNullOrWhiteSpace(lyricsSegment.Lyrics))
                    continue;
                float textHeight = -SongPanel.LyricsFont.MeasureString(lyricsSegment.Lyrics).Y * scale.Y;
                SongPanel.SpriteBatch.DrawString(SongPanel.LyricsFont, lyricsSegment.Lyrics, new Vector2(-SongPosP + getScreenPosX(secondsToTicks(lyricsSegment.Time) + PlaybackOffsetT), -Props.Camera.ViewportSize.Y / 2 + textHeight), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
            SongPanel.SpriteBatch.End();
        }

        public int screenPosToSongPos(float normScreenPos)
        {
            return (int)(NormSongPos * SongLengthT + (double)normScreenPos * ViewWidthT * 0.5f);
        }
        Point getVisibleSongPortionT(double normPos)
        {
            double posT = normPos * SongLengthT;
            return new Point((int)(posT - ViewWidthT), (int)(posT + ViewWidthT));
        }
        public int getPitch(float normPosY)
        {
            normPosY = 1 - normPosY;
            float height = 1 - ProjProps.NormPitchMargin * 2;
            float noteHeight = height / Notes.NumPitches;
            float pos = normPosY - ProjProps.NormPitchMargin;
            return Props.MinPitch + (int)(pos / noteHeight);
        }
        public TrackProps mergeTrackProps(ListView.SelectedIndexCollection listIndices)
        {
            if (listIndices.Count == 0)
                return null;
            TrackProps outProps = TrackViews[listIndices[0]].TrackProps;
            if (listIndices.Count == 1)
                return outProps;
            outProps = outProps.clone(SongPanel);
            for (int i = 1; i < listIndices.Count; i++)
                outProps = (TrackProps)mergeObjects(outProps, TrackViews[listIndices[i]].TrackProps);
            return outProps;
        }

        public object mergeObjects(object first, object second)
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

                    object subMerge = mergeObjects(firstValue, secondValue);
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
                        ((NoteStyle)first).SelectedModEntry = (NoteStyleMod)mergeObjects(((NoteStyle)first).SelectedModEntry, ((NoteStyle)second).SelectedModEntry);
                }
            }

            if (hasSuitableProp || object.Equals(first, second))
                return first;
            else
                return null;
        }

        public void resetTrackProps(ListView.SelectedIndexCollection indices)
        {
            if (indices != null)
            {
                foreach (int index in indices)
                    trackViews[index].TrackProps.resetProps();
            }
            else
                trackViews[0].TrackProps.resetProps();
            createOcTrees();
        }

        public double ticksToSeconds(double ticks)
        {
            if (pbTimeT > ticks)
            {
                pbTimeT = pbTimeS = 0;
                pbTempoEvent = firstTempoEvent;
            }
            else if (pbTimeT == ticks)
                return pbTimeS;

            int nextTempoEvent = pbTempoEvent;
            while (pbTimeT < ticks)
            {
                double nextTimeStepT;
                double currentBps = notes.TempoEvents[pbTempoEvent].Tempo / 60; //beats per seconds
                if (pbTempoEvent + 1 >= notes.TempoEvents.Count)
                    nextTimeStepT = ticks;
                else
                {
                    nextTempoEvent++;
                    nextTimeStepT = notes.TempoEvents[nextTempoEvent].Time + playbackOffsetT;
                    //if (nextTimeStepT < pbTimeT || nextTimeStepT == pbTimeT && bLastTempoEvent)
                    //	throw new Exception("nextTimeStepT < pbTimeT || nextTimeStepT == pbTimeT && bLastTempoEvent");
                    if (nextTimeStepT > ticks)
                        nextTimeStepT = ticks; //always causes loop to exit
                    else
                        pbTempoEvent = nextTempoEvent;
                }
                pbTimeS += (nextTimeStepT - pbTimeT) / (notes.TicksPerBeat * currentBps);
                pbTimeT = nextTimeStepT;
            }
            return pbTimeS;
        }
        double secondsToTicks(double seconds)
        {
            if (pbTimeS > seconds) //Reset seek position
            {
                pbTimeS = pbTimeT = 0;
                pbTempoEvent = firstTempoEvent;
            }

            int nextTempoEvent = pbTempoEvent;
            while (pbTimeS < seconds)
            {
                double nextTimeStepS;
                double currentBps = notes.TempoEvents[pbTempoEvent].Tempo / 60; //beats per seconds
                if (pbTempoEvent + 1 >= notes.TempoEvents.Count)
                    nextTimeStepS = seconds;
                else
                {
                    nextTempoEvent++;
                    double nextTempoTimeS = (notes.TempoEvents[nextTempoEvent].Time + playbackOffsetT - pbTimeT) / (notes.TicksPerBeat * currentBps) + pbTimeS;
                    nextTimeStepS = nextTempoTimeS;
                    if (nextTimeStepS > seconds)
                        nextTimeStepS = seconds; //always causes loop to exit
                    else
                        pbTempoEvent = nextTempoEvent;
                }
                pbTimeT += (nextTimeStepS - pbTimeS) * currentBps * notes.TicksPerBeat;
                pbTimeS = nextTimeStepS;
            }
            return pbTimeT;
        }

        public void setSongPosS(double newTimeS, bool updateScreen)
        {
            double offsetS = 0, offsetT = 0;
            if (Props.PlaybackOffsetS < 0)
            {
                offsetS = -Props.PlaybackOffsetS;
                offsetT = -playbackOffsetT;
            }
            pbTimeT = secondsToTicks(newTimeS);
            double newSongPos = pbTimeT / (double)SongLengthT;
            if (updateScreen)
                NormSongPos = newSongPos;
            else
                normSongPos = newSongPos;
        }

        //Converts normalized song pos to seconds
        //0 as input returns 0 seconds, 1 returns song length in seconds
        public double normSongPosToSeconds(double norm)
        {
            if (notes == null)
                return 0;

            return ticksToSeconds(norm * SongLengthT);
        }

        public void update(double deltaTimeS)
        {
            foreach (var keyFrame in KeyFrames.Values)
            {
                if (keyFrame.Selected)
                    keyFrame.ProjProps.Camera.update(deltaTimeS);
            }
            interpolateFrames();

            //Scroll song depending on user input or playback position.
            if (IsPlaying)
            {
                double timeS;
                if (!AudioHasStarted)
                {
                    timeS = (SongPanel.TotalTimeElapsed - pbStartSysTime).TotalSeconds + pbStartSongTimeS;
                    if (timeS > Props.AudioOffset + Props.PlaybackOffsetS)
                    {
                        AudioHasStarted = true;
                        Media.startPlaybackAtTime(0);
                    }
                }
                else
                {
                    if (!Media.playbackIsRunning()) //playback reached end of song
                    {
                        IsPlaying = false;
                        timeS = SongPosS;
                    }
                    else
                        timeS = Media.getPlaybackPos() + Props.AudioOffset + Props.PlaybackOffsetS;
                }

                setSongPosS(timeS, true);
                if (NormSongPos > 1)
                    togglePlayback();
            }
            //else

            if (NormSongPos < 0)
                NormSongPos = 0;
            if (NormSongPos > 1)
                NormSongPos = 1;
        }
        public void togglePlayback()
        {
            if (Media.getAudioLength() == 0 || Camera.MouseRot)
                return;
            IsPlaying = !IsPlaying;
            //bAudioPlayback = !bAudioPlayback;
            if (!IsPlaying)
            {
                Media.pausePlayback();
                //MessageBox.Show("An error occured while pausing playback.");
            }
            else
            {
                double songPosS = SongPosS;
                double startTime = songPosS - Props.AudioOffset - Props.PlaybackOffsetS;
                if (startTime >= 0)
                {
                    Media.startPlaybackAtTime(startTime);
                    //MessageBox.Show("An error occured while starting playback.");
                    if (!Media.playbackIsRunning()) //Assuming this is because we tried to start playback after end of audio
                        IsPlaying = false;
                    else
                        AudioHasStarted = true;
                }
                else
                {
                    pbStartSysTime = SongPanel.TotalTimeElapsed;
                    pbStartSongTimeS = songPosS;
                    AudioHasStarted = false;
                }
            }
        }

        public void pausePlayback()
        {
            if (isPlaying)
                togglePlayback();
        }

        public void stopPlayback()
        {
            IsPlaying = false;
            //			bAudioPlayback = false;
            Media.stopPlayback();
            // MessageBox.Show("An error occured while stopping playback.");
            NormSongPos = 0;
        }

        public Vector2 getScreenPos(int timeT, int pitch)
        {
            Vector2 p = new Vector2();
            p.X = getScreenPosX(timeT);
            p.Y = getScreenPosY((float)pitch);
            return p;
        }
        public float getScreenPosX(double timeT)
        {
            return (float)((timeT / ViewWidthT) * (Props.Camera.ViewportSize.X));
        }

        public float getScreenPosY(float pitch)
        {
            return (pitch - Props.MinPitch) * Props.NoteHeight + Props.NoteHeight / 2.0f + Props.PitchMargin - Props.Camera.ViewportSize.Y / 2;
        }
        public double pixelsToTicks(double screenX)
        { //Returns time in ticks
            return screenX / Props.Camera.ViewportSize.X * ViewWidthT; //Far right -> screenX = viewPortSize / 2
        }

        public float SongLengthP => (float)(SongLengthT * Props.Camera.ViewportSize.X) / ViewWidthT;

        public int SmallScrollStepT => (int)(ViewWidthT * SongPanel.SmallScrollStep);
        public int LargeScrollStepT => (int)(ViewWidthT * SongPanel.LargeScrollStep);

        public string DefaultFileName { get; set; }
        public string AudioFilePath { get; private set; }

        public const string DefaultFileExt = "vmp";

        public float getCurveScreenY(float x, Curve curve)
        {
            //float pitch = curve.EvaluateCurvature((float)getTimeT(x));
            //return pitch / 100;
            float pitch = curve.Evaluate((float)pixelsToTicks(x));
            return getScreenPosY(pitch);
        }

        public void tempPausePlayback()
        {
            if (IsPlaying)
            {
                tempPausing = true;
                togglePlayback();
            }
        }

        public void resumeTempPausedPlayback()
        {
            if (!IsPlaying && tempPausing)
            {
                tempPausing = false;
                togglePlayback();
            }
        }

        public Vector3 getSpatialNormPosOffset(TrackProps trackProps)
        {
            return normalizeVpVector(GlobalTrackProps.SpatialProps.PosOffset + trackProps.SpatialProps.PosOffset);
        }

        public float normalizeVpScalar(float value)
        {
            return value * Props.Camera.ViewportSize.X / Props.UserViewWidth;
        }
        public Vector3 normalizeVpVector(Vector3 value)
        {
            return value * Props.Camera.ViewportSize.X / Props.UserViewWidth;
        }

        public int insertLyrics()
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

        public int insertKeyFrameAtSongPos()
        {
            return KeyFrames.insert((int)SongPosT);
        }

        public void goToKeyFrame(int index)
        {
            int newPosT = KeyFrames.keyAtIndex(index);
            if (SongLengthT > 0 && newPosT >= 0)
                NormSongPos = (newPosT + 0.5) / SongLengthT;
        }

        public KeyFrame getKeyFrameAtSongPos()
        {
            return KeyFrames[(int)SongPosT];
        }

        void onPlaybackOffsetSChanged()
        {
            if (notes == null)
                return;
            firstTempoEvent = pbTempoEvent = 0;
            pbTimeS = pbTimeT = playbackOffsetT = 0;

            //Set playbackOffsetT
            if (Props.PlaybackOffsetS >= 0)
                playbackOffsetT = (float)(Props.PlaybackOffsetS * notes.TempoEvents[0].Tempo / 60 * notes.TicksPerBeat);
            else
                playbackOffsetT = (float)-secondsToTicks((double)-Props.PlaybackOffsetS);

            //Set firstTempoEvent (if playback offset is negative, playback may start after second event in which firstTempoEvent shouldn't be zero)
            if (Props.PlaybackOffsetS < 0)
            {
                double offsetT = -playbackOffsetT;
                for (int i = 0; i < notes.TempoEvents.Count; i++)
                {
                    if (notes.TempoEvents[i].Time <= offsetT)
                        firstTempoEvent = i;
                    else
                        break;
                }
            }

            pbTimeS = pbTimeT = 0;
            pbTempoEvent = firstTempoEvent;
            SongLengthS = normSongPosToSeconds(1);
        }

        //public Project clone()
        //{
        //    Project dest = Cloning.clone(this);

        //    for (int i = 0; i < trackViews.Count; i++)
        //    {
        //        //dest.trackViews[i] = trackViews[i].clone();
        //        //dest.TrackViews[i].TrackProps.GlobalProps = dest.TrackViews[0].TrackProps;
        //        dest.trackViews[i].MidiTrack = trackViews[i].MidiTrack;
        //        dest.trackViews[i].OcTree = trackViews[i].OcTree;
        //        dest.trackViews[i].Curve = trackViews[i].Curve;
        //    }

        //    dest.notes = notes;
        //    //dest.Props = Props.clone();
        //    //dest.vertViewWidthQn = vertViewWidthQn;
        //    dest.Props.OnPlaybackOffsetSChanged = dest.onPlaybackOffsetSChanged;
        //    dest.Props.OnPlaybackOffsetSChanged();
        //    return dest;
        //}

        public void Dispose()
        {
            foreach (var tv in trackViews)
                tv.OcTree?.Dispose();
        }

        public void copyPropsFrom(Project project)
        {
            var source = project.clone();
            Props = source.Props;
            vertViewWidthQn = Props.ViewWidthQn;
            TrackViews = source.TrackViews;
            KeyFrames = source.KeyFrames;
            //source.Dispose();
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

