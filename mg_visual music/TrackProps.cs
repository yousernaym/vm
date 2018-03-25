﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Forms;


namespace Visual_Music
{
	public enum TexAnchorEnum { Note = 0, Screen, Song };

	[Serializable()]
	public class TrackProps : CloneableProps<TrackProps>, ISerializable
	{
		internal TrackView TrackView { get; set; }
		static int NumTracks { get => TrackView.NumTracks; }
		int TrackNumber { get => TrackView.TrackNumber; }
		static public TrackProps GlobalProps { get; set; }

		internal NoteStyle ActiveNoteStyle
		{
			get => StyleProps.getActiveStyle(TrackNumber, GlobalProps);
			//set => Style.setSelectedNoteStyle(value);
		}

		public StyleProps StyleProps { get; set; }
		public MaterialProps MaterialProps { get; set; }
		public LightProps LightProps { get; set; }
		public SpatialProps SpatialProps { get; set; }

		public TrackProps(TrackView view)
		{
			TrackView = view;
			resetProps();
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
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("style", StyleProps);
			info.AddValue("material", MaterialProps);
			info.AddValue("light", LightProps);
			info.AddValue("spatial", SpatialProps);
		}

		public void loadContent(SongPanel songPanel)
		{
			MaterialProps.loadContent(songPanel);
		}

		new public TrackProps clone()
		{
			TrackView tv = TrackView;
			TrackProps dest = base.clone();
			dest.StyleProps.loadFx();
			dest.TrackView = tv;
			return dest;
		}

		public void resetStyle()
		{
			StyleProps = new StyleProps(TrackNumber);
		}

		public void resetMaterial()
		{
			MaterialProps = new MaterialProps(TrackNumber, NumTracks);
		}

		public void resetLight()
		{
			LightProps = new LightProps(TrackNumber);
		}

		public void resetSpatial()
		{
			SpatialProps = new SpatialProps();
		}

		public void resetProps()
		{
			resetMaterial();
			resetStyle();
			resetLight();
			resetSpatial();
		}
	}

	[Serializable()]
	public class TrackPropsTex : ISerializable
	{
		public Texture2D Texture { get; set; } = null;
		public string Path { get; set; } = "";
		
		SamplerState samplerState = new SamplerState();
		SamplerState samplerStateBacking = new SamplerState();
		bool dirtySamplerState = true;
		public SamplerState SamplerStateBacking
		{
			get { dirtySamplerState = true; return samplerStateBacking; }
			//set { samplerStateBacking = value; }
		}
		public SamplerState SamplerState
		{
			get
			{
				if (dirtySamplerState)
				{
					samplerState.Dispose();
					samplerState = new SamplerState();
					samplerState.AddressU = samplerStateBacking.AddressU;
					samplerState.AddressV = samplerStateBacking.AddressV;
					samplerState.Filter = samplerStateBacking.Filter;
					samplerState.MaxAnisotropy = samplerStateBacking.MaxAnisotropy;
					samplerState.MaxMipLevel = samplerStateBacking.MaxMipLevel;
					dirtySamplerState = false;
				}
				return samplerState;
			}
			//set { samplerState = value; }
		}
		bool? pointSmp = false;
		public bool? PointSmp
		{
			get => pointSmp;
			set
			{
				pointSmp = value;
				if (value != null)
					SamplerStateBacking.Filter = (bool)value ? TextureFilter.Point : TextureFilter.Linear;
			}
		}

		bool? uTile = false;
		public bool? UTile
		{
			get { return uTile; }
			set { uTile = value; }
		}
		bool? vTile = false;
		public bool? VTile
		{
			get { return vTile; }
			set { vTile = value; }
		}
		bool? keepAspect = false;
		public bool? KeepAspect
		{
			get { return keepAspect; }
			set { keepAspect = value; }
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
			samplerStateBacking.AddressU = TextureAddressMode.Wrap;
			samplerStateBacking.AddressV = TextureAddressMode.Wrap;
		}
		public TrackPropsTex(SerializationInfo info, StreamingContext ctxt)
		{
			Path = (string)info.GetValue("path", typeof(string));
			PointSmp = (bool)info.GetValue("PointSmp", typeof(bool));
			keepAspect = (bool)info.GetValue("keepAspect", typeof(bool));
			uTile = (bool)info.GetValue("uTile", typeof(bool));
			vTile = (bool)info.GetValue("vTile", typeof(bool));
			Anchor = (Point)info.GetValue("anchor", typeof(Point));
			Scroll = (Vector2)info.GetValue("scroll", typeof(Vector2));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("path", Path);
			info.AddValue("PointSmp", PointSmp);
			info.AddValue("keepAspect", keepAspect);
			info.AddValue("uTile", uTile);
			info.AddValue("vTile", vTile);
			info.AddValue("anchor", Anchor);
			info.AddValue("scroll", Scroll);
		}
		Texture2D createMipLevels(Texture2D tex, SongPanel songPanel)
		{
			RenderTarget2D renderTarget = new RenderTarget2D(songPanel.GraphicsDevice, tex.Width, tex.Height, true, SurfaceFormat.Color, DepthFormat.None);
			songPanel.GraphicsDevice.SetRenderTarget(renderTarget);

			songPanel.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);
			songPanel.SpriteBatch.Draw(tex, new Vector2(0, 0), Color.White);
			songPanel.SpriteBatch.End();
			
