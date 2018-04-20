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
using System.Threading;

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
			return client.Load(path);
		}

		public static bool IsUrl(this string path)
		{
			return Uri.IsWellFormedUriString(path, UriKind.Absolute);
		}

		async public static Task<string> getFileNameFromUrl(this string url)
		{
			return await client.getFileName(url);
		}
		public static SourceFileForm getImportFormFromFileType(this Form1 mainForm, string fileName)
		{
			string ext = fileName.Split('.').Last().ToLower();
			SourceFileForm importForm = null;

			if (ImportMidiForm.Formats.Contains(ext))
				importForm = mainForm.importMidiForm;
			else if (ImportModForm.Formats.Contains(ext))
				importForm = mainForm.importModForm;
			else if (ImportSidForm.Formats.Contains(ext))
				importForm = mainForm.importSidForm;
			return importForm;
		}

		static void ExecuteBlockingTask()
		{
			client.Active = true;
		}
	}

	class Client : ChromiumWebBrowser
	{
		string savePath { get; set; }
		ProgressForm progressForm = new ProgressForm();
		bool bCheckFileName = false;
		string fileName;
		DownloadHandler downloadHandler;

		public bool Active { set => this.InvokeOnUiThreadIfRequired(() => Visible = value); }

	AutoResetEvent checkFileNameEvent = new AutoResetEvent(false);

		public Client() : base("")
		{
			Width = Height = 0;
			downloadHandler = new DownloadHandler();
			downloadHandler.OnBeforeDownloadFired += OnBeforeDownload;
			downloadHandler.OnDownloadUpdatedFired += OnDownloadUpdated;
			downloadHandler.ShowDialog = false;
			this.DownloadHandler = downloadHandler;
			progressForm.ProgressText = "Download progress";
			Active = false;
		}

		private void OnBeforeDownload(object sender, DownloadItem e)
		{
			if (bCheckFileName)
			{
				bCheckFileName = false;
				fileName = e.SuggestedFileName;
				e.IsCancelled = true;
				checkFileNameEvent.Set();
				Active = false;
				return;
			}
			
			savePath = e.SuggestedFileName = Path.Combine(Visual_Music.Program.TempDir, e.SuggestedFileName);
			//this.InvokeOnUiThreadIfRequired(()=> Show());
		}

		private void OnDownloadUpdated(object sender, DownloadItem e)
		{
			progressForm.InvokeOnUiThreadIfRequired(()=>progressForm.updateProgress(e.PercentComplete / 100.0f));
			if (e.IsComplete)
			{
				progressForm.InvokeOnUiThreadIfRequired(delegate ()
				{
					progressForm.DialogResult = DialogResult.OK;
					progressForm.Close();
				});
				Active = false;
			}
		}

		public new string Load(string url)
		{
			Active = true;
			base.Load(url);
			if (progressForm.ShowDialog() != DialogResult.OK)
			{
				savePath = null;
				if (downloadHandler.UpdateCallback != null && !downloadHandler.UpdateCallback.IsDisposed)
					downloadHandler.UpdateCallback.Cancel();
			}
			Active = false;
			return savePath;

		}
		async public Task<string> getFileName(string url)
		{
			bCheckFileName = true;
			Load(url);
			bool fileFound = await Task<bool>.Run(()=>checkFileNameEvent.WaitOne());
			if (!fileFound)
				fileName = null;
			return fileName;
		}
	}
}

