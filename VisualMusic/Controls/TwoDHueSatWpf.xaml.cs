using ColorSpaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VisualMusic.Controls
{
    public partial class TwoDHueSatWpf : UserControl
    {
        bool _mouseDown;
        bool _updating;
        WriteableBitmap _bitmap;

        public event EventHandler SelectionChanged;

        public TwoDHueSatWpf()
        {
            InitializeComponent();
        }

        // ---- DependencyProperties ----

        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue), typeof(float), typeof(TwoDHueSatWpf),
            new FrameworkPropertyMetadata(0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionChanged));

        public float Hue
        {
            get => (float)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation), typeof(float), typeof(TwoDHueSatWpf),
            new FrameworkPropertyMetadata(1f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionChanged));

        public float Saturation
        {
            get => (float)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (TwoDHueSatWpf)d;
            if (!c._updating)
                c.UpdateCrosshair();
        }

        // ---- Gradient rendering ----

        void RenderGradient()
        {
            int w = Math.Max(1, (int)ActualWidth);
            int h = Math.Max(1, (int)ActualHeight);

            if (_bitmap == null || _bitmap.PixelWidth != w || _bitmap.PixelHeight != h)
                _bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr32, null);

            int stride = w * 4;
            byte[] pixels = new byte[h * stride];

            for (int x = 0; x < w; x++)
            {
                double hue = (double)x / w;
                for (int y = 0; y < h; y++)
                {
                    double sat = 1.0 - (double)y / h;
                    // Blend between full-saturation hue color and white
                    var hslFull = new HslColor(hue, 1.0, 0.5);
                    System.Drawing.Color cFull = hslFull;
                    // White = RGB(255,255,255), lerp by saturation
                    byte r = (byte)(cFull.R + (255 - cFull.R) * (1 - sat));
                    byte g = (byte)(cFull.G + (255 - cFull.G) * (1 - sat));
                    byte b = (byte)(cFull.B + (255 - cFull.B) * (1 - sat));
                    int idx = y * stride + x * 4;
                    pixels[idx + 0] = b;
                    pixels[idx + 1] = g;
                    pixels[idx + 2] = r;
                    pixels[idx + 3] = 255;
                }
            }

            _bitmap.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            gradientImage.Source = _bitmap;
        }

        void UpdateCrosshair()
        {
            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            double cx = Hue * w;
            double cy = (1 - Saturation) * h;
            const double half = 8;

            crossH.X1 = cx - half; crossH.X2 = cx + half;
            crossH.Y1 = cy; crossH.Y2 = cy;
            crossV.X1 = cx; crossV.X2 = cx;
            crossV.Y1 = cy - half; crossV.Y2 = cy + half;

            // Use white crosshair on dark hues, black on light
            var hsl = new HslColor(Hue, 1.0, 0.5);
            System.Drawing.Color c = hsl;
            double brightness = (c.R * 0.299 + c.G * 0.587 + c.B * 0.114) / 255.0;
            var pen = brightness > 0.5 ? Brushes.Black : Brushes.White;
            crossH.Stroke = crossV.Stroke = pen;
        }

        // ---- Mouse handling ----

        void SetFromPoint(Point p)
        {
            _updating = true;
            try
            {
                Hue = (float)Math.Clamp(p.X / ActualWidth, 0, 1);
                Saturation = (float)Math.Clamp(1 - p.Y / ActualHeight, 0, 1);
            }
            finally { _updating = false; }
            UpdateCrosshair();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).CaptureMouse();
            _mouseDown = true;
            SetFromPoint(e.GetPosition((UIElement)sender));
        }

        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
                SetFromPoint(e.GetPosition((UIElement)sender));
        }

        void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            _mouseDown = false;
        }

        void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                ((UIElement)sender).ReleaseMouseCapture();
                _mouseDown = false;
            }
        }

        // ---- Size change ----

        void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderGradient();
            UpdateCrosshair();
        }
    }
}
