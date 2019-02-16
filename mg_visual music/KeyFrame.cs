using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Visual_Music
{
	[Serializable]
	public class KeyFrames : ISerializable, IEnumerable<KeyValuePair<int, KeyFrame>>
	{
		SortedList<int, KeyFrame> frameList;
		SongPanel songPanel;

		public KeyFrames(SongPanel spanel)
		{
			frameList = new SortedList<int, KeyFrame>();
			songPanel = spanel;
			frameList.Add(0, new KeyFrame(songPanel));
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
				return new KeyFrame(songPanel);
			if (frameList.Count == 1)
				return frameList.Values[0];
			if (frameList.ContainsKey(songPosT))
				return frameList[songPosT];
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
			outFrame.ViewWidthQn = (float)Math.Pow(2, interpolate((float)Math.Log(frame1.ViewWidthQn, 2), (float)Math.Log(frame2.ViewWidthQn, 2), interpolant));
			outFrame.Camera.Pos = interpolate(frame1.Camera.Pos, frame2.Camera.Pos, interpolant);
			outFrame.Camera.Orientation = interpolate(frame1.Camera.Orientation, frame2.Camera.Orientation, interpolant);
			outFrame.Camera.SongPanel = frame1.Camera.SongPanel;
			outFrame.Camera.SpatialChanged = frame1.Camera.SpatialChanged;

			return outFrame;
		}

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
			return value1 * (1 - interpolant) + value2 * interpolant;
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
	public class KeyFrame : Cloneable<KeyFrame>, ISerializable
	{
		public const float DefaultViewWidthQn = 16; //Number of quarter notes that fits on screen with default camera
		public string Desc;
		public Camera Camera;
		public float ViewWidthQn;

		public KeyFrame(SongPanel songPanel)
		{
			ViewWidthQn = DefaultViewWidthQn;
			Camera = new Camera();
			Camera.SongPanel = songPanel;
			Camera.SpatialChanged = songPanel.Project.Camera.SpatialChanged;
		}

		public KeyFrame(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "desc")
					Desc = (string)entry.Value;
				else if (entry.Name == "qn_viewWidth")
					ViewWidthQn = (float)entry.Value;
				else if (entry.Name == "camera")
					Camera = (Camera)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("desc", Desc);
			info.AddValue("qn_viewWidth", ViewWidthQn);
			info.AddValue("camera", Camera);
		}

		new public KeyFrame clone()
		{
			var newFrame = base.clone();
			newFrame.Camera.SongPanel = Camera.SongPanel;
			newFrame.Camera.SpatialChanged = Camera.SpatialChanged;
			return newFrame;
		}
	}
}