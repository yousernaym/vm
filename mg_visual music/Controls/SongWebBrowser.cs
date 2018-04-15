// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.Example.RequestEventHandler;
using System.Net;
using System.IO;

namespace Visual_Music
{
    public partial class SongWebBrowser : UserControl
    {
		readonly ChromiumWebBrowser browser;
		readonly Form1 mainForm;

		public string[] SongFormats { get; set; }
		public string Url
		{
			get => urlTextBox.Text;
			set
			{
				string prefix = "";
				if (!value.Contains(@"://"))
					prefix = "https://";
				urlTextBox.Text = prefix + value;
				try
				{
					if (Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.RelativeOrAbsolute))
					{
						browser.Load(urlTextBox.Text);
					}
				}
				catch (FormatException)
				{

				}
			}
		}

		//public event EventHandler<OnBeforeBrowseEventArgs> OnBeforeBrowseEvent
		//{
		//	//add => ((RequestEventHandler)browser.RequestHandler).OnBeforeBrowseEvent += value;
		//	add =>
		//		this.InvokeOnUiThreadIfRequired(() => ((RequestEventHandler)browser.RequestHandler).OnBeforeBrowseEvent += value);
		//	remove =>
		//		this.InvokeOnUiThreadIfRequired(() => ((RequestEventHandler)browser.RequestHandler).OnBeforeBrowseEvent -= value);
		//}
		
		public SongWebBrowser(Form1 form1)
        {
            InitializeComponent();

			mainForm = form1;
			browser = new ChromiumWebBrowser("")
            {
                Dock = DockStyle.Fill,
            };

			RequestEventHandler requestEventHandler = new RequestEventHandler();
			requestEventHandler.OnBeforeBrowseEvent += OnBeforeBrowse;
			browser.RequestHandler = requestEventHandler;
            toolStripContainer.ContentPanel.Controls.Add(browser);

            browser.LoadingStateChanged += OnLoadingStateChanged;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
            browser.StatusMessage += OnBrowserStatusMessage;
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;

            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            var version = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}, Environment: {3}", Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion, bitness);
            DisplayOutput(version);
        }

		private void OnBeforeBrowse(object sender, OnBeforeBrowseEventArgs e)
		{
			if (e.Request.Url.EndsWith(".xm"))
			{
				e.CancelNavigation = true;
				string url = string.Copy(e.Request.Url);
				this.InvokeOnUiThreadIfRequired(delegate
				{
					mainForm.importModForm.NoteFilePath = url;
					mainForm.importModForm.ShowDialog();//(filePath, Form1.Settings.ModInsTrack, mainForm.tpartyIntegrationForm.ModuleMixdown ? MixdownType.Tparty);
				});
			}
		}

		private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            DisplayOutput(string.Format("Line: {0}, Source: {1}, Message: {2}", args.Line, args.Source, args.Message));
        }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            SetCanGoBack(args.CanGoBack);
            SetCanGoForward(args.CanGoForward);

            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

        private void SetCanGoBack(bool canGoBack)
        {
            this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void SetCanGoForward(bool canGoForward)
        {
            this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            goButton.Text = isLoading ?
                "Stop" :
                "Go";
            goButton.Image = isLoading ?
                Properties.Resources.nav_plain_red :
                Properties.Resources.nav_plain_green;

            HandleToolStripLayout();
        }

        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout()
        {
            var width = toolStrip1.Width;
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                browser.Load(url);
            }
        }
    }

	static class ControlExtensions
	{
		/// <summary>
		/// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
		/// </summary>
		/// <param name="control">the control for which the update is required</param>
		/// <param name="action">action to be performed on the control</param>
		public static void InvokeOnUiThreadIfRequired(this Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				control.BeginInvoke(action);
			}
			else
			{
				action.Invoke();
			}
		}
	}
}
