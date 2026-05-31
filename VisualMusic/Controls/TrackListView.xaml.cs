using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls
{
    public partial class TrackListView : UserControl, ITrackSelectionService
    {
        // ITrackSelectionService — called from SongRenderer on the UI thread (DispatcherTimer tick).
        public int TrackListCount => trackListView.Items.Count;

        public void SetTrackSelected(int index, bool selected)
        {
            if (index < 0 || index >= trackListView.Items.Count) return;
            var item = trackListView.Items[index];
            bool isSel = trackListView.SelectedItems.Contains(item);
            if (selected && !isSel) trackListView.SelectedItems.Add(item);
            else if (!selected && isSel) trackListView.SelectedItems.Remove(item);
        }

        const string DragFormat = "VisualMusic.TrackItemList";

        Point _dragStart;
        TrackItemViewModel _dragItem;
        bool _deferSelectionToMouseUp;   // Explorer-style: apply a click on a selected row on mouse-up
        DragAdorner _dragAdorner;
        int _dropIndex = -1;
        TrackItemViewModel _ctrlDropTarget;

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
            _deferSelectionToMouseUp = false;

            // Explorer-style: clicking an already-selected row must not change the selection on
            // mouse-down — defer it to mouse-up so the user can drag the whole selection and see
            // it highlighted while dragging. Without Ctrl a plain click keeps the other selected
            // rows; with Ctrl the clicked row isn't toggled off. (Shift = range select, left alone.)
            bool ctrl  = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool itemSelected = _dragItem != null && trackListView.SelectedItems.Contains(_dragItem);
            if (!shift && itemSelected && (ctrl || trackListView.SelectedItems.Count > 1))
            {
                _deferSelectionToMouseUp = true;
                e.Handled = true;
            }
        }

        void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_deferSelectionToMouseUp) return;
            _deferSelectionToMouseUp = false;

            // We get here only when no drag started (a drag consumes the mouse-up), so apply the
            // click that was suppressed on mouse-down.
            var item = HitTestItem(e.GetPosition(trackListView));
            if (item == null) return;

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                // Ctrl+click toggles just the clicked row.
                if (trackListView.SelectedItems.Contains(item))
                    trackListView.SelectedItems.Remove(item);
                else
                    trackListView.SelectedItems.Add(item);
            }
            else
            {
                // Plain click collapses the selection to the clicked row.
                trackListView.SelectedItems.Clear();
                trackListView.SelectedItem = item;
            }
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragItem == null) return;
            Vector diff = e.GetPosition(null) - _dragStart;
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            var item = _dragItem;
            _dragItem = null;
            _deferSelectionToMouseUp = false;   // a drag is starting; don't collapse on mouse-up

            if (!(DataContext is TrackListViewModel vm)) return;

            // Drag all selected tracks if the grabbed row is among them, else just that row.
            // Selection is intact here (a click on a selected row was suppressed on mouse-down).
            // Global (index 0) is always pinned and excluded.
            var selected = trackListView.SelectedItems.Cast<TrackItemViewModel>().ToList();
            var dragged = selected.Contains(item)
                ? selected
                : new List<TrackItemViewModel> { item };
            dragged = dragged.Where(d => vm.Items.IndexOf(d) > 0).ToList();
            if (dragged.Count == 0) return;

            DragDrop.DoDragDrop(trackListView,
                new DataObject(DragFormat, dragged),
                DragDropEffects.Move);

            // DoDragDrop is synchronous; clean up in case OnDrop/OnDragLeave didn't fire
            RemoveAdorner();
            _dropIndex = -1;
            _ctrlDropTarget = null;
        }

        void OnDragEnter(object sender, DragEventArgs e)
        {
            // Set effects immediately on enter to prevent the brief "no-drop" cursor flash
            // that occurs during the DragLeave→DragOver transition between items.
            e.Effects = e.Data.GetDataPresent(DragFormat)
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DragFormat))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;

            EnsureAdorner();

            bool isCtrl = (e.KeyStates & DragDropKeyStates.ControlKey) != 0;
            var pos = e.GetPosition(trackListView);

            if (isCtrl)
            {
                _dropIndex = -1;
                // Use nearest item so the box stays stable when the cursor is in the
                // pixel gap between two rows (where hit-testing finds no ListViewItem).
                var container = HitTestListViewItem(pos) ?? FindNearestListViewItem(pos);
                if (container != null)
                {
                    _ctrlDropTarget = container.DataContext as TrackItemViewModel;
                    var itemPos = container.TransformToAncestor(trackListView).Transform(new Point(0, 0));
                    _dragAdorner?.ShowTargetBox(new Rect(0, itemPos.Y, trackListView.ActualWidth, container.ActualHeight));
                }
            }
            else
            {
                _ctrlDropTarget = null;
                var (index, lineY) = GetInsertionPoint(pos);
                _dropIndex = index;
                _dragAdorner?.ShowInsertLine(lineY);
            }
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            // DragLeave bubbles from child elements (e.g. between ListViewItems) — ignore
            // those by checking if the mouse is still within the ListView's bounds.
            var pos = e.GetPosition(trackListView);
            if (pos.X >= 0 && pos.Y >= 0 && pos.X <= trackListView.ActualWidth && pos.Y <= trackListView.ActualHeight)
                return;
            RemoveAdorner();
            _dropIndex = -1;
            _ctrlDropTarget = null;
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            int dropIndex = _dropIndex;
            var ctrlTarget = _ctrlDropTarget;
            RemoveAdorner();
            _dropIndex = -1;
            _ctrlDropTarget = null;

            var dragged = e.Data.GetData(DragFormat) as List<TrackItemViewModel>;
            if (dragged == null || dragged.Count == 0 || !(DataContext is TrackListViewModel vm)) return;

            bool isCtrl = (e.KeyStates & DragDropKeyStates.ControlKey) != 0;

            if (isCtrl)
            {
                // COPY: the drop-target row is the property source; dragged items are destinations.
                // Prefer the item directly under the cursor; fall back to the last item
                // the box was highlighting (covers drops in the gap between rows).
                var target = HitTestItem(e.GetPosition(trackListView)) ?? ctrlTarget;
                if (target != null)
                    vm.CopyTabPropsToDropped?.Invoke(target, dragged);
            }
            else
            {
                // MOVE: relocate the whole block to the insertion point.
                if (dropIndex < 1) return;
                vm.ReorderMultiple(dragged, dropIndex);
            }
            // No re-selection needed: the dragged rows stay selected throughout (Items.Move
            // preserves selection, and Ctrl-copy doesn't touch it).
        }

        void EnsureAdorner()
        {
            if (_dragAdorner != null) return;
            var layer = AdornerLayer.GetAdornerLayer(trackListView);
            if (layer == null) return;
            _dragAdorner = new DragAdorner(trackListView);
            layer.Add(_dragAdorner);
        }

        void RemoveAdorner()
        {
            if (_dragAdorner == null) return;
            AdornerLayer.GetAdornerLayer(trackListView)?.Remove(_dragAdorner);
            _dragAdorner = null;
        }

        // Returns the insertion index and the Y coordinate where the line should appear.
        // insertIndex is the "insert before this index" position in the current Items list.
        (int insertIndex, double lineY) GetInsertionPoint(Point pos)
        {
            if (!(DataContext is TrackListViewModel vm)) return (1, 0);
            var items = vm.Items;
            if (items.Count == 0) return (1, 0);

            for (int i = 0; i < items.Count; i++)
            {
                if (trackListView.ItemContainerGenerator.ContainerFromItem(items[i]) is not ListViewItem container)
                    continue;

                var itemPos = container.TransformToAncestor(trackListView).Transform(new Point(0, 0));

                if (pos.Y < itemPos.Y + container.ActualHeight / 2)
                {
                    // Insert before item[i], but never before Global (index 0)
                    int insertAt = Math.Max(1, i);
                    if (trackListView.ItemContainerGenerator.ContainerFromItem(items[insertAt]) is not ListViewItem lineContainer)
                        return (insertAt, itemPos.Y);
                    var linePos = lineContainer.TransformToAncestor(trackListView).Transform(new Point(0, 0));
                    return (insertAt, linePos.Y);
                }
            }

            // Mouse is below all items — append at end
            if (trackListView.ItemContainerGenerator.ContainerFromItem(items[items.Count - 1]) is ListViewItem lastContainer)
            {
                var lastPos = lastContainer.TransformToAncestor(trackListView).Transform(new Point(0, lastContainer.ActualHeight));
                return (items.Count, lastPos.Y);
            }
            return (items.Count, trackListView.ActualHeight);
        }

        ListViewItem HitTestListViewItem(Point pos)
        {
            DependencyObject el = VisualTreeHelper.HitTest(trackListView, pos)?.VisualHit;
            while (el != null)
            {
                if (el is ListViewItem lvi) return lvi;
                el = VisualTreeHelper.GetParent(el);
            }
            return null;
        }

        // Returns the ListViewItem whose vertical extent is closest to pos.Y.
        // Used in Ctrl mode to keep the target box stable when the cursor is in the
        // pixel gap between rows where no ListViewItem is directly hit.
        ListViewItem FindNearestListViewItem(Point pos)
        {
            if (!(DataContext is TrackListViewModel vm)) return null;
            ListViewItem nearest = null;
            double minDist = double.MaxValue;
            foreach (var item in vm.Items)
            {
                if (trackListView.ItemContainerGenerator.ContainerFromItem(item) is not ListViewItem container)
                    continue;
                var itemPos = container.TransformToAncestor(trackListView).Transform(new Point(0, 0));
                double top = itemPos.Y, bottom = itemPos.Y + container.ActualHeight;
                double dist = pos.Y < top ? top - pos.Y : pos.Y > bottom ? pos.Y - bottom : 0;
                if (dist < minDist) { minDist = dist; nearest = container; }
            }
            return nearest;
        }

        TrackItemViewModel HitTestItem(Point pos) =>
            HitTestListViewItem(pos)?.DataContext as TrackItemViewModel;
    }

    class DragAdorner : Adorner
    {
        enum Mode { None, InsertLine, TargetBox }

        Mode _mode = Mode.None;
        double _lineY;
        Rect _boxRect;

        static readonly Pen LinePen;
        static readonly Pen BoxPen;
        static readonly SolidColorBrush BoxFill;

        static DragAdorner()
        {
            LinePen = new Pen(new SolidColorBrush(Color.FromRgb(51, 153, 255)), 2);
            LinePen.Freeze();
            BoxPen = new Pen(new SolidColorBrush(Color.FromRgb(51, 153, 255)), 2);
            BoxPen.Freeze();
            BoxFill = new SolidColorBrush(Color.FromArgb(40, 51, 153, 255));
            BoxFill.Freeze();
        }

        public DragAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsHitTestVisible = false;
        }

        public void ShowInsertLine(double y)
        {
            _mode = Mode.InsertLine;
            _lineY = y;
            InvalidateVisual();
        }

        public void ShowTargetBox(Rect rect)
        {
            _mode = Mode.TargetBox;
            _boxRect = rect;
            InvalidateVisual();
        }

        public void Hide()
        {
            _mode = Mode.None;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            double width = AdornedElement.RenderSize.Width;
            switch (_mode)
            {
                case Mode.InsertLine:
                    dc.DrawLine(LinePen, new Point(0, _lineY), new Point(width, _lineY));
                    break;
                case Mode.TargetBox:
                    dc.DrawRectangle(BoxFill, BoxPen, _boxRect);
                    break;
            }
        }
    }
}
