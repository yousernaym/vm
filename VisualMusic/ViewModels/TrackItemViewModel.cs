using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace VisualMusic.ViewModels
{
    public partial class TrackItemViewModel : ObservableObject
    {
        [ObservableProperty] string _name;
        [ObservableProperty] Color _normalColor;
        [ObservableProperty] Color _hilitedColor;

        public TrackView TrackView { get; }

        public TrackItemViewModel(TrackView trackView, string name, Color normalColor, Color hilitedColor)
        {
            TrackView = trackView;
            this._name = name;
            this._normalColor = normalColor;
            this._hilitedColor = hilitedColor;
        }

        public static Color ToWpfColor(System.Drawing.Color c)
            => Color.FromRgb(c.R, c.G, c.B);
    }
}
