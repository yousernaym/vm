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
        SortedList<int, KeyFrame> _frameList;

        public KeyFrames()
        {
            _frameList = new SortedList<int, KeyFrame>();
            var keyFrame0 = new KeyFrame();
            keyFrame0.Desc = "Key frame 0";
            _frameList.Add(0, keyFrame0);
        }

        public KeyFrames(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "frameList")
                    _frameList = (SortedList<int, KeyFrame>)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("frameList", _frameList);
        }

        public int Insert(int songPosT)
        {
            if (!_frameList.ContainsKey(songPosT))
            {
                _frameList.Add(songPosT, CreateInterpolatedFrame(songPosT));
                return _frameList.IndexOfKey(songPosT);
            }
            else
                return -1;
        }

        public KeyFrame CreateInterpolatedFrame(int songPosT)
        {
            if (_frameList.Count == 0)
                return new KeyFrame();
            if (_frameList.Count == 1)
                return _frameList.Values[0].Clone();
            if (_frameList.ContainsKey(songPosT))
                return _frameList[songPosT].Clone();
            int index1 = _frameList.Count - 1;
            for (int i = 0; i < _frameList.Count; i++)
            {
                if (songPosT < _frameList.Keys[i])
                {
                    index1 = i - 1;
                    break;
                }
            }
            if (index1 < 0)
                return _frameList.Values[0].Clone();
            if (index1 == _frameList.Count - 1)
                return _frameList.Values[index1].Clone();

            KeyFrame frame1 = _frameList.Values[index1];
            KeyFrame frame2 = _frameList.Values[index1 + 1];

            KeyFrame outFrame = frame1.Clone();
            float interpolant = (float)(songPosT - _frameList.Keys[index1]) / (_frameList.Keys[index1 + 1] - _frameList.Keys[index1]);
            InterpolateProjProps(outFrame.ProjProps, frame1.ProjProps, frame2.ProjProps, interpolant);

            return outFrame;
        }

        private void InterpolateProjProps(ProjProps outProjProps, ProjProps projProps1, ProjProps projProps2, float interpolant)
        {
            interpolant = interpolant * interpolant * (3f - 2f * interpolant);
            outProjProps.ViewWidthQn = (float)Math.Pow(2, Interpolate((float)Math.Log(projProps1.ViewWidthQn, 2), (float)Math.Log(projProps2.ViewWidthQn, 2), interpolant));
            outProjProps.Camera.Pos = Interpolate(projProps1.Camera.Pos, projProps2.Camera.Pos, interpolant);
            outProjProps.Camera.Orientation = Interpolate(projProps1.Camera.Orientation, projProps2.Camera.Orientation, interpolant);
            outProjProps.BackgroundImageOpacity = Interpolate(projProps1.BackgroundImageOpacity, projProps2.BackgroundImageOpacity, interpolant);
            outProjProps.BackgroundImageSaturation = Interpolate(projProps1.BackgroundImageSaturation, projProps2.BackgroundImageSaturation, interpolant);
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
                if (_frameList.Count == 0)
                    return null;
                else if (_frameList.ContainsKey(key))
                    return _frameList[key];
                else if (_frameList.Count == 1 || key < _frameList.Keys[0])
                    return _frameList.Values[0];
                else if (_frameList.Keys[_frameList.Count - 1] < key)
                    return _frameList.Values[_frameList.Count - 1];
                else
                    return null;
            }
        }

        private float Interpolate(float value1, float value2, float interpolant)
        {
            return value1 * (1 - interpolant) + value2 * interpolant;
        }

        private Vector3 Interpolate(Vector3 value1, Vector3 value2, float interpolant)
        {
            return value1 * (1 - interpolant) + value2 * interpolant;
        }

        private Quaternion Interpolate(Quaternion value1, Quaternion value2, float interpolant)
        {
            return Quaternion.Slerp(value1, value2, interpolant);
        }

        public void RemoveIndex(int index)
        {
            _frameList.RemoveAt(index);
        }

        public int KeyAtIndex(int index)
        {
            if (index < _frameList.Count)
                return _frameList.Keys[index];
            else
                return -1;
        }

        public IEnumerator<KeyValuePair<int, KeyFrame>> GetEnumerator()
        {
            return _frameList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count => _frameList.Count;
        public IList<int> Keys => _frameList.Keys;
        public IList<KeyFrame> Values => _frameList.Values;

        internal int ChangeTimeOfFrame(int frameNumber, int time)
        {
            if (_frameList.ContainsKey(time))
                return -1;

            var frame = _frameList.Values[frameNumber];
            _frameList.RemoveAt(frameNumber);
            _frameList.Add(time, frame);
            return _frameList.IndexOfKey(time);
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
