using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace VisualMusic.ViewModels
{
    public partial class TrackListViewModel : ObservableObject
    {
        public ObservableCollection<TrackItemViewModel> Items { get; } = new();

        [ObservableProperty] TrackItemViewModel selectedItem;

        // Code-behind pushes the ListView.SelectedItems here (read-only on the ListView itself).
        public IList<TrackItemViewModel> SelectedItems { get; set; } = new List<TrackItemViewModel>();

        public event Action SelectionChanged;

        Project _project;

        public void Rebuild(Project p)
        {
            _project = p;
            Items.Clear();
            if (p?.Notes?.Tracks == null || p.Notes.Tracks.Count == 0) return;

            Items.Add(new TrackItemViewModel(p.TrackViews[0], "Global",
                Colors.Transparent, Colors.Transparent));

            var globalMat = p.GlobalTrackProps.MaterialProps;
            for (int i = 1; i < p.Notes.Tracks.Count; i++)
            {
                int trackNumber = p.TrackViews[i].TrackNumber;
                string name = trackNumber + " - " + p.Notes.Tracks[trackNumber].Name;
                Color normal  = TrackItemViewModel.ToWpfColor(
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

        internal void RaiseSelectionChanged() => SelectionChanged?.Invoke();
    }
}
