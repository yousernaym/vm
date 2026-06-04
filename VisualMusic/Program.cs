using CefSharp;
using CefSharp.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VisualMusic
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static public readonly string AppName = "Visual Music";
        static public readonly string Dir = Path.GetDirectoryName(Application.ExecutablePath);
        static public readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        static public readonly string DefaultUserFilesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppName);
        static public Form1 form1;
        static public readonly string TempDirRoot = Path.Combine(Path.GetTempPath(), AppName).ToLower();
        static public readonly string TempDir = Path.Combine(TempDirRoot, Path.GetRandomFileName()); //If more instances of the program is running simultaneously, every instance will have its own temp dir
        static public readonly string MixdownPath = Path.Combine(TempDir, "mixdown.wav");
        static public string FileVersion => FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion;

        static FileStream s_dirLock = null;
        static string s_appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;

        [STAThread]
        static void Main(string[] args)
        {
            const int semaphoreMaxCount = 10;
            Semaphore semaphore = null;
            try
            {
                semaphore = new Semaphore(semaphoreMaxCount, semaphoreMaxCount, "Global\\" + s_appGuid);
                if (!semaphore.WaitOne(0, false))
                {
                    return;
                }

                try
                {
                    Init();
                    form1 = new Form1(args);
                    Application.Run(form1);
                }
                finally
                {
                    Close();
                }
            }
            finally
            {
                if (semaphore != null)
                {
                    if (semaphore.Release() == semaphoreMaxCount - 1)
                        DeleteTempDirs();
                }
            }
        }

        static void Init()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Directory.SetCurrentDirectory(Dir);
            MidMix.Init();
            if (!Media.InitMF())
            {
                Form1.ShowErrorMsgBox(null, "Couldn't initialize Media library.");
                return;
            }
            Directory.CreateDirectory(TempDir);
            s_dirLock = File.Create(Path.Combine(TempDir, "dontdeletefolder"));
            InitCefSharp();
        }

        static void InitCefSharp()
        {
            var settings = new CefSettings();
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        public static void Close()
        {
            Cef.Shutdown();
            MidMix.Close();
            Media.CloseMF();
        }

        static void DeleteTempDirs()
        {
            if (s_dirLock != null)
            {
                s_dirLock.Close();
                s_dirLock = null;
            }
            try
            {
                Directory.Delete(TempDirRoot, true);
            }
            catch
            {
                //If a file can't be deleted for any reason just leave it. All will be deleted next time.
            }
        }
    }
}
