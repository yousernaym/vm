using ColorSpaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VisualMusic
{
    [DefaultEvent("ColorChanged")]
    class HueSatButton : Button
    {
        public delegate void ColorChangedEH(HueSatButton sender, ColorChangedTventArgs e);
        public event EventHandler<ColorChangedTventArgs> ColorChanged;
        public event EventHandler ColorSubmitted;

        public float Hue
        {
            get => form.Hue;
            set => form.Hue = value;
        }
        public float Saturation
        {
            get => form.Saturation;
            set => form.Saturation = value;
        }
        public Color SelectedColor
        {
            get => ToGdiColor();
            set
            {
                HslColor c = value;
                Hue = (float)c.Hue;
                Saturation = (float)c.Saturation;
                if (value == Color.Black) //signaling that two or more selected tracks had different color. Set to black since HueSatButton colors can never be black normally.
                    BackColor = Color.Black;
                else
                    UpdateColor();
            }
        }

        public HueSatForm form = new HueSatForm();

        public HueSatButton()
        {
            form.SelectionChanged += Form_SelectionChanged;
            Click += This_Click;
            Text = "";
        }

        private void This_Click(object sender, EventArgs e)
        {
            float hue = Hue, sat = Saturation;
            if (form.ShowDialog() != DialogResult.OK)
            {
                Hue = hue;
                Saturation = sat;
                UpdateColor();
                ColorChanged?.Invoke(this, new ColorChangedTventArgs(true));
            }
            else
                ColorSubmitted?.Invoke(this, new EventArgs());
        }

        private void Form_SelectionChanged(object sender, EventArgs e)
        {
            UpdateColor();
            ColorChanged?.Invoke(this, new ColorChangedTventArgs(false));
        }

        void UpdateColor()
        {
            BackColor = ToGdiColor();
        }

        Color ToGdiColor()
        {
            return new HslColor(Hue, Saturation, 0.5 + (1 - Saturation) / 2);
        }
    }

    public class ColorChangedTventArgs : EventArgs
    {
        public bool ChangesCanceled { get; }
        public ColorChangedTventArgs(bool changesCanceled)
        {
            ChangesCanceled = changesCanceled;
        }
    }
}
