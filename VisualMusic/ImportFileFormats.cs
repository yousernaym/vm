using System;
using System.Linq;

namespace VisualMusic
{
    public static class ImportFileFormats
    {
        public static readonly string[] Midi = { "mid" };

        // libopenmpt-supported module / container extensions.
        // "mus" is also a SID dump extension (see Sid); Remuxer content-sniffs Mod→HVL→SID
        // regardless of FileType, so either import path works for the conversion itself.
        public static readonly string[] Mod =
        {
            "mptm", "mod", "s3m", "xm", "it",
            "667", "669", "amf", "ams", "c67", "cba", "dbm", "digi", "dmf", "dsm", "dsym", "dtm",
            "etx", "far", "fc", "fc13", "fc14", "fmt", "fst", "ftm",
            "gdm", "gmc", "gtk", "gt2",
            "imf", "ims", "ice", "itp", "j2b",
            "m15", "mdl", "med", "mms", "mo3", "mt2", "mtm", "mtp", "mus",
            "nru", "nst",
            "okt", "oxm",
            "plm", "ppm", "psm", "pt36", "ptm", "puma",
            "rtm",
            "sfx", "sfx2", "smod", "ss", "st26", "stk", "stm", "stx", "stp", "symmod",
            "tcb",
            "ult", "umx", "unic",
            "wow",
            "xmf", "xpk",
            "mmcmp",
        };

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

        /// <summary>
        /// Maps a file extension to an import type. When <paramref name="preferred"/> is set
        /// (e.g. which browser started the download) and the extension is valid for that type,
        /// preferred wins — so ambiguous extensions like .mus follow Mod vs SID browser context.
        /// </summary>
        public static FileType? FromExtension(string extension, FileType? preferred = null)
        {
            string ext = (extension ?? "").TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) return null;

            if (preferred is FileType pref && For(pref).Contains(ext))
                return pref;

            if (Midi.Contains(ext)) return FileType.Midi;
            if (Mod.Contains(ext)) return FileType.Mod;
            if (Hvl.Contains(ext)) return FileType.Hvl;
            if (Sid.Contains(ext)) return FileType.Sid;
            return null;
        }
    }
}
