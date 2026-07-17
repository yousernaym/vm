using LibSidWiz.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;


namespace VisualMusic
{
    public enum TexAnchorEnum { Note = 0, Screen, Song };
    enum TrackPropsType { TPT_Style = 1, TPT_Material = 2, TPT_Light = 4, TPT_Spatial = 8, TPT_Audio = 16, TPT_All = 255 }

    [Serializable()]
    public class TrackProps : ISerializable
    {
        internal TrackView TrackView { get; set; }
        static int NumTracks { get => TrackView.NumTracks; }
        int TrackNumber { get => TrackView.TrackNumber; }
        internal TrackProps GlobalProps { get; set; }

        internal NoteStyle ActiveNoteStyle
        {
            get => StyleProps.GetActiveStyle(TrackNumber, GlobalProps);
            //set => Style.setSelectedNoteStyle(value);
        }

        public StyleProps StyleProps { get; set; }
        public MaterialProps MaterialProps { get; set; }
        public LightProps LightProps { get; set; }
        public SpatialProps SpatialProps { get; set; }
        public AudioProps AudioProps { get; set; }
        public int TypeFlags { get; set; } //Determines which type of properties should be saved or loaded to/from file.

        public TrackProps(TrackView view)
        {
            TrackView = view;
            ResetProps();
        }

