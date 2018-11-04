using ColorSpaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music.Controls
{
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

		HueSatForm form = new HueSatForm();
		public HueSatButton()
		{
			form.SelectionChanged += form_SelectionChanged;
			Click += OnClick;
		}

		private void OnClick(object sender, EventArgs e)
		{
			float hue = Hue;
			float sat = Saturation;
			if (form.ShowDialog() != DialogResult.OK)
			{
				Hue = hue;
				Saturation = sat;
			}
			
		}

		private void form_SelectionChanged(object sender, EventArgs e)
		{
			BackColor = new HslColor(Hue, Saturation, 0.5); ;
			ColorChanged?.Invoke(sender, e);
		}
	}
}
