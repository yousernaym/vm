using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.Handler;
using CefSharp;
using CefSharp.WinForms;

namespace Visual_Music.Controls
{
	public partial class SongWebBrowser : UserControl
	{
		ChromiumWebBrowser webBrowser = new ChromiumWebBrowser("");
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
					if (Uri.IsWellFormedUriString(urlTb.Text, UriKind.RelativeOrAbsolute))
					{
						webBrowser.Load(urlTb.Text);
					}
				}
				catch (FormatException)
				{

				}
			}
		}


		public SongWebBrowser()
		{
			InitializeComponent();
			Controls.Add(webBrowser);
			webBrowser.AddressChanged += WebBrowser_AddressChanged;
		}

		private void WebBrowser_AddressChanged(object sender, AddressChangedEventArgs e)
		{
			urlTb.Text = e.Address;
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

	class RequestHandler : DefaultRequestHandler
	{
		public override bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
		{
			if (request.Url.EndsWith(".xm"))
				return true;
			else
				return false;
		}
	}
}
