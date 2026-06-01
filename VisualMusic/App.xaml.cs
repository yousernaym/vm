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

            // Apply the user-selected theme before any window is created.
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(
                this,
                AppSettings.Instance.ThemeBaseColorOrDefault,
                AppSettings.Instance.ThemeColorSchemeOrDefault);

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

            // Silently refresh the HVSC song-lengths DB if it's absent or older than 30 days.
            // Fire-and-forget: all exceptions are swallowed inside EnsureRecentAsync.
            _ = Hvsc.EnsureRecentAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MidMix.close();
            Media.closeMF();
            base.OnExit(e);
        }
    }
}
