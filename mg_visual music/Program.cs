using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Security.Permissions;

namespace Visual_Music
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static public string Path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
		static public Form1 form1;
		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] args)
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(exceptionHandler);

			if (!Media.initMF())
			{
				MessageBox.Show("Couldn't initialize Media Foundation.");
				return;
			}
			if (!Midi.Song.initLib(Path+"\\mixdown.wav"))
			{
				MessageBox.Show("Couldn't initialize Mikmod.");
				return;
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			form1 = new Form1(args);
			Application.Run(form1);
			Midi.Song.exitLib();
			if (!Media.closeMF())
				MessageBox.Show("Couldn't close Media Foundation.");
		}
		static void exceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception e = (Exception)args.ExceptionObject;
			MessageBox.Show(e.Message);
			//Console.WriteLine("MyHandler caught : " + e.Message);
			//Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
		}
	

	}
}
