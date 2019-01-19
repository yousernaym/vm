using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Visual_Music
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
