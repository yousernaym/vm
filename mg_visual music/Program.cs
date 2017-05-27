using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Security.Permissions;
using System.IO;

namespace Visual_Music
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static public string Dir = Path.GetDirectoryName(Application.ExecutablePath);
		static public string TpartyDir = Dir + @"\tparty";
		public static string XmPlayDir = Program.TpartyDir + @"\xmplay";
		public static string XmPlayPath = Program.XmPlayDir + @"\xmplay.exe";
		public static string XmPlayOutputDir = XmPlayDir + @"\output";
		public static string XmPlayFileName = Path.GetFileName(Program.XmPlayPath);
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
			if (!Midi.Song.initLib(Dir+"\\mixdown.wav"))
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
