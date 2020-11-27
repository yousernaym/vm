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
using CefSharp.WinForms;

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
		static FileStream dirLock = null;
		static string cefSharpFolder = "CefSharp";

		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//In case the program crashed previously, kill the cefsharp processes that stayed open. Otherwise multiple crashes will cause the number of processes to build up and hog the cpu. This has the drawback of killing cefsharp processes created by other programs, but it's very convenient when debugging a crash.
			Process[] cefSharpProcesses = Process.GetProcessesByName("CefSharp.BrowserSubProcess");
			foreach (var process in cefSharpProcesses)
				process.Kill();

			Cef.EnableHighDPISupport();
			var settings = new CefSettings();

			// Make sure you set performDependencyCheck false
			Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

			//var browser = new BrowserForm();
			//Application.Run(browser);

			try
			{
				MidMix.init();
				if (!Media.initMF())
				{
					Form1.showErrorMsgBox(null, "Couldn't initialize Media Foundation.");
					return;
				}
				Directory.CreateDirectory(TempDir);
				dirLock = File.Create(Path.Combine(TempDir, "dontdeletefolder"));
				//Midi.Song.initLib(TempDir, Path.GetFileName(MixdownPath));
				form1 = new Form1(args);
				Application.Run(form1);
			}
			finally
			{
				MidMix.close();
				Media.closeMF();
				//Midi.Song.exitLib();
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
	}
}
