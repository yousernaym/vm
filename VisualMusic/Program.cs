﻿using System;
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
using System.Threading;
using System.Runtime.InteropServices;

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
		static string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
		[STAThread]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] args)
		{
			//using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
			//{
			//	if (!mutex.WaitOne(0, false))
			//	{
			//		MessageBox.Show("Visual Music already running");
			//		return;
			//	}

				try
				{
					init();
					form1 = new Form1(args);
					Application.Run(form1);
				}
				finally
				{
					close();
				}
			//}			
		}

		static void init()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Directory.SetCurrentDirectory(Dir);
			MidMix.init();
			if (!Media.initMF())
			{
				Form1.showErrorMsgBox(null, "Couldn't initialize Media library.");
				return;
			}
			Directory.CreateDirectory(TempDir);
			dirLock = File.Create(Path.Combine(TempDir, "dontdeletefolder"));
			initCefSharp();
		}

		static void initCefSharp()
		{
			Cef.EnableHighDPISupport();
			var settings = new CefSettings();
			Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
		}

		static void close()
		{
			Cef.Shutdown();
			MidMix.close();
			Media.closeMF();
			dirLock.Close();
			dirLock = null;
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
