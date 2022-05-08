using System.Runtime.InteropServices;

namespace VisualMusic
{
    static class MidMix
    {
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void init();
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool sfLoaded();
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mixdown(string midiPath, string mixdownPath);
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void close();

    }
}
