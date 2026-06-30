using System.Windows.Controls;
using VisualMusic.ViewModels;

namespace VisualMusic.Controls
{
    public partial class TrackPropsView : UserControl
    {
        public TrackPropsView()
        {
            InitializeComponent();
        }

        // Right-click on the track-properties panel (tab strip, labels, background): save/load just the
        // current tab. Keyframeable property controls keep their own keyframe context menu; this one
        // surfaces wherever no more-specific menu applies.
        void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DataContext is not TrackPropsViewModel vm)
            {
                e.Handled = true;
                return;
            }

            var menu = new ContextMenu();

            var load = new MenuItem { Header = "_Load properties for current tab...", IsEnabled = vm.SelectedTrackCount >= 1 };
            load.Click += (_, _) => vm.LoadCurrentTab?.Invoke();
            menu.Items.Add(load);

            var save = new MenuItem { Header = "_Save properties for current tab...", IsEnabled = vm.SelectedTrackCount == 1 };
            save.Click += (_, _) => vm.SaveCurrentTab?.Invoke();
            menu.Items.Add(save);

            var def = new MenuItem { Header = "_Default properties", IsEnabled = vm.SelectedTrackCount >= 1 };
            def.Click += (_, _) => vm.DefaultProps?.Invoke();
            menu.Items.Add(def);

            ((Control)sender).ContextMenu = menu;
        }
    }
}
