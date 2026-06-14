using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace VisualMusic
{
    static class Program
    {
        public static readonly string AppName = "Visual Music";
        public static readonly string Dir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        public static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        public static readonly string DefaultUserFilesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppName);
        public static readonly string TempDirRoot = Path.Combine(Path.GetTempPath(), AppName).ToLowerInvariant();
        public static readonly string TempDir = Path.Combine(TempDirRoot, Path.GetRandomFileName());
        public static readonly string MixdownPath = Path.Combine(TempDir, "mixdown.wav");
        public static string FileVersion => FileVersionInfo.GetVersionInfo(Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location).FileVersion;

        static FileStream s_dirLock;

        public static void InitTempDir()
        {
            Directory.CreateDirectory(TempDir);
            s_dirLock ??= File.Create(Path.Combine(TempDir, "dontdeletefolder"));
        }

        public static void CloseTempDir()
        {
            if (s_dirLock != null)
            {
                s_dirLock.Close();
                s_dirLock = null;
            }

            try
            {
                Directory.Delete(TempDir, true);
            }
            catch
            {
                // Temp files are best-effort cleanup; leftovers are safe to remove next run.
            }
        }
    }
}
