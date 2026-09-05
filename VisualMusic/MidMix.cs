using System.Runtime.InteropServices;

namespace VisualMusic
{
    static class MidMix
    {
        // Fluidsynth state in MidMix.dll is process-global (one synth/player/renderer). Concurrent
        // mixdown from overlapping imports (nested ShowDialog pumping a second browser download)
        // corrupts that state and can throw ExecutionEngineException.
        static readonly object Sync = new object();

        // FluidSynth (via glib g_fopen) treats these paths as UTF-8 on Windows, so marshal them as
        // UTF-8 rather than the P/Invoke default (ANSI system codepage), which silently corrupts any
        // non-ASCII path (e.g. a soundfont or temp mixdown under a non-Latin user profile).
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "init")]
        static extern void NativeInit([MarshalAs(UnmanagedType.LPUTF8Str)] string soundfontPath);
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "mixdown")]
        static extern void NativeMixdown([MarshalAs(UnmanagedType.LPUTF8Str)] string midiPath,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string mixdownPath);
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sfLoaded")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool NativeSfLoaded();
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "close")]
        static extern void NativeClose();

        public static void Init(string soundfontPath)
        {
            lock (Sync) NativeInit(soundfontPath);
        }

        public static void Mixdown(string midiPath, string mixdownPath)
        {
            lock (Sync) NativeMixdown(midiPath, mixdownPath);
        }

        public static bool SfLoaded()
        {
            lock (Sync) return NativeSfLoaded();
        }

        public static void Close()
        {
            lock (Sync) NativeClose();
        }
    }
}
