using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace VisualMusic.ViewModels
{
    public partial class TrackListViewModel : ObservableObject
    {
        public ObservableCollection<TrackItemViewModel> Items { get; } = new();

        [ObservableProperty] TrackItemViewModel _selectedItem;

        // Code-behind pushes the ListView.SelectedItems here (read-only on the ListView itself).
        public IList<TrackItemViewModel> SelectedItems { get; set; } = new List<TrackItemViewModel>();

        public event Action SelectionChanged;

        // Wired by MainViewModel: copy the currently-open tab's props from the drop-target
        // (source) to the dragged items (destinations). Ctrl+drag gesture.
        public Action<TrackItemViewModel, IReadOnlyList<TrackItemViewModel>> CopyTabPropsToDropped { get; set; }

        // Track-list context menu actions (wired by MainViewModel to its selection-based commands).
        public Action SaveSelectedProps { get; set; }
        public Action LoadSelectedProps { get; set; }
        public Action DefaultProps { get; set; }
        public Action RemoveSelectedTracks { get; set; }

        // Wired by the view code-behind (not MainViewModel): inverts the ListView's selection,
        // which lives in the view. Invoked by the context menu and the global Ctrl+I command.
        public Action InvertSelection { get; set; }

        // Wired by MainViewModel: adds an undo item after a drag-drop reorder actually moved a track.
        public Action AfterReorder { get; set; }

        Project _project;

        public void Rebuild(Project p)
        {
            _project = p;
            Items.Clear();
            if (p?.Notes?.Tracks == null || p.Notes.Tracks.Count == 0) return;

            Items.Add(new TrackItemViewModel(p.TrackViews[0], "Global",
                Colors.Transparent, Colors.Transparent));

            var globalMat = p.GlobalTrackProps.MaterialProps;
            // Loop over views, not note-slot indices: after a track removal the model is sparse
            // (fewer views than Notes.Tracks slots, and TrackNumbers may have gaps).
            for (int i = 1; i < p.TrackViews.Count; i++)
            {
                int trackNumber = p.TrackViews[i].TrackNumber;
                string name = trackNumber + " - " + p.Notes.Tracks[trackNumber].Name;
                Color normal = TrackItemViewModel.ToWpfColor(
                    p.TrackViews[i].TrackProps.MaterialProps.GetSysColor(false, globalMat));
                Color hilited = TrackItemViewModel.ToWpfColor(
                    p.TrackViews[i].TrackProps.MaterialProps.GetSysColor(true, globalMat));
                Items.Add(new TrackItemViewModel(p.TrackViews[i], name, normal, hilited));
            }

            if (Items.Count > 0)
                SelectedItem = Items[0];
        }

        // Moves item at fromIndex to toIndex in both the display list and Project.TrackViews.
        public void Reorder(int fromIndex, int toIndex)
        {
            if (_project == null || fromIndex == toIndex || fromIndex <= 0) return;

            Items.Move(fromIndex, toIndex);

            var tv = _project.TrackViews[fromIndex];
            _project.TrackViews.RemoveAt(fromIndex);
            _project.TrackViews.Insert(toIndex, tv);
        }

        // Moves a block of items (preserving their relative order) to insertIndex in both
        // the display list and Project.TrackViews. Global (index 0) is always skipped.
        // Uses ObservableCollection.Move so the ListView keeps the rows selected (rather than
        // removing/re-adding, which would drop the selection and need restoring afterwards).
        public void ReorderMultiple(IReadOnlyList<TrackItemViewModel> draggedItems, int insertIndex)
        {
            if (_project == null) return;
            var block = draggedItems.Where(it => Items.IndexOf(it) > 0)
                                    .OrderBy(it => Items.IndexOf(it)).ToList();
            if (block.Count == 0) return;
            var moving = new HashSet<TrackItemViewModel>(block);

            // Find the first non-moving item at or after insertIndex — the "anchor" we insert
            // before; null means the block goes to the end.
            TrackItemViewModel anchor = null;
            for (int i = insertIndex; i < Items.Count; i++)
                if (!moving.Contains(Items[i])) { anchor = Items[i]; break; }

            // Move each item (in original order) to just before the anchor. Indices are
            // recomputed each step because earlier moves shift them.
            bool moved = false;
            foreach (var it in block)
            {
                int from = Items.IndexOf(it);
                int to = anchor != null
                    ? (from < Items.IndexOf(anchor) ? Items.IndexOf(anchor) - 1 : Items.IndexOf(anchor))
                    : Items.Count - 1;
                if (from == to) continue;
                Items.Move(from, to);
                var tv = _project.TrackViews[from];
                _project.TrackViews.RemoveAt(from);
                _project.TrackViews.Insert(to, tv);
                moved = true;
            }
            if (moved) AfterReorder?.Invoke();
        }

        // Refreshes the color swatches for all non-Global tracks (call after a Material copy).
        public void RefreshColors()
        {
            if (_project == null) return;
            var globalMat = _project.GlobalTrackProps.MaterialProps;
            for (int i = 1; i < Items.Count; i++)
            {
                var mp = _project.TrackViews[i].TrackProps.MaterialProps;
                Items[i].NormalColor = TrackItemViewModel.ToWpfColor(mp.GetSysColor(false, globalMat));
                Items[i].HilitedColor = TrackItemViewModel.ToWpfColor(mp.GetSysColor(true, globalMat));
            }
        }

        internal void RaiseSelectionChanged() => SelectionChanged?.Invoke();
    }
}
