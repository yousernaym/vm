using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Dialog for choosing a SID sub-song.
    /// Shows a list of sub-songs read from the SID file header; lets the user pick one.
    /// </summary>
    public partial class SubSongWindow : MetroWindow
    {
        public int SelectedSong { get; private set; } = 1;
        public float SongLengthS { get; private set; }
        public int NumSongs { get; private set; }

        readonly List<float> _lengths = new();

        public SubSongWindow(string sidPath)
        {
            InitializeComponent();

            // Read the SID header (song count + default song). Use a shared-read open and
            // release it before the HVSC lookup, which reopens the same file.
            int defSong;
            bool validSid;
            using (var f = File.Open(sidPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Magic must be "PSID" or "RSID"; otherwise this isn't a SID file (e.g. a stray HTML
                // page) and the header fields below are meaningless — don't trust the sub-song count.
                byte[] magic = new byte[4];
                validSid = f.Read(magic, 0, 4) == 4 &&
                           (magic[0] == 'P' || magic[0] == 'R') &&
                           magic[1] == 'S' && magic[2] == 'I' && magic[3] == 'D';

                f.Seek(0x0e, SeekOrigin.Begin);
                NumSongs = (f.ReadByte() << 8) | f.ReadByte();
                defSong = (f.ReadByte() << 8) | f.ReadByte();
            }
            // Per the SID spec the sub-song count is 1..256; clamp so a malformed file can't produce a
            // bogus list. A non-SID file is treated as a single sub-song (import then fails cleanly).
            if (!validSid || NumSongs < 1) NumSongs = 1;
            else if (NumSongs > 256) NumSongs = 256;
            if (defSong < 1 || defSong > NumSongs) defSong = 1;

            string[] lengths = Hvsc.GetSongLengths(sidPath);
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
            SelectedSong = defSong;
            SongLengthS = _lengths.Count > defSong - 1 ? _lengths[defSong - 1] : 0;

            // Scroll the default sub-song into view once the list is laid out.
            Loaded += (_, _) =>
            {
                if (subSongList.SelectedItem != null)
                {
                    subSongList.ScrollIntoView(subSongList.SelectedItem);
                    (subSongList.ItemContainerGenerator.ContainerFromIndex(subSongList.SelectedIndex)
                        as System.Windows.Controls.ListBoxItem)?.Focus();
                }
            };
        }

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            int idx = subSongList.SelectedIndex;
            if (idx < 0) idx = 0;
            SelectedSong = idx + 1;
            SongLengthS = idx < _lengths.Count ? _lengths[idx] : 0;
            DialogResult = true;
        }

        void SubSongList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (subSongList.SelectedIndex >= 0)
                Ok_Click(sender, e);
        }
    }
}
