using MahApps.Metro.Controls;
using System.Windows;

namespace VisualMusic.Forms
{
    public partial class HueSatWindow : MetroWindow
    {
        public float Hue
        {
            get => picker.Hue;
            set => picker.Hue = value;
        }

        public float Saturation
        {
            get => picker.Saturation;
            set => picker.Saturation = value;
        }

        /// <summary>Raised whenever the user moves the crosshair (for live preview).</summary>
        public event System.EventHandler SelectionChanged;

        public HueSatWindow()
        {
            InitializeComponent();
        }

        void Picker_SelectionChanged(object sender, System.EventArgs e)
            => SelectionChanged?.Invoke(this, e);

        void OkBtn_Click(object sender, RoutedEventArgs e) => DialogResult = true;
        void CancelBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
