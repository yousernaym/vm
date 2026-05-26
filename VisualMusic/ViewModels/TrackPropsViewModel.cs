using CommunityToolkit.Mvvm.ComponentModel;

namespace VisualMusic.ViewModels
{
    public partial class TrackPropsViewModel : ObservableObject
    {
        [ObservableProperty] TrackProps mergedProps;
        [ObservableProperty] int selectedTabIndex;
    }
}
