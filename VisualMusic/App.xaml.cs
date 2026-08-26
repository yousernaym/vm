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
            MidMix.Init(Program.SoundFontPath);
            if (!Media.InitMF())
            {
                MessageBox.Show("Couldn't initialize Media library.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }
            Program.InitTempDir();

            // Initialize CEF here (not via ChromiumWebBrowser auto-init) so each
            // process gets its own RootCachePath. Keep CefSharp types out of this
            // file — the WPF XAML design-time compiler loads App.xaml.cs.
            if (!CefInit.Initialize())
            {
                MessageBox.Show("Couldn't initialize embedded browser.", Program.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow(e.Args);
            MainWindow = mainWindow;
            mainWindow.Show();

            // Silently refresh the HVSC song-lengths DB if it's absent or older than 30 days.
            // Fire-and-forget: all exceptions are swallowed inside EnsureRecentAsync.
            _ = Hvsc.EnsureRecentAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MidMix.Close();
            Media.CloseMF();
            CefInit.Shutdown();
            base.OnExit(e);
            Program.CloseTempDir();
        }
    }
}
