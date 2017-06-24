#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using WinFormsGraphicsDevice;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;

#endregion

namespace Visual_Music
{
	using GdiPoint = System.Drawing.Point;
	using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
    using WinKeys = System.Windows.Forms.Keys;
    public enum SourceSongType { Midi,Mod,Sid };

    public class SongDrawProps
	{
        public int songPosT;
		public float songPosS;
		public float noteHeight;
		public int yMargin;
		public Point viewportSize;
		public int viewWidthT;
		public Midi.Song song;
        public int minPitch;

		public Point getScreenPos(int timeT, int pitch)
		{
			Vector2 v = getScreenPosF(timeT, pitch);
			return new Point((int)v.X, (int)v.Y);
		}
		public Vector2 getScreenPosF(int timeT, int pitch)
		{
			Vector2 p = new Vector2();
			p.X = getTimeTPosF(timeT);
			//p.Y = (int)((float)viewportSize.Y - (float)(pitch - song.MinPitch) * (float)noteHeight - (float)noteHeight / 2.0f - (float)yMargin);
			p.Y = getPitchScreenPos((float)pitch);
			return p;
		}
		public float getTimeTPosF(int timeT)
		{
			return (float)((double)(timeT - songPosT) / viewWidthT + 0.5) * viewportSize.X - viewportSize.X / 2.0f;
		}
		public int getPitchScreenPos(int pitch)
		{
			return (int)getPitchScreenPos((float)pitch);
		}
		public float getPitchScreenPos(float pitch)
		{
			return (float)viewportSize.Y - (pitch - minPitch) * (float)noteHeight - noteHeight / 2.0f - yMargin - viewportSize.Y / 2.0f;
		}
		public float getSongPosT(float screenX)
		{ //Returns song pos in ticks
			return (float)screenX / (float)viewportSize.X * (float)viewWidthT + (float)songPosT; //Far right -> screenX = viewPortSize/2
		}
		public float getSongPosP(float screenX)
		{ //Returns song pos in pixels
			return getSongPosT(screenX) * viewportSize.X / viewWidthT;
		}
		public float getSongLengthP()
		{
			return (float)(song.SongLengthT * viewportSize.X) / viewWidthT;
		}
		
		public float getCurveScreenY(float x, Curve curve)
		{
			float pitch = curve.Evaluate((float)getSongPosT(x));
			return getPitchScreenPos(pitch);
		}
	}

	[Serializable()]
	public class SongPanel : GraphicsDeviceControl, ISerializable
	{
		public Camera Camera { get; set; } = new Camera();
		public Camera DefaultCamera { get; } = new Camera();
		ContentManager content;
		public ContentManager Content { get { return content; } }
		bool forceSimpleDrawMode = false;
		public bool ForceSimpleDrawMode { get =>forceSimpleDrawMode;
			set
			{
				forceSimpleDrawMode = value;
				Invalidate();
			}
		}
		MixdownType mixdownType;
		public MixdownType MixdownType { get => mixdownType; set => mixdownType = value; } 
		Texture2D regionSelectTexture;
        float normPitchMargin = 1 / 50.0f;
        bool bPlayback = false;
        public bool IsPlaying
        {
            get { return bPlayback; }
        }
        bool bAudioPlayback = false;
        TimeSpan oldTime = new TimeSpan(0);
        TimeSpan pbStartSysTime = new TimeSpan(0);
        double pbStartSongTimeS;
        Stopwatch stopwatch = new Stopwatch();
		//TimeSpan deltaTime;
		double deltaTimeS;
		double renderInterval = 0.0001;
		bool leftMbPressed = false;
		//public bool RightMbPressed { get; set; }
		double scrollCenter = 0;
        bool selectingRegion = false;
		bool mergeRegionSelection = false;
		bool mousePosScrollSong = false;
		Rectangle selectedSongRegion;
        Rectangle selectedScreenRegion;
        public float NormMouseX { get; set; }
        public float NormMouseY { get; set; }
        SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
            //set { spriteBatch = value; }
        }

