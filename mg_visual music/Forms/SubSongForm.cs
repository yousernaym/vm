using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music.Forms
{
	public partial class SubSongForm : Visual_Music.BaseDialog
	{
		int _defaultSong;
		List<int> songLengthList;
		public int SelectedSong { get; private set; }
		public float SongLengthS { get; private set; }
		
		public void init(string songPath)
		{
			if (string.IsNullOrWhiteSpace(songPath))
				return;
			int numSongs;
			using (var file = File.Open(songPath, FileMode.Open, FileAccess.Read))
			{
				file.Seek(0xe, SeekOrigin.Begin); //Seek to number of songs
				numSongs = (file.ReadByte() << 8) | file.ReadByte();
				if (numSongs < 1)
					numSongs = 1;
				_defaultSong = (file.ReadByte() << 8) | file.ReadByte();
				if (_defaultSong < 0 || _defaultSong > numSongs)
					_defaultSong = 1;
			}

			subSongsLB.Items.Clear();
			string[] songLengthStrings = getSongLengths(songPath);
			songLengthList = new List<int>();
			for (int i = 0; i < numSongs; i++)
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
			subSongsLB.SelectedIndex = _defaultSong - 1;
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
			if (subSongsLB.SelectedIndices.Count == 0)
				SelectedSong = _defaultSong;
			else
				SelectedSong = subSongsLB.SelectedIndices[0] + 1;

			if (SelectedSong - 1 < songLengthList.Count)
				SongLengthS = songLengthList[SelectedSong - 1];
			else
				SongLengthS = 0;
		}

		private void subSongsLB_Resize(object sender, EventArgs e)
		{
			int buttonMargin = Height - okBtn.Bottom;
			Height = subSongsLB.Bottom + okBtn.Height + buttonMargin + 10;
		}
	}
}
