using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VisualMusic.Keyframes;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls
{
    public partial class KeyframeListView : UserControl
    {
        KeyframeListViewModel _vm;
        bool _selectingFromService;   // guards re-entrant seek when selecting programmatically
        bool _preservingEditSelection; // guards DataGrid's post-edit navigation until we restore the row
        bool _selectingLyricFromCode;  // guards re-entrant seek when selecting lyric rows programmatically
        bool _preservingLyricEditSelection; // guards lyric grid post-edit navigation
        bool _settingTextFromCode;    // guards FilterBox_TextChanged when we set the text in code
        ICollectionView _propsView;   // filtered view over _vm.Properties for the search popup

        public KeyframeListView()
        {
            InitializeComponent();
            _vm = new KeyframeListViewModel();
            DataContext = _vm;

            _propsView = CollectionViewSource.GetDefaultView(_vm.Properties);

            KeyframeService.KeyframesChanged += () =>
            {
                RebuildPreservingSelection();
                RebuildLyricsPreservingSelection();
                UpdateFilterBoxText();     // reflect the committed selection (if any)
            };

            // A diamond click (or any tick pick) selects the matching row.
            KeyframeService.TickSelected += tick =>
            {
                if (_vm.FindRow(tick) == null) return;
                bool wasSelecting = _selectingFromService;
                _selectingFromService = true;
                try { SelectSingleRowAtTick(tick); }
                finally { _selectingFromService = wasSelecting; }
            };

            // A property's context menu requested the list be filtered to that property.
            KeyframeService.FilterByPropertyRequested += fullId =>
            {
                var item = _vm.Properties.FirstOrDefault(p => p.Id == fullId);
                if (item != null) CommitSelection(item);
            };

            // Close the filter popup on any click elsewhere in the window (incl. non-focusable
            // empty areas that never raise LostKeyboardFocus).
            Loaded += (_, _) =>
            {
                var w = Window.GetWindow(this);
                if (w != null)
                {
                    w.PreviewMouseDown -= Window_PreviewMouseDown;
                    w.PreviewMouseDown += Window_PreviewMouseDown;
                }
            };
            Unloaded += (_, _) =>
            {
                var w = Window.GetWindow(this);
                if (w != null) w.PreviewMouseDown -= Window_PreviewMouseDown;
            };
        }

        void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!filterPopup.IsOpen) return;
            if (filterList.IsMouseOver) return;   // clicking inside the popup list
            // Clicks inside the search box are handled by its own (toggle) handler.
            if (IsDescendantOf(e.OriginalSource as DependencyObject, filterBox)) return;

            filterPopup.IsOpen = false;
            UpdateFilterBoxText();
        }

        /// <summary>Call this when a project is loaded / replaced.</summary>
        public void SetProject(Project project)
        {
            _vm.Project = project;
            UpdateFilterBoxText();
        }

        // ---- Filter search box ----

        void FilterBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!filterBox.IsKeyboardFocusWithin)
            {
                // First click: focus the box (GotKeyboardFocus selects-all + opens the popup).
                e.Handled = true;
                filterBox.Focus();
            }
            else if (filterPopup.IsOpen)
            {
                // Already focused and open → clicking again closes it.
                filterPopup.IsOpen = false;
                UpdateFilterBoxText();
            }
            else
            {
                OpenFilterPopup();
            }
        }

        void FilterBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            filterBox.SelectAll();   // typing replaces the shown property name
            OpenFilterPopup();
        }

        void FilterBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Defer the close: clicking a popup item momentarily steals focus from the search box,
            // and we must let the item-click commit (on mouse-up) run first. A genuine outside click
            // (e.g. the render surface) leaves no commit, so the deferred close still fires.
            Dispatcher.BeginInvoke(
                new Action(CloseFilterPopupIfIdle),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        void CloseFilterPopupIfIdle()
        {
            if (!filterPopup.IsOpen) return;
            if (filterList.IsMouseOver) return;            // still interacting with the list
            if (filterBox.IsKeyboardFocusWithin) return;   // focus returned to the search box
            filterPopup.IsOpen = false;
            // Discard any uncommitted search text by restoring the committed selection's label.
            UpdateFilterBoxText();
        }

        void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settingTextFromCode) return;

            string q = filterBox.Text ?? "";
            ApplyPropsFilter(q);

            // While typing, always highlight the top item so Enter commits the best match.
            if (filterList.Items.Count > 0)
            {
                filterList.SelectedIndex = 0;
                filterList.ScrollIntoView(filterList.SelectedItem);
            }
            filterPopup.IsOpen = filterList.Items.Count > 0;
        }

        void FilterBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (!filterPopup.IsOpen) OpenFilterPopup();
                    else MoveHighlight(+1);
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (filterPopup.IsOpen) MoveHighlight(-1);
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (filterPopup.IsOpen && filterList.SelectedItem is KeyframePropertyViewModel item)
                    {
                        CommitSelection(item);
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    if (filterPopup.IsOpen)
                    {
                        filterPopup.IsOpen = false;
                        UpdateFilterBoxText();
                        e.Handled = true;
                    }
                    break;
            }
        }

        void FilterList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var lbi = FindAncestor<ListBoxItem>(e.OriginalSource as DependencyObject);
            if (lbi?.DataContext is KeyframePropertyViewModel item)
                CommitSelection(item);
        }

        void ClearFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            CommitSelection(null);
        }

        // ---- Filter helpers ----

        void OpenFilterPopup()
        {
            ApplyPropsFilter(null);   // show all properties
            // Highlight the committed selection, or the top item if none.
            var id = _vm.PropertyFilterId;
            var target = id != null ? _vm.Properties.FirstOrDefault(p => p.Id == id) : null;
            filterList.SelectedItem = target;
            if (filterList.SelectedItem == null && filterList.Items.Count > 0)
                filterList.SelectedIndex = 0;
            filterList.ScrollIntoView(filterList.SelectedItem);

            filterPopup.IsOpen = filterList.Items.Count > 0;
        }

        void ApplyPropsFilter(string query)
        {
            if (_propsView == null) return;
            if (string.IsNullOrWhiteSpace(query))
                _propsView.Filter = null;
            else
                _propsView.Filter = o => o is KeyframePropertyViewModel p &&
                    p.Display.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void MoveHighlight(int delta)
        {
            int n = filterList.Items.Count;
            if (n == 0) return;
            int i = filterList.SelectedIndex < 0 ? 0 : filterList.SelectedIndex + delta;
            filterList.SelectedIndex = Math.Max(0, Math.Min(n - 1, i));
            filterList.ScrollIntoView(filterList.SelectedItem);
        }

        /// <summary>Commits a property choice (null = show all), updating rows, text and popup.</summary>
        void CommitSelection(KeyframePropertyViewModel item)
        {
            filterPopup.IsOpen = false;
            ApplyPropsFilter(null);
            _vm.SetPropertyFilter(item?.Id);      // rebuilds rows only
            SetFilterBoxText(item?.Display ?? "");
            filterList.SelectedItem = item;
        }

        void SetFilterBoxText(string text)
        {
            _settingTextFromCode = true;
            try { filterBox.Text = text; filterBox.CaretIndex = text.Length; }
            finally { _settingTextFromCode = false; }
        }

        /// <summary>Restores the search box to the committed selection's label.</summary>
        void UpdateFilterBoxText()
        {
            var id = _vm.PropertyFilterId;
            string disp = id != null
                ? (_vm.Properties.FirstOrDefault(p => p.Id == id)?.Display ?? "")
                : "";
            SetFilterBoxText(disp);
        }

        static T FindAncestor<T>(DependencyObject d) where T : DependencyObject
        {
            while (d != null && d is not T)
                d = VisualTreeHelper.GetParent(d);
            return d as T;
        }

        static bool IsDescendantOf(DependencyObject node, DependencyObject ancestor)
        {
            while (node != null)
            {
                if (ReferenceEquals(node, ancestor)) return true;
                node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
            }
            return false;
        }

        // ---- DataGrid events ----

        void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_preservingEditSelection) return; // ignore transient moves while an edit is finishing
            if (_selectingFromService) return;   // don't re-seek when set by the diamond
            // Only seek when exactly one row is selected (avoid spurious seeks during multi-select)
            if (grid.SelectedItems.Count == 1 && grid.SelectedItem is KeyframeRowViewModel row)
                _vm.SeekToRow(row);
        }

        void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            if (e.Row.Item is not KeyframeRowViewModel row) return;

            var tb = e.EditingElement as TextBox;
            if (tb == null) return;

            string colHeader = (e.Column.Header as string) ?? "";
            string text = tb.Text;
            int originalTick = row.Tick;

            if (colHeader == "Beat")
            {
                // Defer: committing a time edit moves the column and rebuilds the list, which can't
                // happen while the DataGrid is still finishing this edit. Restore one dispatcher
                // turn later so WPF's Enter/tab navigation cannot leave selection on the next row.
                BeginEditSelectionRestore(() => _vm.CommitTimeEdit(row, text) ?? originalTick);
            }
            else if (colHeader == "Description")
            {
                BeginEditSelectionRestore(() =>
                {
                    _vm.CommitDescriptionEdit(row, text);
                    return originalTick;
                });
            }
        }

        void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl-A: select all (DataGrid handles this natively with Extended mode, but handle
            // the case where focus is on the grid itself so it doesn't start an edit)
            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) != 0
                && !IsGridTextEditing())
            {
                grid.SelectAll();
                e.Handled = true;
                return;
            }

            // Delete key removes selected row(s) when the grid has focus. If a cell TextBox is
            // actively editing, leave Delete to the editor so normal text editing still works.
            if (e.Key == Key.Delete && !IsGridTextEditing())
            {
                if (DeleteSelectedRows())
                    e.Handled = true;
            }
        }

        void LyricsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_preservingLyricEditSelection) return;
            if (_selectingLyricFromCode) return;
            if (lyricsGrid.SelectedItems.Count == 1 && lyricsGrid.SelectedItem is LyricsRowViewModel row)
                _vm.Lyrics.SeekToRow(row);
        }

        void LyricsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            if (e.Row.Item is not LyricsRowViewModel row) return;

            var tb = e.EditingElement as TextBox;
            if (tb == null) return;

            string colHeader = (e.Column.Header as string) ?? "";
            string text = tb.Text;

            if (colHeader == "Beat")
            {
                BeginLyricEditSelectionRestore(() => _vm.Lyrics.CommitBeatEdit(row, text));
            }
            else if (colHeader == "Text")
            {
                BeginLyricEditSelectionRestore(() => _vm.Lyrics.CommitTextEdit(row, text));
            }
        }

        void LyricsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) != 0
                && !IsGridTextEditing(lyricsGrid))
            {
                lyricsGrid.SelectAll();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete && !IsGridTextEditing(lyricsGrid))
            {
                if (DeleteSelectedLyricsRows())
                    e.Handled = true;
            }
        }

        void LyricsGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (lyricsGrid.SelectedItem is not LyricsRowViewModel row)
            {
                e.Handled = true;
                return;
            }

            var menu = new ContextMenu();
            var remove = new MenuItem { Header = "_Delete lyric" };
            remove.Click += (_, _) => _vm.Lyrics.DeleteRow(row);
            menu.Items.Add(remove);

            lyricsGrid.ContextMenu = menu;
        }

        // ---- Row context menu (built dynamically per row) ----

        void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (grid.SelectedItem is not KeyframeRowViewModel row)
            {
                e.Handled = true;   // nothing selected → suppress menu
                return;
            }

            var menu = new ContextMenu();

            var removeAll = new MenuItem { Header = "_Remove keyframe (all properties)" };
            removeAll.Click += (_, _) => _vm.DeleteRow(row);
            menu.Items.Add(removeAll);

            var props = new List<(string id, string display)>(_vm.PropertiesAt(row.Tick));
            if (props.Count > 0)
            {
                menu.Items.Add(new Separator());
                foreach (var (id, display) in props)
                {
                    var capturedId = id;
                    var item = new MenuItem { Header = $"Remove \"{display}\"" };
                    item.Click += (_, _) => _vm.RemoveProperty(row, capturedId);
                    menu.Items.Add(item);
                }
            }

            grid.ContextMenu = menu;
        }

        // ---- Helpers ----

        void RebuildPreservingSelection()
        {
            var selectedTicks = grid.SelectedItems
                .OfType<KeyframeRowViewModel>()
                .Select(r => r.Tick)
                .Distinct()
                .ToList();

            bool wasSelecting = _selectingFromService;
            _selectingFromService = true;
            try
            {
                _vm.Rebuild();
                RestoreSelectionByTicks(selectedTicks);
            }
            finally
            {
                _selectingFromService = wasSelecting;
            }
        }

        void RebuildLyricsPreservingSelection()
        {
            var selectedSegments = lyricsGrid.SelectedItems
                .OfType<LyricsRowViewModel>()
                .Select(r => r.Segment)
                .Distinct()
                .ToList();

            bool wasSelecting = _selectingLyricFromCode;
            _selectingLyricFromCode = true;
            try
            {
                _vm.Lyrics.Project = _vm.Project;
                RestoreLyricsSelectionBySegments(selectedSegments);
            }
            finally
            {
                _selectingLyricFromCode = wasSelecting;
            }
        }

        void RestoreSelectionByTicks(IEnumerable<int> ticks)
        {
            grid.UnselectAll();

            KeyframeRowViewModel first = null;
            foreach (int tick in ticks)
            {
                var row = _vm.FindRow(tick);
                if (row == null) continue;
                grid.SelectedItems.Add(row);
                first ??= row;
            }

            if (first == null)
            {
                grid.CurrentCell = default(DataGridCellInfo);
                return;
            }

            grid.CurrentItem = first;
            if (grid.Columns.Count > 0)
                grid.CurrentCell = new DataGridCellInfo(first, grid.Columns[0]);
            grid.ScrollIntoView(first);
        }

        void SelectSingleRowAtTick(int tick)
        {
            var row = _vm.FindRow(tick);
            if (row == null)
            {
                grid.UnselectAll();
                grid.CurrentCell = default(DataGridCellInfo);
                return;
            }

            grid.UnselectAll();
            grid.SelectedItem = row;
            grid.CurrentItem = row;
            if (grid.Columns.Count > 0)
                grid.CurrentCell = new DataGridCellInfo(row, grid.Columns[0]);
            grid.ScrollIntoView(row);
        }

        void RestoreLyricsSelectionBySegments(IEnumerable<LyricsSegment> segments)
        {
            lyricsGrid.UnselectAll();

            LyricsRowViewModel first = null;
            foreach (var segment in segments)
            {
                var row = _vm.Lyrics.FindRow(segment);
                if (row == null) continue;
                lyricsGrid.SelectedItems.Add(row);
                first ??= row;
            }

            if (first == null)
            {
                lyricsGrid.CurrentCell = default(DataGridCellInfo);
                return;
            }

            lyricsGrid.CurrentItem = first;
            if (lyricsGrid.Columns.Count > 0)
                lyricsGrid.CurrentCell = new DataGridCellInfo(first, lyricsGrid.Columns[0]);
            lyricsGrid.ScrollIntoView(first);
        }

        void SelectSingleLyricsRow(LyricsSegment segment)
        {
            var row = _vm.Lyrics.FindRow(segment);
            if (row == null)
            {
                lyricsGrid.UnselectAll();
                lyricsGrid.CurrentCell = default(DataGridCellInfo);
                return;
            }

            lyricsGrid.UnselectAll();
            lyricsGrid.SelectedItem = row;
            lyricsGrid.CurrentItem = row;
            if (lyricsGrid.Columns.Count > 0)
                lyricsGrid.CurrentCell = new DataGridCellInfo(row, lyricsGrid.Columns[0]);
            lyricsGrid.ScrollIntoView(row);
        }

        void BeginEditSelectionRestore(Func<int> commitEdit)
        {
            _preservingEditSelection = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                int targetTick;
                try
                {
                    targetTick = commitEdit();
                }
                catch
                {
                    _preservingEditSelection = false;
                    throw;
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    bool wasSelecting = _selectingFromService;
                    _selectingFromService = true;
                    try
                    {
                        SelectSingleRowAtTick(targetTick);
                    }
                    finally
                    {
                        _selectingFromService = wasSelecting;
                        _preservingEditSelection = false;
                    }
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        void BeginLyricEditSelectionRestore(Func<LyricsSegment> commitEdit)
        {
            _preservingLyricEditSelection = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                LyricsSegment target;
                try
                {
                    target = commitEdit();
                }
                catch
                {
                    _preservingLyricEditSelection = false;
                    throw;
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    bool wasSelecting = _selectingLyricFromCode;
                    _selectingLyricFromCode = true;
                    try
                    {
                        SelectSingleLyricsRow(target);
                    }
                    finally
                    {
                        _selectingLyricFromCode = wasSelecting;
                        _preservingLyricEditSelection = false;
                    }
                }), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        bool DeleteSelectedRows()
        {
            var selected = grid.SelectedItems.OfType<KeyframeRowViewModel>().ToList();
            if (selected.Count == 0) return false;

            string msg = BuildDeleteMessage(selected.Count);
            var result = MessageBox.Show(msg, "Delete keyframe(s)",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.OK) return true;

            _vm.DeleteRows(selected);
            grid.UnselectAll();
            return true;
        }

        bool DeleteSelectedLyricsRows()
        {
            var selected = lyricsGrid.SelectedItems.OfType<LyricsRowViewModel>().ToList();
            if (selected.Count == 0) return false;

            string msg = selected.Count == 1
                ? "Delete this lyric?"
                : $"Delete {selected.Count} selected lyrics?";
            var result = MessageBox.Show(msg, "Delete lyric(s)",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.OK) return true;

            _vm.Lyrics.DeleteRows(selected);
            lyricsGrid.UnselectAll();
            return true;
        }

        bool IsGridTextEditing()
            => IsGridTextEditing(grid);

        bool IsGridTextEditing(DataGrid ownerGrid)
        {
            if (Keyboard.FocusedElement is not DependencyObject focused) return false;
            return IsDescendantOf(focused, ownerGrid) && FindAncestor<TextBox>(focused) != null;
        }

        string BuildDeleteMessage(int count)
        {
            string filterId = _vm.PropertyFilterId;
            if (filterId != null)
            {
                string propDisplay = KeyframeService.GetDisplayNameForId(filterId);
                return count == 1
                    ? $"Remove the \"{propDisplay}\" keyframe at this position?"
                    : $"Remove the \"{propDisplay}\" keyframe from {count} selected position(s)?";
            }
            else
            {
                return count == 1
                    ? "Delete this keyframe and all its property associations?"
                    : $"Delete {count} selected keyframes and all their property associations?";
            }
        }
    }
}
