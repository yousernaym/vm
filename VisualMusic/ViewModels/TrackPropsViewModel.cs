using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic.ViewModels
{
    public partial class TrackPropsViewModel : ObservableObject
    {
        [ObservableProperty] int selectedTabIndex;

        TrackProps _mergedProps;
        public TrackProps MergedProps
        {
            get => _mergedProps;
            set
            {
                _mergedProps = value;
                RefreshAll();
            }
        }

        // ---- Write-back callbacks (set by MainViewModel) ----

        /// <summary>Apply a mutation to all selected TrackProps, then repaint.</summary>
        public Action<Action<TrackProps>> ApplyToSelected { get; set; }

        /// <summary>Apply + rebuild octrees (needed for style/geometry changes).</summary>
        public Action<Action<TrackProps>> ApplyAndRebuild { get; set; }

        void Apply(Action<TrackProps> fn) => ApplyToSelected?.Invoke(fn);
        void Rebuild(Action<TrackProps> fn) => ApplyAndRebuild?.Invoke(fn);

        // ---- RefreshAll: raise PropertyChanged for every derived property ----

        void RefreshAll()
        {
            // Style
            OnPropertyChanged(nameof(StyleTypeIndex));
            OnPropertyChanged(nameof(LineStyleVisible));
            OnPropertyChanged(nameof(LineWidth));
            OnPropertyChanged(nameof(QnGapThreshold));
            OnPropertyChanged(nameof(Continuous));
            OnPropertyChanged(nameof(LineTypeIndex));
            OnPropertyChanged(nameof(LineHlTypeIndex));
            OnPropertyChanged(nameof(HlSize));
            OnPropertyChanged(nameof(HlMovementPow));
            OnPropertyChanged(nameof(MovingHl));
            OnPropertyChanged(nameof(ShrinkingHl));
            OnPropertyChanged(nameof(HlBorder));

            // Material
            OnPropertyChanged(nameof(Transp));
            OnPropertyChanged(nameof(MaterialHue));
            OnPropertyChanged(nameof(NormalSat));
            OnPropertyChanged(nameof(NormalLum));
            OnPropertyChanged(nameof(HiliteSat));
            OnPropertyChanged(nameof(HiliteLum));
            OnPropertyChanged(nameof(DisableTexture));
            OnPropertyChanged(nameof(PointSmp));
            OnPropertyChanged(nameof(TexColBlend));
            OnPropertyChanged(nameof(UTile));
            OnPropertyChanged(nameof(VTile));
            OnPropertyChanged(nameof(KeepAspect));
            OnPropertyChanged(nameof(UAnchorNote));
            OnPropertyChanged(nameof(UAnchorScreen));
            OnPropertyChanged(nameof(UAnchorSong));
            OnPropertyChanged(nameof(VAnchorNote));
            OnPropertyChanged(nameof(VAnchorScreen));
            OnPropertyChanged(nameof(UScroll));
            OnPropertyChanged(nameof(VScroll));
            OnPropertyChanged(nameof(TexturePath));
            OnPropertyChanged(nameof(TextureThumbnail));

            // Light
            OnPropertyChanged(nameof(UseGlobalLight));
            OnPropertyChanged(nameof(LightDirX));
            OnPropertyChanged(nameof(LightDirY));
            OnPropertyChanged(nameof(LightDirZ));
            OnPropertyChanged(nameof(AmbientAmount));
            OnPropertyChanged(nameof(AmbientXnaColor));
            OnPropertyChanged(nameof(DiffuseAmount));
            OnPropertyChanged(nameof(DiffuseXnaColor));
            OnPropertyChanged(nameof(SpecAmount));
            OnPropertyChanged(nameof(SpecXnaColor));
            OnPropertyChanged(nameof(SpecPower));
            OnPropertyChanged(nameof(MasterAmount));
            OnPropertyChanged(nameof(MasterXnaColor));

            // Spatial
            OnPropertyChanged(nameof(XOffset));
            OnPropertyChanged(nameof(YOffset));
            OnPropertyChanged(nameof(ZOffset));

            // Audio
            OnPropertyChanged(nameof(AudioFilename));
        }

        // =====================================================================
        // STYLE TAB
        // =====================================================================

        StyleProps SP => _mergedProps?.StyleProps;
        NoteStyle_Line LineStyle => SP?.getLineStyle();

        /// <summary>0=Default, 1=Bar, 2=Line; -1 if null/mixed.</summary>
        public int StyleTypeIndex
        {
            get => SP?.Type == null ? -1 : (int)SP.Type;
            set
            {
                if (SP == null) return;
                var t = value < 0 ? (NoteStyleType?)null : (NoteStyleType)value;
                Rebuild(tp => tp.StyleProps.Type = t);
                OnPropertyChanged();
                OnPropertyChanged(nameof(LineStyleVisible));
            }
        }

        public bool LineStyleVisible => SP?.Type == NoteStyleType.Line;

        public double? LineWidth
        {
            get => (double?)LineStyle?.LineWidth;
            set { if (value != null) Rebuild(tp => tp.StyleProps.getLineStyle().LineWidth = (float)value); OnPropertyChanged(); }
        }

        public double? QnGapThreshold
        {
            get => (double?)LineStyle?.Qn_gapThreshold;
            set { if (value != null) Rebuild(tp => tp.StyleProps.getLineStyle().Qn_gapThreshold = (float)value); OnPropertyChanged(); }
        }

        public bool? Continuous
        {
            get => LineStyle?.Continuous;
            set { if (value != null) Apply(tp => tp.StyleProps.getLineStyle().Continuous = value); OnPropertyChanged(); }
        }

        public int LineTypeIndex
        {
            get => LineStyle?.LineType == null ? -1 : (int)LineStyle.LineType;
            set
            {
                if (value < 0) return;
                var lt = (LineType)value;
                Rebuild(tp => tp.StyleProps.getLineStyle().LineType = lt);
                OnPropertyChanged();
            }
        }

        public int LineHlTypeIndex
        {
            get => LineStyle?.HlType == null ? -1 : (int)LineStyle.HlType;
            set
            {
                if (value < 0) return;
                var ht = (LineHlType)value;
                Apply(tp => tp.StyleProps.getLineStyle().HlType = ht);
                OnPropertyChanged();
            }
        }

        public double? HlSize
        {
            get => (double?)LineStyle?.HlSize;
            set { if (value != null) Apply(tp => tp.StyleProps.getLineStyle().HlSize = (float)value); OnPropertyChanged(); }
        }

        public double? HlMovementPow
        {
            get => (double?)LineStyle?.HlMovementPow;
            set { if (value != null) Apply(tp => tp.StyleProps.getLineStyle().HlMovementPow = (float)value); OnPropertyChanged(); }
        }

        public bool? MovingHl
        {
            get => LineStyle?.MovingHl;
            set { if (value != null) Apply(tp => tp.StyleProps.getLineStyle().MovingHl = value); OnPropertyChanged(); }
        }

        public bool? ShrinkingHl
        {
            get => LineStyle?.ShrinkingHl;
            set { if (value != null) Apply(tp => tp.StyleProps.getLineStyle().ShrinkingHl = value); OnPropertyChanged(); }
        }

        public bool? HlBorder
        {
            get => LineStyle?.HlBorder;
            set { if (value != null) Apply(tp => tp.StyleProps.getLineStyle().HlBorder = value); OnPropertyChanged(); }
        }

        // =====================================================================
        // MATERIAL TAB
        // =====================================================================

        MaterialProps MP => _mergedProps?.MaterialProps;

        public double? Transp
        {
            get => (double?)MP?.Transp;
            set { if (value != null) Apply(tp => tp.MaterialProps.Transp = (float)value); OnPropertyChanged(); }
        }

        public double? MaterialHue
        {
            get => (double?)MP?.Hue;
            set { if (value != null) Apply(tp => tp.MaterialProps.Hue = (float)value); OnPropertyChanged(); }
        }

        public double? NormalSat
        {
            get => (double?)MP?.Normal.Sat;
            set { if (value != null) Apply(tp => tp.MaterialProps.Normal.Sat = (float)value); OnPropertyChanged(); }
        }

        public double? NormalLum
        {
            get => (double?)MP?.Normal.Lum;
            set { if (value != null) Apply(tp => tp.MaterialProps.Normal.Lum = (float)value); OnPropertyChanged(); }
        }

        public double? HiliteSat
        {
            get => (double?)MP?.Hilited.Sat;
            set { if (value != null) Apply(tp => tp.MaterialProps.Hilited.Sat = (float)value); OnPropertyChanged(); }
        }

        public double? HiliteLum
        {
            get => (double?)MP?.Hilited.Lum;
            set { if (value != null) Apply(tp => tp.MaterialProps.Hilited.Lum = (float)value); OnPropertyChanged(); }
        }

        TrackPropsTex TexProps => MP?.TexProps;

        public bool? DisableTexture
        {
            get => TexProps?.DisableTexture;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.DisableTexture = value); OnPropertyChanged(); }
        }

        public bool? PointSmp
        {
            get => TexProps?.PointSmp;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.PointSmp = value); OnPropertyChanged(); }
        }

        public bool? TexColBlend
        {
            get => TexProps?.TexColBlend;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.TexColBlend = value); OnPropertyChanged(); }
        }

        public bool? UTile
        {
            get => TexProps?.UTile;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.UTile = value); OnPropertyChanged(); }
        }

        public bool? VTile
        {
            get => TexProps?.VTile;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.VTile = value); OnPropertyChanged(); }
        }

        public bool? KeepAspect
        {
            get => TexProps?.KeepAspect;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.KeepAspect = value); OnPropertyChanged(); }
        }

        public bool UAnchorNote
        {
            get => TexProps?.UAnchor == TexAnchorEnum.Note;
            set { if (value) Apply(tp => tp.MaterialProps.TexProps.UAnchor = TexAnchorEnum.Note); OnPropertyChanged(); }
        }
        public bool UAnchorScreen
        {
            get => TexProps?.UAnchor == TexAnchorEnum.Screen;
            set { if (value) Apply(tp => tp.MaterialProps.TexProps.UAnchor = TexAnchorEnum.Screen); OnPropertyChanged(); }
        }
        public bool UAnchorSong
        {
            get => TexProps?.UAnchor == TexAnchorEnum.Song;
            set { if (value) Apply(tp => tp.MaterialProps.TexProps.UAnchor = TexAnchorEnum.Song); OnPropertyChanged(); }
        }
        public bool VAnchorNote
        {
            get => TexProps?.VAnchor == TexAnchorEnum.Note;
            set { if (value) Apply(tp => tp.MaterialProps.TexProps.VAnchor = TexAnchorEnum.Note); OnPropertyChanged(); }
        }
        public bool VAnchorScreen
        {
            get => TexProps?.VAnchor == TexAnchorEnum.Screen;
            set { if (value) Apply(tp => tp.MaterialProps.TexProps.VAnchor = TexAnchorEnum.Screen); OnPropertyChanged(); }
        }

        public double? UScroll
        {
            get => (double?)TexProps?.UScroll;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.UScroll = (float)value); OnPropertyChanged(); }
        }

        public double? VScroll
        {
            get => (double?)TexProps?.VScroll;
            set { if (value != null) Apply(tp => tp.MaterialProps.TexProps.VScroll = (float)value); OnPropertyChanged(); }
        }

        public string TexturePath => TexProps?.Path ?? "";

        public BitmapSource TextureThumbnail => BuildTextureThumbnail();

        BitmapSource BuildTextureThumbnail()
        {
            if (TexProps?.Texture == null) return null;
            try
            {
                var tex = TexProps.Texture;
                int w = tex.Width, h = tex.Height;
                var data = new Color[w * h];
                tex.GetData(data);
                var bmp = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
                byte[] pixels = new byte[w * h * 4];
                for (int i = 0; i < data.Length; i++)
                {
                    pixels[i * 4 + 0] = data[i].B;
                    pixels[i * 4 + 1] = data[i].G;
                    pixels[i * 4 + 2] = data[i].R;
                    pixels[i * 4 + 3] = data[i].A;
                }
                bmp.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), pixels, w * 4, 0);
                return bmp;
            }
            catch { return null; }
        }

        // ---- Texture load (called from MaterialTab code-behind) ----

        public Action<string> LoadTexture { get; set; }
        public Action UnloadTexture { get; set; }

        // =====================================================================
        // LIGHT TAB
        // =====================================================================

        LightProps LP => _mergedProps?.LightProps;

        public bool? UseGlobalLight
        {
            get => LP?.UseGlobalLight;
            set { if (value != null) Apply(tp => tp.LightProps.UseGlobalLight = value); OnPropertyChanged(); }
        }

        public double? LightDirX
        {
            get => (double?)LP?.DirX;
            set { if (value != null) Apply(tp => tp.LightProps.DirX = (float)value); OnPropertyChanged(); }
        }
        public double? LightDirY
        {
            get => (double?)LP?.DirY;
            set { if (value != null) Apply(tp => tp.LightProps.DirY = (float)value); OnPropertyChanged(); }
        }
        public double? LightDirZ
        {
            get => (double?)LP?.DirZ;
            set { if (value != null) Apply(tp => tp.LightProps.DirZ = (float)value); OnPropertyChanged(); }
        }

        public double? AmbientAmount
        {
            get => (double?)LP?.AmbientAmount;
            set { if (value != null) Apply(tp => tp.LightProps.AmbientAmount = (float)value); OnPropertyChanged(); }
        }

        public XnaColor? AmbientXnaColor
        {
            get => LP?.AmbientColor;
            set { if (value != null) Apply(tp => tp.LightProps.AmbientColor = value); OnPropertyChanged(); }
        }

        public double? DiffuseAmount
        {
            get => (double?)LP?.DiffuseAmount;
            set { if (value != null) Apply(tp => tp.LightProps.DiffuseAmount = (float)value); OnPropertyChanged(); }
        }

        public XnaColor? DiffuseXnaColor
        {
            get => LP?.DiffuseColor;
            set { if (value != null) Apply(tp => tp.LightProps.DiffuseColor = value); OnPropertyChanged(); }
        }

        public double? SpecAmount
        {
            get => (double?)LP?.SpecAmount;
            set { if (value != null) Apply(tp => tp.LightProps.SpecAmount = (float)value); OnPropertyChanged(); }
        }

        public XnaColor? SpecXnaColor
        {
            get => LP?.SpecColor;
            set { if (value != null) Apply(tp => tp.LightProps.SpecColor = value); OnPropertyChanged(); }
        }

        public double? SpecPower
        {
            get => (double?)LP?.SpecPower;
            set { if (value != null) Apply(tp => tp.LightProps.SpecPower = (float)value); OnPropertyChanged(); }
        }

        public double? MasterAmount
        {
            get => (double?)LP?.MasterAmount;
            set { if (value != null) Apply(tp => tp.LightProps.MasterAmount = (float)value); OnPropertyChanged(); }
        }

        public XnaColor? MasterXnaColor
        {
            get => LP?.MasterColor;
            set { if (value != null) Apply(tp => tp.LightProps.MasterColor = value); OnPropertyChanged(); }
        }

        // =====================================================================
        // SPATIAL TAB
        // =====================================================================

        SpatialProps SpatP => _mergedProps?.SpatialProps;

        public double? XOffset
        {
            get => (double?)SpatP?.XOffset;
            set { if (value != null) Apply(tp => tp.SpatialProps.XOffset = (float)value); OnPropertyChanged(); }
        }

        public double? YOffset
        {
            get => (double?)SpatP?.YOffset;
            set { if (value != null) Apply(tp => tp.SpatialProps.YOffset = (float)value); OnPropertyChanged(); }
        }

        public double? ZOffset
        {
            get => (double?)SpatP?.ZOffset;
            set { if (value != null) Apply(tp => tp.SpatialProps.ZOffset = (float)value); OnPropertyChanged(); }
        }

        // =====================================================================
        // AUDIO TAB
        // =====================================================================

        public string AudioFilename
        {
            get => _mergedProps?.AudioProps?.Filename ?? "";
            set { Apply(tp => tp.AudioProps.Filename = value); OnPropertyChanged(); }
        }

        public Action BrowseAudioFile { get; set; }
    }
}
