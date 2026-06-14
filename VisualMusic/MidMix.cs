using System.Runtime.InteropServices;

namespace VisualMusic
{
    static class MidMix
    {
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "init")]
        public static extern void Init();
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "mixdown")]
        public static extern void Mixdown(string midiPath, string mixdownPath);
        [DllImport("MidMix.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "close")]
        public static extern void Close();

    }
}
