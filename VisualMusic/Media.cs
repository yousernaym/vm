using System;
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
        // Path args are UTF-8: FFmpeg expects UTF-8 filenames on Windows, and openAudioFile
        // converts via MultiByteToWideChar(CP_UTF8). The P/Invoke default (ANSI code page)
        // silently corrupts non-ASCII paths (e.g. under a non-Latin user profile).
        //General stuff
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "initMF")]
        public static extern bool InitMF();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "closeMF")]
        public static extern bool CloseMF();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "openAudioFile")]
        public static extern bool OpenAudioFile([MarshalAs(UnmanagedType.LPUTF8Str)] string file);
        [DllImport("media.dll", EntryPoint = "getAudioFilePath", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetAudioFilePath_intptr();
        public static string GetAudioFilePath()
        {
            return Marshal.PtrToStringUTF8(GetAudioFilePath_intptr());
        }

        //Encoding
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "beginVideoEnc")]
        public static extern bool BeginVideoEnc(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string outputFile,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string audioFile,
            VideoFormat vidFmt, double audioOffsetSeconds, bool spherical, bool sphericalStereo,
            AVCodecID videoCodec, [MarshalAs(UnmanagedType.LPUTF8Str)] string crf);
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "writeFrame")]
        public static extern bool WriteFrame(UInt32[] videoFrameBuffer);
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "endVideoEnc")]
        public static extern void EndVideoEnc();

        //Playback
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "playbackIsRunning")]
        public static extern bool PlaybackIsRunning();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "closeAudioFile")]
        public static extern bool CloseAudioFile();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getPlaybackPos")]
        public static extern double GetPlaybackPos();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "startPlaybackAtTime")]
        public static extern bool StartPlaybackAtTime(double timeS);
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "startPlayback")]
        public static extern bool StartPlayback();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "stopPlayback")]
        public static extern bool StopPlayback();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "pausePlayback")]
        public static extern bool PausePlayback();
        [DllImport("media.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getAudioLength")]
        public static extern double GetAudioLength();
    }

    /// <summary>
    /// Playback-only Media seam used by <see cref="Project"/>. Media Foundation can report a
    /// transitional clock state immediately after a successful seek/start, so Project owns the
    /// higher-level playback state while this interface exposes the native facts it needs.
    /// </summary>
    internal interface IAudioPlayback
    {
        double GetLength();
        bool IsRunning();
        double GetPosition();
        bool StartAtTime(double timeS);
        bool Pause();
        bool Stop();
    }

    internal sealed class MediaAudioPlayback : IAudioPlayback
    {
        public static MediaAudioPlayback Instance { get; } = new MediaAudioPlayback();

        MediaAudioPlayback() { }

        public double GetLength() => Media.GetAudioLength();
        public bool IsRunning() => Media.PlaybackIsRunning();
        public double GetPosition() => Media.GetPlaybackPos();
        public bool StartAtTime(double timeS) => Media.StartPlaybackAtTime(timeS);
        public bool Pause() => Media.PausePlayback();
        public bool Stop() => Media.StopPlayback();
    }
}
