using System;
using System.IO;
using System.Windows;

namespace VisualMusic
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            MidMix.init();
            if (!Media.initMF())
            {
                MessageBox.Show("Couldn't initialize Media library.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }
            Directory.CreateDirectory(Program.TempDir);

            // CefSharp.Wpf auto-initializes on first ChromiumWebBrowser creation
            // and registers an Application.Exit handler for shutdown.
            // Explicit Cef.Initialize() is skipped to avoid loading the C++/CLI
            // CefSharp.Common.NETCore assembly in the WPF XAML design-time compiler.

            var mainWindow = new MainWindow(e.Args);
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MidMix.close();
            Media.closeMF();
            base.OnExit(e);
        }
    }
}
