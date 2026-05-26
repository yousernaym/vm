using ColorSpaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VisualMusic.Forms;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic.Controls
{
    public partial class HueSatButtonWpf : UserControl
    {
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
            var win = new HueSatWindow { Owner = Window.GetWindow(this) };

            if (SelectedXnaColor != null)
            {
                var xna = SelectedXnaColor.Value;
                var gdi = System.Drawing.Color.FromArgb(xna.R, xna.G, xna.B);
                HslColor hsl = gdi;
                win.Hue = (float)hsl.Hue;
                win.Saturation = (float)hsl.Saturation;
            }

            float origHue = win.Hue, origSat = win.Saturation;

            win.SelectionChanged += (_, _2) =>
            {
                SelectedXnaColor = HslToXna(win.Hue, win.Saturation);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            };

            if (win.ShowDialog() == true)
            {
                SelectedXnaColor = HslToXna(win.Hue, win.Saturation);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SelectedXnaColor = HslToXna(origHue, origSat);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        static XnaColor HslToXna(float hue, float saturation)
        {
            var gdi = (System.Drawing.Color)new HslColor(hue, saturation, 0.5);
            return new XnaColor((byte)gdi.R, (byte)gdi.G, (byte)gdi.B, (byte)255);
        }
    }
}
