using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace Visual_Music
{
    public partial class ImportSidForm : ImportNotesWithAudioForm
    {
		static public readonly string[] Formats = Properties.Resources.SidFormats.ToLower().Split(null);
		int subSong = 1;
		int numSubSongs = 1;

		public ImportSidForm()
        {
            InitializeComponent();
        }
        public ImportSidForm(Form1 _parent) : base(_parent)
        {
            InitializeComponent();
        }
        
        private void Ok_Click(object sender, EventArgs e)
        {
			importFiles();
        }

		public override void importFiles()
		{
			if (!checkNoteFile())
				return;
			double songLengthS = getSongLength(); //song length in seconds. //0 = NoteExtractor default
			//Form1.setSidImport(subSong, numSubSongs, songLengthS);
			importFiles(false, false, parent.tpartyIntegrationForm.SidMixdown, songLengthS, Midi.FileType.Sid);
		}

		private void ImportSidForm_Shown(object sender, EventArgs e)
		{
			if (!parent.tpartyIntegrationForm.SidMixdown)
				existingAudioRbtn.Text = "Audio file";
			else
				existingAudioRbtn.Text = "Audio file (leave empty for SID file audio)";
		}
		double getSongLength()
		{
			//return 300;
			using (var stream = File.OpenRead(NoteFilePath))
			{
				using (var md5 = MD5.Create())
				{
					byte[] bytes = md5.ComputeHash(stream);
					string hash1 = "";
					foreach (byte b in bytes)
					{
						hash1 += b.ToString("X2");
					}
					hash1 = hash1.ToLower();
					using (StreamReader reader = new StreamReader(new MemoryStream(Properties.Resources.Songlengths)))
					{
						while (!reader.EndOfStream)
						{
							string line = reader.ReadLine().Trim();
							if (!string.IsNullOrEmpty(line) && line[0] != ';')
							{
								int equalSignIndex = line.IndexOf('=');
								if (equalSignIndex > 0)
								{
									string hash2 = line.Substring(0, equalSignIndex).ToLower();
									if (hash1 == hash2)
									{
										string timeString = line.Substring(equalSignIndex + 1, line.Length - equalSignIndex - 1);
										string subSongTimeString = timeString.Split(' ')[subSong - 1];
										string[] minsec = subSongTimeString.Split(':');
										int seconds = int.Parse(minsec[0]) * 60 + int.Parse(minsec[1]);
										//MessageBox.Show("Match found");
										return seconds;
									}
								}

							}
						}
					}
				}
			}
			return 0;
		}

		private void ImportSidForm_Load(object sender, EventArgs e)
		{
			createFormatFilter("Sid files", Formats);
		}
	}
}
