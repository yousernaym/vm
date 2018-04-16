using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music
{
	public static class Download
	{
		static Client client = new Client();
		
		public static string downloadFile(this string path)
		{
			if (path.IsUrl())
			{
				string fileName = path.Split('/').Last().Split('#').Last();
				Uri url = new Uri(path);
				string downloadedFilePath = Path.Combine(Program.TempDir, fileName);
				client.DownloadFileAsync(url, downloadedFilePath);
				if (client.ProgressForm.ShowDialog() != DialogResult.OK)
					downloadedFilePath = null;
				return downloadedFilePath;
			}
			else
				return path;
		}

		public static bool IsUrl(this string path)
		{
			return Uri.IsWellFormedUriString(path, UriKind.Absolute);
		}
	}

	class Client : WebClient
	{
		public readonly ProgressForm ProgressForm = new ProgressForm();
		public Client()
		{
			DownloadFileCompleted += OnDownloadCompleted;
			DownloadProgressChanged += OnDownloadProgressChanged;
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			ProgressForm.updateProgress(e.ProgressPercentage / 100.0f);
		}

		private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			//NoteFilePath = downloadedFilePath;
			ProgressForm.DialogResult = DialogResult.OK;
			ProgressForm.Hide();
		}
	}
}
