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

        public static string[] For(FileType fileType) => fileType switch
        {
            FileType.Midi => Midi,
            FileType.Mod => Mod,
            FileType.Sid => Sid,
            FileType.Hvl => Hvl,
            _ => Array.Empty<string>(),
        };

        public static FileType? FromExtension(string extension)
        {
            string ext = (extension ?? "").TrimStart('.').ToLowerInvariant();
            if (Midi.Contains(ext)) return FileType.Midi;
            if (Mod.Contains(ext)) return FileType.Mod;
            if (Hvl.Contains(ext)) return FileType.Hvl;
            if (Sid.Contains(ext)) return FileType.Sid;
            return null;
        }
    }
}
