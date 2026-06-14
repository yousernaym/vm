using System;
using System.Linq;

namespace VisualMusic
{
    public static class ImportFileFormats
    {
        public static readonly string[] Midi = { "mid" };
        public static readonly string[] Mod = { "mod", "s3m", "xm", "it", "amf", "far", "imf", "med", "669", "mtm", "okt", "ult", "gdm", "dsm", "stm" };
        public static readonly string[] Sid = { "sid", "psid", "dat", "rsid", "mus" };
        public static readonly string[] Hvl = { "hvl", "ahx" };

        public static string[] For(global::Midi.FileType fileType) => fileType switch
        {
            global::Midi.FileType.Midi => Midi,
            global::Midi.FileType.Mod => Mod,
            global::Midi.FileType.Sid => Sid,
            global::Midi.FileType.Hvl => Hvl,
            _ => Array.Empty<string>(),
        };

        public static global::Midi.FileType? FromExtension(string extension)
        {
            string ext = (extension ?? "").TrimStart('.').ToLowerInvariant();
            if (Midi.Contains(ext)) return global::Midi.FileType.Midi;
            if (Mod.Contains(ext)) return global::Midi.FileType.Mod;
            if (Hvl.Contains(ext)) return global::Midi.FileType.Hvl;
            if (Sid.Contains(ext)) return global::Midi.FileType.Sid;
            return null;
        }
    }
}
