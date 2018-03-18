using System;
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
	public class TrackProps : ISerializable
	{
		internal TrackView TrackView { get; set; }
		static int NumTracks { get => TrackView.NumTracks; }
		int TrackNumber { get => TrackView.TrackNumber; }
		static public TrackProps GlobalProps { get; set; }

		public NoteStyleEnum? NoteStyleType { get; set; }
		NoteStyle[] noteStyles = new NoteStyle[Enum.GetNames(typeof(NoteStyleEnum)).Length];

		internal NoteStyle SelectedNoteStyle
		{
			get
			{
				if (NoteStyleType == NoteStyleEnum.Default)
				{
					if (TrackNumber == 0)  //Global track
						return getBarNoteStyle();
					else
						return GlobalProps.SelectedNoteStyle;
				}
				else
					return getNoteStyle(NoteStyleType);
			}
			set
			{
				if (value.GetType() == typeof(NoteStyle_Bar))
				{
					NoteStyleType = NoteStyleEnum.Bar;
					noteStyles[(int)NoteStyleEnum.Bar] = value;
				}
				else if (value.GetType() == typeof(NoteStyle_Bar))
				{
					NoteStyleType = NoteStyleEnum.Line;
					noteStyles[(int)NoteStyleEnum.Line] = value;
				}
				else if (value == null)
					NoteStyleType = NoteStyleEnum.Default;
			}
		}

		public Material Material { get; set; } = new Material();
		public Light Light { get; set; } = new Light();
		public Spatial Spatial { get; set; } = new Spatial();

		public TrackProps(TrackView view)
		{
			TrackView = view;
			resetProps();
		}

		public TrackProps(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "noteStyles")
					noteStyles = (NoteStyle[])entry.Value;
				else if (entry.Name == "noteStyleType")
					NoteStyleType = (NoteStyleEnum)entry.Value;
				else if (entry.Name == "material")
					Material = (Material)entry.Value;
				else if (entry.Name == "light")
					Light = (Light)entry.Value;
				else if (entry.Name == "spatial")
					Spatial = (Spatial)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("noteStyles", noteStyles);
			info.AddValue("noteStyleType", NoteStyleType);
			info.AddValue("material", Material);
			info.AddValue("light", Light);
			info.AddValue("spatial", Spatial);
		}

		public void loadContent(SongPanel songPanel)
		{
			Material.loadContent(songPanel);
		}

		public TrackProps clone()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(TrackProps), Form1.projectSerializationTypes);
			MemoryStream stream = new MemoryStream();
			dcs.WriteObject(stream, this);
			stream.Flush();
			stream.Position = 0;
			TrackView tv = TrackView;
			TrackProps dest = (TrackProps)dcs.ReadObject(stream);
			//dest.MidiTrack = midiTrack;
			dest.loadNoteStyleFx();
			dest.TrackView = tv;
			return dest;
		}

		public void loadNoteStyleFx()
		{
			foreach (NoteStyle ns in noteStyles)
			{
				if (ns != null)
					ns.loadFx();
			}
		}


		public void resetStyle()
		{
			int[] styleTypes = (int[])Enum.GetValues(typeof(NoteStyleEnum));
			string[] styleNames = (string[])Enum.GetNames(typeof(NoteStyleEnum));
			for (int i = 0; i < noteStyles.Length; i++)
			{
				if ((NoteStyleEnum)styleTypes[i] != NoteStyleEnum.Default)
				{
					noteStyles[i] = (NoteStyle)Activator.CreateInstance(Type.GetType("Visual_Music.NoteStyle_" + styleNames[i]));
					noteStyles[i].loadFx();
				}
			}
			if (TrackNumber == 0)
			{
				NoteStyleType = NoteStyleEnum.Bar;
				//UseGlobalLight = false;
			}
			else
			{
				NoteStyleType = NoteStyleEnum.Default;
				//UseGlobalLight = true;
			}
		}

		public void resetMaterial()
		{
			Material.reset(TrackNumber, NumTracks);
		}

		public void resetLight()
		{
			Light.reset(TrackNumber);
		}

		public void resetSpatial()
		{
			Spatial.reset();
		}

		public void resetProps()
		{
			resetMaterial();
			resetStyle();
			resetLight();
			resetSpatial();
		}

		public NoteStyle getNoteStyle(NoteStyleEnum? styleType)
		{
			return noteStyles[(int)styleType];
		}
		public NoteStyle_Bar getBarNoteStyle()
		{
			return (NoteStyle_Bar)noteStyles[(int)NoteStyleEnum.Bar];
		}
		public NoteStyle_Line getLineNoteStyle()
		{
			return (NoteStyle_Line)noteStyles[(int)NoteStyleEnum.Line];
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
	public class Material : ISerializable
	{
		public float? Transp { get; set; }
		public float? Hue { get; set; }
		NoteTypeMaterial normal;
		public NoteTypeMaterial Normal { get => normal; set => normal = value; }
		public NoteTypeMaterial Hilited { get; set; }
		public TrackPropsTex TexProps { get; set; } = new TrackPropsTex();
		public TrackPropsTex HmapProps { get; set; } = new TrackPropsTex();
		
		public float? AmbientAmount { get; set; }
		public float? DiffuseAmount { get; set; }
		public float? SpecAmount { get; set; }
		public float? SpecPower { get; set; }

		public Material()
		{

		}

		public Material(SerializationInfo info, StreamingContext ctxt)
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
			info.AddValue("transp", Transp);
			info.AddValue("hue", Hue);
			info.AddValue("normal", Normal);
			info.AddValue("hilited", Hilited);
			info.AddValue("texProps", TexProps);
			info.AddValue("hmapProps", HmapProps);
			info.AddValue("ambientAmount", AmbientAmount);
			info.AddValue("diffuseAmount", DiffuseAmount);
			info.AddValue("specAmount", SpecAmount);
			info.AddValue("specPower", SpecPower);
		}

		internal void loadContent(SongPanel songPanel)
		{
			TexProps.loadContent(songPanel);
			HmapProps.loadContent(songPanel);
		}

		public Texture2D getTexture(bool bhilited, Material globalMaterial)
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
		public System.Drawing.Color getSysColor(bool bhilited, Material globalMaterial)
		{
			Color c = getColor(bhilited, globalMaterial, false);
			return System.Drawing.Color.FromArgb(c.R, c.G, c.B);
		}
		public Color getColor(bool bhilited, Material globalMaterial, bool alpha)
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

		public void reset(int trackNumber, int numTracks)
		{
			TexProps.unloadTexture();
			TexProps = new TrackPropsTex();
			if (trackNumber == 0)
			{
				Transp = 1;
				Hue = 0.1f;
				Normal = new NoteTypeMaterial(1, 0.27f);
				Hilited = new NoteTypeMaterial(0.8f, 0.75f);
				AmbientAmount = 0;
				DiffuseAmount = 2;
				SpecAmount = 1;
				SpecPower = 50;
			}
			else
			{
				Transp = 0.5f;
				Hue = (float)(trackNumber - 1) / (numTracks - 1);
				Normal = new NoteTypeMaterial();
				Hilited = new NoteTypeMaterial(); ;
				AmbientAmount = 1;
				DiffuseAmount = 1;
				SpecAmount = 1;
				SpecPower = 1;
			}
		}
	}

	[Serializable()]
	public class Light : ISerializable
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

		public Light()
		{

		}

		public Light(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "dir")
					Dir = (Vector3)entry.Value;
				else if (entry.Name == "useGlobalLight")
					UseGlobalLight = (bool)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("dir", Dir);
			info.AddValue("useGlobalLight", UseGlobalLight);
		}

		public void reset(int trackNumber)
		{
			if (trackNumber == 0)
				UseGlobalLight = false;
			else
				UseGlobalLight = true;
			Dir = new Vector3(-1, -1, 1);
		}
	}

	[Serializable()]
	public class Spatial : ISerializable
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

		public Spatial()
		{

		}

		public Spatial(SerializationInfo info, StreamingContext ctxt)
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

		public void reset()
		{
			PosOffset = new Vector3();
		}
	}

}
