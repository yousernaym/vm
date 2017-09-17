using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;

namespace Visual_Music
{
	[Serializable()]
	public class TrackView : ISerializable
	{
		public OcTree<Geo> ocTree;
		public TrackProps TrackProps { get; set; }
		int trackNumber;
		public int TrackNumber
		{
			get { return trackNumber; }
		}

		static int numTracks;
		static public int NumTracks
		{
			get { return numTracks; }
			set { numTracks = value; }
		}
		Curve curve = new Curve();
		public Curve Curve
		{
			get { return curve; }
			set { curve = value; }
		}
		Midi.Track midiTrack;
		public Midi.Track MidiTrack
		{
			get { return midiTrack; }
			set { midiTrack = value; }
		}

		public TrackView(int _trackNumber, int _numTracks, Midi.Song song)
		{
			trackNumber = _trackNumber;
			numTracks = _numTracks;
			midiTrack = song.Tracks[_trackNumber];
			TrackProps = new TrackProps(this);
			createCurve();
		}

		public TrackView(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "trackNumber")
					trackNumber = (int)entry.Value;
				if (entry.Name == "trackProps")
				{
					TrackProps = (TrackProps)entry.Value;
					TrackProps.TrackView = this;
				}
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("trackNumber", trackNumber);
			info.AddValue("trackProps", TrackProps);
		}

		public void cloneTrackProps(TrackView dest)
		{
			dest.TrackProps = TrackProps.clone();
			dest.TrackProps.TrackView = dest;
		}

		public void createCurve(/*Midi.Song song*/)
		{
			curve = new Curve();
			foreach (Midi.Note note in midiTrack.Notes)
			{
				float pos = (float)note.start;
				//float value = (note.pitch - song.MinPitch) / (float)song.NumPitches;
				float value = note.pitch;
				curve.Keys.Add(new CurveKey(pos, value));
			}
			setCurveTangents();
		}
		public void setCurveTangents()
		{
			for (int i = 0; i < curve.Keys.Count; i++)
			{
				int prevIndex = i == 0 ? i : i - 1;
				int nextIndex = i == curve.Keys.Count - 1 ? i : i + 1;
				CurveKey prev = curve.Keys[prevIndex], cur = curve.Keys[i], next = curve.Keys[nextIndex];

				float dt = next.Position - prev.Position;
				float dv = next.Value - prev.Value;
				if (Math.Abs(dv) < float.Epsilon)
				{
					cur.TangentIn = 0;
					cur.TangentOut = 0;
				}
				else
				{
					cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
					cur.TangentOut = dv * (next.Position - cur.Position) / dt;
				}
			}
		}

		public void drawTrack(SongDrawProps songDrawProps, TrackProps globalTrackProps, bool selectingRegion)
		{
			TrackProps texTrackProps = TrackProps.getTexture(false, null) != null ? TrackProps : globalTrackProps;
			if (selectingRegion)
				TrackProps.getNoteStyle(NoteStyleEnum.Bar).drawTrack(midiTrack, songDrawProps, TrackProps, globalTrackProps, selectingRegion, texTrackProps);
			else
				TrackProps.SelectedNoteStyle.drawTrack(midiTrack, songDrawProps, TrackProps, globalTrackProps, selectingRegion, texTrackProps);
		}

		public void createOcTree(SongDrawProps songDrawProps, TrackProps globalTrackProps)
		{
			if (MidiTrack.Notes.Count == 0)
				return;
			Midi.Note firstNote = midiTrack.Notes[0];
			Midi.Note lastNote = midiTrack.Notes[midiTrack.Notes.Count - 1];
			Vector2 minPos2d = songDrawProps.getScreenPosF(firstNote.start, songDrawProps.minPitch);
			Vector2 maxPos2d = songDrawProps.getScreenPosF(lastNote.start, 127); //Todo: use maxPitch

			//Todo: use posOffset.z for z-component
			Vector3 minPos = new Vector3(minPos2d.X, minPos2d.Y, -100);
			Vector3 maxPos = new Vector3(maxPos2d.X, maxPos2d.Y, 100);
			TrackProps texTrackProps = TrackProps.getTexture(false, null) != null ? TrackProps : globalTrackProps;

			if (ocTree != null)
				ocTree.dispose();
			NoteStyle noteStyle = TrackProps.SelectedNoteStyle;
			ocTree = new OcTree<Geo>(minPos, maxPos - minPos, new Vector3(1000, 1000, 1000), noteStyle.createGeoChunk, noteStyle.drawGeoChunk);
			ocTree.createGeo(midiTrack, songDrawProps, TrackProps, globalTrackProps, texTrackProps);
			//TrackProps.SelectedNoteStyle.createOcTree(minPos, maxPos - minPos, midiTrack, songDrawProps, globalTrackProps, TrackProps, texTrackProps);
		}
	}		
}
