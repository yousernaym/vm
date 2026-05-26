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
    public partial class TrackListView : UserControl
    {
        Point _dragStart;
        TrackItemViewModel _dragItem;
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

            // DoDragDrop is synchronous; clean up in case OnDrop/OnDragLeave didn't fire
            RemoveAdorner();
            _dropIndex = -1;
            _ctrlDropTarget = null;
        }

        void OnDragEnter(object sender, DragEventArgs e)
        {
            // Set effects immediately on enter to prevent the brief "no-drop" cursor flash
            // that occurs during the DragLeave→DragOver transition between items.
            e.Effects = e.Data.GetDataPresent(typeof(TrackItemViewModel))
                ? DragDropEffects.Move
                : DragDropEffects.None;
            e.Handled = true;
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TrackItemViewModel)))
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

            var dragged = e.Data.GetData(typeof(TrackItemViewModel)) as TrackItemViewModel;
            if (dragged == null || !(DataContext is TrackListViewModel vm)) return;

            bool isCtrl = (e.KeyStates & DragDropKeyStates.ControlKey) != 0;

            if (isCtrl)
            {
                // Prefer the item directly under the cursor; fall back to the last item
                // the box was highlighting (covers drops in the gap between rows).
                var target = HitTestItem(e.GetPosition(trackListView)) ?? ctrlTarget;
                if (target != null && dragged != target)
                {
                    int from = vm.Items.IndexOf(dragged);
                    int to   = vm.Items.IndexOf(target);
                    if (from > 0)
                        vm.Reorder(from, to);
                }
            }
            else
            {
                int from = vm.Items.IndexOf(dragged);
                if (from <= 0 || dropIndex < 1) return;
                // Items.Move(from, to) removes the item then inserts at to (index in reduced list)
                int to = from < dropIndex ? dropIndex - 1 : dropIndex;
                if (from != to)
                    vm.Reorder(from, to);
            }
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
