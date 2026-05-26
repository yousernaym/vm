using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace VisualMusic.ViewModels
{
    public partial class TrackItemViewModel : ObservableObject
    {
        [ObservableProperty] string name;
        [ObservableProperty] Color normalColor;
        [ObservableProperty] Color hilitedColor;

        public TrackView TrackView { get; }

        public TrackItemViewModel(TrackView trackView, string name, Color normalColor, Color hilitedColor)
        {
            TrackView = trackView;
            this.name = name;
            this.normalColor = normalColor;
            this.hilitedColor = hilitedColor;
        }

        public static Color ToWpfColor(System.Drawing.Color c)
            => Color.FromRgb(c.R, c.G, c.B);
    }
}
