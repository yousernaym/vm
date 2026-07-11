using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic.ViewModels
{
    public partial class TrackPropsViewModel : ObservableObject
    {
        [ObservableProperty] int _selectedTabIndex;

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

        /// <summary>Apply + rebuild track geometry (needed for style/geometry changes).</summary>
        public Action<Action<TrackProps>> ApplyAndRebuild { get; set; }

        /// <summary>Refreshes track-list color swatches after material color edits.</summary>
        public Action RefreshTrackColors { get; set; }

        void Apply(Action<TrackProps> fn) => ApplyToSelected?.Invoke(fn);
        void Rebuild(Action<TrackProps> fn) => ApplyAndRebuild?.Invoke(fn);
        void ApplyMaterial(Action<TrackProps> fn)
        {
            Apply(fn);
            RefreshTrackColors?.Invoke();
        }

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

            // Modulation
            OnPropertyChanged(nameof(ModulationVisible));
            OnPropertyChanged(nameof(ModEntries));
            OnPropertyChanged(nameof(ModEntryComboEnabled));
            OnPropertyChanged(nameof(ModEntryIndex));
            OnPropertyChanged(nameof(ModEntryDetailsVisible));
            OnPropertyChanged(nameof(ModXOrigin));
            OnPropertyChanged(nameof(ModYOrigin));
            OnPropertyChanged(nameof(ModXOriginEnable));
            OnPropertyChanged(nameof(ModYOriginEnable));
            OnPropertyChanged(nameof(ModXOriginEnabled));
            OnPropertyChanged(nameof(ModYOriginEnabled));
            OnPropertyChanged(nameof(ModCombineEnabled));
            OnPropertyChanged(nameof(ModCombineIndex));
            OnPropertyChanged(nameof(ModSquareAspect));
            OnPropertyChanged(nameof(ModColorDestEnable));
            OnPropertyChanged(nameof(ModColorDest));
            OnPropertyChanged(nameof(ModAngleDestEnable));
            OnPropertyChanged(nameof(ModAngleDest));
            OnPropertyChanged(nameof(ModStart));
            OnPropertyChanged(nameof(ModStop));
            OnPropertyChanged(nameof(ModFadeIn));
            OnPropertyChanged(nameof(ModFadeOut));
            OnPropertyChanged(nameof(ModPower));
            OnPropertyChanged(nameof(ModDiscardAfterStop));
            OnPropertyChanged(nameof(ModInvert));

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
            OnPropertyChanged(nameof(UAnchorIndex));
            OnPropertyChanged(nameof(UAnchorNote));
            OnPropertyChanged(nameof(UAnchorScreen));
            OnPropertyChanged(nameof(UAnchorSong));
            OnPropertyChanged(nameof(VAnchorIndex));
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
            OnPropertyChanged(nameof(PitchOffset));
            OnPropertyChanged(nameof(ViewWidthQn));

            // Audio
            OnPropertyChanged(nameof(AudioFilename));
            OnPropertyChanged(nameof(SilenceThreshold));
        }

        /// <summary>
        /// Called every frame by <see cref="MainViewModel.NotifyScrollPositionChanged"/> while track
        /// keyframes are active to push interpolated style values to the UI.
        /// </summary>
        public void RefreshLiveValues()
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

            // Modulation values (raised every frame while track keyframes exist so live
            // playback drives the sliders and checkboxes in the Mod section).
            // Structural properties (ModEntries, ModEntryIndex) are intentionally excluded.
            OnPropertyChanged(nameof(ModXOrigin));
            OnPropertyChanged(nameof(ModYOrigin));
            OnPropertyChanged(nameof(ModXOriginEnable));
            OnPropertyChanged(nameof(ModYOriginEnable));
            OnPropertyChanged(nameof(ModXOriginEnabled));
            OnPropertyChanged(nameof(ModYOriginEnabled));
            OnPropertyChanged(nameof(ModCombineEnabled));
            OnPropertyChanged(nameof(ModCombineIndex));
            OnPropertyChanged(nameof(ModSquareAspect));
            OnPropertyChanged(nameof(ModColorDestEnable));
            OnPropertyChanged(nameof(ModColorDest));
            OnPropertyChanged(nameof(ModAngleDestEnable));
            OnPropertyChanged(nameof(ModAngleDest));
            OnPropertyChanged(nameof(ModStart));
            OnPropertyChanged(nameof(ModStop));
            OnPropertyChanged(nameof(ModFadeIn));
            OnPropertyChanged(nameof(ModFadeOut));
            OnPropertyChanged(nameof(ModPower));
            OnPropertyChanged(nameof(ModDiscardAfterStop));
            OnPropertyChanged(nameof(ModInvert));

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
            OnPropertyChanged(nameof(UAnchorIndex));
            OnPropertyChanged(nameof(VAnchorIndex));
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
            OnPropertyChanged(nameof(PitchOffset));
            OnPropertyChanged(nameof(ViewWidthQn));
        }

        // =====================================================================
        // STYLE TAB
        // =====================================================================

        StyleProps SP => _mergedProps?.StyleProps;
        NoteStyle_Line LineStyle => SP?.GetLineStyle();

        // Non-null only when a concrete (Bar/Line) style is selected.
        // Deliberately avoids ActiveNoteStyle which would follow the Default→GlobalProps chain
        // on merged clones (where GlobalProps is null).
        NoteStyle AS => (SP == null || SP.Type == null || SP.Type == NoteStyleType.Default)
                        ? null : SP.GetStyle(SP.Type);
        NoteStyleMod ME => AS?.SelectedModEntry;

        void ApplyMod(Action<NoteStyleMod> fn) =>
            Apply(tp => { var m = tp.ActiveNoteStyle?.SelectedModEntry; if (m != null) fn(m); });

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
                OnPropertyChanged(nameof(ModulationVisible));
                OnPropertyChanged(nameof(ModEntries));
                OnPropertyChanged(nameof(ModEntryComboEnabled));
                OnPropertyChanged(nameof(ModEntryIndex));
                OnPropertyChanged(nameof(ModEntryDetailsVisible));
            }
        }

        public bool LineStyleVisible => SP?.Type == NoteStyleType.Line;

        public double? LineWidth
        {
            get => (double?)LineStyle?.LineWidth;
            set { if (value != null) Rebuild(tp => tp.StyleProps.GetLineStyle().LineWidth = (float)value); OnPropertyChanged(); }
        }

        public double? QnGapThreshold
        {
            get => (double?)LineStyle?.Qn_gapThreshold;
            set { if (value != null) Rebuild(tp => tp.StyleProps.GetLineStyle().Qn_gapThreshold = (float)value); OnPropertyChanged(); }
        }

        public bool? Continuous
        {
            get => LineStyle?.Continuous;
            set { Apply(tp => tp.StyleProps.GetLineStyle().Continuous = value ?? false); OnPropertyChanged(); }
        }

        public int LineTypeIndex
        {
            get => LineStyle?.LineType == null ? -1 : (int)LineStyle.LineType;
            set
            {
                if (value < 0) return;
                var lt = (LineType)value;
                Rebuild(tp => tp.StyleProps.GetLineStyle().LineType = lt);
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
                Apply(tp => tp.StyleProps.GetLineStyle().HlType = ht);
                OnPropertyChanged();
            }
        }

        public double? HlSize
        {
            get => (double?)LineStyle?.HlSize;
            set { if (value != null) Apply(tp => tp.StyleProps.GetLineStyle().HlSize = (float)value); OnPropertyChanged(); }
        }

        public double? HlMovementPow
        {
            get => (double?)LineStyle?.HlMovementPow;
            set { if (value != null) Apply(tp => tp.StyleProps.GetLineStyle().HlMovementPow = (float)value); OnPropertyChanged(); }
        }

        public bool? MovingHl
        {
            get => LineStyle?.MovingHl;
            set { Apply(tp => tp.StyleProps.GetLineStyle().MovingHl = value ?? false); OnPropertyChanged(); }
        }

        public bool? ShrinkingHl
        {
            get => LineStyle?.ShrinkingHl;
            set { Apply(tp => tp.StyleProps.GetLineStyle().ShrinkingHl = value ?? false); OnPropertyChanged(); }
        }

        public bool? HlBorder
        {
            get => LineStyle?.HlBorder;
            set { Apply(tp => tp.StyleProps.GetLineStyle().HlBorder = value ?? false); OnPropertyChanged(); }
        }

        // ---- Default buttons ----

        public Action ResetStyle { get; set; }
        public Action ResetMaterial { get; set; }
        public Action ResetLight { get; set; }
        public Action ResetSpatial { get; set; }
        /// <summary>Re-bake geometry at the new effective viewport width after a commit (wired by MainViewModel).</summary>
        public Action CommitViewWidth { get; set; }

        // ---- Per-tab save/load (track-properties context menu) ----

        /// <summary>Save the currently-open tab's properties to a .tp file (wired by MainViewModel).</summary>
        public Action SaveCurrentTab { get; set; }
        /// <summary>Load the currently-open tab's properties from a .tp file (wired by MainViewModel).</summary>
        public Action LoadCurrentTab { get; set; }
        /// <summary>Reset the selected track(s) to default properties (wired by MainViewModel).</summary>
        public Action DefaultProps { get; set; }

        /// <summary>Number of tracks selected in the track list; drives context-menu enable state.
        /// Set by MainViewModel on every selection change.</summary>
        public int SelectedTrackCount { get; set; }

        // ---- Modulation ----

        public bool ModulationVisible => SP?.Type == NoteStyleType.Bar || SP?.Type == NoteStyleType.Line;

        // Snapshot the live List<NoteStyleMod> so each OnPropertyChanged(nameof(ModEntries)) hands the
        // ComboBox a fresh collection reference. AddModEntry/CloneModEntry/DeleteModEntry mutate the
        // model list in place without CollectionChanged notifications; binding ItemsSource to that live
        // list lets WPF reuse a stale collection view, which shows blank entries and eventually throws
        // "ItemsControl is inconsistent with its items source". A new list forces a clean rebuild.
        public System.Collections.IEnumerable ModEntries => AS?.ModEntries?.ToList();

        public bool ModEntryComboEnabled => AS?.ModEntries != null;

        public bool ModEntryDetailsVisible => ME != null;

        public int ModEntryIndex
        {
            get => AS?.SelectedModEntryIndex ?? -1;
            set
            {
                // The ComboBox writes -1 back while ItemsSource is being swapped on track switch;
                // ignore it (and no-op reselects) so each track's stored SelectedModEntryIndex —
                // its "last selected modulation" — survives and is re-displayed by RefreshAll.
                if (value < 0 || value == (AS?.SelectedModEntryIndex ?? -1)) return;
                SelectModEntry?.Invoke(value);
            }
        }

        // Callbacks for structural modulation changes (wired in MainViewModel)
        public Action AddModEntry { get; set; }
        public Action CloneModEntry { get; set; }
        public Action DeleteModEntry { get; set; }
        public Action<int> SelectModEntry { get; set; }

        // -- Pixel position --

        public double? ModXOrigin
        {
            get => (double?)ME?.XOrigin;
            set { if (value != null) ApplyMod(m => m.XOrigin = (float)value); OnPropertyChanged(); }
        }

        public double? ModYOrigin
        {
            get => (double?)ME?.YOrigin;
            set { if (value != null) ApplyMod(m => m.YOrigin = (float)value); OnPropertyChanged(); }
        }

        public bool? ModXOriginEnable
        {
            get => ME?.XOriginEnable;
            set
            {
                ApplyMod(m => m.XOriginEnable = value ?? false);
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModXOriginEnabled));
                OnPropertyChanged(nameof(ModCombineEnabled));
            }
        }

        public bool? ModYOriginEnable
        {
            get => ME?.YOriginEnable;
            set
            {
                ApplyMod(m => m.YOriginEnable = value ?? false);
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModYOriginEnabled));
                OnPropertyChanged(nameof(ModCombineEnabled));
            }
        }

        public bool ModXOriginEnabled => ME?.XOriginEnable == true;
        public bool ModYOriginEnabled => ME?.YOriginEnable == true;
        public bool ModCombineEnabled => ME?.XOriginEnable == true && ME?.YOriginEnable == true;

        public int ModCombineIndex
        {
            get => ME?.CombineXY ?? -1;
            set { if (value >= 0) ApplyMod(m => m.CombineXY = value); OnPropertyChanged(); }
        }

        public bool? ModSquareAspect
        {
            get => ME?.SquareAspect;
            set { ApplyMod(m => m.SquareAspect = value ?? false); OnPropertyChanged(); }
        }

        // -- Destinations --

        public bool? ModColorDestEnable
        {
            get => ME?.ColorDestEnable;
            set { ApplyMod(m => m.ColorDestEnable = value ?? false); OnPropertyChanged(); }
        }

        public XnaColor? ModColorDest
        {
            get => ME?.ColorDest;
            set { if (value != null) ApplyMod(m => m.ColorDest = value); OnPropertyChanged(); }
        }

        public bool? ModAngleDestEnable
        {
            get => ME?.AngleDestEnable;
            set { ApplyMod(m => m.AngleDestEnable = value ?? false); OnPropertyChanged(); }
        }

        public double? ModAngleDest
        {
            get => (double?)ME?.AngleDest;
            set { if (value != null) ApplyMod(m => m.AngleDest = (int)value); OnPropertyChanged(); }
        }

        // -- Interpolation --

        public double? ModStart
        {
            get => (double?)ME?.Start;
            set { if (value != null) ApplyMod(m => m.Start = (float)value); OnPropertyChanged(); }
        }

        public double? ModStop
        {
            get => (double?)ME?.Stop;
            set { if (value != null) ApplyMod(m => m.Stop = (float)value); OnPropertyChanged(); }
        }

        public double? ModFadeIn
        {
            get => (double?)ME?.FadeIn;
            set { if (value != null) ApplyMod(m => m.FadeIn = (float)value); OnPropertyChanged(); }
        }

        public double? ModFadeOut
        {
            get => (double?)ME?.FadeOut;
            set { if (value != null) ApplyMod(m => m.FadeOut = (float)value); OnPropertyChanged(); }
        }

        public double? ModPower
        {
            get => (double?)ME?.Power;
            set { if (value != null) ApplyMod(m => m.Power = (float)value); OnPropertyChanged(); }
        }

        public bool? ModDiscardAfterStop
        {
            get => ME?.DiscardAfterStop;
            set { ApplyMod(m => m.DiscardAfterStop = value ?? false); OnPropertyChanged(); }
        }

        public bool? ModInvert
        {
            get => ME?.Invert;
            set { ApplyMod(m => m.Invert = value ?? false); OnPropertyChanged(); }
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
            set { if (value != null) ApplyMaterial(tp => tp.MaterialProps.Hue = (float)value); OnPropertyChanged(); }
        }

        public double? NormalSat
        {
            get => (double?)MP?.Normal.Sat;
            set { if (value != null) ApplyMaterial(tp => tp.MaterialProps.Normal.Sat = (float)value); OnPropertyChanged(); }
        }

        public double? NormalLum
        {
            get => (double?)MP?.Normal.Lum;
            set { if (value != null) ApplyMaterial(tp => tp.MaterialProps.Normal.Lum = (float)value); OnPropertyChanged(); }
        }

        public double? HiliteSat
        {
            get => (double?)MP?.Hilited.Sat;
            set { if (value != null) ApplyMaterial(tp => tp.MaterialProps.Hilited.Sat = (float)value); OnPropertyChanged(); }
        }

        public double? HiliteLum
        {
            get => (double?)MP?.Hilited.Lum;
            set { if (value != null) ApplyMaterial(tp => tp.MaterialProps.Hilited.Lum = (float)value); OnPropertyChanged(); }
        }

        TrackPropsTex TexProps => MP?.TexProps;

        public bool? DisableTexture
        {
            get => TexProps?.DisableTexture;
            set { Apply(tp => tp.MaterialProps.TexProps.DisableTexture = value ?? false); OnPropertyChanged(); }
        }

        public bool? PointSmp
        {
            get => TexProps?.PointSmp;
            set { Apply(tp => tp.MaterialProps.TexProps.PointSmp = value ?? false); OnPropertyChanged(); }
        }

        public bool? TexColBlend
        {
            get => TexProps?.TexColBlend;
            set { Apply(tp => tp.MaterialProps.TexProps.TexColBlend = value ?? false); OnPropertyChanged(); }
        }

        public bool? UTile
        {
            get => TexProps?.UTile;
            // Bug fix: route through Rebuild so createGeos() is called (tex coords baked into geometry)
            set { Rebuild(tp => tp.MaterialProps.TexProps.UTile = value ?? false); OnPropertyChanged(); }
        }

        public bool? VTile
        {
            get => TexProps?.VTile;
            set { Rebuild(tp => tp.MaterialProps.TexProps.VTile = value ?? false); OnPropertyChanged(); }
        }

        public bool? KeepAspect
        {
            get => TexProps?.KeepAspect;
            set { Rebuild(tp => tp.MaterialProps.TexProps.KeepAspect = value ?? false); OnPropertyChanged(); }
        }

        public int UAnchorIndex
        {
            get => TexProps?.UAnchor == null ? -1 : (int)TexProps.UAnchor;
            set
            {
                if (value < 0) return;
                Rebuild(tp => tp.MaterialProps.TexProps.UAnchor =
                    (TexAnchorEnum)Math.Clamp(value, 0, 2));
                OnPropertyChanged();
                OnPropertyChanged(nameof(UAnchorNote));
                OnPropertyChanged(nameof(UAnchorScreen));
                OnPropertyChanged(nameof(UAnchorSong));
            }
        }

        public int VAnchorIndex
        {
            get => TexProps?.VAnchor == null ? -1 : (int)TexProps.VAnchor;
            set
            {
                if (value < 0) return;
                Rebuild(tp => tp.MaterialProps.TexProps.VAnchor =
                    (TexAnchorEnum)Math.Clamp(value, 0, 1));
                OnPropertyChanged();
                OnPropertyChanged(nameof(VAnchorNote));
                OnPropertyChanged(nameof(VAnchorScreen));
            }
        }

        public bool UAnchorNote
        {
            get => TexProps?.UAnchor == TexAnchorEnum.Note;
            set
            {
                if (value) Rebuild(tp => tp.MaterialProps.TexProps.UAnchor = TexAnchorEnum.Note);
                OnPropertyChanged();
                OnPropertyChanged(nameof(UAnchorIndex));
                OnPropertyChanged(nameof(UAnchorScreen));
                OnPropertyChanged(nameof(UAnchorSong));
            }
        }
        public bool UAnchorScreen
        {
            get => TexProps?.UAnchor == TexAnchorEnum.Screen;
            set
            {
                if (value) Rebuild(tp => tp.MaterialProps.TexProps.UAnchor = TexAnchorEnum.Screen);
                OnPropertyChanged();
                OnPropertyChanged(nameof(UAnchorIndex));
                OnPropertyChanged(nameof(UAnchorNote));
                OnPropertyChanged(nameof(UAnchorSong));
            }
        }
        public bool UAnchorSong
        {
            get => TexProps?.UAnchor == TexAnchorEnum.Song;
            set
            {
                if (value) Rebuild(tp => tp.MaterialProps.TexProps.UAnchor = TexAnchorEnum.Song);
                OnPropertyChanged();
                OnPropertyChanged(nameof(UAnchorIndex));
                OnPropertyChanged(nameof(UAnchorNote));
                OnPropertyChanged(nameof(UAnchorScreen));
            }
        }
        public bool VAnchorNote
        {
            get => TexProps?.VAnchor == TexAnchorEnum.Note;
            set
            {
                if (value) Rebuild(tp => tp.MaterialProps.TexProps.VAnchor = TexAnchorEnum.Note);
                OnPropertyChanged();
                OnPropertyChanged(nameof(VAnchorIndex));
                OnPropertyChanged(nameof(VAnchorScreen));
            }
        }
        public bool VAnchorScreen
        {
            get => TexProps?.VAnchor == TexAnchorEnum.Screen;
            set
            {
                if (value) Rebuild(tp => tp.MaterialProps.TexProps.VAnchor = TexAnchorEnum.Screen);
                OnPropertyChanged();
                OnPropertyChanged(nameof(VAnchorIndex));
                OnPropertyChanged(nameof(VAnchorNote));
            }
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

        string _textureThumbnailPath;
        BitmapSource _textureThumbnailCache;

        BitmapSource BuildTextureThumbnail()
        {
            var path = TexProps?.Path ?? "";
            if (path == _textureThumbnailPath)
                return _textureThumbnailCache;

            _textureThumbnailPath = path;
            _textureThumbnailCache = null;

            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;  // load fully into memory, don't lock the file
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                _textureThumbnailCache = bmp;
                return _textureThumbnailCache;
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
            set { Apply(tp => tp.LightProps.UseGlobalLight = value ?? false); OnPropertyChanged(); }
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

        public double? PitchOffset
        {
            get => (double?)SpatP?.PitchOffset;
            set { if (value != null) Apply(tp => tp.SpatialProps.PitchOffset = (float)value); OnPropertyChanged(); }
        }

        public double? ViewWidthQn
        {
            get => (double?)SpatP?.ViewWidthQn;
            set
            {
                if (value != null && value <= 0) return;   // reject 0/negative typed values
                Apply(tp => tp.SpatialProps.ViewWidthQn = (float?)value);   // null clears the override (inherit)
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // AUDIO TAB
        // =====================================================================

        public string AudioFilename
        {
            get => _mergedProps?.AudioProps?.Filename ?? "";
            set
            {
                Apply(tp => tp.AudioProps.Filename = value);
                OnPropertyChanged();
                _ = LoadSelectedTracksAudio?.Invoke();
            }
        }

        /// <summary>
        /// Silence threshold in seconds as text; empty = inherit the global track's value.
        /// String-typed so the box can be cleared (and shows blank when selected tracks differ).
        /// Committed per keystroke (UpdateSourceTrigger=PropertyChanged) so a refresh — e.g. at
        /// playback start — can't revert a pending edit; hence no OnPropertyChanged here, which
        /// would disturb the caret while typing. Invalid text simply leaves the last valid value.
        /// </summary>
        public string SilenceThreshold
        {
            get => _mergedProps?.AudioProps?.SilenceThresholdS?.ToString() ?? "";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Apply(tp => tp.AudioProps.SilenceThresholdS = null);
                else if (float.TryParse(value, out float s) && s >= 0)
                    Apply(tp => tp.AudioProps.SilenceThresholdS = s);
            }
        }

        public Action BrowseAudioFile { get; set; }
        public Func<Task> LoadSelectedTracksAudio { get; set; }
    }
}
