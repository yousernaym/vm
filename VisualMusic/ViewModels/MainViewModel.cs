using CommunityToolkit.Mvvm.ComponentModel;

namespace VisualMusic.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string windowTitle = Program.AppName;
    }
}
