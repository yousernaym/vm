using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisualMusic.Keyframes;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls
{
    public partial class KeyframeListView : UserControl
    {
        KeyframeListViewModel _vm;
        bool _selectingFromService;   // guards re-entrant seek when selecting programmatically

        public KeyframeListView()
        {
            InitializeComponent();
            _vm = new KeyframeListViewModel();
            DataContext = _vm;

            KeyframeService.KeyframesChanged += () =>
            {
                _vm.Project = _vm.Project; // triggers Rebuild()
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
        }

        /// <summary>Call this when a project is loaded / replaced.</summary>
        public void SetProject(Project project)
        {
            _vm.Project = project;
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
            if (grid.SelectedItem is KeyframeRowViewModel row)
                _vm.DeleteRow(row);
        }

        // ---- DataGrid events ----

        void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectingFromService) return;          // don't re-seek when selection was set by the diamond
            if (grid.SelectedItem is KeyframeRowViewModel row)
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
            // Delete key removes the selected keyframe column
            if (e.Key == Key.Delete && grid.SelectedItem is KeyframeRowViewModel row
                && grid.CurrentCell.Column?.IsReadOnly != false)
            {
                var result = MessageBox.Show(
                    "Delete this keyframe and all its property associations?",
                    "Delete keyframe",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _vm.DeleteRow(row);
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

            var props = new System.Collections.Generic.List<(string id, string display)>(_vm.PropertiesAt(row.Tick));
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
    }
}
