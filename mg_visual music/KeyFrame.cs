using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Visual_Music
{
	[Serializable]
	public class KeyFrames : ISerializable
	{
		SortedList<int, KeyFrame> frameList;

		public KeyFrames()
		{
			frameList = new SortedList<int, KeyFrame>();
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

		public void insert(int songPosT)
		{
			frameList.Add(songPosT, createInterpolatedFrame(songPosT));
		}

		public KeyFrame createInterpolatedFrame(int songPosT)
		{
			if (frameList.Count == 1)
				return frameList.Values[0];
			int index1 = frameList.Count;
			for (int i = 0; i < frameList.Count; i++)
			{
				if (songPosT < frameList.Keys[i])
				{
					index1 = i - 1;
					break;
				}
			}
			if (index1 < 0)
				return frameList.Values[0];
			if (index1 == frameList.Count - 1)
				return frameList.Values[index1];

			KeyFrame frame1 = frameList.Values[index1];
			KeyFrame frame2 = frameList.Values[index1 + 1];

			KeyFrame outFrame = new KeyFrame();
			float interpolant = (songPosT - frameList.Keys[index1]) / (frameList.Keys[index1 + 1] - frameList.Keys[index1]);
			outFrame.ViewWidthQn = Math.Pow(interpolate(Math.Log(frame1.ViewWidthQn, 2), Math.Log(frame2.ViewWidthQn, 2), interpolant), 2);
			outFrame.Camera = frame1.Camera; //TODO: interpolate camera
			return outFrame;
		}
	
		private float interpolate(float value1, float value2, float interpolant)
		{
			return value1 * interpolant + value2 + (1 - interpolant);
		}
	}

	//KeyFrame------------------
	[Serializable]
	public class KeyFrame : ISerializable
	{
		public const float DefaultViewWidthQn = 16; //Number of quarter notes that fits on screen with default camera
		public Camera Camera;
		public float ViewWidthQn;

		public KeyFrame()
		{
			ViewWidthQn = DefaultViewWidthQn;
			Camera = new Camera();
		}

		public KeyFrame(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "qn_viewWidth")
					ViewWidthQn = (float)entry.Value;
				else if (entry.Name == "camera")
					Camera = (Camera)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("qn_viewWidth", ViewWidthQn);
			info.AddValue("camera", Camera);
		}
	}
}