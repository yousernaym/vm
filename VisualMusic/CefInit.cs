using CefSharp;
using CefSharp.Wpf;
using System.IO;

namespace VisualMusic
{
    /// <summary>
    /// Starts CefSharp with a cache path unique to this process so a second VM.exe
    /// is a real second instance instead of Chromium's default Chrome window.
    /// </summary>
    static class CefInit
    {
        public static bool Initialize()
        {
            if (Cef.IsInitialized == true)
                return true;

            // TempDir is already per-process (random subfolder). Chromium's process
            // singleton is keyed on RootCachePath; sharing the default
            // %LocalAppData%\CEF\User Data folder is what hijacks a second launch.
            string cacheRoot = Path.Combine(Program.TempDir, "cef");
            Directory.CreateDirectory(cacheRoot);

            using var settings = new CefSettings
            {
                RootCachePath = cacheRoot,
                CachePath = cacheRoot,
                LogFile = Path.Combine(Program.TempDir, "cef.log"),
            };

            return Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        public static void Shutdown()
        {
            if (Cef.IsInitialized == true)
                Cef.Shutdown();
        }
    }
}
