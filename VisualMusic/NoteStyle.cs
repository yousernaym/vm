using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace VisualMusic
{
    public enum NoteStyleType { Default, Bar, Line };
    //public enum ModXSourceEnum { DistFromLeft, DistFromCenter, DistFromRight };
    //public enum ModYSourceEnum { DistFromTop, DistFromCenter, DistFromBottom };
    //public enum ModSourceCombineEnum { Add = "iuh", Multiply, Max};

    public struct TestVertex : IVertexType
    {
        public Vector4 pos;
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0));
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
        public TestVertex(Vector4 _pos)
        {
            pos = _pos;
        }
    }

    [Serializable()]
    public class NoteStyleMod : ISerializable
    {
        public string Name { get; set; }
        /// <summary>Stable identity for this mod entry (assigned once, survives reorder/delete of siblings).</summary>
        public string Id { get; set; }
        internal Vector2 Origin
        {
            get => new Vector2((float)XOrigin, (float)YOrigin);
            set
            {
                XOrigin = value.X;
                YOrigin = value.Y;
            }
        }

        public float? XOrigin { get; set; } = 0.5f;
        public float? YOrigin { get; set; } = 0.5f;
        public bool? XOriginEnable { get; set; } = true;
        public bool? YOriginEnable { get; set; } = true;

        public int? CombineXY { get; set; } = 0;
        public bool? SquareAspect { get; set; } = false;
        public bool? ColorDestEnable { get; set; } = false;
        public bool? AlphaDestEnable { get; set; } = false;
        public bool? AngleDestEnable { get; set; } = false;
        public Color? ColorDest { get; set; } = Color.White;
        public int? AngleDest { get; set; } = 45;
        public float RadAngleDest => (float)AngleDest * (float)Math.PI / 180;
        public float? Start { get; set; } = 0;
        public float? Stop { get; set; } = 1;
        public float? FadeIn { get; set; } = 0;
        public float? FadeOut { get; set; } = 0;
        public float? Power { get; set; } = 1;
        public bool? DiscardAfterStop { get; set; } = true;
        public bool? Invert { get; set; } = false;

        public NoteStyleMod(string _name = "")
        {
            Name = _name;
            Id = Guid.NewGuid().ToString("N");
        }

        public NoteStyleMod(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "id")
                    Id = (string)entry.Value;
                else if (entry.Name == "name")
                    Name = (string)entry.Value;
                else if (entry.Name == "origin")
                    Origin = (Vector2)entry.Value;
                else if (entry.Name == "xOriginEnable")
                    XOriginEnable = (bool)entry.Value;
                else if (entry.Name == "yOriginEnable")
                    YOriginEnable = (bool)entry.Value;
                else if (entry.Name == "combineXY")
                    CombineXY = (int)entry.Value;
                else if (entry.Name == "squareAspect")
                    SquareAspect = (bool)entry.Value;
                else if (entry.Name == "colorDestEnable")
                    ColorDestEnable = (bool)entry.Value;
                else if (entry.Name == "angleDestEnable")
                    AngleDestEnable = (bool)entry.Value;
                else if (entry.Name == "colorDest")
                {
                    if (entry.Value is Color) //Compatibility with older save files
                        ColorDest = (Color)entry.Value;
                }
                else if (entry.Name == "angleDest")
                    AngleDest = (int)entry.Value;
                else if (entry.Name == "start")
                    Start = (float)entry.Value;
                else if (entry.Name == "stop")
                    Stop = (float)entry.Value;
                else if (entry.Name == "fadeIn")
                    FadeIn = (float)entry.Value;
                else if (entry.Name == "fadeOut")
                    FadeOut = (float)entry.Value;
                else if (entry.Name == "power")
                    Power = (float)entry.Value;
                else if (entry.Name == "discardAfterStop")
                    DiscardAfterStop = (bool)entry.Value;
                else if (entry.Name == "invert")
                    Invert = (bool)entry.Value;
            }
            // Old files have no "id" — generate a stable one now.
            if (Id == null) Id = Guid.NewGuid().ToString("N");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("id", Id);
            info.AddValue("name", Name);
            info.AddValue("origin", Origin);
            info.AddValue("xOriginEnable", XOriginEnable);
            info.AddValue("yOriginEnable", YOriginEnable);
            info.AddValue("combineXY", CombineXY);
            info.AddValue("squareAspect", SquareAspect);
            info.AddValue("colorDestEnable", ColorDestEnable);
            info.AddValue("angleDestEnable", AngleDestEnable);
            info.AddValue("colorDest", ColorDest);
            info.AddValue("angleDest", AngleDest);
            info.AddValue("start", Start);
            info.AddValue("stop", Stop);
            info.AddValue("fadeIn", FadeIn);
            info.AddValue("fadeOut", FadeOut);
            info.AddValue("power", Power);
            info.AddValue("discardAfterStop", DiscardAfterStop);
            info.AddValue("invert", Invert);
        }

        public NoteStyleMod Clone()
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(NoteStyleMod), ProjectSerializer.KnownTypes);
            MemoryStream stream = new MemoryStream();
            dcs.WriteObject(stream, this);
            stream.Flush();
            stream.Position = 0;
            NoteStyleMod mod = (NoteStyleMod)dcs.ReadObject(stream);
            mod.Id   = Guid.NewGuid().ToString("N");  // new entry → new stable id
            mod.Name += " clone";
            return mod;
        }
    }

    [Serializable()]
    abstract public class NoteStyle : ISerializable
    {
        public float TexTileScale => Project.Props.Camera.ViewportSize.X / 1920;
        protected const int NumDynamicVerts = 30000;
        public class Textures
        {
            public Texture2D hilited;
            public Texture2D normal;
        }

        protected Effect _fx;

        bool HasEffectParameter(string name) => _fx?.Parameters[name] != null;

        //protected TrackProps trackProps = null;
        //public TrackProps TrackProps
        //{
        //	get { return trackProps; }
        //	set { trackProps = value; }
        //}

        protected static SongPanel SongPanel => Form1.SongPanel;

        static GraphicsDevice s_graphicsDeviceOverride;
        static Project s_projectOverride;
        static Microsoft.Xna.Framework.Content.ContentManager s_contentOverride;

        public static void SetGraphicsDevice(GraphicsDevice gd) => s_graphicsDeviceOverride = gd;
        public static void SetProject(Project p) => s_projectOverride = p;
        public static void SetContent(Microsoft.Xna.Framework.Content.ContentManager cm) => s_contentOverride = cm;

        protected static GraphicsDevice GraphicsDevice => s_graphicsDeviceOverride ?? SongPanel?.GraphicsDevice;
        protected static Project Project => s_projectOverride ?? SongPanel?.Project;
        protected static Microsoft.Xna.Framework.Content.ContentManager Content => s_contentOverride ?? SongPanel?.Content;

        //Serializable----------
        protected NoteStyleType _styleType; //Set in constructor of inherited class
                                           //public BindingList<NoteStyleMod> ModEntries { get; set; } = new BindingList<NoteStyleMod>();
        internal List<NoteStyleMod> ModEntries { get; set; } = new List<NoteStyleMod>();
        public int? SelectedModEntryIndex { get; set; } = -1;
        internal NoteStyleMod SelectedModEntry
        {
            get
            {
                if (ModEntries == null || SelectedModEntryIndex == null || SelectedModEntryIndex < 0 || SelectedModEntryIndex >= ModEntries.Count)
                    return null;
                else
                    return ModEntries[(int)SelectedModEntryIndex];
            }
            set
            {
                ModEntries[(int)SelectedModEntryIndex] = value;
            }
        }

        public NoteStyle()
        {
        }
        public NoteStyle(TrackProps tprops)
        {
            //trackProps = tprops;
        }
        public NoteStyle(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "styleType")
                    _styleType = (NoteStyleType)entry.Value;
                else if (entry.Name == "modEntries")
                {
                    ModEntries = (List<NoteStyleMod>)entry.Value;
                    //if (ModEntries != null && ModEntries.Count > 0)
                    //SelectedModEntryIndex = 0;
                }
                else if (entry.Name == "selectedModEntryIndex")
                    SelectedModEntryIndex = (int)entry.Value;
            }
        }

        virtual public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("styleType", _styleType);
            info.AddValue("modEntries", ModEntries);
            info.AddValue("selectedModEntryIndex", SelectedModEntryIndex);
        }

        public static void SInitAllStyles()
        {
            NoteStyle_Bar.SInit();
            NoteStyle_Line.SInit();
        }
        public abstract void LoadFx();
        public abstract void CreateGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial);
        public abstract void DrawGeoChunk(Geo geo);

        protected void GetMaterial(TrackProps trackProps, float x1, float x2, out Vector4 color, out Texture2D texture)
        {
            bool bHilited = false;
            if (x1 < 0 && x2 > 0)
                bHilited = true;
            GetMaterial(trackProps, bHilited, out color, out texture);
        }
        protected void GetMaterial(TrackProps trackProps, bool bHilited, out Vector4 color, out Texture2D texture)
        {
            color = trackProps.MaterialProps.GetColor(bHilited, Project.GlobalTrackProps.MaterialProps);
            texture = trackProps.MaterialProps.GetTexture(bHilited, Project.GlobalTrackProps.MaterialProps);
            //if (texture == null)
            //{
            //	if (bHilited)
            //		texture = defaultTextures[(int)styleType].hilited;
            //	else
            //		texture = defaultTextures[(int)styleType].normal;
            //}
        }
        protected List<Midi.Note> GetNotes(int leftMargin, Midi.Track track)
        {   //Get currently visible notes in specified track
            return track.GetNotes((int)(Project.SongPosT - Project.ViewWidthT / 2 - leftMargin), (int)(Project.SongPosT + Project.ViewWidthT / 2 + leftMargin));
        }

        abstract public void DrawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial);

        protected void DrawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial, out float songPosP)
        {
            float songFade = 1;
            float songPosS = (float)Project.SongPosS;
            float songLength = (float)Project.SongLengthS;
            if (Project.Props.FadeIn > 0 && songPosS < Project.Props.FadeIn)
                songFade = songPosS / Project.Props.FadeIn;
            else if (Project.Props.FadeOut > 0 && songLength - songPosS < Project.Props.FadeOut)
                songFade = (songLength - songPosS) / Project.Props.FadeOut;

            _fx.Parameters["SongFade"].SetValue(songFade);
            _fx.Parameters["BlurredEdge"].SetValue(0.002f * Project.Props.Camera.ViewportSize.X);
            songPosP = Project.SongPosP - Project.PlaybackOffsetP;
            _fx.Parameters["SongPos"].SetValue(songPosP);
            _fx.Parameters["ViewportSize"].SetValue(new Vector2(Project.Props.Camera.ViewportSize.X, Project.Props.Camera.ViewportSize.Y));
            _fx.Parameters["VpMat"].SetValue(Project.Props.Camera.VpMat);
            _fx.Parameters["VertWidthScale"].SetValue(Project.ViewWidthQnScale);
            //fx.Parameters["TexWidthScale"].SetValue(texMaterial.TexProps.UAnchor == TexAnchorEnum.Screen ? VertWidthScale : 1);

            //Common notestyle props
            //EffectParameterCollection fxModEntries = fx.Parameters["ModEntries"].Elements;
            _fx.Parameters["ActiveModEntries"].SetValue(ModEntries.Count);
            for (int i = 0; i < ModEntries.Count; i++)
            {
                //EffectParameterCollection fxModEntry = fxModEntries[i].StructureMembers;
                //fxModEntry["XSource"].SetValue(ModEntries[i].XSource);
                //fxModEntry["YSource"].SetValue(ModEntries[i].YSource);
                //fxModEntry["CombineXY"].SetValue(ModEntries[i].CombineXY);
                //fxModEntry["squareAspect"].SetValue(ModEntries[i].SquareAspect);
                //fxModEntry["ColorDestEnable"].SetValue(ModEntries[i].ColorDestEnable);
                //fxModEntry["ColorDest"].SetValue(ModEntries[i].ColorDest);
                //fxModEntry["AngleDestEnable"].SetValue(ModEntries[i].AngleDestEnable);
                //fxModEntry["AngleDest"].SetValue(ModEntries[i].AngleDest);
                //fxModEntry["Start"].SetValue(ModEntries[i].Start);
                //fxModEntry["Stop"].SetValue(ModEntries[i].Stop);
                //fxModEntry["FadeIn"].SetValue(ModEntries[i].FadeIn);
                //fxModEntry["FadeOut"].SetValue(ModEntries[i].FadeOut);
                //fxModEntry["Power"].SetValue(ModEntries[i].Power);
                //fxModEntry["Scale"].SetValue(ModEntries[i].Scale);

                _fx.Parameters["Origin"].Elements[i].SetValue(ModEntries[i].Origin);
                _fx.Parameters["XOriginEnable"].Elements[i].SetValue((bool)ModEntries[i].XOriginEnable);
                _fx.Parameters["YOriginEnable"].Elements[i].SetValue((bool)ModEntries[i].YOriginEnable);
                _fx.Parameters["CombineXY"].Elements[i].SetValue((int)ModEntries[i].CombineXY);
                _fx.Parameters["SquareAspect"].Elements[i].SetValue((bool)ModEntries[i].SquareAspect);
                _fx.Parameters["ColorDestEnable"].Elements[i].SetValue((bool)ModEntries[i].ColorDestEnable);
                _fx.Parameters["AngleDestEnable"].Elements[i].SetValue((bool)ModEntries[i].AngleDestEnable);
                _fx.Parameters["AlphaDestEnable"].Elements[i].SetValue((bool)ModEntries[i].AlphaDestEnable);
                _fx.Parameters["ColorDest"].Elements[i].SetValue(((Color)ModEntries[i].ColorDest).ToVector4());
                _fx.Parameters["AngleDest"].Elements[i].SetValue(ModEntries[i].RadAngleDest);
                _fx.Parameters["Start"].Elements[i].SetValue((float)ModEntries[i].Start);
                _fx.Parameters["Stop"].Elements[i].SetValue((float)ModEntries[i].Stop);
                _fx.Parameters["FadeIn"].Elements[i].SetValue((float)ModEntries[i].FadeIn);
                _fx.Parameters["FadeOut"].Elements[i].SetValue((float)ModEntries[i].FadeOut);
                _fx.Parameters["Power"].Elements[i].SetValue((float)ModEntries[i].Power);
                _fx.Parameters["DiscardAfterStop"].Elements[i].SetValue((bool)ModEntries[i].DiscardAfterStop);
                _fx.Parameters["Invert"].Elements[i].SetValue((bool)ModEntries[i].Invert);
            }

            //Material
            GraphicsDevice.SamplerStates[0] = texMaterial.TexProps.SamplerState;
            GraphicsDevice.SamplerStates[1] = texMaterial.TexProps.SamplerState;
            Texture2D texture;
            Vector4 color;
            GetMaterial(trackProps, false, out color, out texture);
            bool useTexture = texture != null && !(bool)trackProps.MaterialProps.GetTexProps(0).DisableTexture && !(bool)Project.GlobalTrackProps.MaterialProps.GetTexProps(0).DisableTexture;
            if (useTexture)
                _fx.Parameters["Texture"].SetValue(texture);
            Texture2D transitionTexture = texMaterial.TexProps.TransitionTexture;
            bool useTexture2 = transitionTexture != null && !(bool)trackProps.MaterialProps.GetTexProps(0).DisableTexture && !(bool)Project.GlobalTrackProps.MaterialProps.GetTexProps(0).DisableTexture;
            bool supportsTextureBlend = HasEffectParameter("Texture2")
                && HasEffectParameter("UseTexture2")
                && HasEffectParameter("TextureBlend");
            if (supportsTextureBlend && useTexture2)
                _fx.Parameters["Texture2"].SetValue(transitionTexture);
            _fx.Parameters["UseTexture"].SetValue(useTexture);
            if (supportsTextureBlend)
            {
                _fx.Parameters["UseTexture2"].SetValue(useTexture2);
                _fx.Parameters["TextureBlend"].SetValue((useTexture || useTexture2)
                    ? texMaterial.TexProps.TextureBlend
                    : 0f);
            }
            _fx.Parameters["Color"].SetValue(color);
            Vector4 hlColor = trackProps.MaterialProps.GetColor(true, Project.GlobalTrackProps.MaterialProps);
            _fx.Parameters["HlColor"].SetValue(hlColor);
            _fx.Parameters["TexColBlend"].SetValue((bool)texMaterial.TexProps.TexColBlend);

            //Light
            LightProps lightProps = (bool)trackProps.LightProps.UseGlobalLight ? Project.GlobalTrackProps.LightProps : trackProps.LightProps;
            Vector3 normLightDir = lightProps.Dir;
            normLightDir.Normalize();
            _fx.Parameters["LightDir"].SetValue(normLightDir);
            _fx.Parameters["AmbientColor"].SetValue(((Color)lightProps.AmbientColor).ToVector4() * (float)lightProps.AmbientAmount);
            _fx.Parameters["DiffuseColor"].SetValue(((Color)lightProps.DiffuseColor).ToVector4() * (float)lightProps.DiffuseAmount);
            _fx.Parameters["SpecColor"].SetValue(((Color)lightProps.SpecColor).ToVector4() * (float)lightProps.SpecAmount);
            _fx.Parameters["SpecPower"].SetValue((float)(lightProps.SpecPower));
            _fx.Parameters["LightFilter"].SetValue(((Color)lightProps.MasterColor).ToVector4() * (float)lightProps.MasterAmount);

            //Spatial props
            _fx.Parameters["PosOffset"].SetValue(Project.GetSpatialNormPosOffset(trackProps)); ;
            //Scale world space if rendering to cubemap to make non-360 and 360 videos look the same
            Matrix worldMat = Matrix.CreateScale(Project.Props.Camera.RenderingToCubemap ? 5f / 3 : 1);
            _fx.Parameters["WorldMat"].SetValue(worldMat);

            _fx.Parameters["CamPos"].SetValue(Project.Props.Camera.Pos);

            //Texture scrolling including adjustment for screen anchoring
            Texture2D coordTexture = texMaterial.TexProps.CoordTexture;
            if (coordTexture != null)
            {
                TrackPropsTex texProps = texMaterial.TexProps;
                Vector2 texScrollOffset = (float)Project.SongPosB * texProps.Scroll;
                if (texProps.UAnchor == TexAnchorEnum.Screen)
                {
                    Vector2 texSize = new Vector2(coordTexture.Width, coordTexture.Height) * TexTileScale;
                    float xOffset = songPosP + Project.Props.Camera.ViewportSize.X / 2;
                    if ((bool)texProps.UTile)
                    {
                        if ((bool)texProps.KeepAspect)
                            texSize.X /= texSize.Y * Project.Props.Camera.XYRatio;
                        xOffset /= texSize.X; ;
                    }
                    else
                        xOffset /= Project.Props.Camera.ViewportSize.X;

                    texScrollOffset.X += xOffset;
                }
                _fx.Parameters["TexScrollOffset"].SetValue(texScrollOffset);
            }
        }

        //public abstract void createGeoChunk(BoundingBox bbox, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, Material texMaterial);

        public void AddModEntry(bool selectItem, string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Entry " + ModEntries.Count;
            ModEntries.Add(new NoteStyleMod(name));
            if (selectItem)
                SelectedModEntryIndex = ModEntries.Count - 1;
        }

        public void DeleteModEntry(int entry = -1)
        {
            if (entry < 0)
                entry = (int)SelectedModEntryIndex;
            if (ModEntries.Count > 0)
            {
                ModEntries.RemoveAt(entry);
                SelectedModEntryIndex = ModEntries.Count - 1;
            }
        }

        public void CloneModEntry(bool selectItem, int entry = -1)
        {
            if (entry < 0)
                entry = (int)SelectedModEntryIndex;
            ModEntries.Add(ModEntries[entry].Clone());
            if (selectItem)
                SelectedModEntryIndex = ModEntries.Count - 1;

        }

        protected void CalcRectTexCoords(out Vector2 topLeft_tex, out Vector2 size_tex, Vector2 texSize, Vector2 topLeft_world, Vector2 size_world, MaterialProps texMaterial)
        {
            topLeft_tex = CalcTexCoords(texSize, topLeft_world, size_world, new Vector2(0, 0), texMaterial);
            size_tex = CalcTexCoords(texSize, topLeft_world, size_world, size_world, texMaterial) - topLeft_tex;

            if ((bool)texMaterial.TexProps.KeepAspect)
            {
                float uTexelsPerPixel = size_tex.X * texSize.X / size_world.X;
                float vTexelsPerPixel = size_tex.Y * texSize.Y / size_world.Y;
                float uvRatio = -uTexelsPerPixel / vTexelsPerPixel;
                if ((bool)texMaterial.TexProps.UTile && !(bool)texMaterial.TexProps.VTile)
                {
                    topLeft_tex.X = topLeft_tex.X / uvRatio;
                    size_tex.X = size_tex.X / uvRatio;
                }
                else if (!(bool)texMaterial.TexProps.UTile && (bool)texMaterial.TexProps.VTile)
                {
                    topLeft_tex.Y = topLeft_tex.Y * uvRatio;
                    size_tex.Y = size_tex.Y * uvRatio;
                }
            }
            //topLeft_tex -= Project.SongPosB * texMaterial.TexProps.Scroll;
        }

        protected Vector2 CalcTexCoords(Vector2 texSize, Vector2 notePos, Vector2 noteSize, Vector2 posOffset, MaterialProps texMaterial)
        {
            Vector2 coords = new Vector2();
            coords.X = CalcTexCoordComponent(texSize.X, Project.Props.Camera.ViewportSize.X, notePos.X, noteSize.X, posOffset.X, (bool)texMaterial.TexProps.UTile, (TexAnchorEnum)texMaterial.TexProps.UAnchor, true);
            coords.Y = CalcTexCoordComponent(texSize.Y, Project.Props.Camera.ViewportSize.Y, notePos.Y, noteSize.Y, posOffset.Y, (bool)texMaterial.TexProps.VTile, (TexAnchorEnum)texMaterial.TexProps.VAnchor, false);
            coords.Y *= -1;
            return coords;
        }

        float CalcTexCoordComponent(float texSize, float vpSize, float notePos, float noteSize, float posOffset, bool tile, TexAnchorEnum anchor, bool u)
        {
            if (tile)
                texSize *= TexTileScale;

            if (anchor == TexAnchorEnum.Screen)
            {
                float screenPos = notePos + posOffset;
                if (!u)
                    screenPos += vpSize / 2;
                if (!tile)
                    return screenPos / vpSize;
                else
                    return screenPos / texSize;
            }
            else if (anchor == TexAnchorEnum.Note)
            {
                if (!tile)
                    return posOffset / noteSize;
                else
                    return posOffset / texSize;
            }
            else //Anchor at song start. Can only be true for U
            {
                float x = Project.SongPosP + notePos + posOffset;
                if (!tile)
                    return x / Project.SongLengthP;
                else
                    return x / texSize;
            }
        }
    }

    public abstract class Geo : IDisposable
    {
        public List<BoundingBoxEx> bboxes = new List<BoundingBoxEx>();
        int _refCount = 0;

        public Geo AddRef()
        {
            _refCount++;
            return this;
        }

        public void Dispose()
        {
            if (_refCount-- == 0)
                throw new AccessViolationException("Object already disposed");
            if (_refCount > 0)
                return;
            ReleaseResources();
        }

        protected abstract void ReleaseResources();

        public bool AreObjectsInFrustum(BoundingFrustum frustum, float songPos, Project project, TrackProps trackProps)
        {
            foreach (var bbox in bboxes)
            {
                BoundingBoxEx bb = bbox.Clone();
                bb.Scale(new Vector3(project.ViewWidthQnScale, 1, 1));

                Vector3 posOffset = project.GetSpatialNormPosOffset(trackProps);
                posOffset.X -= songPos;
                bb.Translate(posOffset);

                if (bb.Intersects(frustum))
                    return true;
            }
            return false;
        }
    }

    public class BoundingBoxEx
    {
        Vector3[] _normals = new Vector3[3];
        Vector3[] _corners = new Vector3[8];
        BoundingBox _aabb;

        BoundingBoxEx(Vector3[] corners, Vector3[] normals)
        {
            corners.CopyTo(_corners, 0);
            normals.CopyTo(_normals, 0);
        }

        public BoundingBoxEx(Vector3 min, Vector3 max)
        {
            _aabb = new BoundingBox(min, max);

            _normals[0] = new Vector3(1, 0, 0); //Right
            _normals[1] = new Vector3(0, 1, 0); //Up
            _normals[2] = new Vector3(0, 0, 1); //Front

            for (int i = 0; i < 4; i++)
                _corners[i] = _aabb.Min;

            _corners[1].X = _aabb.Max.X;
            _corners[2].X = _aabb.Max.X;
            _corners[2].Y = _aabb.Max.Y;
            _corners[3].Y = _aabb.Max.Y;
            for (int i = 0; i < 4; i++)
            {
                _corners[i + 4] = _corners[i];
                _corners[i + 4].Z = _aabb.Max.Z;
            }
        }

        public BoundingBoxEx(Vector3 origin, Vector3 spanX, Vector3 spanY, Vector3 spanZ)
        {
            _normals[0] = spanX; _normals[0].Normalize();
            _normals[1] = spanY; _normals[1].Normalize();
            if (Vector3.Zero == spanZ)
                _normals[2] = Vector3.Cross(_normals[0], _normals[1]);
            else
                _normals[2] = spanZ;
            _normals[2].Normalize();

            _corners[0] = origin + spanX + spanY + spanZ;
            _corners[1] = origin - spanX + spanY + spanZ;
            _corners[2] = origin + spanX - spanY + spanZ;
            _corners[3] = origin - spanX - spanY + spanZ;
            _corners[4] = origin + spanX + spanY - spanZ;
            _corners[5] = origin - spanX + spanY - spanZ;
            _corners[6] = origin + spanX - spanY - spanZ;
            _corners[7] = origin - spanX - spanY - spanZ;
            _aabb = BoundingBox.CreateFromPoints(_corners);
        }

        public BoundingBoxEx Clone()
        {
            var newBox = new BoundingBoxEx(_corners, _normals);
            newBox._aabb = _aabb;
            return newBox;
        }

        public bool Intersects(BoundingFrustum frustum)
        {
            //A simple aabb test will discard most boxes
            if (!frustum.Intersects(_aabb))
                return false;

            //SAT test
            //Test box normals
            Vector3[] frustumCorners = frustum.GetCorners();
            foreach (var normal in _normals)
            {
                if (!IntersectsWhenProjected(_corners, frustumCorners, normal))
                    return false;
            }

            //Test frustum plane normals
            var frustumPlanes = frustum.GetPlanes();
            for (int i = 1; i < 6; i++) //Skip near plane since it's parallell to far plane
            {
                if (!IntersectsWhenProjected(_corners, frustumCorners, frustumPlanes[i].Normal))
                    return false;
            }

            //Create the 6 frustum plane edges that has unique directions
            var frustumEdges = new Vector3[6];
            //Create vectors from the four corners in the near plane to the corresponding corners in the far plane
            for (int i = 0; i < 4; i++)
                frustumEdges[i] = frustumCorners[i] - frustumCorners[i + 4];
            //Create the two edges of the near plane.
            frustumEdges[4] = frustumCorners[0] - frustumCorners[1];
            frustumEdges[5] = frustumCorners[0] - frustumCorners[3];

            //Test the 18 cross products between the box edges and frustum edges
            foreach (var boxEdge in _normals)  //Box normals are parallell to edges
            {
                foreach (var frustumEdge in frustumEdges)
                {
                    Vector3 cross = Vector3.Cross(boxEdge, frustumEdge);
                    if (!IntersectsWhenProjected(_corners, frustumCorners, cross))
                        return false;
                }
            }
            return true;
        }

        //bool intersects(Plane plane)
        //{
        //	float d = Vector3.Dot(_center, plane.Normal) + plane.D;
        //	float projLengthSum = 0;
        //	foreach (var v in _spanVecs)
        //	{
        //		projLengthSum += Math.Abs(Vector3.Dot(v, plane.Normal));
        //	}
        //	return d < projLengthSum;
        //}

        public void Translate(Vector3 offset)
        {
            _aabb.Min += offset;
            _aabb.Max += offset;
            for (int i = 0; i < 8; i++)
                _corners[i] += offset;
            //_center += offset;
        }

        public void Scale(Vector3 scale)
        {
            _aabb.Min *= scale;
            _aabb.Max *= scale;

            for (int i = 0; i < 8; i++)
                _corners[i] *= scale;
        }

        // aCorn and bCorn are arrays containing all corners (vertices) of the two OBBs
        static bool IntersectsWhenProjected(Vector3[] aCorn, Vector3[] bCorn, Vector3 axis)
        {
            // Handles the cross product = {0,0,0} case
            if (axis == Vector3.Zero)
                return true;

            float aMin = float.MaxValue;
            float aMax = float.MinValue;
            float bMin = float.MaxValue;
            float bMax = float.MinValue;

            // Define two intervals, a and b. Calculate their min and max values
            for (int i = 0; i < 8; i++)
            {
                float aDist = Vector3.Dot(aCorn[i], axis);
                aMin = (aDist < aMin) ? aDist : aMin;
                aMax = (aDist > aMax) ? aDist : aMax;
                float bDist = Vector3.Dot(bCorn[i], axis);
                bMin = (bDist < bMin) ? bDist : bMin;
                bMax = (bDist > bMax) ? bDist : bMax;
            }

            // One-dimensional intersection test between a and b
            float longSpan = Math.Max(aMax, bMax) - Math.Min(aMin, bMin);
            float sumSpan = aMax - aMin + bMax - bMin;
            return longSpan <= sumSpan; // Change this to <= if you want the case were they are touching but not overlapping, to count as an intersection
        }
    }
}
