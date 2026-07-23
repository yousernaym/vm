using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;

namespace VisualMusic
{
    [Serializable()]
    public class TrackView : ISerializable
    {

        Geo _geo;
        public Geo Geo
        {
            set
            {
                _geo?.Dispose();
                _geo = value?.AddRef();
            }
            get => _geo;
        }
        public TrackProps TrackProps { get; set; }
        int _trackNumber;
        public int TrackNumber
        {
            get => _trackNumber;
            set => _trackNumber = value;
        }

        static int s_numTracks;
        static public int NumTracks
        {
            get { return s_numTracks; }
            set { s_numTracks = value; }
        }
        Curve _curve = new Curve();
        public Curve Curve
        {
            get { return _curve; }
            set { _curve = value; }
        }
        Midi.Track _midiTrack;
        public Midi.Track MidiTrack
        {
            get { return _midiTrack; }
            set { _midiTrack = value; }
        }

        public TrackView(int trackNumber, int numTracks, Midi.Song song)
        {
            _trackNumber = trackNumber;
            s_numTracks = numTracks;
            _midiTrack = song.Tracks[trackNumber];
            TrackProps = new TrackProps(this);
            CreateCurve();
        }

        public TrackView(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "trackNumber")
                    _trackNumber = (int)entry.Value;
                if (entry.Name == "trackProps")
                {
                    TrackProps = (TrackProps)entry.Value;
                    TrackProps.TrackView = this;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("trackNumber", _trackNumber);
            info.AddValue("trackProps", TrackProps);
        }

        public void CreateCurve(/*Midi.Song song*/)
        {
            _curve = new Curve();
            foreach (Midi.Note note in _midiTrack.Notes)
            {
                float pos = (float)note.start;
                //float value = (note.pitch - song.MinPitch) / (float)song.NumPitches;
                float value = note.pitch;
                _curve.Keys.Add(new CurveKey(pos, value));
            }
            _curve.ComputeTangents(CurveTangent.Smooth);
            _curve.ComputeNextVectors();
            //setCurveTangents();
        }

        public void SetCurveTangents()
        {
            for (int i = 0; i < _curve.Keys.Count; i++)
            {
                int prevIndex = i == 0 ? i : i - 1;
                int nextIndex = i == _curve.Keys.Count - 1 ? i : i + 1;
                CurveKey prev = _curve.Keys[prevIndex], cur = _curve.Keys[i], next = _curve.Keys[nextIndex];

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

        public void DrawTrack(TrackProps globalTrackProps, bool defaultStyle)
        {
            if (_midiTrack?.Notes?.Count == 0)
                return;
            MaterialProps texMaterial = TrackProps.MaterialProps.HasLocalTextureForRender(false) ? TrackProps.MaterialProps : globalTrackProps.MaterialProps;
            if (defaultStyle)
                TrackProps.StyleProps.GetStyle(NoteStyleType.Bar).DrawTrack(_midiTrack, TrackProps, texMaterial);
            else
                TrackProps.ActiveNoteStyle.DrawTrack(_midiTrack, TrackProps, texMaterial);
        }

        public void CreateGeo(Project project, TrackProps globalTrackProps)
        {
            if (MidiTrack.Notes.Count == 0)
                return;
            // Style FX / vertex buffers need MonoGame Content. CreateTrackViews can run headless
            // (unit tests) or before SetContent (e.g. import while Song host is Collapsed); soft-skip
            // here — NoteStyle.SetContent retries via Project.LoadStyleFxAndCreateGeos.
            if (!NoteStyle.HasContent)
                return;
            Midi.Note firstNote = _midiTrack.Notes[0];
            Midi.Note lastNote = _midiTrack.Notes[_midiTrack.Notes.Count - 1];
            Vector2 minPos2d = project.GetScreenPos(firstNote.start, project.Props.MinPitch);
            Vector2 maxPos2d = project.GetScreenPos(lastNote.start, project.Props.MaxPitch);

            //Todo: use posOffset.z for z-component
            Vector3 minPos = new Vector3(minPos2d.X, minPos2d.Y, 0);
            Vector3 maxPos = new Vector3(maxPos2d.X, maxPos2d.Y, 0);
            MaterialProps texMaterial = TrackProps.MaterialProps.HasLocalTextureForRender(false) ? TrackProps.MaterialProps : globalTrackProps.MaterialProps;

            NoteStyle noteStyle = TrackProps.ActiveNoteStyle;
            noteStyle.CreateGeoChunk(out Geo newGeo, new BoundingBox(minPos, maxPos), _midiTrack, TrackProps, texMaterial);
            newGeo.RefWidthQn = project.GlobalViewWidthQn;   // width this geometry was baked at
            Geo = newGeo;
        }

        public TrackView Clone()
        {
            var dest = Cloning.Clone(this);
            //dest.TrackProps = TrackProps.clone();
            dest.TrackProps.TrackView = dest;
            dest._midiTrack = _midiTrack;
            dest._curve = _curve;
            return dest;
        }
    }
}
