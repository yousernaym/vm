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

            // Read the SID header (song count + default song). Use a shared-read open and
            // release it before the HVSC lookup, which reopens the same file.
            int defSong;
            using (var f = File.Open(sidPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                f.Seek(0x0e, SeekOrigin.Begin);
                NumSongs = (f.ReadByte() << 8) | f.ReadByte();
                defSong  = (f.ReadByte() << 8) | f.ReadByte();
            }
            if (NumSongs < 1) NumSongs = 1;
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
            SongLengthS  = _lengths.Count > defSong - 1 ? _lengths[defSong - 1] : 0;
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