			Texture2D outTex = (Texture2D)renderTarget;
			songPanel.GraphicsDevice.SetRenderTarget(null);
			tex.Dispose();
			return outTex;
		}

		public bool loadTexture(string path, FileStream stream, SongPanel songPanel)
		{
			Path = path;
			Texture2D tex = Texture;
			if (tex != null)
			{
				tex.Dispose();
				tex = null;
			}
			tex = Texture2D.FromStream(songPanel.GraphicsDevice, stream);
			Texture = createMipLevels(tex, songPanel);
			//trackProps[index].Texture = tex;
			return tex != null;
		}

		public bool loadTexture(string path, SongPanel songPanel)
		{
			using (FileStream stream = File.Open(path, FileMode.Open))
			{
				return loadTexture(path, stream, songPanel);
			}
		}
		public void unloadTexture()
		{
			Path = "";
			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}
		}

		internal void loadContent(SongPanel songPanel)
		{
			//Deserialization inits PJath but not Texture because Texture2D is not serializable, and you can't load texture before the device is created.
			try
			{
				if (!string.IsNullOrEmpty(Path))
					loadTexture(Path, songPanel);
			}
			catch (Exception)
			{
				MessageBox.Show("Failed to load texture " + Path);
			}
		}
	}

	[Serializable()]
	public class NoteTypeMaterial : ISerializable
	{
		//TrackProps parent;
		float? sat = 1;
		public float? Sat
		{
			get { return sat; }
			set { sat = value; }
		}
		float? lum = 1;
		public float? Lum
		{
			get { return lum; }
			set { lum = value; }
		}
		Texture2D texture = null;
		public Texture2D Texture
		{
			get { return texture; }
			set { texture = value; }
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
			sat = 1;
			lum = 1;
			texture = null;
		}
		public NoteTypeMaterial(float _sat, float _lum, Texture2D tex = null)
		{
			sat = _sat;
			lum = _lum;
			texture = tex;
		}
		public NoteTypeMaterial(SerializationInfo info, StreamingContext ctxt)
		{
			sat = (float)info.GetValue("sat", typeof(float));
			lum = (float)info.GetValue("lum", typeof(float));
			texture = (Texture2D)info.GetValue("texture", typeof(Texture2D));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("sat", sat);
			info.AddValue("lum", lum);
			info.AddValue("texture", texture);
		}
	}

	//---------------------------------------
	//Tab props

	[Serializable()]
	public class StyleProps : CloneableProps<StyleProps>, ISerializable
	{
		public NoteStyleType? Type { get; set; }
		NoteStyle[] styles = new NoteStyle[Enum.GetNames(typeof(NoteStyleType)).Length];

		public StyleProps(int trackNumber)
		{
			int[] styleTypes = (int[])Enum.GetValues(typeof(NoteStyleType));
			string[] styleNames = (string[])Enum.GetNames(typeof(NoteStyleType));
			for (int i = 0; i < styles.Length; i++)
			{
				if ((NoteStyleType)styleTypes[i] != NoteStyleType.Default)
				{
					styles[i] = (NoteStyle)Activator.CreateInstance(System.Type.GetType("Visual_Music.NoteStyle_" + styleNames[i]));
					styles[i].loadFx();
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
					styles = (NoteStyle[])entry.Value;
				else if (entry.Name == "noteStyleType")
					Type = (NoteStyleType)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("noteStyles", styles);
			info.AddValue("noteStyleType", Type);
		}

		public NoteStyle getActiveStyle(int trackNumber, TrackProps globalProps)
		{
			if (Type == NoteStyleType.Default)
			{
				if (trackNumber == 0)  //Global track
					return getBarStyle();
				else
					return globalProps.ActiveNoteStyle;
			}
			else
				return getStyle(Type);
		}

		internal NoteStyle SelectedStyle
		{
			get => getStyle(Type);
			set
			{
				if (value.GetType() == typeof(NoteStyle_Bar))
				{
					Type = NoteStyleType.Bar;
					styles[(int)NoteStyleType.Bar] = value;
				}
				else if (value.GetType() == typeof(NoteStyle_Line))
				{
					Type = NoteStyleType.Line;
					styles[(int)NoteStyleType.Line] = value;
				}
				else if (value == null)
					Type = NoteStyleType.Default;
			}
		}

		public NoteStyle getStyle(NoteStyleType? type)
		{
			return styles[(int)type];
		}
		public NoteStyle_Bar getBarStyle()
		{
			return (NoteStyle_Bar)styles[(int)NoteStyleType.Bar];
		}
		public NoteStyle_Line getLineStyle()
		{
			return (NoteStyle_Line)styles[(int)NoteStyleType.Line];
		}

		public void loadFx()
		{
			foreach (NoteStyle ns in styles)
			{
				if (ns != null)
					ns.loadFx();
			}
		}

		new public StyleProps clone()
		{
			StyleProps dest = base.clone();
			dest.loadFx();
			return dest;
		}
	}

	[Serializable()]
	public class MaterialProps : CloneableProps<MaterialProps>, ISerializable
	{
		public float? Transp { get; set; }
		public float? Hue { get; set; }
		NoteTypeMaterial normal;
		public NoteTypeMaterial Normal { get => normal; set => normal = value; }
		public NoteTypeMaterial Hilited { get; set; }
		public TrackPropsTex TexProps { get; set; } = new TrackPropsTex();
		public TrackPropsTex HmapProps { get; set; } = new TrackPropsTex();
		
		public MaterialProps(int trackNumber, int numTracks)
		{
			TexProps.unloadTexture();
			TexProps = new TrackPropsTex();
			if (trackNumber == 0)
			{
				Transp = 1;
				Hue = 0.1f;
				Normal = new NoteTypeMaterial(1, 0.27f);
				Hilited = new NoteTypeMaterial(0.8f, 0.75f);
			}
			else
			{
				Transp = 0.5f;
				Hue = (float)(trackNumber - 1) / (numTracks - 1);
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

		internal void loadContent(SongPanel songPanel)
		{
			TexProps.loadContent(songPanel);
			HmapProps.loadContent(songPanel);
		}

		public Texture2D getTexture(bool bhilited, MaterialProps globalMaterial)
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
					tex = globalMaterial.getTexture(bhilited, null);
			}
			return tex;
		}
		public System.Drawing.Color getSysColor(bool bhilited, MaterialProps globalMaterial)
		{
			Color c = getColor(bhilited, globalMaterial, false);
			return System.Drawing.Color.FromArgb(c.R, c.G, c.B);
		}
		public Color getColor(bool bhilited, MaterialProps globalMaterial, bool alpha)
		{
			double h, s, l;
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
			h = (double)(Hue + globalMaterial.Hue);
			if (h > 1) h -= 1;
			if (h < 0) h += 1;
			s = (double)(tp2.Sat * globalTp2.Sat);
			l = (double)(tp2.Lum * globalTp2.Lum);
			if (s > 1)
				s = 1;
			if (l > 1)
				l = 1;
			Color c = SongPanel.HSLA2RGBA(h, s, l, alpha ? (float)(Transp * globalMaterial.Transp) : 1);

			//c *= (transp * globalProps.transp * 255);
			return c;
		}

		public TrackPropsTex getTexProps(int selector)
		{
			if (selector == 0)
				return TexProps;
			else
				return HmapProps;
		}
	}

	[Serializable()]
	public class LightProps : CloneableProps<LightProps>, ISerializable
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
		public float? DiffuseAmount { get; set; }
		public float? SpecAmount { get; set; }
		public float? SpecPower { get; set; }

		public LightProps(int trackNumber)
		{
			if (trackNumber == 0)
			{
				UseGlobalLight = false;
				AmbientAmount = 0.2f;
				DiffuseAmount = 2;
				SpecAmount = 1;
				SpecPower = 50;
			}
			else
			{
				UseGlobalLight = true;
				AmbientAmount = 0;
				DiffuseAmount = 0;
				SpecAmount = 0;
				SpecPower = 0;
			}
			Dir = new Vector3(-1, -1, 1);
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
				else if (entry.Name == "specPower")
					SpecPower = (float)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("dir", Dir);
			info.AddValue("useGlobalLight", UseGlobalLight);
			info.AddValue("ambientAmount", AmbientAmount);
			info.AddValue("diffuseAmount", DiffuseAmount);
			info.AddValue("specAmount", SpecAmount);
			info.AddValue("specPower", SpecPower);
		}
	}

	[Serializable()]
	public class SpatialProps : CloneableProps<SpatialProps>, ISerializable
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
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("posOffset", PosOffset);
		}

	}

	abstract public class CloneableProps<T>
	{
		public T clone()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(T), Form1.projectSerializationTypes);
			MemoryStream stream = new MemoryStream();
			dcs.WriteObject(stream, this);
			stream.Flush();
			stream.Position = 0;
			return (T)dcs.ReadObject(stream);
		}
	}

}
