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
		static string dir = Path.GetDirectoryName(Application.ExecutablePath);
		static public string Dir { get => dir; }
		static public Form1 form1;
		static string tempDir = Path.Combine(Dir, "temp");
		
		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] args)
		{
			//AppDomain currentDomain = AppDomain.CurrentDomain;
			//currentDomain.UnhandledException += new UnhandledExceptionEventHandler(exceptionHandler);

			if (!Media.initMF())
			{
				MessageBox.Show("Couldn't initialize Media Foundation.");
				return;
			}
			Midi.Song.initLib(Path.Combine(tempDir, Path.GetRandomFileName()));
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			form1 = new Form1(args);
			Application.Run(form1);

			if (!Media.closeMF())
				MessageBox.Show("Couldn't close Media Foundation.");

			//Midi.Song.deleteMixdownDir(); //Optional because last instance deletes the whole temp folder
			//Delete temp folder in case of earlier instances of the app crashed and couldn't delete its mixdowns
			if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length == 1) //no other instances running?
				Directory.Delete(tempDir, true);
								
			Midi.Song.exitLib();
			DirectoryInfo xmPlayOutputDir = new DirectoryInfo(TpartyIntegrationForm.XmPlayOutputDir);
			xmPlayOutputDir.clean();
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
		static public float getSecondsF(this TimeSpan ts)
		{
			return (float)((double)ts.Ticks / (double)Stopwatch.Frequency);
		}
	}
}
