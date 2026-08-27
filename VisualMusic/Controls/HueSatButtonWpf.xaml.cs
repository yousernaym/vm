using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic.Controls
{
    public partial class HueSatButtonWpf : UserControl
    {
        /// <summary>
        /// Raised once when the user confirms a new color in the picker (OK). Live preview while
        /// dragging does not raise this — keyframing/undo treat it as the commit, like a slider's
        /// <c>CommitChanges</c>.
        /// </summary>
        public event EventHandler SelectionChanged;

        public HueSatButtonWpf()
        {
            InitializeComponent();
        }

        // ---- DependencyProperty: SelectedXnaColor ----

        public static readonly DependencyProperty SelectedXnaColorProperty = DependencyProperty.Register(
            nameof(SelectedXnaColor), typeof(XnaColor?), typeof(HueSatButtonWpf),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public XnaColor? SelectedXnaColor
        {
            get => (XnaColor?)GetValue(SelectedXnaColorProperty);
            set => SetValue(SelectedXnaColorProperty, value);
        }

        static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((HueSatButtonWpf)d).UpdateSwatch();

        void UpdateSwatch()
        {
            if (SelectedXnaColor == null)
            {
                colorRect.Fill = Brushes.Transparent;
                return;
            }
            var c = SelectedXnaColor.Value;
            colorRect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(c.R, c.G, c.B));
        }

        // ---- Button click: open dialog ----

        void Button_Click(object sender, RoutedEventArgs e)
        {
            var original = SelectedXnaColor;
            var win = new HueSatWindow { Owner = Window.GetWindow(this) };

            if (original is XnaColor xna)
            {
                HueSatConvert.FromRgb(xna.R, xna.G, xna.B, out float hue, out float sat);
                win.Hue = hue;
                win.Saturation = sat;
            }

            // Live preview only — updates the bound color (and 3D view) without an undo snapshot.
            bool pickerMoved = false;
            win.SelectionChanged += (_, _) =>
            {
                pickerMoved = true;
                SelectedXnaColor = HueSatConvert.ToXna(win.Hue, win.Saturation);
            };

            bool accepted = win.ShowDialog() == true;
            bool raiseCommit = HueSatConvert.FinishDialog(original, accepted, pickerMoved,
                win.Hue, win.Saturation, out var color);
            SelectedXnaColor = color;
            if (raiseCommit)
                SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
