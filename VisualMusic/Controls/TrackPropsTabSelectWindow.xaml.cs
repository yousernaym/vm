using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Lets the user pick which track-property tabs to include when saving or loading a .tp file.
    /// Each checkbox maps to a <see cref="TrackPropsType"/> flag. Boxes whose tab is absent from
    /// <c>availableFlags</c> are disabled (used on load: tabs not stored in the file can't be applied).
    /// </summary>
    public partial class TrackPropsTabSelectWindow : MetroWindow
    {
        public int SelectedFlags { get; private set; }

        readonly (CheckBox box, int flag)[] _boxes;

        public TrackPropsTabSelectWindow(string title, int availableFlags, int checkedFlags)
        {
            InitializeComponent();

            Title = title;
            _boxes = new[]
            {
                (cbStyle,    (int)TrackPropsType.TPT_Style),
                (cbMaterial, (int)TrackPropsType.TPT_Material),
                (cbLight,    (int)TrackPropsType.TPT_Light),
                (cbSpatial,  (int)TrackPropsType.TPT_Spatial),
                (cbAudio,    (int)TrackPropsType.TPT_Audio),
            };

            foreach (var (box, flag) in _boxes)
            {
                bool available = (availableFlags & flag) != 0;
                box.IsEnabled = available;
                box.IsChecked = available && (checkedFlags & flag) != 0;
            }
        }

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            int flags = 0;
            foreach (var (box, flag) in _boxes)
                if (box.IsChecked == true)
                    flags |= flag;
            SelectedFlags = flags;
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
