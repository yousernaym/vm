using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Dialog for choosing video export settings.
    /// Edits a <see cref="VideoExportOptions"/> in-place; caller reads <see cref="Options"/>
    /// after ShowDialog() returns true.
    /// </summary>
    public partial class VideoExportWindow : MetroWindow
    {
        public VideoExportOptions Options { get; private set; }

        public VideoExportWindow(VideoExportOptions options)
        {
            // Initialise Options before InitializeComponent so event handlers that fire
            // during control construction (e.g. NumericUpDown.ValueChanged) don't NRE.
            Options = options ?? new VideoExportOptions();

            InitializeComponent();

            // Populate SSAA combo (Disabled / 2x / 4x / 8x → SSAAFactor 1 / 2 / 4 / 8)
            ssFactorCombo.Items.Add("Disabled");
            ssFactorCombo.Items.Add("2x");
            ssFactorCombo.Items.Add("4x");
            ssFactorCombo.Items.Add("8x");

            // Populate quality-loss combo
            qualityLossCombo.Items.Add("0 - lossless");
            for (int i = 1; i < 10; i++)
                qualityLossCombo.Items.Add(i.ToString());
            qualityLossCombo.Items.Add("10 - smallest file");

            SetOptions(Options);
        }

        // ---- Internal helpers ----

        void SetOptions(VideoExportOptions options)
        {
            Options = options;

            sphereCb.IsChecked = options.Sphere;
            sphericalMetadataCb.IsChecked = options.SphericalMetadata;
            sphericalMetadataCb.IsEnabled = options.Sphere;
            stereoscopicCb.IsChecked = options.SphericalStereo;

            UpdateResoItems();

            int ssaaIdx = Math.Max(0, Math.Min(options.SsaaIndex, ssFactorCombo.Items.Count - 1));
            ssFactorCombo.SelectedIndex = ssaaIdx;

            int qualIdx = Math.Max(0, Math.Min(options.VideoQualityLoss, qualityLossCombo.Items.Count - 1));
            qualityLossCombo.SelectedIndex = qualIdx;

            fpsUd.Value = options.Fps;
        }

        void UpdateResoItems()
        {
            resoCombo.Items.Clear();
            if (Options.Sphere)
            {
                if (Options.SphericalStereo)
                {
                    resoCombo.Items.Add("4096 x 4096");
                    resoCombo.Items.Add("8192 x 8192");
                }
                else
                {
                    resoCombo.Items.Add("4096 x 2048");
                    resoCombo.Items.Add("8192 x 4096");
                }
            }
            else
            {
                resoCombo.Items.Add("1920 x 1080");
                resoCombo.Items.Add("3840 x 2160");
                resoCombo.Items.Add("7680 x 4320");
            }

            int idx = Options.ResoIndex;
            if (idx < 0 || idx >= resoCombo.Items.Count) idx = 0;
            resoCombo.SelectedIndex = idx;
            Options.ResoIndex = idx;           // keep in sync regardless of whether SelectionChanged fires
            UpdateResoColor();
        }

        bool ParseReso()
        {
            string[] xy = resoCombo.Text.Split(new[] { 'x', 'X' });
            if (xy.Length != 2) return false;
            try
            {
                int w = int.Parse(xy[0].Trim());
                int h = int.Parse(xy[1].Trim());
                if (w <= 0 || h <= 0) return false;
                Options.Width = w;
                Options.Height = h;
                return true;
            }
            catch (FormatException) { return false; }
        }

        void UpdateResoColor()
        {
            if (ParseReso())
                resoCombo.ClearValue(ForegroundProperty);  // inherit theme default
            else
                resoCombo.Foreground = Brushes.Red;
        }

        // ---- Event handlers ----

        void SphereCb_Click(object sender, RoutedEventArgs e)
        {
            Options.Sphere = sphereCb.IsChecked == true;
            // WPF CheckBox.IsChecked is bool? — set them separately to avoid implicit bool?→bool error.
            sphericalMetadataCb.IsChecked = Options.Sphere;
            sphericalMetadataCb.IsEnabled = Options.Sphere;
            UpdateResoItems();
        }

        void SphericalMetadataCb_Click(object sender, RoutedEventArgs e)
        {
            Options.SphericalMetadata = sphericalMetadataCb.IsChecked == true;
        }

        void StereoscopicCb_Click(object sender, RoutedEventArgs e)
        {
            Options.SphericalStereo = stereoscopicCb.IsChecked == true;
            UpdateResoItems();
        }

        void ResoCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Options == null || resoCombo.SelectedIndex < 0) return;
            Options.ResoIndex = resoCombo.SelectedIndex;
            UpdateResoColor();
        }

        void ResoCombo_KeyUp(object sender, KeyEventArgs e) => UpdateResoColor();

        void ResoCombo_DropDownOpened(object sender, EventArgs e)
            => resoCombo.ClearValue(ForegroundProperty);

        void ResoCombo_DropDownClosed(object sender, EventArgs e) => UpdateResoColor();

        void SsFactorCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Options == null || ssFactorCombo.SelectedIndex < 0) return;
            // Index 0 = Disabled (factor 1), 1 = 2x, 2 = 4x, 3 = 8x → 2^index
            Options.SSAAFactor = (int)Math.Pow(2, ssFactorCombo.SelectedIndex);
        }

        void QualityLossCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Options == null || qualityLossCombo.SelectedIndex < 0) return;
            Options.VideoQualityLoss = qualityLossCombo.SelectedIndex;
        }

        void FpsUd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (Options == null || !e.NewValue.HasValue) return;
            Options.Fps = (float)e.NewValue.Value;
        }

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!ParseReso())
                MessageBox.Show("Invalid resolution format", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else if (Options.Width % 2 != 0 || Options.Height % 2 != 0)
                MessageBox.Show("Resolution width and height must be even numbers", Program.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else
                DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
