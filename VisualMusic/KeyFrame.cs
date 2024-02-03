using Microsoft.Xna.Framework;
using SharpDX.Direct3D11;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Media.Animation;

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
            foreach (var prop in outFrame.ProjProps.GetType().GetProperties())
                interpolateProperty(prop, frame1, frame2, outFrame, interpolant);
            outFrame.ProjProps.Camera.Pos = interpolate(frame1.ProjProps.Camera.Pos, frame2.ProjProps.Camera.Pos, interpolant);
            outFrame.ProjProps.Camera.Orientation = interpolate(frame1.ProjProps.Camera.Orientation, frame2.ProjProps.Camera.Orientation, interpolant);

            return outFrame;
        }

        private void interpolateProperty(PropertyInfo prop, object object1, object object2, object outObject, float interpolant)
        {
            var value1 = prop.GetValue(object1);
            var value2 = prop.GetValue(object2);

            if (prop.GetCustomAttribute<KeyframeLogInterpolation>() != null)
                outObject = (float)Math.Pow(2, interpolate((float)Math.Log((float)object1, 2), (float)Math.Log((float)object2, 2), interpolant));
            else if (HasProperties(prop.PropertyType))
                interpolateProperty(prop, value1, value2, prop.get);
            else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(int))
            {
                var value = interpolate((float)value1, (float)value2, interpolant);
                if (prop.PropertyType == typeof(float))
                    prop.SetValue(outValue, value);
                else
                    prop.SetValue(outValue, (int)value);
            }
        }

        static bool HasProperties(Type type)
        {
            if (type == typeof(string))
                return false;

            if (type.IsClass || (type.IsInterface && !type.IsPrimitive))
                return type.GetProperties().Length > 0;
            return false;
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
                //The following are for legacy file format
                else if (entry.Name == "qn_viewWidth")
                {
                    if (ProjProps == null)
                        ProjProps = new ProjProps();
                    ProjProps.ViewWidthQn = (float)entry.Value;
                }
                else if (entry.Name == "camera")
                {
                    if (ProjProps == null)
                        ProjProps = new ProjProps();
                    ProjProps.Camera = (Camera)entry.Value;
                }
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

    public class KeyframeLogInterpolation : Attribute
    {
    }
}