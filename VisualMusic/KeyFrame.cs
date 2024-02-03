using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VisualMusic
{
    [Serializable]
    public class KeyFrames : ISerializable, IEnumerable<KeyValuePair<int, KeyFrame>>
    {
        SortedList<int, KeyFrame> frameList;
        SongPanel SongPanel => Form1.SongPanel;

        public KeyFrames()
        {
            frameList = new SortedList<int, KeyFrame>();
            var keyFrame0 = new KeyFrame();
            keyFrame0.Desc = "Key frame 0";
            frameList.Add(0, keyFrame0);
        }

        public KeyFrames(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "frameList")
                    frameList = (SortedList<int, KeyFrame>)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("frameList", frameList);
        }

        public int insert(int songPosT)
        {
            if (!frameList.ContainsKey(songPosT))
            {
                frameList.Add(songPosT, createInterpolatedFrame(songPosT));
                return frameList.IndexOfKey(songPosT);
            }
            else
                return -1;
        }

        public KeyFrame createInterpolatedFrame(int songPosT)
        {
            if (frameList.Count == 0)
                return new KeyFrame();
            if (frameList.Count == 1)
                return frameList.Values[0].clone();
            if (frameList.ContainsKey(songPosT))
                return frameList[songPosT].clone();
            int index1 = frameList.Count - 1;
            for (int i = 0; i < frameList.Count; i++)
            {
                if (songPosT < frameList.Keys[i])
                {
                    index1 = i - 1;
                    break;
                }
            }
            if (index1 < 0)
                return frameList.Values[0].clone();
            if (index1 == frameList.Count - 1)
                return frameList.Values[index1].clone();

            KeyFrame frame1 = frameList.Values[index1];
            KeyFrame frame2 = frameList.Values[index1 + 1];

            KeyFrame outFrame = frame1.clone();
            float interpolant = (float)(songPosT - frameList.Keys[index1]) / (frameList.Keys[index1 + 1] - frameList.Keys[index1]);
            interpolant = (float)(1 - Math.Cos(interpolant * Math.PI)) / 2f;
            outFrame.ProjProps.ViewWidthQn = (float)Math.Pow(2, interpolate((float)Math.Log(frame1.ProjProps.ViewWidthQn, 2), (float)Math.Log(frame2.ProjProps.ViewWidthQn, 2), interpolant));
            outFrame.ProjProps.Camera.Pos = interpolate(frame1.ProjProps.Camera.Pos, frame2.ProjProps.Camera.Pos, interpolant);
            outFrame.ProjProps.Camera.Orientation = interpolate(frame1.ProjProps.Camera.Orientation, frame2.ProjProps.Camera.Orientation, interpolant);

            return outFrame;
        }

        //Returns keyframe that matches current song position if there's a match, otherwise null
        public KeyFrame this[int key]
        {
            get
            {
                if (frameList.Count == 0)
                    return null;
                else if (frameList.ContainsKey(key))
                    return frameList[key];
                else if (frameList.Count == 1 || key < frameList.Keys[0])
                    return frameList.Values[0];
                else if (frameList.Keys[frameList.Count - 1] < key)
                    return frameList.Values[frameList.Count - 1];
                else
                    return null;
            }
        }

        private float interpolate(float value1, float value2, float interpolant)
        {
            return value1 * (1 - interpolant) + value2 * interpolant;
        }

        private Vector3 interpolate(Vector3 value1, Vector3 value2, float interpolant)
        {
            return value1 * (1 - interpolant) + value2 * interpolant;
        }

        private Quaternion interpolate(Quaternion value1, Quaternion value2, float interpolant)
        {
            return Quaternion.Slerp(value1, value2, interpolant);
        }

        public void removeIndex(int index)
        {
            frameList.RemoveAt(index);
        }

        public int keyAtIndex(int index)
        {
            if (index < frameList.Count)
                return frameList.Keys[index];
            else
                return -1;
        }

        public IEnumerator<KeyValuePair<int, KeyFrame>> GetEnumerator()
        {
            return frameList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count => frameList.Count;
        public IList<int> Keys => frameList.Keys;
        public IList<KeyFrame> Values => frameList.Values;

        internal int changeTimeOfFrame(int frameNumber, int time)
        {
            if (frameList.ContainsKey(time))
                return -1;

            var frame = frameList.Values[frameNumber];
            frameList.RemoveAt(frameNumber);
            frameList.Add(time, frame);
            return frameList.IndexOfKey(time);
        }
    }

    //KeyFrame------------------
    [Serializable]
    public class KeyFrame : ISerializable
    {
        public string Desc;
        public bool Selected;
        public List<TrackProps> TrackProps { get; set; } = new List<TrackProps>();
        public ProjProps ProjProps { get; set; } = new ProjProps();
        //public Camera Camera;
        //public float ViewWidthQn;
        //public Dictionary<string, string> Properties { get; set; }

        public KeyFrame()
        {
            ProjProps.ViewWidthQn = ProjProps.DefaultViewWidthQn;
        }

        public KeyFrame(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "desc")
                    Desc = (string)entry.Value;
                else if (entry.Name == "projProps")
                    ProjProps = (ProjProps)entry.Value;
                else if (entry.Name == "trackProps")
                    TrackProps = (List<TrackProps>)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //base.GetObjectData(info, context);
            info.AddValue("desc", Desc);
            info.AddValue("projProps", ProjProps);
            info.AddValue("trackProps", TrackProps);
        }
    }
}