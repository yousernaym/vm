using ColorSpaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music
{
	[DefaultEvent("ColorChanged")]
	class HueSatButton : Button
	{
		public event EventHandler ColorChanged;
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
			get => new HslColor(Hue, Saturation, 0.5);
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
			Click += OnClick;
			Text = "";
		}

		private void OnClick(object sender, EventArgs e)
		{
			float hue = Hue, sat = Saturation;
			if (form.ShowDialog() != DialogResult.OK)
			{
				Hue = hue;
				Saturation = sat;
				updateColor();
				ColorChanged(sender, e);
			}			
		}

		private void form_SelectionChanged(object sender, EventArgs e)
		{
			updateColor();
			ColorChanged?.Invoke(sender, e);
		}

		void updateColor()
		{
			BackColor = new HslColor(Hue, Saturation, 0.5 + (1 - Saturation) / 2); ;
		}
	}
}