        BlendState blendState;
        public BlendState BlendState
        {
            get { return blendState; }
            //set { blendState = value; }
        }
        internal TrackProps GlobalTrackProps
        {
            get { return trackProps[0]; }
            set { trackProps[0] = value; }
        }
        List<TrackProps> trackProps;
        internal List<TrackProps> TrackProps
        {
            get { return trackProps; }
            set { trackProps = value; }
        }

        RenderTarget2D videoFrame;
        Midi.Song notes;
        public Midi.Song Notes { get { return notes; } }
        string noteFilePath = "";
        public string NoteFilePath
        {
            get { return noteFilePath; }
            set { noteFilePath = value; }
            //get { return ((Form1)Parent).sourceFileForm.NoteFilePath; }
            //set { ((Form1)Parent).sourceFileForm.NoteFilePath = value; }
        }
        string audioFilePath = "";
        public string AudioFilePath
        {
            get { return mixdownType == MixdownType.None ? audioFilePath : ""; }
            set { audioFilePath = value; }
            //get { return ((Form1)Parent).sourceFileForm.AudioFilePath; }
            //set { ((Form1)Parent).sourceFileForm.AudioFilePath = value; }
        }
        bool insTrack;
        public bool InsTrack
        {
            get { return insTrack; }
        }

        SourceSongType sourceSongType;
        public SourceSongType SourceSongType { get => sourceSongType; set => sourceSongType = value; }
		Point videoSize = new Point(1920, 1080);
		double desiredSongLengthS = 0; //Desired song length in seconds when importing note file. 0 = not specified. Currently not used.
		public int SongLengthT { get { return notes != null ? notes.SongLengthT : 0; } } //Song length in ticks
        public int SongPosT { get { return (int)(normSongPos * SongLengthT); } } //Current song position at center of screen
        double normSongPos; //Song position normalized to [0,1]
        public double NormSongPos
		{
			get => normSongPos;
			set
			{
				double oldPos = normSongPos;
				normSongPos = value;
				if (oldPos != value)
				{ 
					Invalidate();
					if (OnSongPosChanged != null)
						OnSongPosChanged();
				}
			}
		}
		public delegate void Delegate_songPosChanged();
		public Delegate_songPosChanged OnSongPosChanged { get; set; }
		const float DefaultViewWidthQn = 16; //Number of quarter notes that fits on screen
		float viewWidthQn = DefaultViewWidthQn; 
		
        public float ViewWidthQn
		{
			get {return viewWidthQn;}
			set
			{
				viewWidthQn = value;
				if (notes != null)
					viewWidthT = (int)(viewWidthQn * notes.TicksPerBeat);
			}
		}
    
		int viewWidthT; ////Number of ticks that fits on screen
		public int ViewWidthT { get => viewWidthT; }
        public double AudioOffset { get; set; }
        public int MinPitch { get; set; } 
        public int MaxPitch { get; set; }
        int NumPitches { get { return MaxPitch - MinPitch + 1; } }
		
