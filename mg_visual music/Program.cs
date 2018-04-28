using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Security.Permissions;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CefSharp;
using System.Reflection;

namespace Visual_Music
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static public readonly string Dir = Path.GetDirectoryName(Application.ExecutablePath);
		static public readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Visual Music");
		static public Form1 form1;
		static public readonly string TempDirRoot = Path.Combine(Path.GetTempPath(), "visual music").ToLower();
		static public readonly string TempDir = Path.Combine(TempDirRoot, Path.GetRandomFileName().ToLower()); //If more instances of the program is running simultaneously, every instance will have its own temp dir
		static public readonly string MixdownPath = Path.Combine(TempDir, "mixdown.wav");
		static FileStream dirLock = null;

		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] args)
		{
			//AppDomain currentDomain = AppDomain.CurrentDomain;
			//currentDomain.UnhandledException += new UnhandledExceptionEventHandler(exceptionHandler);
			AppDomain.CurrentDomain.AssemblyResolve += Resolver;
			LoadApp(args);
		}

		static void exceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception e = (Exception)args.ExceptionObject;
			MessageBox.Show(e.Message);
			//Console.WriteLine("MyHandler caught : " + e.Message);
			//Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
		}
		public static void clean(this DirectoryInfo directory)
		{
			foreach (FileInfo file in directory.GetFiles()) file.Delete();
			foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void LoadApp(string[] args)
		{
			Cef.EnableHighDPISupport();
			var settings = new CefSettings();

			// For AnyCPU, Set BrowserSubProcessPath based on app bitness at runtime
			string platformString = Environment.Is64BitProcess ? "x64" : "x86";
			// For x86, use root app folder
			#if X86
				platformString = "";
			#endif
			settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
												   platformString,
												   "CefSharp.BrowserSubprocess.exe");
					

			// Make sure you set performDependencyCheck false
			Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);

			//var browser = new BrowserForm();
			//Application.Run(browser);

			try
			{
				if (!Media.initMF())
				{
					Form1.showErrorMsgBox(null, "Couldn't initialize Media Foundation.");
					return;
				}
				Directory.CreateDirectory(TempDir);
				dirLock = File.Create(Path.Combine(TempDir, "dontdeletefolder"));
				Midi.Song.initLib(TempDir, Path.GetFileName(MixdownPath));
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				form1 = new Form1(args);
				Application.Run(form1);
			}
			finally
			{
				Media.closeMF();
				Midi.Song.exitLib();
				dirLock.Close();
				dirLock = null;
				try
				{
					//if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length == 1) //no other instances running?
					Directory.Delete(TempDirRoot, true);
				}
				catch
				{
					//If a file can't be deleted for any reason just leave it. All will be deleted next time.
				}
			}
		}

		// Will attempt to load missing assembly from either x86 or x64 subdir
		private static Assembly Resolver(object sender, ResolveEventArgs args)
		{
			if (args.Name.StartsWith("CefSharp"))
			{
				string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
				string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
													   Environment.Is64BitProcess ? "x64" : "x86",
													   assemblyName);

				return File.Exists(archSpecificPath)
						   ? Assembly.LoadFile(archSpecificPath)
						   : null;
			}

			return null;
		}
	}
}
