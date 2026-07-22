using System.Runtime.InteropServices;

namespace VisualMusic
{
    static class MidMix
    {
        // FluidSynth (via glib g_fopen) treats these paths as UTF-8 on Windows, so marshal them as
        // UTF-8 rather than the P/Invoke default (ANSI system codepage), which silently corrupts any
        // non-ASCII path (e.g. a soundfont or temp mixdown under a non-Latin user profile).
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "init")]
        public static extern void Init([MarshalAs(UnmanagedType.LPUTF8Str)] string soundfontPath);
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "mixdown")]
        public static extern void Mixdown([MarshalAs(UnmanagedType.LPUTF8Str)] string midiPath,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string mixdownPath);
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sfLoaded")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SfLoaded();
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "close")]
        public static extern void Close();

    }
}
