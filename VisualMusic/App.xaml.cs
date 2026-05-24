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

            // TODO Phase 3: initCefSharp() — move from Program.cs

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
