using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VisualMusic
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	struct VideoFormat
	{
		public UInt32 width;
		public UInt32 height;
		public float fps;
		public VideoFormat(UInt32 width, UInt32 height, float fps = 60f)
		{
			this.width = width;
			this.height = height;
			this.fps = fps;
		}

	};

	static class Media
	{
		//General stuff
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool initMF();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool closeMF();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool openAudioFile(string file);
		[DllImport("media.dll", EntryPoint = "getAudioFilePath", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr getAudioFilePath_intptr();
		public static string getAudioFilePath()
		{
			return Marshal.PtrToStringAnsi(getAudioFilePath_intptr());
		}

		//Encoding
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool beginVideoEnc(string outputFile, string audioFile, VideoFormat vidFmt, double audioOffsetSeconds, bool spherical, AVCodecID videoCodec);
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool writeFrame(UInt32[] videoFrameBuffer);
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void endVideoEnc();

		//Playback
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool playbackIsRunning();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool closeAudioFile();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern double getPlaybackPos();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool startPlaybackAtTime(double timeS);
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool startPlayback();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool stopPlayback();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool pausePlayback();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern double getAudioLength();
	}
}
