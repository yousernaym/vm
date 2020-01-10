using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace VisualMusic.Forms
{
	public partial class SubSongForm : VisualMusic.BaseDialog
	{
		int _defaultSong;
		List<int> songLengthList;
		int _selectedSong;
		public int SelectedSong
		{
			get => _selectedSong;
			set
			{
				_selectedSong = Math.Max(value, 1);
				_selectedSong = Math.Min(_selectedSong, NumSongs);
				subSongsLB.SelectedIndex = _selectedSong - 1;
				if (_selectedSong - 1 < songLengthList.Count)
					SongLengthS = songLengthList[_selectedSong - 1];
				else
					SongLengthS = 0;
			}
		}
		public float SongLengthS { get; private set; }
		public int NumSongs { get; private set; }
		
		public void init(string songPath)
		{
			if (string.IsNullOrWhiteSpace(songPath))
				return;
			using (var file = File.Open(songPath, FileMode.Open, FileAccess.Read))
			{
				file.Seek(0xe, SeekOrigin.Begin); //Seek to number of songs
				NumSongs = (file.ReadByte() << 8) | file.ReadByte();
				if (NumSongs < 1)
					NumSongs = 1;
				_defaultSong = (file.ReadByte() << 8) | file.ReadByte();
				if (_defaultSong < 0 || _defaultSong > NumSongs)
					_defaultSong = 1;
			}

			subSongsLB.Items.Clear();
			string[] songLengthStrings = getSongLengths(songPath);
			songLengthList = new List<int>();
			for (int i = 0; i < NumSongs; i++)
			{
				string defaultSuffix = i == _defaultSong - 1 ? " (default song)" : "";
				if (songLengthStrings != null && songLengthStrings.Length > i)
				{
					subSongsLB.Items.Add((i + 1) + " - " + songLengthStrings[i] + defaultSuffix);
					string[] minsec = ((string)songLengthStrings[i]).Split(':');
					songLengthList.Add(int.Parse(minsec[0]) * 60 + int.Parse(minsec[1]));
				}
				else
					subSongsLB.Items.Add(i + " - unknown length" + defaultSuffix);
			}
			SelectedSong = _defaultSong;
		}
		
		public SubSongForm()
		{
			InitializeComponent();
			subSongsLB.AutoSize = true;
		}

		string[] getSongLengths(string songPath)
		{
			using (var stream = File.OpenRead(songPath))
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

					using (StreamReader reader = new StreamReader(getSongLengthsStream()))
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
										return timeString.Split(' ');
									}
								}

							}
						}
					}
				}
			}
			return null;
		}

		private void okBtn_Click(object sender, EventArgs e)
		{
			SelectedSong = subSongsLB.SelectedIndex + 1;
		}

		private void subSongsLB_Resize(object sender, EventArgs e)
		{
			int buttonMargin = (Height - okBtn.Bottom);
			Height = subSongsLB.Bottom + okBtn.Height + 10 * DeviceDpi / 96 + buttonMargin;
		}

		Stream getSongLengthsStream()
		{
			Stream sr;
			if (Form1.TpartyIntegrationForm.HvscSongLengths)
				sr = new FileStream(Form1.TpartyIntegrationForm.SongLengthsPath, FileMode.Open, FileAccess.Read);
			else
				sr = new MemoryStream(Properties.Resources.Songlengths);
			return sr;
		}
	}
}
