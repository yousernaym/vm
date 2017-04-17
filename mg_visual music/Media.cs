using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Visual_Music
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	struct VideoFormat
	{
		public UInt32 width;
		public UInt32 height;
		public UInt32 fps;
		public UInt32 bitRate;
		public UInt32 audioSampleRate;
	};

	static class Media
	{
		//Global stuff
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool initMF();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool closeMF();
		
		//Encoding
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool beginVideoEnc(string outPutFile, string audioFile, VideoFormat vidFmt, bool _bVideo);
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool writeFrame(UInt32[] videoFrameBuffer, UInt64 rtStart, ref UInt64 rtDuration, double audioOffset, bool bFlush);
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void endVideoEnc();

		//Playback
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool closePlaybackSession();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool playbackIsRunning();
		[DllImport("media.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool openFileForPlayback(string file);
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
