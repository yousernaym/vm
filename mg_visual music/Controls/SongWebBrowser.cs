using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music.Controls
{
	public partial class SongWebBrowser : UserControl
	{
		public string Url
		{
			get => urlTb.Text;
			set
			{
				string prefix = "";
				if (!value.Contains(@"://"))
					prefix = "https://";
				urlTb.Text = prefix + value;
				try
				{
					webBrowser1.Url = new Uri(urlTb.Text);
				}
				catch (FormatException)
				{

				}
			}
		}

		public SongWebBrowser()
		{
			InitializeComponent();
		}

		private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			urlTb.Text = e.Url.AbsoluteUri;
		}

		private void urlTb_Validated(object sender, EventArgs e)
		{
			Url = urlTb.Text;
		}

		private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			//if ()
		}
	}
}
