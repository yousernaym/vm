using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace VisualMusic.Controls
{
    /// <summary>
    /// WPF replacement for the WinForms SubSongForm.
    /// Shows a list of sub-songs read from the SID file header; lets the user pick one.
    /// </summary>
    public partial class SubSongWindow : MetroWindow
    {
        public int SelectedSong  { get; private set; } = 1;
        public float SongLengthS { get; private set; }
        public int NumSongs      { get; private set; }

        readonly List<float> _lengths = new();

        public SubSongWindow(string sidPath)
        {
            InitializeComponent();

            using (var f = File.Open(sidPath, FileMode.Open, FileAccess.Read))
            {
                f.Seek(0x0e, SeekOrigin.Begin);
                NumSongs    = (f.ReadByte() << 8) | f.ReadByte();
                int defSong = (f.ReadByte() << 8) | f.ReadByte();
                if (NumSongs < 1)  NumSongs = 1;
                if (defSong < 1 || defSong > NumSongs) defSong = 1;

                string[] lengths = GetSongLengths(sidPath);
                for (int i = 0; i < NumSongs; i++)
                {
                    bool isDefault = i == defSong - 1;
                    float sec = 0;
                    string label;
                    if (lengths != null && i < lengths.Length)
                    {
                        label = $"{i + 1} - {lengths[i]}{(isDefault ? " (default)" : "")}";
                        string[] parts = lengths[i].Split(':');
                        if (parts.Length == 2 &&
                            float.TryParse(parts[0], out float m) &&
                            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float s))
                            sec = m * 60 + s;
                    }
                    else
                        label = $"{i + 1} - unknown length{(isDefault ? " (default)" : "")}";

                    _lengths.Add(sec);
                    subSongList.Items.Add(label);
                }

                subSongList.SelectedIndex = Math.Max(0, defSong - 1);
                SelectedSong  = defSong;
                SongLengthS   = _lengths.Count > defSong - 1 ? _lengths[defSong - 1] : 0;
            }
        }

        static string[] GetSongLengths(string sidPath)
        {
            // Replicate the path from TpartyIntegrationForm without instantiating it.
            string dbPath = Path.Combine(TpartyIntegrationForm.TpartyDir, "hvsc", "songlenghts.md5");
            if (!File.Exists(dbPath)) return null;

            string hash;
            using (var stream = File.OpenRead(sidPath))
            {
                byte[] bytes = MD5.HashData(stream);
                hash = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }

            using var reader = new StreamReader(dbPath);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line) || line[0] == ';') continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                if (line.Substring(0, eq).ToLowerInvariant() == hash)
                    return line.Substring(eq + 1).Split(' ');
            }
            return null;
        }

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            int idx = subSongList.SelectedIndex;
            if (idx < 0) idx = 0;
            SelectedSong  = idx + 1;
            SongLengthS   = idx < _lengths.Count ? _lengths[idx] : 0;
            DialogResult  = true;
        }

        void SubSongList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (subSongList.SelectedIndex >= 0)
                Ok_Click(sender, e);
        }
    }
}
