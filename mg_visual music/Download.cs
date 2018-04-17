using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.Example;

namespace Visual_Music
{
	public static class Download
	{
		static Client client = new Client();

		public static void init(Form form)
		{
			form.Controls.Add(client);
		}
		public static string downloadFile(this string path)
		{
			if (path.IsUrl())
			{
				client.Load(path);
				string savePath = null;
				if (client.ProgressForm.ShowDialog() == DialogResult.OK)
					savePath = Client.SavePath;
				return savePath;
			}
			else
				return path;
		}

		public static bool IsUrl(this string path)
		{
			return Uri.IsWellFormedUriString(path, UriKind.Absolute);
		}
	}

	class Client : ChromiumWebBrowser
	{
		public static string SavePath { get; private set; }
		public ProgressForm ProgressForm;
		public Client() : base("")
		{
			Width = Height = 0;
			this.InvokeOnUiThreadIfRequired(() =>  ProgressForm = new ProgressForm());
			DownloadHandler downloadHandler = new DownloadHandler();
			downloadHandler.OnBeforeDownloadFired += OnBeforeDownload;
			downloadHandler.OnDownloadUpdatedFired += OnDownloadUpdated;
			downloadHandler.ShowDialog = false;
			this.DownloadHandler = downloadHandler;
			
		}

		private void OnBeforeDownload(object sender, DownloadItem e)
		{
			SavePath = e.SuggestedFileName = Path.Combine(Visual_Music.Program.TempDir, e.SuggestedFileName);
		}

		private void OnDownloadUpdated(object sender, DownloadItem e)
		{
			ProgressForm.InvokeOnUiThreadIfRequired(()=>ProgressForm.updateProgress(e.PercentComplete / 100.0f));
			if (e.IsComplete)
			{
				ProgressForm.InvokeOnUiThreadIfRequired(delegate ()
				{
					ProgressForm.DialogResult = DialogResult.OK;
					ProgressForm.Hide();
				});
			}
		}
	}
}

