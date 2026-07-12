using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Modal for assigning multiple audio files to tracks by drag-and-drop, with automatic
    /// name-based pairing. Opened from the Audio tab when only the Global track is selected.
    /// Read <see cref="Assignments"/> (track row index → full file path) after DialogResult == true.
    /// </summary>
    public partial class AssignAudioFilesWindow : MetroWindow
    {
        const string DragFormat = "VisualMusic.AudioFileBox";

        public class FileBoxVm
        {
            public string FullPath { get; init; }
            public string DisplayName { get; init; }
        }

        public class TrackRowVm : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            void Notify([CallerMemberName] string name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            public string TrackName { get; init; }
            public int TrackIndex { get; init; }

            FileBoxVm _assignedFile;
            public FileBoxVm AssignedFile
            {
                get => _assignedFile;
                set
                {
                    _assignedFile = value;
                    Notify();
                    Notify(nameof(HasFile));
                    Notify(nameof(HasNoFile));
                }
            }
            public bool HasFile => AssignedFile != null;
            public bool HasNoFile => AssignedFile == null;

            bool _isDropTarget;
            public bool IsDropTarget
            {
                get => _isDropTarget;
                set { _isDropTarget = value; Notify(); }
            }
        }

        readonly List<TrackRowVm> _trackRows;
        readonly ObservableCollection<FileBoxVm> _pool;

        /// <summary>Track row index → full file path; valid after DialogResult == true.</summary>
        public IReadOnlyDictionary<int, string> Assignments { get; private set; }

        // Drag state: chip pressed but not yet dragged, and the row it sits in (null = pool).
        Point _dragStart;
        FileBoxVm _pressedBox;
        TrackRowVm _pressedSourceRow;

        public AssignAudioFilesWindow(IReadOnlyList<string> trackNames, IReadOnlyList<string> filePaths)
        {
            InitializeComponent();

            _trackRows = trackNames
                .Select((name, i) => new TrackRowVm { TrackName = name, TrackIndex = i })
                .ToList();
            _pool = new ObservableCollection<FileBoxVm>(filePaths.Select(p => new FileBoxVm
            {
                FullPath = p,
                DisplayName = Path.GetFileName(p)
            }));

            trackItems.ItemsSource = _trackRows;
            poolItems.ItemsSource = _pool;

            AutoPair();
        }

        // ---- Auto-pairing: longest-common-substring on normalized names ----

        void AutoPair()
        {
            var trackKeys = _trackRows
                .Select(r => Normalize(Regex.Replace(r.TrackName ?? "", @"^\d+\s*-\s*", "")))
                .ToList();
            var fileKeys = _pool
                .Select(f => Normalize(Path.GetFileNameWithoutExtension(f.FullPath)))
                .ToList();

            var candidates = new List<(int len, int track, int file)>();
            for (int t = 0; t < trackKeys.Count; t++)
            {
                if (trackKeys[t].Length == 0) continue;
                for (int f = 0; f < fileKeys.Count; f++)
                {
                    if (fileKeys[f].Length == 0) continue;
                    int len = LongestCommonSubstring(trackKeys[t], fileKeys[f]);
                    if (len >= 3)
                        candidates.Add((len, t, f));
                }
            }

            var trackUsed = new bool[trackKeys.Count];
            var fileUsed = new bool[fileKeys.Count];
            var files = _pool.ToList();   // index-stable snapshot while removing from _pool
            foreach (var (_, t, f) in candidates
                .OrderByDescending(c => c.len).ThenBy(c => c.track).ThenBy(c => c.file))
            {
                if (trackUsed[t] || fileUsed[f]) continue;
                trackUsed[t] = true;
                fileUsed[f] = true;
                _trackRows[t].AssignedFile = files[f];
                _pool.Remove(files[f]);
            }
        }

        static string Normalize(string s) =>
            new string(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        static int LongestCommonSubstring(string a, string b)
        {
            int best = 0;
            var prev = new int[b.Length + 1];
            var cur = new int[b.Length + 1];
            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    cur[j] = a[i - 1] == b[j - 1] ? prev[j - 1] + 1 : 0;
                    if (cur[j] > best) best = cur[j];
                }
                (prev, cur) = (cur, prev);
            }
            return best;
        }

        // ---- Drag-drop ----

        void Chip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FileBoxVm box) return;
            _dragStart = e.GetPosition(null);
            _pressedBox = box;
            _pressedSourceRow = _trackRows.FirstOrDefault(r => r.AssignedFile == box);
            e.Handled = true;
        }

        void Chip_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _pressedBox == null) return;
            Vector diff = e.GetPosition(null) - _dragStart;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            var box = _pressedBox;
            _pressedBox = null;   // click-vs-drag disambiguation: a started drag is not a click

            DragDrop.DoDragDrop((DependencyObject)sender,
                new DataObject(DragFormat, box),
                DragDropEffects.Move);

            // DoDragDrop is synchronous; clear lingering state in case Drop/DragLeave didn't fire.
            _pressedSourceRow = null;
            foreach (var row in _trackRows)
                row.IsDropTarget = false;
        }

        // Click (no drag) on an assigned chip returns it to the pool; pool chips ignore clicks.
        void Chip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var box = _pressedBox;
            var sourceRow = _pressedSourceRow;
            _pressedBox = null;
            _pressedSourceRow = null;
            if (box == null || sourceRow == null) return;

            sourceRow.AssignedFile = null;
            _pool.Add(box);
            e.Handled = true;
        }

        void TrackRow_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DragFormat) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
            if (e.Effects == DragDropEffects.Move &&
                (sender as FrameworkElement)?.DataContext is TrackRowVm row)
                row.IsDropTarget = true;
        }

        void TrackRow_DragLeave(object sender, DragEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is TrackRowVm row)
                row.IsDropTarget = false;
        }

        void TrackRow_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if ((sender as FrameworkElement)?.DataContext is not TrackRowVm target) return;
            target.IsDropTarget = false;
            if (e.Data.GetData(DragFormat) is not FileBoxVm box) return;

            var sourceRow = _pressedSourceRow;
            _pressedSourceRow = null;
            if (sourceRow == target) return;   // dropped back where it came from

            // Displaced chip goes back to the pool.
            if (target.AssignedFile != null)
                _pool.Add(target.AssignedFile);

            target.AssignedFile = box;
            if (sourceRow != null)
                sourceRow.AssignedFile = null;
            else
                _pool.Remove(box);
        }

        void Pool_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DragFormat) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        void Pool_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetData(DragFormat) is not FileBoxVm box) return;

            var sourceRow = _pressedSourceRow;
            _pressedSourceRow = null;
            if (sourceRow == null) return;   // already in the pool

            sourceRow.AssignedFile = null;
            _pool.Add(box);
        }

        // ---- OK ----

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            Assignments = _trackRows
                .Where(r => r.AssignedFile != null)
                .ToDictionary(r => r.TrackIndex, r => r.AssignedFile.FullPath);
            DialogResult = true;
        }
    }
}