		public SongPanel()
		{
		}
		public SongPanel(SerializationInfo info, StreamingContext ctxt):base()
		{
            SongFormat.readVersion = (int)info.GetValue("version", typeof(int));
            noteFilePath = (string)info.GetValue("noteFilePath", typeof(string));
			insTrack = (bool)info.GetValue("insTrack", typeof(bool));
			mixdownType = (MixdownType)info.GetValue("mixdownType", typeof(MixdownType));
			audioFilePath = (string)info.GetValue("audioFilePath", typeof(string));
			trackProps = (List<TrackProps>)info.GetValue("trackProps", typeof(List<TrackProps>));
   			ViewWidthQn = (float)info.GetValue("qn_viewWidth", typeof(float));
			AudioOffset = (double)info.GetValue("audioOffset", typeof(double));
			MaxPitch = (int)info.GetValue("maxPitch", typeof(int));
			MinPitch = (int)info.GetValue("minPitch", typeof(int));
            sourceSongType = (SourceSongType)info.GetValue("sourceSongType", typeof(SourceSongType));
			ImportNotesWithAudioForm.TpartyApp = info.GetString("tpartyApp");
			ImportNotesWithAudioForm.TpartyArgs = info.GetString("tpartyArgs");
			ImportNotesWithAudioForm.TpartyOutputDir = info.GetString("tpartyOutputDir");
			desiredSongLengthS = info.GetDouble("desiredSongLengthS");
			Camera = (Camera)info.GetValue("camera", typeof(Camera));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
            info.AddValue("version", SongFormat.writeVersion);
            info.AddValue("noteFilePath", noteFilePath);
			info.AddValue("audioFilePath", audioFilePath);
			info.AddValue("insTrack", insTrack);
			info.AddValue("mixdownType", mixdownType);
            info.AddValue("trackProps", trackProps);
            info.AddValue("qn_viewWidth", ViewWidthQn);
			info.AddValue("audioOffset", AudioOffset);
			info.AddValue("maxPitch", MaxPitch);
			info.AddValue("minPitch", MinPitch);
            info.AddValue("sourceSongType", sourceSongType);
			info.AddValue("tpartyApp", ImportNotesWithAudioForm.TpartyApp);
			info.AddValue("tpartyArgs", ImportNotesWithAudioForm.TpartyArgs);
			info.AddValue("tpartyOutputDir", ImportNotesWithAudioForm.TpartyOutputDir);
			info.AddValue("desiredSongLengthS", desiredSongLengthS);
			info.AddValue("camera", Camera);
		}

		protected override void Initialize()
        {
			stopwatch.Start();
			spriteBatch = new SpriteBatch(GraphicsDevice);
			blendState = new BlendState();
			videoFrame = new RenderTarget2D(GraphicsDevice, videoSize.X, videoSize.Y);

			blendState.AlphaDestinationBlend = Blend.DestinationAlpha;
			//blendState.AlphaDestinationBlend = Blend.One;
			blendState.AlphaSourceBlend = Blend.InverseDestinationAlpha;
			blendState.ColorDestinationBlend = Blend.DestinationAlpha;
			//blendState.ColorDestinationBlend = Blend.One;
			blendState.ColorSourceBlend = Blend.InverseDestinationAlpha;
			blendState.ColorWriteChannels = ColorWriteChannels.All;
			blendState.AlphaBlendFunction = BlendFunction.Add;
			blendState.ColorBlendFunction = BlendFunction.Add;

            content = new ContentManager(Services, "Content");
            NoteStyle.sInitAllStyles(this);

            regionSelectTexture = new Texture2D(GraphicsDevice, 1, 1);
			regionSelectTexture.SetData(new[] { Color.White });

			if (!string.IsNullOrWhiteSpace(noteFilePath))
			{
				importSong(noteFilePath, audioFilePath, false, insTrack, mixdownType, desiredSongLengthS);
				if (trackProps != null)
				{
					for (int i = 0; i < trackProps.Count; i++)
						trackProps[i].loadContent(this);
				}
			}
			Camera.SongPanel = this;
			DefaultCamera.SongPanel = this;
		}

