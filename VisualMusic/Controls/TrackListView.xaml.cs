using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls
{
    public partial class TrackListView : UserControl
    {
        Point _dragStart;
        TrackItemViewModel _dragItem;

        public TrackListView()
        {
            InitializeComponent();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is TrackListViewModel vm)
            {
                vm.SelectedItems = trackListView.SelectedItems
                    .Cast<TrackItemViewModel>().ToList();
                vm.RaiseSelectionChanged();
            }
        }

        void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
            _dragItem = HitTestItem(e.GetPosition(trackListView));
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragItem == null) return;
            Vector diff = e.GetPosition(null) - _dragStart;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            var item = _dragItem;
            _dragItem = null;
            DragDrop.DoDragDrop(trackListView,
                new DataObject(typeof(TrackItemViewModel), item),
                DragDropEffects.Move);
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(TrackItemViewModel))
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            var dragged = e.Data.GetData(typeof(TrackItemViewModel)) as TrackItemViewModel;
            var target  = HitTestItem(e.GetPosition(trackListView));
            if (dragged == null || target == null || dragged == target) return;

            if (DataContext is TrackListViewModel vm)
            {
                int from = vm.Items.IndexOf(dragged);
                int to   = vm.Items.IndexOf(target);
                if (from > 0)
                    vm.Reorder(from, to);
            }
        }

        TrackItemViewModel HitTestItem(Point pos)
        {
            var hit = VisualTreeHelper.HitTest(trackListView, pos);
            DependencyObject el = hit?.VisualHit;
            while (el != null)
            {
                if (el is ListViewItem lvi) return lvi.DataContext as TrackItemViewModel;
                el = VisualTreeHelper.GetParent(el);
            }
            return null;
        }
    }
}
