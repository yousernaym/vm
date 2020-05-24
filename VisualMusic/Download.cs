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

namespace VisualMusic
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

		public static SourceFileForm getImportFormFromFileType(this Form1 mainForm, string fileName)
		{
			string ext = fileName.Split('.').Last().ToLower();
			SourceFileForm importForm = null;

			if (ImportMidiForm.Formats.Contains(ext))
				importForm = Form1.ImportMidiForm;
			else if (ImportModForm.Formats.Contains(ext))
				importForm = Form1.ImportModForm;
			else if (ImportSidForm.Formats.Contains(ext))
				importForm = Form1.ImportSidForm;
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
		DownloadHandler downloadHandler;

		public bool Active { set => this.InvokeOnUiThreadIfRequired(() => Visible = value); }

		string url;
		AutoResetEvent checkFileNameEvent = new AutoResetEvent(false);

		public Client() : base("")
		{
			Width = Height = 0;
			downloadHandler = new DownloadHandler();
			downloadHandler.OnBeforeDownloadFired += OnBeforeDownload;
			downloadHandler.OnDownloadUpdatedFired += OnDownloadUpdated;
			downloadHandler.ShowDialog = false;
			this.DownloadHandler = downloadHandler;
			LoadError += OnLoadError;
			AddressChanged += OnAddressChanged;
			progressForm.ProgressText = "Download progress";
			Active = false;
		}

		private void OnAddressChanged(object sender, AddressChangedEventArgs e)
		{
			progressForm.DialogResult = DialogResult.Abort;
		}

		private void OnLoadError(object sender, LoadErrorEventArgs e)
		{
			if (e.ErrorCode != CefErrorCode.Aborted) //aborted means download started instead of a page loading
				progressForm.DialogResult = DialogResult.Cancel;
		}

		private void OnBeforeDownload(object sender, DownloadItem e)
		{
			savePath = e.SuggestedFileName = Path.Combine(VisualMusic.Program.TempDir, e.SuggestedFileName);
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
			DialogResult dlgRes = DialogResult.Abort;
			this.url = url;
			try
			{
				Active = true;
				base.Load(url);
				dlgRes = progressForm.ShowDialog();
			}
			finally
			{
				if (dlgRes != DialogResult.OK)
				{
					savePath = null;
					if (downloadHandler.UpdateCallback != null && !downloadHandler.UpdateCallback.IsDisposed)
						downloadHandler.UpdateCallback.Cancel();
				}
				Active = false;
			}
			if (dlgRes == DialogResult.Abort)
				throw new IOException("Unexpected error while downloading from url: " + url);

			return savePath;
		}
	}
}

