using System.Collections;
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

namespace Visual_Music
{
	using GdiPoint = System.Drawing.Point;
	public enum SourceSongType { Midi, Mod, Sid };

	[Serializable()]
	public class Project : ISerializable
	{
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
		string noteFilePath = "";
		public string NoteFilePath
		{
			get { return noteFilePath; }
			set { noteFilePath = value; }
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

		MixdownType mixdownType;
		public MixdownType MixdownType { get => mixdownType; set => mixdownType = value; }
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
					viewWidthT = (int)(viewWidthQn * notes.TicksPerBeat);
			}
		}
		float vertViewWidthQn;
		public float VertWidthScale => vertViewWidthQn / viewWidthQn;

		int viewWidthT; ////Number of ticks that fits on screen
		public int ViewWidthT { get => viewWidthT; }

		
		public double AudioOffset { get; set; }
		public int MinPitch { get; set; }
		public int MaxPitch { get; set; }
		int NumPitches { get { return MaxPitch - MinPitch + 1; } }
		SourceSongType sourceSongType;
		public SourceSongType SourceSongType { get => sourceSongType; set => sourceSongType = value; }
		double desiredSongLengthS = 0; //Desired song length in seconds when importing note file. 0 = not specified. Currently not used.
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

		public int SongLengthT { get { return notes != null ? notes.SongLengthT : 0; } } //Song length in ticks
		public int SongPosT => (int)(normSongPos * SongLengthT); //Current song position in ticks
		public float SongPosB => (float)SongPosT / Notes.TicksPerBeat; //Current song position in beats
		double normSongPos; //Song position normalized to [0,1]
		public double NormSongPos
		{
			get => normSongPos;
			set
			{
				if (normSongPos != value)
				{
					normSongPos = value;
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
			set
			{
				isPlaying = value;
				if (!value)
					AudioHasStarted = false;
			}
		}
		public bool AudioHasStarted { get; set; }
		
		public Project(SongPanel spanel)
		{
			SongPanel = spanel;
		}

		public void loadContent()
		{
			if (!string.IsNullOrWhiteSpace(noteFilePath))
			{
				importSong(noteFilePath, audioFilePath, false, insTrack, mixdownType, desiredSongLengthS);
				if (trackViews != null)
				{
					for (int i = 0; i < trackViews.Count; i++)
						trackViews[i].TrackProps.loadContent(songPanel);
				}
			}
		}

		public Project(SerializationInfo info, StreamingContext ctxt) : base()
		{
			SongFormat.readVersion = (int)info.GetValue("version", typeof(int));
			noteFilePath = (string)info.GetValue("noteFilePath", typeof(string));
			audioFilePath = (string)info.GetValue("audioFilePath", typeof(string));
			insTrack = (bool)info.GetValue("insTrack", typeof(bool));
			mixdownType = (MixdownType)info.GetValue("mixdownType", typeof(MixdownType));

			trackViews = (List<TrackView>)info.GetValue("trackProps", typeof(List<TrackView>));
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
			info.AddValue("trackProps", trackViews);
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
			SongPanel.Invalidate();
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
				MessageBox.Show("Couldn't load note file " + Path.GetFileName(NoteFilePath), "Note file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			createTrackViews(notes.Tracks.Count, eraseCurrent);

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
				MessageBox.Show("Couldn't load audio file " + file, "Audio file error");
				return false;
			}
			mixdownType = _mixdownType;
			if (notes != null)
				notes.SongLengthT = (int)secondsToTicks(Media.getAudioLength());
			return true;
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
					trackViews[i].TrackProps.loadNoteStyleFx();
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
			if (trackViews == null)
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

			for (int t = notes.Tracks.Count - 1; t >= 0; t--)
				trackViews[t].drawTrack(GlobalTrackProps, SongPanel.ForceDefaultNoteStyle);
		}

		public int screenPosToSongPos(float normScreenPos)
		{
			return (int)(NormSongPos * Notes.SongLengthT + (double)normScreenPos * ViewWidthT * 0.5f);
		}
		Point getVisibleSongPortionT(double normPos)
		{
			int posT = (int)(normPos * Notes.SongLengthT);
			return new Point(posT - ViewWidthT, posT + ViewWidthT);
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
			if (listIndices.Count == 1)
				return TrackViews[listIndices[0]].TrackProps;
			TrackProps mergedPRops = TrackViews[listIndices[0]].TrackProps.clone();
			for (int i = 1; i < listIndices.Count; i++)
				mergedPRops = (TrackProps)mergeObjects(mergedPRops, TrackViews[listIndices[i]].TrackProps);
			return mergedPRops;
		}

		public object mergeObjects(object first, object second)
		{
			if (first == null || second == null)
				return null;
			PropertyInfo[] props = first.GetType().GetProperties();
			bool hasSuitableProp = false;
			if (props.Length > 0)
			{
				foreach (PropertyInfo propertyInfo in props)
				{
					if (!propertyInfo.CanRead || !propertyInfo.CanWrite || propertyInfo.GetMethod.IsStatic)
						continue;
					hasSuitableProp = true;

					object firstValue = propertyInfo.GetValue(first, null);
					object secondValue = propertyInfo.GetValue(second, null);
					if (!(firstValue is string) && firstValue is IEnumerable)
					{
						continue;
						//if (!(firstValue is IList && firstValue.GetType().IsGenericType && firstValue.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))))
						//	continue;

						//Activator.CreateInstance()
						//List<object> returnValues = new List<object>();
						//for (int i = 0; i < ((List<object>)firstValue).Count; i++)
						//{
						//	object element = ((List<object>)firstValue)[i];
						//	if (element != null)
						//		returnValues.Add(element);


						//var firstEnum = ((IEnumerable)firstValue).GetEnumerator();
						//var secondEnum = ((IEnumerable)secondValue).GetEnumerator();
						//while (firstEnum.MoveNext() && secondEnum.MoveNext())
						//{
						//object value = mergeObjects(firstEnum.Current, secondEnum.Current);
						//if (value != null)
						//returnValues.Add(value);
						//}

						//propertyInfo.SetValue(first, returnValues);
					}
					else
						propertyInfo.SetValue(first, mergeObjects(firstValue, secondValue));
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

		public void setSongPosInSeconds(double newTimeS, bool updateScreen)
		{
			int currentTempoEvent = 0;
			double currentTimeT = 0;
			double currentTimeS = 0;
			setSongPosInSeconds(ref currentTempoEvent, ref currentTimeT, ref currentTimeS, newTimeS, updateScreen);
		}

		public void setSongPosInSeconds(ref int currentTempoEvent, ref double currentTimeT, ref double currentTimeS, double newTimeS, bool updateScreen)
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

		public void update(double deltaTimeS)
		{
			//Scroll song depending on user input or playback position.
			if (IsPlaying)
			{
				double timeS;
				if (!AudioHasStarted)
				{
					timeS = (SongPanel.TotalTimeElapsed - pbStartSysTime).TotalMilliseconds / 1000.0 + pbStartSongTimeS;
					if (timeS > AudioOffset)
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
						timeS = Media.getPlaybackPos() + AudioOffset;
				}
				setSongPosInSeconds(timeS, true);
				if (NormSongPos > 1)
					togglePlayback();
			}
			else
				SongPanel.scrollSong();

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
				double startTime = getSongPosInSeconds() - AudioOffset;
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
		public float getScreenPosX(int timeT)
		{
			return ((float)timeT / viewWidthT) * Camera.ViewportSize.X;
		}
	
		public float getScreenPosY(float pitch)
		{
			return (pitch - MinPitch) * NoteHeight + NoteHeight / 2.0f + PitchMargin - Camera.ViewportSize.Y / 2;
		}
		public float getTimeT(float screenX)
		{ //Returns time in ticks
			return (float)screenX / Camera.ViewportSize.X * (float)viewWidthT; //Far right -> screenX = viewPortSize / 2
		}
		public float getSongPosP(float screenXOffset)
		{ //Returns song pos in pixels 
			return (getTimeT(screenXOffset) + SongPosT) * Camera.ViewportSize.X / viewWidthT;
		}
		public float SongLengthP =>
			(float)(notes.SongLengthT * Camera.ViewportSize.X) / viewWidthT;
		
		public float getCurveScreenY(float x, Curve curve)
		{
			float pitch = curve.Evaluate((float)getTimeT(x));
			return getScreenPosY(pitch);
		}
	}


	static class SongFormat
	{
		public const int writeVersion = 1;
		public static int readVersion;
	}
}