        public TrackProps(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "style")
                    StyleProps = (StyleProps)entry.Value;
                else if (entry.Name == "material")
                    MaterialProps = (MaterialProps)entry.Value;
                else if (entry.Name == "light")
                    LightProps = (LightProps)entry.Value;
                else if (entry.Name == "spatial")
                    SpatialProps = (SpatialProps)entry.Value;
                else if (entry.Name == "audio")
                    AudioProps = (AudioProps)entry.Value;
                else if (entry.Name == "typeFlags")
                    TypeFlags = (int)entry.Value;

            }
            if (AudioProps == null)
                AudioProps = new();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("style", StyleProps);
            info.AddValue("material", MaterialProps);
            info.AddValue("light", LightProps);
            info.AddValue("spatial", SpatialProps);
            info.AddValue("audio", AudioProps);
            info.AddValue("typeFlags", TypeFlags);
        }

        public void LoadContent()
        {
            MaterialProps.LoadContent();
        }

        public void ResetStyle()
        {
            StyleProps = new StyleProps(TrackNumber);
        }

        // Parameterless reset assumes a contiguous track set (ordinal == TrackNumber-1), which holds at
        // track-creation time. After a sparse removal, callers that re-apply defaults (the "Default
        // material" button) pass the track's true ordinal position via the overload below.
        public void ResetMaterial()
        {
            ResetMaterial(TrackNumber - 1, NumTracks - 1);
        }

        public void ResetMaterial(int hueOrdinal, int hueCount)
        {
            MaterialProps = new MaterialProps(TrackNumber, MaterialProps.DefaultHue(hueOrdinal, hueCount));
        }

        public void ResetLight()
        {
            LightProps = new LightProps(TrackNumber);
        }

        public void ResetSpatial()
        {
            SpatialProps = new SpatialProps();
            if (TrackView != null && TrackView.TrackNumber == 0)
                SpatialProps.ViewWidthQn = ProjProps.DefaultViewWidthQn;
        }

        public void ResetProps()
        {
            ResetMaterial();
            ResetStyle();
            ResetLight();
            ResetSpatial();
            ResetAudio();
        }

        void ResetAudio()
        {
            AudioProps?.Dispose();
            AudioProps = new AudioProps();
        }

        /// <summary>Resets only the audio settings (silence threshold, waveform view width and the
        /// trigger settings), keeping the assigned audio file. Unlike <see cref="ResetAudio"/>, does
        /// not dispose AudioProps or clear the filename. Regular tracks reset to null (inherit
        /// global); the Global track has nothing to inherit, so it resets to the hardcoded defaults
        /// (mirrors <see cref="ResetSpatial"/>).</summary>
        public void ResetAudioSettings()
        {
            bool isGlobal = TrackView != null && TrackView.TrackNumber == 0;
            AudioProps.SilenceThresholdS = isGlobal ? AudioProps.DefaultSilenceThresholdS : null;
            AudioProps.WaveformViewWidthMs = isGlobal ? AudioProps.DefaultViewWidthMs : null;
            AudioProps.TriggerAlgorithmName = isGlobal ? AudioProps.TriggerAlgorithms[0].Type.Name : null;
            AudioProps.TriggerLookaheadFrames = isGlobal ? AudioProps.DefaultTriggerLookahead : (int?)null;
            AudioProps.TriggerLookaheadOnFailureFrames = isGlobal ? AudioProps.DefaultTriggerLookaheadOnFailure : (int?)null;
            AudioProps.ShapeStability = isGlobal ? AudioProps.DefaultShapeStability : (float?)null;
            AudioProps.PitchSplitLayout = isGlobal ? AudioProps.DefaultPitchSplitLayout : (int?)null;
        }

        public TrackProps Clone(ISongDrawHost host = null)
        {
            TrackProps newProps = new TrackProps(TrackView);
            newProps.CloneFrom(this, (int)TrackPropsType.TPT_All, host);
            return newProps;
        }

        internal void CloneFrom(TrackProps source, int type, ISongDrawHost host = null)
        {
            if ((type & (int)TrackPropsType.TPT_Style) > 0)
                StyleProps = source.StyleProps.Clone();
            if ((type & (int)TrackPropsType.TPT_Material) > 0)
                MaterialProps = source.MaterialProps.Clone();
            if ((type & (int)TrackPropsType.TPT_Light) > 0)
                LightProps = source.LightProps.Clone();
            if ((type & (int)TrackPropsType.TPT_Spatial) > 0)
                SpatialProps = source.SpatialProps.Clone();
            if ((type & (int)TrackPropsType.TPT_Audio) > 0)
                AudioProps = source.AudioProps;
        }
    }

    [Serializable()]
    public class TrackPropsTex : ISerializable
    {
        internal Texture2D Texture { get; set; } = null;
        internal Texture2D TransitionTexture { get; private set; } = null;
        internal Texture2D CoordTexture => Texture ?? TransitionTexture;
        internal float TextureBlend { get; private set; } = 0;
        public string Path { get; set; } = "";
        string _transitionPath = "";

        SamplerState _samplerState = new SamplerState();
        SamplerState _samplerStateBacking = new SamplerState();
        bool _dirtySamplerState = true;
        public SamplerState SamplerStateBacking
        {
            get { _dirtySamplerState = true; return _samplerStateBacking; }
            //set { samplerStateBacking = value; }
        }
        public SamplerState SamplerState
        {
            get
            {
                if (_dirtySamplerState)
                {
                    _samplerState.Dispose();
                    _samplerState = new SamplerState();
                    _samplerState.AddressU = _samplerStateBacking.AddressU;
                    _samplerState.AddressV = _samplerStateBacking.AddressV;
                    _samplerState.Filter = _samplerStateBacking.Filter;
                    _samplerState.MaxAnisotropy = _samplerStateBacking.MaxAnisotropy;
                    _samplerState.MaxMipLevel = _samplerStateBacking.MaxMipLevel;
                    _dirtySamplerState = false;
                }
                return _samplerState;
            }
            //set { samplerState = value; }
        }
        public bool? DisableTexture { get; set; } = false;
        bool? _pointSmp = false;
        public bool? PointSmp
        {
            get => _pointSmp;
            set
            {
                _pointSmp = value;
                if (value != null)
                    SamplerStateBacking.Filter = (bool)value ? TextureFilter.Point : TextureFilter.Linear;
            }
        }

        public bool? TexColBlend { get; set; } = false;

        bool? _uTile = false;
        public bool? UTile
        {
            get { return _uTile; }
            set { _uTile = value; }
        }
        bool? _vTile = false;
        public bool? VTile
        {
            get { return _vTile; }
            set { _vTile = value; }
        }
        bool? _keepAspect = false;
        public bool? KeepAspect
        {
            get { return _keepAspect; }
            set { _keepAspect = value; }
        }
        Point Anchor
        {
            get => new Point((int)UAnchor, (int)VAnchor);
            set
            {
                UAnchor = (TexAnchorEnum)value.X;
                VAnchor = (TexAnchorEnum)value.Y;
            }
        }
        public TexAnchorEnum? UAnchor { get; set; } = TexAnchorEnum.Note;
        public TexAnchorEnum? VAnchor { get; set; } = TexAnchorEnum.Note;

        //Vector2? scroll;
        internal Vector2 Scroll
        {
            get => new Vector2((float)UScroll, (float)VScroll);
            set
            {
                UScroll = value.X;
                VScroll = value.Y;
            }
        }
        public float? UScroll { get; set; } = 0;
        public float? VScroll { get; set; } = 0;

        //Methods----------------------
        public TrackPropsTex()
        {
            _samplerStateBacking.AddressU = TextureAddressMode.Wrap;
            _samplerStateBacking.AddressV = TextureAddressMode.Wrap;
        }
        public TrackPropsTex(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (var entry in info)
            {
                if (entry.Name == "path")
                    Path = (string)entry.Value;
                else if (entry.Name == "disableTexture")
                    DisableTexture = (bool)entry.Value;
                else if (entry.Name == "pointSmp")
                    PointSmp = (bool)entry.Value;
                else if (entry.Name == "texColBlend")
                    TexColBlend = (bool)entry.Value;
                else if (entry.Name == "keepAspect")
                    _keepAspect = (bool)entry.Value;
                else if (entry.Name == "uTile")
                    _uTile = (bool)entry.Value;
                else if (entry.Name == "vTile")
                    _vTile = (bool)entry.Value;
                else if (entry.Name == "anchor")
                    Anchor = (Point)entry.Value;
                else if (entry.Name == "scroll")
                    Scroll = (Vector2)entry.Value;
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("path", Path);
            info.AddValue("disableTexture", DisableTexture);
            info.AddValue("pointSmp", PointSmp);
            info.AddValue("texColBlend", TexColBlend);
            info.AddValue("keepAspect", _keepAspect);
            info.AddValue("uTile", _uTile);
            info.AddValue("vTile", _vTile);
            info.AddValue("anchor", Anchor);
            info.AddValue("scroll", Scroll);
        }
        Texture2D CreateMipLevels(Texture2D tex, GraphicsDevice gd, SpriteBatch sb)
        {
            RenderTarget2D renderTarget = new RenderTarget2D(gd, tex.Width, tex.Height, true, SurfaceFormat.Color, DepthFormat.None);
            gd.SetRenderTarget(renderTarget);
            sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);
            sb.Draw(tex, new Vector2(0, 0), Color.White);
            sb.End();
            Texture2D outTex = (Texture2D)renderTarget;
            gd.SetRenderTarget(null);
            tex.Dispose();
            return outTex;
        }

        public bool LoadTexture(string path, FileStream stream, ISongDrawHost host)
            => LoadTexture(path, stream, host.GraphicsDevice, host.SpriteBatch);

        bool LoadTexture(string path, FileStream stream, GraphicsDevice gd, SpriteBatch sb)
        {
            ClearTextureTransition();
            Path = path;
            Texture2D tex = Texture;
            if (tex != null) { tex.Dispose(); tex = null; }
            tex = Texture2D.FromStream(gd, stream);
            Texture = CreateMipLevels(tex, gd, sb);
            return tex != null;
        }

        public bool LoadTexture(string path, ISongDrawHost host)
        {
            using var stream = File.Open(path, FileMode.Open);
            ClearTextureTransition();
            return LoadTexture(path, stream, host);
        }
        public void UnloadTexture()
        {
            ClearTextureTransition();
            Path = "";
            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }
        }

        internal bool SetTextureTransition(string path, float blend, ISongDrawHost host)
        {
            path ??= "";
            blend = Math.Clamp(blend, 0f, 1f);

            if (blend <= 0 || string.Equals(path, Path ?? "", StringComparison.OrdinalIgnoreCase))
            {
                bool changed = TransitionTexture != null || !string.IsNullOrEmpty(_transitionPath);
                ClearTextureTransition();
                return changed;
            }

            bool sourceChanged = !string.Equals(path, _transitionPath, StringComparison.OrdinalIgnoreCase);
            TextureBlend = blend;

            if (string.IsNullOrEmpty(path))
            {
                if (TransitionTexture != null)
                {
                    TransitionTexture.Dispose();
                    TransitionTexture = null;
                }
                _transitionPath = "";
                return sourceChanged;
            }

            if (!sourceChanged && TransitionTexture != null)
                return false;

            bool hadTransitionTexture = TransitionTexture != null;
            if (TransitionTexture != null)
            {
                TransitionTexture.Dispose();
                TransitionTexture = null;
            }

            _transitionPath = path;
            if (host == null)
                return sourceChanged;

            try
            {
                using var stream = File.Open(path, FileMode.Open);
                Texture2D tex = Texture2D.FromStream(host.GraphicsDevice, stream);
                TransitionTexture = CreateMipLevels(tex, host.GraphicsDevice, host.SpriteBatch);
            }
            catch
            {
                TransitionTexture = null;
            }

            return sourceChanged || hadTransitionTexture != (TransitionTexture != null);
        }

        internal void ClearTextureTransition()
        {
            TextureBlend = 0;
            _transitionPath = "";
            if (TransitionTexture != null)
            {
                TransitionTexture.Dispose();
                TransitionTexture = null;
            }
        }

        internal void LoadContent()
        {
            //Deserialization inits Path but not Texture because Texture2D is not serializable, and you can't load texture before the device is created.
            try
            {
                if (!string.IsNullOrEmpty(Path))
                    LoadTexture(Path, Project.StaticDrawHost);
            }
            catch (Exception)
            {
                MetroMessageBox.Show("Failed to load texture " + Path);
            }
        }
    }

    //Highlighted or normal note material (currently same for both)
    [Serializable()]
    public class NoteTypeMaterial : ISerializable
    {
        //TrackProps parent;
        float? _sat = 1;
        public float? Sat
        {
            get { return _sat; }
            set { _sat = value; }
        }
        float? _lum = 1;
        public float? Lum
        {
            get { return _lum; }
            set { _lum = value; }
        }
        Texture2D _texture = null;
        public Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }
        //public Color color;
        //public Color Color
        //{
        //	get { return color; }
        //}
        //public System.Drawing.Color SysColor
        //{
        //	get { return System.Drawing.Color.FromArgb(color.R, color.G, color.B); }
        //}
        public NoteTypeMaterial()
        {
            _sat = 1;
            _lum = 1;
            _texture = null;
        }
        public NoteTypeMaterial(float sat, float lum, Texture2D tex = null)
        {
            _sat = sat;
            _lum = lum;
            _texture = tex;
        }
        public NoteTypeMaterial(SerializationInfo info, StreamingContext ctxt)
        {
            _sat = (float)info.GetValue("sat", typeof(float));
            _lum = (float)info.GetValue("lum", typeof(float));
            _texture = (Texture2D)info.GetValue("texture", typeof(Texture2D));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("sat", _sat);
            info.AddValue("lum", _lum);
            info.AddValue("texture", _texture);
        }
    }

    //---------------------------------------
    //Tab props

    [Serializable()]
    public class StyleProps : ISerializable
    {
        public NoteStyleType? Type { get; set; }
        NoteStyle[] _styles = new NoteStyle[Enum.GetNames(typeof(NoteStyleType)).Length];

        public StyleProps(int trackNumber)
        {
            int[] styleTypes = (int[])Enum.GetValues(typeof(NoteStyleType));
            string[] styleNames = (string[])Enum.GetNames(typeof(NoteStyleType));
            for (int i = 0; i < _styles.Length; i++)
            {
                if ((NoteStyleType)styleTypes[i] != NoteStyleType.Default)
                {
                    _styles[i] = (NoteStyle)Activator.CreateInstance(System.Type.GetType("VisualMusic.NoteStyle_" + styleNames[i]));
                    _styles[i].LoadFx();
                }
            }
            if (trackNumber == 0)
            {
                Type = NoteStyleType.Bar;
                //UseGlobalLight = false;
            }
            else
            {
                Type = NoteStyleType.Default;
                //UseGlobalLight = true;
            }
        }

        public StyleProps(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "noteStyles")
                {
                    _styles = (NoteStyle[])entry.Value;
                    LoadFx();
                }
                else if (entry.Name == "noteStyleType")
                    Type = (NoteStyleType)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("noteStyles", _styles);
            info.AddValue("noteStyleType", Type);
        }

        public NoteStyle GetActiveStyle(int trackNumber, TrackProps globalProps)
        {
            if (Type == NoteStyleType.Default)
            {
                if (trackNumber == 0)  //Global track
                    return GetBarStyle();
                else
                    return globalProps.ActiveNoteStyle;
            }
            else
                return GetStyle(Type);
        }

        internal NoteStyle SelectedStyle
        {
            get => GetStyle(Type);
            set
            {
                if (value.GetType() == typeof(NoteStyle_Bar))
                {
                    Type = NoteStyleType.Bar;
                    _styles[(int)NoteStyleType.Bar] = value;
                }
                else if (value.GetType() == typeof(NoteStyle_Line))
                {
                    Type = NoteStyleType.Line;
                    _styles[(int)NoteStyleType.Line] = value;
                }
                else if (value == null)
                    Type = NoteStyleType.Default;
            }
        }

        public NoteStyle GetStyle(NoteStyleType? type)
        {
            return _styles[(int)type];
        }
        public NoteStyle_Bar GetBarStyle()
        {
            return (NoteStyle_Bar)_styles[(int)NoteStyleType.Bar];
        }
        public NoteStyle_Line GetLineStyle()
        {
            return (NoteStyle_Line)_styles[(int)NoteStyleType.Line];
        }

        public void LoadFx()
        {
            foreach (NoteStyle ns in _styles)
            {
                if (ns != null)
                    ns.LoadFx();
            }
        }

        public StyleProps Clone()
        {
            StyleProps dest = Cloning.Clone(this);
            //dest.loadFx();
            return dest;
        }
    }

    [Serializable()]
    public class MaterialProps : ISerializable
    {
        public float? Transp { get; set; }
        public float? Hue { get; set; }
        NoteTypeMaterial _normal;
        public NoteTypeMaterial Normal { get => _normal; set => _normal = value; }
        public NoteTypeMaterial Hilited { get; set; }
        public TrackPropsTex TexProps { get; set; } = new TrackPropsTex();
        public TrackPropsTex HmapProps { get; set; } = new TrackPropsTex();

        /// <summary>
        /// Even hue spread for a non-global track's default material: the track's ordinal position
        /// among the non-global tracks (0-based) over the number of non-global tracks. Basing it on
        /// ordinal position (not the raw, possibly-sparse <see cref="TrackView.TrackNumber"/>) keeps
        /// the spread even after a middle track is removed, and dividing by the count (not count-1)
        /// keeps the last track short of hue 1.0 (which wraps back to the first track's hue 0).
        /// </summary>
        public static float DefaultHue(int hueOrdinal, int hueCount)
            => hueCount > 0 ? (float)hueOrdinal / hueCount : 0f;

        public MaterialProps(int trackNumber, float hue)
        {
            TexProps.UnloadTexture();
            TexProps = new TrackPropsTex();
            if (trackNumber == 0)
            {
                Transp = 0.5f;
                Hue = 0.1f;
                Normal = new NoteTypeMaterial(1, 0.6f);
                Hilited = new NoteTypeMaterial(1, 1.5f);
            }
            else
            {
                Transp = 1;
                Hue = hue;
                Normal = new NoteTypeMaterial();
                Hilited = new NoteTypeMaterial(); ;
            }
        }

        public MaterialProps(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "transp")
                    Transp = (float)entry.Value;
                else if (entry.Name == "hue")
                    Hue = (float)entry.Value;
                else if (entry.Name == "normal")
                    Normal = (NoteTypeMaterial)entry.Value;
                else if (entry.Name == "hilited")
                    Hilited = (NoteTypeMaterial)entry.Value;
                else if (entry.Name == "texProps")
                    TexProps = (TrackPropsTex)entry.Value;
                else if (entry.Name == "hmapProps")
                    HmapProps = (TrackPropsTex)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("transp", Transp);
            info.AddValue("hue", Hue);
            info.AddValue("normal", Normal);
            info.AddValue("hilited", Hilited);
            info.AddValue("texProps", TexProps);
            info.AddValue("hmapProps", HmapProps);
        }

        internal void LoadContent()
        {
            TexProps.LoadContent();
            HmapProps.LoadContent();
        }

        public Texture2D GetTexture(bool bhilited, MaterialProps globalMaterial)
        {
            Texture2D tex;
            if (bhilited && Hilited.Texture != null)
                tex = Hilited.Texture;
            else if (!bhilited && Normal.Texture != null)
                tex = Normal.Texture;
            else
                tex = TexProps.Texture;
            if (tex == null)
            {
                if (globalMaterial == null) //This means texture in globalProps was null, so use default note type texture
                {

                }
                else
                    tex = globalMaterial.GetTexture(bhilited, null);
            }
            return tex;
        }

        public bool HasLocalTextureForRender(bool bhilited)
        {
            if (bhilited && Hilited.Texture != null)
                return true;
            if (!bhilited && Normal.Texture != null)
                return true;
            return TexProps.Texture != null || TexProps.TransitionTexture != null;
        }

        public System.Drawing.Color GetSysColor(bool bhilited, MaterialProps globalMaterial)
        {
            Vector4 hsla = GetColor(bhilited, globalMaterial);
            //hsla.Z *= 0.5f;
            Color rgba = SongRenderer.HSLA2RGBA(hsla);
            return System.Drawing.Color.FromArgb(rgba.R, rgba.G, rgba.B);
        }
        public Vector4 GetColor(bool bhilited, MaterialProps globalMaterial)
        {
            if (Hue == null)
                return new Vector4(0, 0, 0, 0);
            float h, s, l;
            NoteTypeMaterial tp2;
            NoteTypeMaterial globalTp2;
            if (bhilited)
            {
                tp2 = Hilited;
                globalTp2 = globalMaterial.Hilited;
            }
            else
            {
                tp2 = Normal;
                globalTp2 = globalMaterial.Normal;
            }
            if (tp2.Lum == null || tp2.Sat == null)
                return new Vector4(0, 0, 0, 0);
            h = (float)(Hue + globalMaterial.Hue);
            if (h >= 1)
                h -= 1;
            else if (h < 0)
                h += 1;
            s = (float)(tp2.Sat * globalTp2.Sat);
            l = (float)(tp2.Lum * globalTp2.Lum);
            //if (s > 1)
            //	s = 1;
            //if (l > 1)
            //	l = 1;
            l *= 0.5f;
            return new Vector4(h, s, l, (float)(Transp * globalMaterial.Transp));
        }

        public TrackPropsTex GetTexProps(int selector)
        {
            if (selector == 0)
                return TexProps;
            else
                return HmapProps;
        }
        public MaterialProps Clone()
        {
            MaterialProps dest = Cloning.Clone(this);
            return dest;
        }
    }

    [Serializable()]
    public class LightProps : ISerializable
    {
        public bool? UseGlobalLight { get; set; }
        internal Vector3 Dir
        {
            get => new Vector3((float)DirX, (float)DirY, (float)DirZ);
            set
            {
                DirX = value.X;
                DirY = value.Y;
                DirZ = value.Z;
            }
        }

        public float? DirX { get; set; }
        public float? DirY { get; set; }
        public float? DirZ { get; set; }

        public float? AmbientAmount { get; set; }
        public Color? AmbientColor { get; set; } = Color.White;
        public float? DiffuseAmount { get; set; }
        public Color? DiffuseColor { get; set; } = Color.White;
        public float? SpecAmount { get; set; }
        public Color? SpecColor { get; set; } = Color.White;
        public float? SpecPower { get; set; }
        public float? MasterAmount { get; set; } = 1;
        public Color? MasterColor { get; set; } = Color.White;

        public LightProps(int trackNumber)
        {
            if (trackNumber == 0)
                UseGlobalLight = false;
            else
                UseGlobalLight = true;
            Dir = new Vector3(-1, -1, 1);
            AmbientAmount = 0.2f;
            DiffuseAmount = 2;
            SpecAmount = 1;
            MasterAmount = 1;
            AmbientColor = DiffuseColor = SpecColor = MasterColor = Color.White;
            SpecPower = 50;
        }

        public LightProps(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "dir")
                    Dir = (Vector3)entry.Value;
                else if (entry.Name == "useGlobalLight")
                    UseGlobalLight = (bool)entry.Value;
                else if (entry.Name == "ambientAmount")
                    AmbientAmount = (float)entry.Value;
                else if (entry.Name == "diffuseAmount")
                    DiffuseAmount = (float)entry.Value;
                else if (entry.Name == "specAmount")
                    SpecAmount = (float)entry.Value;
                else if (entry.Name == "ambientColor")
                    AmbientColor = (Color)entry.Value;
                else if (entry.Name == "diffuseColor")
                    DiffuseColor = (Color)entry.Value;
                else if (entry.Name == "specColor")
                    SpecColor = (Color)entry.Value;
                else if (entry.Name == "specPower")
                    SpecPower = (float)entry.Value;
                else if (entry.Name == "masterAmount")
                    MasterAmount = (float)entry.Value;
                else if (entry.Name == "masterColor")
                    MasterColor = (Color)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("dir", Dir);
            info.AddValue("useGlobalLight", UseGlobalLight);
            info.AddValue("ambientAmount", AmbientAmount);
            info.AddValue("diffuseAmount", DiffuseAmount);
            info.AddValue("specAmount", SpecAmount);
            info.AddValue("masterAmount", MasterAmount);
            info.AddValue("ambientColor", AmbientColor);
            info.AddValue("diffuseColor", DiffuseColor);
            info.AddValue("specColor", SpecColor);
            info.AddValue("specPower", SpecPower);
            info.AddValue("masterColor", MasterColor);
        }
    }

    [Serializable()]
    public class SpatialProps : ISerializable
    {
        internal Vector3 PosOffset
        {
            get => new Vector3((float)XOffset, (float)YOffset, (float)ZOffset);
            private set
            {
                XOffset = value.X;
                YOffset = value.Y;
                ZOffset = value.Z;
            }
        }

        public float? XOffset { get; set; }
        public float? YOffset { get; set; }
        public float? ZOffset { get; set; }

        /// <summary>Shifts notes vertically by this many pitches (semitones). Global and track offsets add.</summary>
        public float? PitchOffset { get; set; } = 0;

        /// <summary>Quarter notes visible per screen for this track. Null = inherit the global track's value.</summary>
        public float? ViewWidthQn { get; set; }

        public SpatialProps()
        {
            PosOffset = new Vector3();
        }

        public SpatialProps(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "posOffset")
                    PosOffset = (Vector3)entry.Value;
                else if (entry.Name == "pitchOffset" && entry.Value != null)
                    PitchOffset = Convert.ToSingle(entry.Value);
                else if (entry.Name == "viewWidthQn" && entry.Value != null)
                    ViewWidthQn = Convert.ToSingle(entry.Value);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("posOffset", PosOffset);
            if (PitchOffset != null)
                info.AddValue("pitchOffset", PitchOffset.Value);
            if (ViewWidthQn != null)
                info.AddValue("viewWidthQn", ViewWidthQn.Value);
        }
    }


    [Serializable]
    public class AudioProps : ISerializable, IDisposable
    {
        [IgnoreDataMember]
        public LibSidWiz.Channel SidWizChannel { get; } = new LibSidWiz.Channel(false)
        {
            Algorithm = new PeakSpeedTrigger(),
            // Label rendered above the waveform (top-centered). The font is (re)created per output
            // resolution in Project.RefreshSidWizChannels, which also drives the colour from the
            // track colour; the white here is only the pre-first-frame value. A blank label draws
            // nothing, so the default (empty) shows no text.
            LabelColor = System.Drawing.Color.White,
            LabelAlignment = System.Drawing.ContentAlignment.TopCenter,
            LabelMargins = new LibSidWiz.Padding(4, 4, 4, 4),
            // DC-offset removal is always on: SID digi / FM stems often carry a DC bias that would
            // otherwise push the waveform off-centre. autoReloadOnSettingChanged is false, so this
            // just sets the flag before the first LoadDataAsync().
            HighPassFilter = true,
        };

        /// <summary>
        /// Trigger algorithms offered in the Audio-tab dropdown, in fixed display order. Index 0 is
        /// the hard-coded fallback used by the cascade (per-track "Default" → global track → this).
        /// </summary>
        public static readonly (string Display, Type Type)[] TriggerAlgorithms =
        {
            ("Peak speed", typeof(PeakSpeedTrigger)),
            ("Rising edge", typeof(RisingEdgeTrigger)),
            ("Auto-correlation", typeof(AutoCorrelationTrigger)),
            ("Widest wave", typeof(WidestWaveTrigger)),
            ("Middle widest", typeof(MiddleWidest)),
            ("Biggest wave area", typeof(BiggestWaveAreaTrigger)),
            ("Biggest positive area", typeof(BiggestPositiveWaveAreaTrigger)),
            ("None", typeof(NullTrigger)),
        };

        /// <summary>
        /// Trigger algorithm type name (e.g. "RisingEdgeTrigger"). Null = inherit the global track's
        /// value; when that is also null the first entry of <see cref="TriggerAlgorithms"/> is used.
        /// </summary>
        public string TriggerAlgorithmName { get; set; }

        /// <summary>Default/fallback normal trigger lookahead (used to seed the global track).
        /// 1 = a 2-frame window (~33 ms at the 60 fps SidWiz frame grid), syncing down to ~30 Hz.</summary>
        public const int DefaultTriggerLookahead = 1;

        /// <summary>
        /// Frames the trigger may look ahead (helps low-frequency channels find a stable sync point).
        /// Null = inherit the global track's value; when that is also null,
        /// <see cref="DefaultTriggerLookahead"/> is used.
        /// </summary>
        public int? TriggerLookaheadFrames { get; set; }

        /// <summary>Default/fallback on-failure trigger lookahead (matches the LibSidWiz Channel
        /// default). 3 extra frames beyond the normal window gives ~83 ms total (~12 Hz floor).</summary>
        public const int DefaultTriggerLookaheadOnFailure = 3;

        /// <summary>
        /// Additional frames (beyond the normal lookahead window) the trigger searches when the
        /// normal lookahead finds no trigger at all (e.g. a bass wave whose period is longer than
        /// the frame). Unlike <see cref="TriggerLookaheadFrames"/>, this extended search only runs
        /// on failure frames, so it steadies low-frequency channels without the choppiness a large
        /// always-on lookahead causes. Zero disables the retry. Null = inherit the global track's
        /// value; when that is also null, <see cref="DefaultTriggerLookaheadOnFailure"/> is used.
        /// </summary>
        public int? TriggerLookaheadOnFailureFrames { get; set; }

        /// <summary>Default/fallback shape-stability weight (used to seed the global track). 0 = off.</summary>
        public const float DefaultShapeStability = 0f;

        /// <summary>
        /// Weight (0..1) for preferring the trigger candidate whose surrounding waveform best matches
        /// the previous frame, steadying complex timbres that hop between similar cycles. 0 = position
        /// only. Null = inherit the global track's value; when that is also null,
        /// <see cref="DefaultShapeStability"/> is used.
        /// </summary>
        public float? ShapeStability { get; set; }

        // Pitch-split count is no longer a setting: the number of split waveforms is automatic (one
        // per source channel an instrument is played on; see Project.RefreshSidWizChannels).

        /// <summary>Default/fallback pitch-split layout (used to seed the global track). 1 = Overlaid.</summary>
        public const int DefaultPitchSplitLayout = 1;

        /// <summary>
        /// How split waveforms are arranged, as a <see cref="LibSidWiz.SplitLayout"/> value. Null =
        /// inherit the global track's value; when that is also null,
        /// <see cref="DefaultPitchSplitLayout"/> (Overlaid) is used.
        /// </summary>
        public int? PitchSplitLayout { get; set; }

        public string Filename
        {
            get => SidWizChannel.Filename;
            set => SidWizChannel.Filename = value;
        }

        /// <summary>
        /// Per-voice source WAVs for an exact pitch split: (source channel, path). Empty/null = a
        /// plain single-file track (see <see cref="Filename"/>). Produced by a per-instrument import
        /// with per-track audio; serialised as parallel arrays. Forwarded to the channel by
        /// <see cref="LoadAudioAsync"/>, which then sums the voices and builds the split.
        /// </summary>
        public List<(int Channel, string Path)> VoiceAudioFiles { get; set; }

        /// <summary>User-supplied caption rendered above this track's waveform. Blank by default.</summary>
        public string Label
        {
            get => SidWizChannel.Label;
            set => SidWizChannel.Label = value ?? "";
        }

        public System.Drawing.Color LineColor { get => SidWizChannel.LineColor; set => SidWizChannel.LineColor = value; }

        public const float DefaultSilenceThresholdS = 5;

        /// <summary>
        /// Seconds of upcoming silence after which the track's waveform is hidden.
        /// Null = inherit the global track's value (which defaults to
        /// <see cref="DefaultSilenceThresholdS"/>).
        /// </summary>
        public float? SilenceThresholdS { get; set; }

        /// <summary>Default/fallback waveform time window, in ms (used to seed the global track).</summary>
        public const float DefaultViewWidthMs = 15.0f;

        /// <summary>
        /// Waveform time window in milliseconds (horizontal zoom): smaller = zoom in, larger = zoom out.
        /// Null = inherit the global track's value; when that is also null
        /// <see cref="DefaultViewWidthMs"/> is used.
        /// </summary>
        public float? WaveformViewWidthMs { get; set; }

        public AudioProps()
        {

        }

        public AudioProps(System.Drawing.Color lineColor)
        {
            LineColor = lineColor;
        }

        public AudioProps(SerializationInfo info, StreamingContext ctxt)
        {
            int[] voiceChannels = null;
            string[] voicePaths = null;
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "audioFile")
                {
                    SidWizChannel.Filename = (string)entry.Value;
                    //SidWizChannel.LoadDataAsync();
                }
                else if (entry.Name == "voiceAudioChannels" && entry.Value != null)
                    voiceChannels = (int[])entry.Value;
                else if (entry.Name == "voiceAudioFiles" && entry.Value != null)
                    voicePaths = (string[])entry.Value;
                else if (entry.Name == "audioLabel" && entry.Value != null)
                    SidWizChannel.Label = (string)entry.Value;
                else if (entry.Name == "silenceThreshold" && entry.Value != null)
                    SilenceThresholdS = Convert.ToSingle(entry.Value);
                else if (entry.Name == "waveformViewWidthMs" && entry.Value != null)
                    WaveformViewWidthMs = Convert.ToSingle(entry.Value);
                else if (entry.Name == "triggerAlgorithm" && entry.Value != null)
                    TriggerAlgorithmName = (string)entry.Value;
                else if (entry.Name == "triggerLookahead" && entry.Value != null)
                    TriggerLookaheadFrames = Convert.ToInt32(entry.Value);
                else if (entry.Name == "triggerLookaheadOnFailure" && entry.Value != null)
                    TriggerLookaheadOnFailureFrames = Convert.ToInt32(entry.Value);
                else if (entry.Name == "shapeStability" && entry.Value != null)
                    ShapeStability = Convert.ToSingle(entry.Value);
                else if (entry.Name == "pitchSplitLayout" && entry.Value != null)
                    PitchSplitLayout = Convert.ToInt32(entry.Value);
            }
            if (voiceChannels != null && voicePaths != null && voiceChannels.Length == voicePaths.Length)
            {
                VoiceAudioFiles = new List<(int, string)>(voicePaths.Length);
                for (int i = 0; i < voicePaths.Length; i++)
                    VoiceAudioFiles.Add((voiceChannels[i], voicePaths[i]));
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("audioFile", SidWizChannel.Filename);
            if (!string.IsNullOrEmpty(SidWizChannel.Label))
                info.AddValue("audioLabel", SidWizChannel.Label);
            if (SilenceThresholdS != null)
                info.AddValue("silenceThreshold", SilenceThresholdS.Value);
            if (WaveformViewWidthMs != null)
                info.AddValue("waveformViewWidthMs", WaveformViewWidthMs.Value);
            if (!string.IsNullOrEmpty(TriggerAlgorithmName))
                info.AddValue("triggerAlgorithm", TriggerAlgorithmName);
            if (TriggerLookaheadFrames != null)
                info.AddValue("triggerLookahead", TriggerLookaheadFrames.Value);
            if (TriggerLookaheadOnFailureFrames != null)
                info.AddValue("triggerLookaheadOnFailure", TriggerLookaheadOnFailureFrames.Value);
            if (ShapeStability != null)
                info.AddValue("shapeStability", ShapeStability.Value);
            if (PitchSplitLayout != null)
                info.AddValue("pitchSplitLayout", PitchSplitLayout.Value);
            if (VoiceAudioFiles != null && VoiceAudioFiles.Count > 0)
            {
                info.AddValue("voiceAudioChannels", VoiceAudioFiles.Select(v => v.Channel).ToArray());
                info.AddValue("voiceAudioFiles", VoiceAudioFiles.Select(v => v.Path).ToArray());
            }
        }

        public async Task LoadAudioAsync()
        {
            // Sync the channel's voice-file list from our serialised state so a voice-backed track
            // sums its voices; a plain track (null/empty) loads from Filename as before.
            SidWizChannel.VoiceFiles = VoiceAudioFiles?
                .Select(v => new LibSidWiz.VoiceFile(v.Channel, v.Path)).ToList();
            await SidWizChannel.LoadDataAsync();
        }

        public void Dispose()
        {
            SidWizChannel.Dispose();
        }
    }
}
