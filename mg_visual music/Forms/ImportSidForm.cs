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
			if (!checkNoteFile())
				return;
			double songLengthS = 0; //song length in seconds. //0 = NoteExtractor default
			//if (parent.tpartyIntegrationForm.HvscSongLengths)
				//songLengthS = getSongLength();
			importFiles(false, false, parent.tpartyIntegrationForm.SidMixdown, songLengthS);
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
			return 300;
			using (var stream = File.OpenRead(noteFilePath.Text))
			{
				using (var md5 = MD5.Create())
				{
					//TODO: Create accurate sid hash
					byte[] bytes = md5.ComputeHash(stream);
					string hash1 = "";
					foreach (byte b in bytes)
					{
						hash1 += b.ToString("X2");
					}
					hash1 = hash1.ToLower();
					using (StreamReader reader = new StreamReader(parent.tpartyIntegrationForm.SongLengthsPath))
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
										//read song time
										MessageBox.Show("Match found");
									}
								}

							}
						}
					}
				}
				
			}
		}
	}
}
