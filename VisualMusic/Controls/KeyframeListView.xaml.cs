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

        public KeyframeListView()
        {
            InitializeComponent();
            _vm = new KeyframeListViewModel();
            DataContext = _vm;

            KeyframeService.KeyframesChanged += () =>
            {
                _vm.Project = _vm.Project; // triggers Rebuild()
            };
        }

        /// <summary>Call this when a project is loaded / replaced.</summary>
        public void SetProject(Project project)
        {
            _vm.Project = project;
        }

        // ---- DataGrid events ----

        void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            if (colHeader == "Time")
                _vm.CommitTimeEdit(row, tb.Text);
            else if (colHeader == "Description")
                _vm.CommitDescriptionEdit(row, tb.Text);
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
    }
}