		public void update()
		{
			if (notes == null)
				return;
			TimeSpan newTime = stopwatch.Elapsed;
			deltaTimeS = (newTime - oldTime).getSecondsF();
			//if (deltaTimeS < renderInterval)
				//return;

			Camera.update((float)deltaTimeS);
			oldTime = newTime;
			selectRegion();

			#region Scroll song depending on user input or playback position.
			if (bPlayback)
			{
				double timeS;
				if (!bAudioPlayback)
				{
					timeS = (stopwatch.Elapsed - pbStartSysTime).TotalMilliseconds / 1000.0 + pbStartSongTimeS;
					if (timeS > AudioOffset)
					{
						bAudioPlayback = true;
						Media.startPlaybackAtTime(0);
					}
				}
				else
				{
					if (!Media.playbackIsRunning())
					{
						bPlayback = bAudioPlayback = false;
						timeS = getSongPosInSeconds();
					}
					else
						timeS = Media.getPlaybackPos() + AudioOffset;
				}
				setSongPosInSeconds(timeS, true);
				if (normSongPos > 1)
					togglePlayback();
			}
			else
				scrollSong();

			if (normSongPos < 0)
				normSongPos = 0;
			if (normSongPos > 1)
				normSongPos = 1;
			#endregion
		}

		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
		protected override void Draw()
        {
			GraphicsDevice.Clear(Color.Transparent);
			if (selectingRegion)
			{
				spriteBatch.Begin();
				Rectangle normRect = normalizeRect(selectedScreenRegion);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, normRect.Width, 1), Color.White);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, 1, normRect.Height), Color.White);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Bottom, normRect.Width, 1), Color.White);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Right, normRect.Top, 1, normRect.Height), Color.White);
				spriteBatch.End();
			}
			drawSong(new Point(ClientRectangle.Size.Width, ClientRectangle.Size.Height), normSongPos);
		}

		void selectRegion()
		{
			if (((Form1)Parent).trackListItems.Count == 0)
				return;
			if (leftMbPressed)
			{
				Invalidate();
				selectingRegion = true;
				int x = screenPosToSongPos(NormMouseX);
				selectedSongRegion.Width = x - selectedSongRegion.X;
				int y = getPitch(NormMouseY);
				selectedSongRegion.Height = y - selectedSongRegion.Y;

				Point mousePos = new Point((int)((NormMouseX * 0.5f + 0.5f) * ClientRectangle.Width), (int)(NormMouseY * ClientRectangle.Height));
				selectedScreenRegion.Width = mousePos.X - selectedScreenRegion.X;
				selectedScreenRegion.Height = mousePos.Y - selectedScreenRegion.Y;

				Rectangle normRect = normalizeRect(selectedSongRegion);
				int selectedCount = 0;
				for (int i = 1; i < notes.Tracks.Count; i++)
				{
					List<Midi.Note> noteList = trackProps[i].MidiTrack.getNotes(normRect.Left, normRect.Right, normRect.Top, normRect.Bottom);
					if (noteList.Count > 0)
					{
						((Form1)Parent).trackListItems[i].Selected = true;
						selectedCount++;
					}
					else if (!mergeRegionSelection && ((Form1)Parent).trackListItems.Count > 1)
						((Form1)Parent).trackListItems[i].Selected = false;
					//trackProps[i].Selected = false;
				}
				if (selectedCount == 0 && !mergeRegionSelection)
					((Form1)Parent).trackListItems[0].Selected = true;
				else if (selectedCount > 0)
					((Form1)Parent).trackListItems[0].Selected = false;
			}
			else if (selectingRegion)
			{
				selectingRegion = false;
				Invalidate();
			}
		}
		Rectangle normalizeRect(Rectangle _rect)
		{
			Rectangle rect = new Rectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height);
			if (rect.Height < 0)
			{
				int height = -rect.Height;
				rect.Y -= height;
				rect.Height = height;
			}
			if (rect.Width < 0)
			{
				int Width = -rect.Width;
				rect.X -= Width;
				rect.Width = Width;
			}
			return rect;
		}
		int screenPosToSongPos(float normScreenPos)
		{
            return (int)(normSongPos * notes.SongLengthT + (double)normScreenPos * viewWidthT * 0.5f);
		}
		Point getVisibleSongPortionT(double normPos)
		{
			int posT = (int)(normPos * notes.SongLengthT);
			return new Point(posT - viewWidthT, posT + viewWidthT);
		}
		int getPitch(float normPosY)
		{
			normPosY = 1 - normPosY;
			int height = ClientRectangle.Height - (int)(normPitchMargin * 2 * ClientRectangle.Height);
			float noteHeight = (float)height / notes.NumPitches;
			int pos = (int)((normPosY - normPitchMargin) * ClientRectangle.Height + 1);
			return MinPitch + (int)(pos / noteHeight);
		}
		void scrollSong()
		{
			if (mousePosScrollSong && !selectingRegion)
			{
				double dNormMouseX = (double)NormMouseX - scrollCenter;
				NormSongPos += (float)(Math.Pow(dNormMouseX, 2) * Math.Sign(dNormMouseX) * deltaTimeS * 0.13f);
			}
		}

		public void createTrackProps(int numTracks, bool eraseCurrent)
		{
			Visual_Music.TrackProps.NumTracks = numTracks;
			int startTrack; //At which index to start creating new (default) track props
			if (eraseCurrent || trackProps == null)
			{
				startTrack = 0;
				trackProps = new List<TrackProps>(numTracks);
			}
			else
				startTrack = trackProps.Count; //Keep current props but add new props if the new imported note file has more tracks than the current song. Start assigning default track props at current song's track count and up.
			
			for (int i = 0; i < numTracks; i++)
			{
				if (i < startTrack) //Just update notes, not visual props. Also reload note style effects in case a project file is being loaded, causing songPanel to be recreated with a new grapics device.
				{
					trackProps[i].MidiTrack = notes.Tracks[trackProps[i].TrackNumber];
					trackProps[i].createCurve();
                    trackProps[i].loadNoteStyleFx();
                }
				else //New note file has more tracks than current project or we're creating a new project. Create new track props for the new tracks.
				{
					TrackProps props = new Visual_Music.TrackProps(i, numTracks, notes);
					trackProps.Add(props);
				}				
			}
			if (startTrack >= numTracks && numTracks > 0)  //New note file has fewer tracks than current song. Remove the extra track props.
				trackProps.RemoveRange(numTracks, startTrack - numTracks);
            //Reload all notestyle fx files even if no new track props were created, since there is the possibility that songPanel was recreated with a new graphics device.
            //foreach (Visual_Music.TrackProps tp in trackProps)
               // tp.loadNoteStyleFx();

		}
		
		public void drawSong(Point viewportSize, double normPos)
		{
			if (notes == null)
				return;

			SongDrawProps songDrawProps = new SongDrawProps();
			songDrawProps.yMargin = (int)(normPitchMargin * viewportSize.Y);
			songDrawProps.noteHeight = (float)(viewportSize.Y - songDrawProps.yMargin * 2) / (NumPitches);
            songDrawProps.songPosT = SongPosT;
			songDrawProps.songPosS = (float)getSongPosInSeconds();
			songDrawProps.viewportSize = viewportSize;
			songDrawProps.viewWidthT = viewWidthT;
			songDrawProps.song = notes;
            songDrawProps.minPitch = MinPitch;

			for (int t=notes.Tracks.Count-1;t>=0;t--)
			{
				trackProps[t].drawTrack(songDrawProps, GlobalTrackProps, selectingRegion || ForceSimpleDrawMode);
			}
		}
		public bool importSong(string songFile, string audioFile, bool eraseCurrent, bool _insTrack, MixdownType mixdownType, double songLengthS)
		{
			desiredSongLengthS = songLengthS;
			Media.closeAudioFile();
			if (!openNoteFile(songFile, ref audioFile, eraseCurrent, _insTrack, mixdownType == MixdownType.Internal, songLengthS))
				return false;
			if (!openAudioFile(audioFile, mixdownType))
				return false;
			return true;
		}
		public bool openNoteFile(string file, ref string audioFile, bool eraseCurrent, bool _insTrack, bool mixdown, double songLengthS)
		{
			Invalidate();
			stopPlayback();
				
			noteFilePath = file;
			insTrack = _insTrack;
			int minPitch = 0, maxPitch = 0;
			if (eraseCurrent)
			{
				ViewWidthQn = DefaultViewWidthQn;
				AudioOffset = 0;
			}

			//if (string.IsNullOrWhiteSpace(noteFilePath))
			//{
			//	notes = null;
			//	trackProps = null;
			//	return true;
			//}

			Midi.Song newNotes = new Midi.Song();
			try
			{
				newNotes.openFile(noteFilePath, ref audioFile, _insTrack, mixdown, songLengthS);
			}
			catch (Exception)
			{
				//notes = null;
				//MessageBox.Show(Parent, e.Message, "Note file error");
				MessageBox.Show(Parent, "Couldn't load note file " + Path.GetFileName(NoteFilePath), "Note file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			notes = newNotes;
			if (eraseCurrent)
			{
				minPitch = notes.MinPitch;
				maxPitch = notes.MaxPitch;
			}

			notes.createNoteBsp();
				
			viewWidthT = (int)(ViewWidthQn * notes.TicksPerBeat);
			createTrackProps(notes.Tracks.Count, eraseCurrent);
		
			return true;
		}
		public bool openAudioFile(string file, MixdownType _mixdownType)
		{
			//internalMixdown = false;// mixdownType == MixdownType.Internal;
			if (_mixdownType == MixdownType.Tparty)
			{
				file = ImportNotesWithAudioForm.runTpartyProcess();
			}
			audioFilePath = file;

			if (string.IsNullOrWhiteSpace(file))
			{
				return true;
			}
			if (!Media.openAudioFile(file))
			{
				MessageBox.Show(Parent, "Couldn't load audio file " + file, "Audio file error");
				return false;
			}
			mixdownType = _mixdownType;
			if (notes != null)
				notes.SongLengthT = (int)secondsToTicks(Media.getAudioLength());
			return true;
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
		public void renderVideo(string videoFilePath, RenderProgressForm progressForm)
		{
			lock (renderLock)
			{
				//Visual_Music.TrackProps.Bgr = true;

				VideoFormat videoFormat = new VideoFormat();
				videoFormat.bitRate = 24000000;
				videoFormat.fps = 30;
				videoFormat.height = (uint)videoSize.Y;
				videoFormat.width = (uint)videoSize.X;
				//videoFormat.audioSampleRate = 48000;
				videoFormat.audioSampleRate = 44100;

				//ulong bla = 4;
				//Media.writeFrame(5, new uint[] { 0, 4 }, 0, ref bla, AudioOffset, false);
				//Media.endVideoEnc();
				if (!Media.beginVideoEnc(videoFilePath, videoFormat, true))
				{
					lock (progressForm.cancelLock)
						progressForm.Cancel = true;
					progressForm.showMessage("Couldn't initialize video encoding.");
				}
				else
				{
					//GraphicsDevice.SetRenderTarget(videoFrame);

					//int frames = 1000;
					uint[] frameData = new uint[videoSize.X * videoSize.Y]; //video size = rendertarget size
					UInt64 frameDuration = 0;
					UInt64 frameStart = 0;
					int currentTempoEvent = 0;
					int frames = 0;
					double songPosInSeconds = 0;
					const double startSongPosS = 0;
					double songPosInTicks = 0;
					double normSongPosBackup = normSongPos;
					setSongPosInSeconds(ref currentTempoEvent, ref songPosInTicks, ref songPosInSeconds, startSongPosS, false);

					while ((int)songPosInTicks < notes.SongLengthT && !progressForm.Cancel)
					{
						BeginDraw();
						//lock (progressForm)
						//progressForm.Progress = (double);
						progressForm.updateProgress((int)(frameStart / 10000000));
						GraphicsDevice.SetRenderTarget(videoFrame);
						GraphicsDevice.Clear(Color.Transparent);

						//spriteBatch.Begin(SpriteSortMode.Deferred, blendState);
						drawSong(videoSize, (float)songPosInTicks / notes.SongLengthT);
						//spriteBatch.End();

						GraphicsDevice.SetRenderTarget(null);
						videoFrame.GetData<uint>(frameData);
						for (uint i = 0; i < frameData.Length; i++)
						{
							uint c = frameData[i];
							frameData[i] = ((c & 0xff) << 16) | (c & 0xff00) | ((c & 0xff0000) >> 16);
						}

						if (!Media.writeFrame(frameData, frameStart, ref frameDuration, AudioOffset, false))
						{
							lock (progressForm.cancelLock)
								progressForm.Cancel = true;
							progressForm.showMessage("Couldn't add frame");
							break;
						}
						frameStart += frameDuration;

						setSongPosInSeconds(ref currentTempoEvent, ref songPosInTicks, ref songPosInSeconds, songPosInSeconds + 1.0f / videoFormat.fps, false);
						frames++;
						//EndDraw();
					}
					normSongPos = normSongPosBackup;
				}
				GraphicsDevice.SetRenderTarget(null);
				Media.endVideoEnc();
				Visual_Music.TrackProps.Bgr = false;
			}
		}
		double secondsToTicks(double seconds)
		{
			int currentTempoEvent = 0;
			double currentTimeT = 0;
			double currentTimeS = 0;
			double normSongPosTemp = normSongPos;
			setSongPosInSeconds(ref currentTempoEvent, ref currentTimeT, ref currentTimeS, seconds, false);
			//Todo: create function that returns these values without modifying Mormsongpos
			normSongPos = normSongPosTemp;
			return currentTimeT;
		}

		void setSongPosInSeconds(double newTimeS, bool updateScreen)
		{
			int currentTempoEvent = 0;
			double currentTimeT = 0;
			double currentTimeS = 0;
			setSongPosInSeconds(ref currentTempoEvent, ref currentTimeT, ref currentTimeS, newTimeS, updateScreen);
		}
		void setSongPosInSeconds(ref int currentTempoEvent, ref double currentTimeT, ref double currentTimeS, double newTimeS, bool updateScreen)
		{
			int nextTempoEvent = currentTempoEvent; 
			bool bLastTempoEvent = false;
			while ((float)currentTimeS < (float)newTimeS)
			{
				double nextTimeStepS;
				double currentBps = notes.TempoEvents[currentTempoEvent].Tempo / 60; //beats per seconds
				if (bLastTempoEvent)
					nextTimeStepS = newTimeS;
				else
				{
					if (currentTempoEvent + 1 < notes.TempoEvents.Count)
						nextTempoEvent++;
					else
						bLastTempoEvent = true;
					
					double nextTempoTimeS = ((double)notes.TempoEvents[nextTempoEvent].Time - currentTimeT) / (notes.TicksPerBeat * currentBps) + (double)currentTimeS;
					nextTimeStepS = nextTempoTimeS;
					if (nextTimeStepS > newTimeS || nextTimeStepS < currentTimeS || nextTimeStepS == currentTimeS && bLastTempoEvent)
						nextTimeStepS = newTimeS; //always causes loop to exit
					else
						currentTempoEvent = nextTempoEvent;
				}			
				currentTimeT += (nextTimeStepS - currentTimeS) * currentBps * notes.TicksPerBeat;
				currentTimeS = nextTimeStepS;
			}
			double newSongPos = currentTimeT / (double)notes.SongLengthT;
			if (updateScreen)
				NormSongPos = newSongPos;
			else
				normSongPos = newSongPos;
		}

		double getSongPosInSeconds()
		{
			return getSongPosInSeconds(normSongPos);
		}
		public double getSongPosInSeconds(double _normSongPos)
		{
			if (notes == null)
				return 0;
			int songPosT = (int)(_normSongPos * notes.SongLengthT);
			int nextTempoEvent = 0;
			int currentTempoEvent = 0;
			bool bLastTempoEvent = false;
			int currentTimeT = 0;
			double currentTimeS = 0;

			while (currentTimeT < songPosT)
			{
				int nextTimeStepT;
				double currentBps = notes.TempoEvents[currentTempoEvent].Tempo / 60; //beats per seconds
				if (bLastTempoEvent)
					nextTimeStepT = songPosT;
				else
				{
					if (currentTempoEvent + 1 < notes.TempoEvents.Count)
						nextTempoEvent++;
					else
						bLastTempoEvent = true;

					nextTimeStepT = notes.TempoEvents[nextTempoEvent].Time;
					if (nextTimeStepT > songPosT || nextTimeStepT < currentTimeT || nextTimeStepT == currentTimeT && bLastTempoEvent)
						nextTimeStepT = songPosT; //always causes loop to exit
					else
						currentTempoEvent = nextTempoEvent;
				}
				currentTimeS += (nextTimeStepT - currentTimeT) / (notes.TicksPerBeat * currentBps);
				currentTimeT = nextTimeStepT;
			}
			return currentTimeS;
		}
		public void togglePlayback()
		{
			if (Media.getAudioLength() == 0)
				return;
			bPlayback = !bPlayback;
			bAudioPlayback = !bAudioPlayback;
			if (!bPlayback)
			{
				if (!Media.pausePlayback())
					MessageBox.Show("An error occured while pausing playback.");
			}
			else
			{
				double startTime = getSongPosInSeconds() - AudioOffset;
				if (startTime >= 0)
				{
					if (!Media.startPlaybackAtTime(startTime))
						MessageBox.Show("An error occured while starting playback.");
					if (!Media.playbackIsRunning()) //Assuming this is because we tried to start playback after end of audio
					{
						bPlayback = false;
						bAudioPlayback = false;
					}
				}
				else
				{
					pbStartSysTime = stopwatch.Elapsed;
					pbStartSongTimeS = getSongPosInSeconds();
					bAudioPlayback = false;
				}
			}
		}
		public void stopPlayback()
		{
			bPlayback = false;
			bAudioPlayback = false;
			if (!Media.stopPlayback())
				MessageBox.Show("An error occured while stopping playback.");
			NormSongPos = 0;
		}
		public static Color HSLA2RGBA(double h, double s, double l, float a, bool bgr)
		{
			double v;
			double r, g, b;
			r = l;   // default to gray
			g = l;
			b = l;
			v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);
			if (v > 0)
			{
				double m;
				double sv;
				int sextant;
				double fract, vsf, mid1, mid2;
				m = l + l - v;
				sv = (v - m) / v;
				h *= 6.0;
				sextant = (int)h;
				fract = h - sextant;
				vsf = v * sv * fract;
				mid1 = m + vsf;
				mid2 = v - vsf;
				switch (sextant)
				{
					case 0:
						r = v;
						g = mid1;
						b = m;
						break;
					case 1:
						r = mid2;
						g = v;
						b = m;
						break;
					case 2:
						r = m;
						g = v;
						b = mid1;
						break;
					case 3:
						r = m;
						g = mid2;
						b = v;
						break;
					case 4:
						r = mid1;
						g = m;
						b = v;
						break;
					case 5:
						r = v;
						g = m;
						b = mid2;
						break;
				}
			}
			Color rgb;
			if (bgr)
				rgb = new Color((float)b, (float)g, (float)r, 1);
			else
				rgb = new Color((float)r, (float)g, (float)b, (float)1);
			//rgb *= a;
			rgb.A = (byte)(a*255);

			return rgb;
		}
		//NoteDrawingProps getNoteDrawingProps(int track, bool hilited)
		//{
		//    NoteDrawingProps props = new NoteDrawingProps();
		//    props.texture = trackProps[track].getTexture(hilited, GlobalTrackProps);
		//    props.color = trackProps[track].getColor(hilited, GlobalTrackProps, true);
		//    return props;
		//}
		
		public void updateTimeStamp()
		{
			oldTime = stopwatch.Elapsed;
		}
		public void resetTrackProps(ListView.SelectedIndexCollection indices)
		{
			foreach (int index in indices)
				trackProps[index].resetProps();
		}
        protected override void OnResize(EventArgs e)
        {
            Invalidate();
        }

		protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (notes == null)
                return;
			if (e.Button == MouseButtons.Left)
			{
				leftMbPressed = true;
				if (ModifierKeys.HasFlag(WinKeys.Shift))
					mergeRegionSelection = true;
				selectedSongRegion.X = screenPosToSongPos(NormMouseX);
				selectedSongRegion.Y = getPitch(NormMouseY);
				selectedScreenRegion.X = (int)((NormMouseX * 0.5f + 0.5f) * ClientRectangle.Width);
				selectedScreenRegion.Y = (int)(NormMouseY * ClientRectangle.Height);
			}
			if (e.Button == MouseButtons.Right)
			{
				mousePosScrollSong = true;
			}

		}
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left)
			{
				leftMbPressed = false;
				mergeRegionSelection = false;
			}
			if (e.Button == MouseButtons.Right)
				mousePosScrollSong = false;
		}
    }

    static class SongFormat
    {
        public const int writeVersion = 1;
        public static int readVersion; 
    }
}

