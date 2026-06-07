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
                _vm.Project = _vm.Project; // triggers Rebuild() (properties + rows)
                UpdateFilterBoxText();     // reflect the committed selection (if any)
            };

            // A diamond click (or any tick pick) selects the matching row.
            KeyframeService.TickSelected += tick =>
            {
                var row = _vm.FindRow(tick);
                if (row == null) return;
                _selectingFromService = true;
                try { grid.SelectedItem = row; grid.ScrollIntoView(row); }
                finally { _selectingFromService = false; }
            };

            // A property's context menu requested the list be filtered to that property.
            KeyframeService.FilterByPropertyRequested += fullId =>
            {
                var item = _vm.Properties.FirstOrDefault(p => p.Id == fullId);
                if (item != null) CommitSelection(item);
            };
        }

        /// <summary>Call this when a project is loaded / replaced.</summary>
        public void SetProject(Project project)
        {
            _vm.Project = project;
            UpdateFilterBoxText();
        }

        // ---- Header buttons ----

        void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            int tick = _vm.AddEmptyAtPlayhead();
            // Select the newly added (or existing) row at that tick
            var row = _vm.FindRow(tick);
            if (row != null) { grid.SelectedItem = row; grid.ScrollIntoView(row); }
        }

        void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = grid.SelectedItems.OfType<KeyframeRowViewModel>().ToList();
            if (selected.Count == 0) return;

            string msg = BuildDeleteMessage(selected.Count);
            var result = MessageBox.Show(msg, "Delete keyframe(s)",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            _vm.DeleteRows(selected);
            grid.UnselectAll();
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
            else if (!filterPopup.IsOpen)
            {
                OpenFilterPopupDeferred();
            }
        }

        void FilterBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            filterBox.SelectAll();   // typing replaces the shown property name
            OpenFilterPopupDeferred();
        }

        /// <summary>
        /// Opens the popup after the current input (the click that triggered it) has finished.
        /// Opening a StaysOpen=False popup during the mouse-down makes the following mouse-up look
        /// like an outside click, which would close it again immediately.
        /// </summary>
        void OpenFilterPopupDeferred()
        {
            Dispatcher.BeginInvoke(
                new Action(OpenFilterPopup),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        void FilterBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
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

        // ---- DataGrid events ----

        void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            if (colHeader == "Beat")
                // Defer: committing a time edit moves the column and rebuilds the list, which can't
                // happen while the DataGrid is still finishing this edit.
                Dispatcher.BeginInvoke(new System.Action(() => _vm.CommitTimeEdit(row, text)));
            else if (colHeader == "Description")
                _vm.CommitDescriptionEdit(row, text);
        }

        void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl-A: select all (DataGrid handles this natively with Extended mode, but handle
            // the case where focus is in a read-only cell so it doesn't start an edit)
            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) != 0
                && grid.CurrentCell.Column?.IsReadOnly != false)
            {
                grid.SelectAll();
                e.Handled = true;
                return;
            }

            // Delete key removes the selected keyframe row(s)
            if (e.Key == Key.Delete && grid.CurrentCell.Column?.IsReadOnly != false)
            {
                var selected = grid.SelectedItems.OfType<KeyframeRowViewModel>().ToList();
                if (selected.Count == 0) return;

                string msg = BuildDeleteMessage(selected.Count);
                var result = MessageBox.Show(msg, "Delete keyframe(s)",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _vm.DeleteRows(selected);
                    grid.UnselectAll();
                    e.Handled = true;
                }
            }
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
