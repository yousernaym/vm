using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Windows;

namespace VisualMusic.ViewModels
{
    public enum AppScreen { Song, ModBrowser, SidBrowser, MidiBrowser }

    public partial class MainViewModel : ObservableObject, IImportService
    {
        [ObservableProperty]
        private string windowTitle = Program.AppName;

        [ObservableProperty]
        private AppScreen currentScreen = AppScreen.Song;

        [RelayCommand] void ShowSong()        => CurrentScreen = AppScreen.Song;
        [RelayCommand] void ShowModBrowser()  => CurrentScreen = AppScreen.ModBrowser;
        [RelayCommand] void ShowSidBrowser()  => CurrentScreen = AppScreen.SidBrowser;
        [RelayCommand] void ShowMidiBrowser() => CurrentScreen = AppScreen.MidiBrowser;

        // IImportService — import dialogs are WinForms and migrated in Phase 6.
        // For now, only the file-extension routing is done; the dialogs are stubs.
        public void ImportFromUrl(string url, string suggestedFileName)
        {
            string ext = suggestedFileName.Split('.').Last().ToLower();

            // TODO Phase 6: replace with WPF import dialogs
            MessageBox.Show(
                $"Import from browser not yet supported in WPF mode.\n\nFile: {suggestedFileName}",
                Program.AppName,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
