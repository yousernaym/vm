using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Security.Permissions;
using System.IO;
using System.Diagnostics;

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
		static readonly string tempDirRoot = Path.Combine(Path.GetTempPath(), "Visual Music");
		static public readonly string TempDir = Path.Combine(tempDirRoot, Path.GetRandomFileName()); //If more instances of the program is running simultaneously, every instance will have its own temp dir
		static public readonly string MixdownPath = Path.Combine(TempDir, "mixdown.wav");

		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] args)
		{
			//AppDomain currentDomain = AppDomain.CurrentDomain;
			//currentDomain.UnhandledException += new UnhandledExceptionEventHandler(exceptionHandler);
			try
			{
				if (!Media.initMF())
				{
					Form1.showErrorMsgBox(null, "Couldn't initialize Media Foundation.");
					return;
				}
				
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
				try
				{
					if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length == 1) //no other instances running?
						Directory.Delete(tempDirRoot, true);
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
