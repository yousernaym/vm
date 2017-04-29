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
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace Visual_Music
{
	using GdiPoint = System.Drawing.Point;
	using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
    using WinKeys = System.Windows.Forms.Keys;

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
			return (float)((double)(timeT - songPosT) / viewWidthT + 0.5) * viewportSize.X;
		}
		public int getPitchScreenPos(int pitch)
		{
			return (int)getPitchScreenPos((float)pitch);
		}
		public float getPitchScreenPos(float pitch)
		{
			return (float)viewportSize.Y - (pitch - minPitch) * (float)noteHeight - noteHeight / 2.0f - yMargin;
		}
		public float getSongPosT(float screenX)
		{ //Returns song pos in ticks
			return (((float)screenX / (float)viewportSize.X - 0.5f) * (float)viewWidthT + (float)songPosT);
		}
		public float getSongPosP(float screenX)
		{ //Returns song pos in pixels
			return getSongPosT(screenX) * viewportSize.X / viewWidthT;
		}
		public float getSongLengthP()
		{
			return (float)(song.SongLengthInTicks * viewportSize.X) / viewWidthT;
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
        ContentManager content;
        public ContentManager Content { get { return content; } }
        bool audioFromMod;
        Texture2D regionSelectTexture;
        float normPitchMargin = 1 / 50.0f;
        bool bPlayback = false;
        public bool IsPlaying
        {
            get { return bPlayback; }
        }
        bool bAudioFileLoaded = false;
        bool bAudioPlayback = false;
        TimeSpan oldTime = new TimeSpan(0);
        TimeSpan pbStartSysTime = new TimeSpan(0);
        double pbStartSongTimeS;
        Stopwatch stopwatch = new Stopwatch();
        public bool MbPressed { get; set; }
        public bool RightMbPressed { get; set; }
        bool scrollingSong = false;
        double scrollCenter = 0;
        bool selectingRegion = false;
        bool mergeRegionSelection = false;
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
            //			set { blendState = value; }
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
            get { return audioFilePath; }
            set { audioFilePath = value; }
            //get { return ((Form1)Parent).sourceFileForm.AudioFilePath; }
            //set { ((Form1)Parent).sourceFileForm.AudioFilePath = value; }
        }
        bool modInsTrack;
        public bool ModInsTrack
        {
            get { return modInsTrack; }
        }

        bool isMod = false;
        public bool IsMod { get => isMod; set => isMod = value; }
		Point videoSize = new Point(1920, 1080);
        public int SongLengthT { get { return notes != null ? notes.SongLengthInTicks : 0; } }
        public int SongPosT { get { return (int)(normSongPos * SongLengthT); } }
        double normSongPos;
        public double NormSongPos { get => normSongPos; set => normSongPos = value; }
        const float defaultQn_viewWidth = 16; //Number of quarter notes that fits on screen
		float qn_viewWidth = defaultQn_viewWidth; 
		
        public float Qn_viewWidth
		{
			get {return qn_viewWidth;}
			set
			{
				qn_viewWidth = value;
				if (notes != null)
					viewWidthT = (int)(qn_viewWidth * notes.TimeDiv);
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
			audioFilePath = (string)info.GetValue("audioFilePath", typeof(string));
			modInsTrack = (bool)info.GetValue("modInsTrack", typeof(bool));
			//int trackPropsCountFromFile = (int)info.GetValue("trackPropsCount", typeof(int));
            trackProps = (List<TrackProps>)info.GetValue("trackProps", typeof(List<TrackProps>));
   //         for (int i = 0; i < notes.Tracks.Count && i < trackPropsCountFromFile; i++)
			//{
				
			//	trackProps[i].MidiTrack = notes.Tracks[trackProps[i].TrackNumber];
			//	trackProps[i].createCurve();
			//	//trackProps[i].NumTracks = trackPropsCountFromFile;
			//}
			
			Qn_viewWidth = (float)info.GetValue("qn_viewWidth", typeof(float));
			AudioOffset = (double)info.GetValue("audioOffset", typeof(double));
			MaxPitch = (int)info.GetValue("maxPitch", typeof(int));
			MinPitch = (int)info.GetValue("minPitch", typeof(int));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
            info.AddValue("version", SongFormat.writeVersion);
            info.AddValue("noteFilePath", noteFilePath);
			info.AddValue("audioFilePath", audioFilePath);
			info.AddValue("modInsTrack", modInsTrack);
            //info.AddValue("trackPropsCount", trackProps.Count);
            //for (int i = 0; i < trackProps.Count; i++)
            //info.AddValue("trackProps"+i, trackProps[i]);
            info.AddValue("trackProps", trackProps);
            info.AddValue("qn_viewWidth", Qn_viewWidth);
			info.AddValue("audioOffset", AudioOffset);
			info.AddValue("maxPitch", MaxPitch);
			info.AddValue("minPitch", MinPitch);
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

            importSong(noteFilePath, audioFilePath, false, modInsTrack);
            if (trackProps != null)
            {
			    for (int i = 0; i < trackProps.Count; i++)
                    trackProps[i].loadContent(this);
			}
			
			
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
			
			//try
			//{
				GraphicsDevice.Clear(Color.Transparent);
				if (notes == null)
                    return;

				//newKbState = Keyboard.GetState();
				//if (newKbState.IsKeyDown(XnaKeys.Space) && oldKbState.IsKeyUp(XnaKeys.Space))
				//togglePlayback();
				selectRegion();

				TimeSpan newTime = stopwatch.Elapsed;
				TimeSpan deltaTime = newTime - oldTime;
				oldTime = newTime;

				#region Scroll song depending on user input or playback position.
				if (bPlayback)
				{
					Invalidate();
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
					setSongPosInSeconds(timeS);
					//double songPosInTicks = 0;
					//songPosInTicks = secondsToTicks(timeS);
					//normSongPos = songPosInTicks / (double)notes.SongLengthInTicks;
					if (normSongPos > 1)
						togglePlayback();
				}
				else
					scrollSong(deltaTime);

				if (normSongPos < 0)
					normSongPos = 0;
				if (normSongPos > 1)
					normSongPos = 1;
				#endregion
				//spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				drawSong(new Point(ClientRectangle.Size.Width, ClientRectangle.Size.Height), normSongPos);
			//}
			//catch (Exception e)
			//{
			//	MessageBox.Show(e.Message);
			//	throw e;
			//}
		}

		void selectRegion()
		{
			if (selectingRegion)
			{
				int x = screenPosToSongPos(NormMouseX);
				selectedSongRegion.Width = x - selectedSongRegion.X;
				int y = getPitch(NormMouseY);
				selectedSongRegion.Height = y - selectedSongRegion.Y;

				Point mousePos = new Point((int)((NormMouseX * 0.5f + 0.5f) * ClientRectangle.Width), (int)(NormMouseY * ClientRectangle.Height));
				selectedScreenRegion.Width = mousePos.X - selectedScreenRegion.X;
				selectedScreenRegion.Height = mousePos.Y - selectedScreenRegion.Y;

				spriteBatch.Begin();
				Rectangle normRect = normalizeRect(selectedScreenRegion);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, normRect.Width, 1), Color.White);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, 1, normRect.Height), Color.White);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Bottom, normRect.Width, 1), Color.White);
				spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Right, normRect.Top, 1, normRect.Height), Color.White);
				spriteBatch.End();

				normRect = normalizeRect(selectedSongRegion);
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
            return (int)(normSongPos * notes.SongLengthInTicks + (double)normScreenPos * viewWidthT * 0.5f);
		}
		Point getVisibleSongPortionT(double normPos)
		{
			int posT = (int)(normPos * notes.SongLengthInTicks);
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
		void scrollSong(TimeSpan deltaTime)
		{
			double dNormMouseX = (double)NormMouseX;
			if (RightMbPressed == true && !selectingRegion)
			{
				if (!scrollingSong)
				{
					scrollingSong = true;
				}
				dNormMouseX -= scrollCenter;
				normSongPos += (float)(Math.Pow(dNormMouseX, 2) * Math.Sign(dNormMouseX) * deltaTime.Ticks / 20000000.0);
			}
			else
				scrollingSong = false;
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
				if (i < startTrack) //Just update notes, not visual props.
				{
					trackProps[i].MidiTrack = notes.Tracks[trackProps[i].TrackNumber];
					trackProps[i].createCurve();
				}
				else //New note file has more tracks than current song. Create new track props for the new tracks.
				{
					TrackProps props = new Visual_Music.TrackProps(i, numTracks, notes);
					trackProps.Add(props);
				}				
			}
			if (startTrack >= numTracks && numTracks > 0)  //New note file has fewer tracks than current song. Remove the extra track props.
				trackProps.RemoveRange(numTracks, startTrack - numTracks);
            //Reload all notestyle fx files even if no new track props were created, since there is the possibility that songPanel was recreated with a new graphics device.
            foreach (Visual_Music.TrackProps tp in trackProps)
                tp.loadNoteStyleFx();

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
				trackProps[t].drawTrack(songDrawProps, GlobalTrackProps, selectingRegion);
			}
		}
		public bool importSong(string songFile, string audioFile, bool eraseCurrent, bool modInsTrack)
		{
			bool b = Media.closePlaybackSession();
			bool noAudioFile = false;
			if (string.IsNullOrEmpty(audioFile))
				noAudioFile = true;
			if (!openNoteFile(songFile, ref audioFile, eraseCurrent, modInsTrack))
				return false;
			if (!string.IsNullOrEmpty(audioFile) && noAudioFile)
				audioFromMod = true;
			else
				audioFromMod = false;
			if (!openAudioFile(audioFile))
				return false;
			return true;
		}
		public bool openNoteFile(string file, ref string audioFile, bool eraseCurrent, bool _modInsTrack)
		{
			//try
			//{
				Invalidate();
				stopPlayback();
				noteFilePath = file;
				modInsTrack = _modInsTrack;
				int minPitch = 0, maxPitch = 0;
				if (eraseCurrent)
				{
					Qn_viewWidth = defaultQn_viewWidth;
					AudioOffset = 0;
				}
				else if (notes != null)
				{
					
				}
				if (string.IsNullOrEmpty(noteFilePath))
				{
					notes = null;
					trackProps = null;
					return true;
				}
				
				notes = new Midi.Song();
				notes.openFile(noteFilePath, ref audioFile, modInsTrack);
				if (eraseCurrent)
				{
                    minPitch = notes.MinPitch;
                    maxPitch = notes.MaxPitch;
				}

				notes.createNoteBsp();
				
				viewWidthT = (int)(Qn_viewWidth * notes.TimeDiv);
				createTrackProps(notes.Tracks.Count, eraseCurrent);
			//}
			//catch (Exception e)
			//{
			//	MessageBox.Show(Parent, e.Message, "Note file error");
			//	return false;
			//}
			return true;
		}
		public bool openAudioFile(string file)
		{
			if (!audioFromMod)
				audioFilePath = file;
			stopPlayback();
			if (Form1.isEmpty(file))
			{
				bAudioFileLoaded = false;
				return true;
			}
			if (!Media.openFileForPlayback(file))
			{
				MessageBox.Show(Parent, "Couldn't load audio file " + file, "Audio file error");
				return false;
			}
			bAudioFileLoaded = true;
			if (notes != null)
				notes.SongLengthInTicks = (int)secondsToTicks(Media.getAudioLength());
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

				if (!Media.beginVideoEnc(videoFilePath, audioFromMod ? Midi.Song.getModMixdownFilename() : audioFilePath, videoFormat, true))
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
					setSongPosInSeconds(ref currentTempoEvent, ref songPosInTicks, ref songPosInSeconds, startSongPosS);

					while ((int)songPosInTicks < notes.SongLengthInTicks && !progressForm.Cancel)
					{
						BeginDraw();
						//lock (progressForm)
						//progressForm.Progress = (double);
						progressForm.updateProgress((int)(frameStart / 10000000));
						GraphicsDevice.SetRenderTarget(videoFrame);
						GraphicsDevice.Clear(Color.Transparent);

						//spriteBatch.Begin(SpriteSortMode.Deferred, blendState);
						drawSong(videoSize, (float)songPosInTicks / notes.SongLengthInTicks);
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

						setSongPosInSeconds(ref currentTempoEvent, ref songPosInTicks, ref songPosInSeconds, songPosInSeconds + 1.0f / videoFormat.fps);
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
			setSongPosInSeconds(ref currentTempoEvent, ref currentTimeT, ref currentTimeS, seconds);
			normSongPos = normSongPosTemp;
			return currentTimeT;
		}

		void setSongPosInSeconds(double newTimeS)
		{
			int currentTempoEvent = 0;
			double currentTimeT = 0;
			double currentTimeS = 0;
			setSongPosInSeconds(ref currentTempoEvent, ref currentTimeT, ref currentTimeS, newTimeS);
		}
		void setSongPosInSeconds(ref int currentTempoEvent, ref double currentTimeT, ref double currentTimeS, double newTimeS)
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
					
					double nextTempoTimeS = ((double)notes.TempoEvents[nextTempoEvent].Time - currentTimeT) / (notes.TimeDiv * currentBps) + (double)currentTimeS;
					nextTimeStepS = nextTempoTimeS;
					if (nextTimeStepS > newTimeS || nextTimeStepS < currentTimeS || nextTimeStepS == currentTimeS && bLastTempoEvent)
						nextTimeStepS = newTimeS; //always causes loop to exit
					else
						currentTempoEvent = nextTempoEvent;
				}			
				currentTimeT += (nextTimeStepS - currentTimeS) * currentBps * notes.TimeDiv;
				currentTimeS = nextTimeStepS;
			}
			normSongPos = currentTimeT / (double)notes.SongLengthInTicks;
		}

		double getSongPosInSeconds()
		{
			return getSongPosInSeconds(normSongPos);
		}
		public double getSongPosInSeconds(double _normSongPos)
		{
			if (notes == null)
				return 0;
			int songPosT = (int)(_normSongPos * notes.SongLengthInTicks);
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
				currentTimeS += (nextTimeStepT - currentTimeT) / (notes.TimeDiv * currentBps);
				currentTimeT = nextTimeStepT;
			}
			return currentTimeS;
		}
		public void togglePlayback()
		{
			if (!bAudioFileLoaded)
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
			normSongPos = 0;
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
            if (e.Button != MouseButtons.Left || notes == null)
                return;
            selectingRegion = true;
            if (ModifierKeys.HasFlag(WinKeys.Shift))
                mergeRegionSelection = true;
            selectedSongRegion.X = screenPosToSongPos(NormMouseX);
            selectedSongRegion.Y = getPitch(NormMouseY);
            selectedScreenRegion.X = (int)((NormMouseX * 0.5f + 0.5f) * ClientRectangle.Width);
            selectedScreenRegion.Y = (int)(NormMouseY * ClientRectangle.Height);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            selectingRegion = false;
            mergeRegionSelection = false;
        }
    }

    static class SongFormat
    {
        public const int writeVersion = 1;
        public static int readVersion; 
    }
}

