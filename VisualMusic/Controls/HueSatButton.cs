using ColorSpaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			get => toGdiColor();
			set
			{
				HslColor c = value;
				Hue = (float)c.Hue;
				Saturation = (float)c.Saturation;
				if (value == Color.Black) //signaling that two or more selected tracks had different color. Set to black since HueSatButton colors can never be black normally.
					BackColor = Color.Black;
				else
					updateColor();
			}
		}

		public HueSatForm form = new HueSatForm();

		public HueSatButton()
		{
			form.SelectionChanged += form_SelectionChanged;
			Click += this_Click;
			Text = "";
		}

		private void this_Click(object sender, EventArgs e)
		{
			float hue = Hue, sat = Saturation;
			if (form.ShowDialog() != DialogResult.OK)
			{
				Hue = hue;
				Saturation = sat;
				updateColor();
				ColorChanged?.Invoke(this, new ColorChangedTventArgs(true));
			}
			else
				ColorSubmitted?.Invoke(this, new EventArgs());
		}

		private void form_SelectionChanged(object sender, EventArgs e)
		{
			updateColor();
			ColorChanged?.Invoke(this, new ColorChangedTventArgs(false));
		}

		void updateColor()
		{
			BackColor = toGdiColor();
		}

		Color toGdiColor()
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
