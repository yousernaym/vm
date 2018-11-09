﻿using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Visual_Music
{
	using GdiPoint = System.Drawing.Point;
	public enum SourceSongType { Midi, Mod, Sid };

	[Serializable()]
	public class Project : ISerializable
	{
		public float UserViewWidth = 1000f;
		const float NormPitchMargin = 1 / 100.0f;
		float PitchMargin => NormPitchMargin * Camera.ViewportSize.Y;
		public float NoteHeight => (Camera.ViewportSize.Y - PitchMargin * 2) / NumPitches;

		SongPanel songPanel;
		public SongPanel SongPanel
		{
			get => songPanel;
			set
			{
				if (songPanel != value)
				{
					songPanel = value;
					songPanel.Project = this;
					Camera.SongPanel = DefaultCamera.SongPanel = songPanel;
					loadContent();
				}
			}
		}
		TimeSpan pbStartSysTime = new TimeSpan(0);
		double pbStartSongTimeS;

		//Serialization----------------------------------

		public ImportOptions ImportOptions { get; set; }
				
		List<TrackView> trackViews;
		public List<TrackView> TrackViews
		{
			get { return trackViews; }
			set { trackViews = value; }
		}

		public const float DefaultViewWidthQn = 16; //Number of quarter notes that fits on screen

		float viewWidthQn = DefaultViewWidthQn;
		public float ViewWidthQn
		{
			get { return viewWidthQn; }
			set
			{
				viewWidthQn = value;
				if (notes != null)
					viewWidthT = viewWidthQn * notes.TicksPerBeat;
			}
		}
		float vertViewWidthQn;
		public float VertWidthScale => vertViewWidthQn / viewWidthQn;

		float viewWidthT; ////Number of ticks that fits on screen
		public float ViewWidthT { get => viewWidthT; }

		public double AudioOffset { get; set; }
		public double PlaybackOffsetS { get; set; } = 0;
		public double PlaybackOffsetT
		{
			get
			{
				if (PlaybackOffsetS >= 0)
					return PlaybackOffsetS * notes.TempoEvents[0].Tempo / 60 * notes.TicksPerBeat;
				else
					return -secondsToTicks(-PlaybackOffsetS);
			}
		}
		double TempoEventsOffset => PlaybackOffsetS > 0 ? PlaybackOffsetT : 0;
		public float PlaybackOffsetP => getScreenPosX(PlaybackOffsetT);

		public int MinPitch { get; set; }
		public int MaxPitch { get; set; }
		int NumPitches { get { return MaxPitch - MinPitch + 1; } }
		
		public Camera Camera { get; set; } = new Camera();
		public Camera DefaultCamera { get; } = new Camera();
		//----------------------------------------------------

		public TrackProps GlobalTrackProps
		{
			get { return trackViews[0].TrackProps; }
			set { trackViews[0].TrackProps = value; }
		}

		Midi.Song notes;
		public Midi.Song Notes { get { return notes; } }

		public double SongLengthT => (notes != null ? notes.SongLengthT : 0) + PlaybackOffsetT; //Song length in ticks
		public double SongPosT => (int)(normSongPos * SongLengthT); //Current song position in ticks
		public double SongPosB => (float)SongPosT / Notes.TicksPerBeat; //Current song position in beats
		public float SongPosP => getScreenPosX(SongPosT); //Current song position in pixels
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
					songPanel.Invalidate();
					if (SongPanel.OnSongPosChanged != null)
						SongPanel.OnSongPosChanged();
				}
			}
		}
		
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
		
		public Project(SongPanel spanel)
		{
			SongPanel = spanel;
		}

		public void loadContent()
		{
			if (ImportOptions == null)
				return;
			if (string.IsNullOrWhiteSpace(ImportOptions.NotePath))
				throw new FileFormatException("Note file path missing from song file.");
			if (!File.Exists(ImportOptions.NotePath))
				throw new FileNotFoundException("Note file missing: " + ImportOptions.NotePath);
			if (!string.IsNullOrWhiteSpace(ImportOptions.AudioPath) && !File.Exists(ImportOptions.AudioPath))
				Form1.showWarningMsgBox("Audio file missing: " + ImportOptions.AudioPath);
			
			ImportOptions.EraseCurrent = false;
			importSong(ImportOptions);
			if (trackViews != null)
			{
				for (int i = 0; i < trackViews.Count; i++)
					trackViews[i].TrackProps.loadContent(songPanel);
			}
		}

		public Project(SerializationInfo info, StreamingContext ctxt) : base()
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "version")
					SongFormat.readVersion = (int)entry.Value;
				else if (entry.Name == "importOptions")
					ImportOptions = (ImportOptions)entry.Value;
				else if (entry.Name == "trackViews")
					trackViews = (List<TrackView>)entry.Value;
				else if (entry.Name == "qn_viewWidth")
					ViewWidthQn = (float)entry.Value;
				else if (entry.Name == "audioOffset")
					AudioOffset = (double)entry.Value;
				else if (entry.Name == "maxPitch")
					MaxPitch = (int)entry.Value;
				else if (entry.Name == "minPitch")
					MinPitch = (int)entry.Value;
				else if (entry.Name == "tpartyApp")
					ImportNotesWithAudioForm.TpartyApp = (string)entry.Value;
				else if (entry.Name == "tpartyArgs")
					ImportNotesWithAudioForm.TpartyArgs = (string)entry.Value;
				else if (entry.Name == "tpartyOutputDir")
				{
					string dir = ((string)entry.Value);
					if (!string.IsNullOrWhiteSpace(dir))
					{
						dir = dir.ToLower();
						if (dir.Contains(Program.TempDirRoot))
							dir = Program.TempDir;
						ImportNotesWithAudioForm.TpartyOutputDir = dir;
					}
				}
				else if (entry.Name == "camera")
					Camera = (Camera)entry.Value;
				else if (entry.Name == "userViewWidth")
					UserViewWidth = (float)entry.Value;
			}
			//noteFileType = (Midi.FileType)info.GetValue("noteFileType", typeof(Midi.FileType));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("version", SongFormat.writeVersion);
			info.AddValue("importOptions", ImportOptions);
			info.AddValue("trackViews", trackViews);
			info.AddValue("qn_viewWidth", ViewWidthQn);
			info.AddValue("audioOffset", AudioOffset);
			info.AddValue("maxPitch", MaxPitch);
			info.AddValue("minPitch", MinPitch);
			info.AddValue("tpartyApp", ImportNotesWithAudioForm.TpartyApp);
			info.AddValue("tpartyArgs", ImportNotesWithAudioForm.TpartyArgs);
			info.AddValue("tpartyOutputDir", ImportNotesWithAudioForm.TpartyOutputDir);
			info.AddValue("camera", Camera);
			info.AddValue("userViewWidth", UserViewWidth);
		}

		public void importSong(ImportOptions options)
		{
			string mixdownPath;
			Media.closeAudioFile();

			openNoteFile(options, out mixdownPath);
			openAudioFile(string.IsNullOrWhiteSpace(mixdownPath) ? options.AudioPath : mixdownPath, options.MixdownType);
					
			ImportOptions = options;
			if (options.EraseCurrent)
				DefaultFileName = Path.GetFileName(ImportOptions.NotePath) + DefaultFileExt;
			createTrackViews(notes.Tracks.Count, options.EraseCurrent);
		}

		public bool openNoteFile(ImportOptions options, out string mixdownPath)
		{
			SongPanel.Invalidate();
			stopPlayback();
			mixdownPath = null;

			if (options.EraseCurrent)
			{
				ViewWidthQn = DefaultViewWidthQn;
				AudioOffset = 0;
				PlaybackOffsetS = 0;
				Camera = new Camera(songPanel);
			}

			Midi.Song newNotes = new Midi.Song();
			newNotes.openFile(options, out mixdownPath);
			if (newNotes.Tracks == null || newNotes.Tracks.Count == 0 || newNotes.SongLengthT == 0)
				throw new FileFormatException(new Uri(options.RawNotePath), "No notes found.");
									
			notes = newNotes;
			notes.createNoteBsp();

			viewWidthT = (int)(ViewWidthQn * notes.TicksPerBeat);
			return true;
		}
		public void openAudioFile(string file, Midi.MixdownType mixdownType)
		{
			if (mixdownType == Midi.MixdownType.Tparty)
				file = ImportNotesWithAudioForm.runTpartyProcess();

			//if (string.IsNullOrWhiteSpace(file))
				//return;
			if (!File.Exists(file))  //If loading project file and audio file is no longer where it should be, or tparty process failed, keep loading/importing but skip audio. (If importing, import won't even start if trying to import with non-empty incorrect audio path.)
				return;
						
			if (!Media.openAudioFile(file))
				throw new FileFormatException(new Uri(file));
						
			if (notes != null)
				notes.SongLengthT = (int)secondsToTicks((float)(Media.getAudioLength() + AudioOffset));
		}

		public void resetPitchLimits()
		{
			MaxPitch = Notes.MaxPitch;
			MinPitch = Notes.MinPitch;
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
				startTrack = trackViews.Count; //Keep current props but add new props if the new imported note file has more tracks than the current song. Start assigning default track props at current song's track count and up.

			for (int i = 0; i < numTracks; i++)
			{
				if (i < startTrack) //If updating source files without creating new projects, or loading project file.
				{  //No need to update visual props
				   // Update notes
					trackViews[i].MidiTrack = notes.Tracks[trackViews[i].TrackNumber];
					trackViews[i].createCurve();
					//If a project file is being loaded, track views was deserialized, and further init involving the graphics device is needed here, because it was not initialized at the time of deserialization.
					trackViews[i].TrackProps.StyleProps.loadFx();
				}
				else //New note file has more tracks than current project or we're creating a new project. Create new track props for the new tracks.
				{
					TrackView view = new TrackView(i, numTracks, notes);
					trackViews.Add(view);
				}
			}
			if (startTrack >= numTracks && numTracks > 0)  //New note file has fewer tracks than current song. Remove the extra trackViews.
				trackViews.RemoveRange(numTracks, startTrack - numTracks);
			TrackProps.GlobalProps = trackViews[0].TrackProps;
			createOcTrees();
		}

		public void createOcTrees()
		{
			if (trackViews == null || viewWidthQn == 0 || Notes == null)
				return;
			vertViewWidthQn = viewWidthQn;
			//for (int i = TrackViews.Count - 1; i > 0; i--)
			
			for (int i = 1; i < trackViews.Count; i++)
				TrackViews[i].createOcTree(this, GlobalTrackProps);
		}

		public void drawSong(Point viewportSize, double normPos)
		{
			if (notes == null)
				return;

			DepthStencilState oldDss = songPanel.GraphicsDevice.DepthStencilState;
			DepthStencilState dss = new DepthStencilState();
			dss.StencilEnable = true;
			dss.StencilFunction = CompareFunction.Greater;
			dss.StencilPass = StencilOperation.Replace;
			dss.ReferenceStencil = 1;
			songPanel.GraphicsDevice.DepthStencilState = dss;
			for (int t = 1; t < trackViews.Count; t++)
			{
				songPanel.GraphicsDevice.Clear(ClearOptions.Stencil | ClearOptions.DepthBuffer, Color.AliceBlue, 1, 0);
				trackViews[t].drawTrack(GlobalTrackProps, SongPanel.ForceDefaultNoteStyle);
			}
			songPanel.GraphicsDevice.DepthStencilState = oldDss;
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
			float height = 1 - NormPitchMargin * 2;
			float noteHeight = height / Notes.NumPitches;
			float pos = normPosY - NormPitchMargin;
			return MinPitch + (int)(pos / noteHeight);
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
			foreach (int index in indices)
				trackViews[index].TrackProps.resetProps();
			createOcTrees();
		}

		double secondsToTicks(double seconds)
		{
			int currentTempoEvent = 0;
			double currentTimeT = 0;
			double currentTimeS = 0;
			int nextTempoEvent = currentTempoEvent;
			bool bLastTempoEvent = false;
			while ((float)currentTimeS < (float)seconds)
			{
				double nextTimeStepS;
				double currentBps = notes.TempoEvents[currentTempoEvent].Tempo / 60; //beats per seconds
				if (bLastTempoEvent)
					nextTimeStepS = seconds;
				else
				{
					if (currentTempoEvent + 1 < notes.TempoEvents.Count)
						nextTempoEvent++;
					else
						bLastTempoEvent = true;

					double nextTempoTimeS = (notes.TempoEvents[nextTempoEvent].Time + TempoEventsOffset - currentTimeT) / (notes.TicksPerBeat * currentBps) + currentTimeS;
					nextTimeStepS = nextTempoTimeS;
					if (nextTimeStepS > seconds || nextTimeStepS < currentTimeS || nextTimeStepS == currentTimeS && bLastTempoEvent)
						nextTimeStepS = seconds; //always causes loop to exit
					else
						currentTempoEvent = nextTempoEvent;
				}
				currentTimeT += (nextTimeStepS - currentTimeS) * currentBps * notes.TicksPerBeat;
				currentTimeS = nextTimeStepS;
			}
			return currentTimeT;
		}

		public void setSongPosInSeconds(double newTimeS, bool updateScreen)
		{
			int currentTempoEvent = 0;
			double currentTimeT = 0;
			double currentTimeS = 0;
			setSongPosInSeconds(ref currentTempoEvent, ref currentTimeT, ref currentTimeS, newTimeS, updateScreen);
		}

		public void setSongPosInSeconds(ref int currentTempoEvent, ref double currentTimeT, ref double currentTimeS, double newTimeS, bool updateScreen)
		{
			double offsetS = 0, offsetT = 0;
			if (PlaybackOffsetS < 0)
			{
				offsetS = -PlaybackOffsetS;
				offsetT = -PlaybackOffsetT;
			}
			currentTimeT = secondsToTicks(newTimeS + offsetS) - offsetT;
			double newSongPos = currentTimeT / (double)SongLengthT;
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
			double offsetS = 0, offsetT = 0;
			if (PlaybackOffsetS < 0)
			{
				offsetS = -PlaybackOffsetS;
				offsetT = -PlaybackOffsetT;
			}

			double songPosT = _normSongPos * SongLengthT + offsetT;
			int nextTempoEvent = 0;
			int currentTempoEvent = 0;
			bool bLastTempoEvent = false;
			double currentTimeT = 0;
			double currentTimeS = 0;
			
			while (currentTimeT < songPosT)
			{
				double nextTimeStepT;
				double currentBps = notes.TempoEvents[currentTempoEvent].Tempo / 60; //beats per seconds
				if (bLastTempoEvent)
					nextTimeStepT = songPosT;
				else
				{
					if (currentTempoEvent + 1 < notes.TempoEvents.Count)
						nextTempoEvent++;
					else
						bLastTempoEvent = true;

					nextTimeStepT = notes.TempoEvents[nextTempoEvent].Time + TempoEventsOffset;
					if (nextTimeStepT > songPosT || nextTimeStepT < currentTimeT || nextTimeStepT == currentTimeT && bLastTempoEvent)
						nextTimeStepT = songPosT; //always causes loop to exit
					else
						currentTempoEvent = nextTempoEvent;
				}
				currentTimeS += (nextTimeStepT - currentTimeT) / (notes.TicksPerBeat * currentBps);
				currentTimeT = nextTimeStepT;
			}
			return currentTimeS - offsetS;
		}

		public void update(double deltaTimeS)
		{
			Camera.update(deltaTimeS);
			//Scroll song depending on user input or playback position.
			if (IsPlaying)
			{
				double timeS;
				if (!AudioHasStarted)
				{
					timeS = (SongPanel.TotalTimeElapsed - pbStartSysTime).TotalMilliseconds / 1000.0 + pbStartSongTimeS;
					if (timeS > AudioOffset + PlaybackOffsetS)
					{
						AudioHasStarted = true;
						Media.startPlaybackAtTime(0);
					}
				}
				else
				{
					if (!Media.playbackIsRunning())
					{
						IsPlaying = false;
						timeS = getSongPosInSeconds();
					}
					else
						timeS = Media.getPlaybackPos() + AudioOffset + PlaybackOffsetS;
				}
				setSongPosInSeconds(timeS, true);
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
			if (Media.getAudioLength() == 0)
				return;
			IsPlaying = !IsPlaying;
			//bAudioPlayback = !bAudioPlayback;
			if (!IsPlaying)
			{
				if (!Media.pausePlayback())
					MessageBox.Show("An error occured while pausing playback.");
			}
			else
			{
				double startTime = getSongPosInSeconds() - AudioOffset - PlaybackOffsetS;
				if (startTime >= 0)
				{
					if (!Media.startPlaybackAtTime(startTime))
						MessageBox.Show("An error occured while starting playback.");
					if (!Media.playbackIsRunning()) //Assuming this is because we tried to start playback after end of audio
					{
						IsPlaying = false;
						//bAudioPlayback = false;
					}
					else
						AudioHasStarted = true;
				}
				else
				{
					pbStartSysTime = SongPanel.TotalTimeElapsed;
					pbStartSongTimeS = getSongPosInSeconds();
					AudioHasStarted = false;
				}
			}
		}

		public void stopPlayback()
		{
			IsPlaying = false;
//			bAudioPlayback = false;
			if (!Media.stopPlayback())
				MessageBox.Show("An error occured while stopping playback.");
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
			return (float)((timeT / viewWidthT) * (Camera.ViewportSize.X));
		}
	
		public float getScreenPosY(float pitch)
		{
			return (pitch - MinPitch) * NoteHeight + NoteHeight / 2.0f + PitchMargin - Camera.ViewportSize.Y / 2;
		}
		public double getTimeT(double screenX)
		{ //Returns time in ticks
			return screenX / Camera.ViewportSize.X * viewWidthT; //Far right -> screenX = viewPortSize / 2
		}
		public double getSongPosP(double screenXOffset)
		{ //Returns song pos in pixels 
			return (getTimeT(screenXOffset) + SongPosT) * Camera.ViewportSize.X / viewWidthT;
		}
		public float SongLengthP =>
			(float)(SongLengthT * Camera.ViewportSize.X) / viewWidthT;

		public int SmallScrollStepT => (int)(ViewWidthT * SongPanel.SmallScrollStep);
		public int LargeScrollStepT => (int)(ViewWidthT * SongPanel.LargeScrollStep);

		public string DefaultFileName { get; set; }
		public const string DefaultFileExt = ".vms";

		public float getCurveScreenY(float x, Curve curve)
		{
			//float pitch = curve.EvaluateCurvature((float)getTimeT(x));
			//return pitch / 100;
			float pitch = curve.Evaluate((float)getTimeT(x));
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

		public void updateFromImportForm()
		{
			//ImportOptions.NotePath = ImportOptions.ImportForm.RawNoteFilePath;
			//DefaultFileName = Path.GetFileName(ImportOptions.ImportForm.NoteFilePath) + DefaultFileExt;
		}

		public Vector3 getSpatialNormPosOffset(TrackProps trackProps)
		{
			return normalizeVpVector(GlobalTrackProps.SpatialProps.PosOffset + trackProps.SpatialProps.PosOffset);
		}

		public float normalizeVpScalar(float value)
		{
			return value * Camera.ViewportSize.X / UserViewWidth;
		}
		public Vector3 normalizeVpVector(Vector3 value)
		{
			return value * Camera.ViewportSize.X / UserViewWidth;
		}
	}


	static class SongFormat
	{
		public const int writeVersion = 1;
		public static int readVersion;
	}
}
