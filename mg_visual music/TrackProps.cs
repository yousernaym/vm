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

		TrackPropsTex texProps = new TrackPropsTex();
		public TrackPropsTex TexProps
		{
			get { return texProps; }
			set { texProps = value; }
		}
		TrackPropsTex hmapProps = new TrackPropsTex();
		public TrackPropsTex HmapProps
		{
			get { return hmapProps; }
			set { hmapProps = value; }
		}
		public TrackPropsTex getTexProps(int selector)
		{
			if (selector == 0)
				return texProps;
			else
				return hmapProps;
		}

		float? transp;
		public float? Transp
		{
			get { return transp; }
			set { transp = value; }
		}
		float? hue;
		public float? Hue
		{
			get { return hue; }
			set { hue = value; }
		}
		public NoteStyleEnum? NoteStyleType { get; set; }
		NoteStyle[] noteStyles = new NoteStyle[Enum.GetNames(typeof(NoteStyleEnum)).Length];

		//NoteStyle_Bar barNoteStyle;
		//NoteStyle_Line lineNoteStyle;
		public NoteStyle SelectedNoteStyle
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
		}

		Material normal;
		public Material Normal
		{
			get { return normal; }
			set { normal = value; }
		}
		Material hilited;
		public Material Hilited
		{
			get { return hilited; }
			set { hilited = value; }
		}

		bool? useGlobalLight;
		public bool? UseGlobalLight
		{
			get { return useGlobalLight; }
			set { useGlobalLight = value; }
		}
		
		internal Vector3 LightDir
		{
			get => new Vector3((float)LightDirX, (float)LightDirY, (float)LightDirZ);
			set
			{
				LightDirX = value.X;
				LightDirY = value.Y;
				LightDirZ = value.Z;
			}
		}
		
		public float? LightDirX { get; set; }
		public float? LightDirY { get; set; }
		public float? LightDirZ { get; set; }
		
		float? specAmount;
		public float? SpecAmount
		{
			get { return specAmount; }
			set { specAmount = value; }
		}
		float? specPower;
		public float? SpecPower
		{
			get { return specPower; }
			set { specPower = value; }
		}
		//float specFov;
		//public float SpecFov
		//{
		//	get { return specFov; }
		//	set { specFov = value; }
		//}

		public TrackProps(TrackView view)
		{
			TrackView = view;
			resetProps();
		}

		public TrackProps(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "transp")
					transp = (float)entry.Value;
				if (entry.Name == "hue")
					hue = (float)entry.Value;
				if (entry.Name == "normal")
					normal = (Material)entry.Value;
				if (entry.Name == "hilited")
					hilited = (Material)entry.Value;
				if (entry.Name == "texProps")
					texProps = (TrackPropsTex)entry.Value;
				if (entry.Name == "hmapProps")
					hmapProps = (TrackPropsTex)entry.Value;
				if (entry.Name == "noteStyles")
					noteStyles = (NoteStyle[])entry.Value;
				if (entry.Name == "noteStyleType")
					NoteStyleType = (NoteStyleEnum)entry.Value;
				if (entry.Name == "lightDir")
					LightDir = (Vector3)entry.Value;
				if (entry.Name == "specAmount")
					specAmount = (float)entry.Value;
				if (entry.Name == "specPower")
					specPower = (float)entry.Value;
				//specFov = (float)info.GetValue("specFov", typeof(float));
				if (entry.Name == "useGlobalLight")
					useGlobalLight = (bool)entry.Value;
				if (entry.Name == "posOffset")
					PosOffset = (Vector3)entry.Value;
			}
			
			foreach (NoteStyle ns in noteStyles)
			{
				if (ns != null)
					ns.TrackProps = this;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("transp", transp);
			info.AddValue("hue", hue);
			info.AddValue("normal", normal);
			info.AddValue("hilited", hilited);
			info.AddValue("texProps", texProps);
			info.AddValue("hmapProps", hmapProps);
			info.AddValue("noteStyles", noteStyles);
			info.AddValue("noteStyleType", NoteStyleType);
			//info.AddValue("lineStyleProps", lineStyleProps);
			//info.AddValue("curve", curve);
			info.AddValue("lightDir", LightDir);
			info.AddValue("specAmount", specAmount);
			info.AddValue("specPower", specPower);
			//info.AddValue("specFov", specFov);
			info.AddValue("useGlobalLight", useGlobalLight);
			info.AddValue("posOffset", PosOffset);
		}

		public void loadContent(SongPanel songPanel)
		{
			//Deserialization inits trackProps.texPath but not trackProps.texture because Texture2D is not serializable, and you can't load texture before the device is created.
			string path = TexProps.Path;
			try
			{
				if (!string.IsNullOrEmpty(path))
					TexProps.loadTexture(path, songPanel);
				path = HmapProps.Path;
				if (!string.IsNullOrEmpty(path))
					HmapProps.loadTexture(path, songPanel);
			}
			catch (Exception)
			{
				MessageBox.Show("Failed to load texture " + path);
			}
		}

		public TrackProps clone()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(TrackProps), Form1.projectSerializationTypes);
			MemoryStream stream = new MemoryStream();
			dcs.WriteObject(stream, this);
			stream.Flush();
			stream.Position = 0;
			TrackProps dest = (TrackProps)dcs.ReadObject(stream);
			//dest.MidiTrack = midiTrack;
			dest.loadNoteStyleFx();
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
				UseGlobalLight = false;
			}
			else
			{
				NoteStyleType = NoteStyleEnum.Default;
				UseGlobalLight = true;
			}
		}

		public void resetMaterial()
		{
			texProps.unloadTexture();
			texProps = new TrackPropsTex();
			if (TrackNumber == 0)
			{
				transp = 1;
				hue = 0.1f;
				normal = new Material(1, 0.27f);
				hilited = new Material(0.8f, 0.75f);
			}
			else
			{
				transp = 0.5f;
				hue = (float)(TrackNumber - 1) / (NumTracks - 1);
				normal = new Material();
				hilited = new Material(); ;
			}

		}

		public void resetLight()
		{
			if (TrackNumber == 0)
				UseGlobalLight = false;
			else
				UseGlobalLight = true;
			LightDir = new Vector3(-1, -1, 1);
			specAmount = 0.75f;
			specPower = 50;
			//specFov = 90;
		}

		public void resetSpatial()
		{
			PosOffset = new Vector3();
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
		public Texture2D getTexture(bool bhilited, TrackProps globalProps)
		{
			Texture2D tex;
			if (bhilited && hilited.Texture != null)
				tex = hilited.Texture;
			else if (!bhilited && normal.Texture != null)
				tex = normal.Texture;
			else
				tex = texProps.Texture;
			if (tex == null)
			{
				if (globalProps == null) //This means texture in globalProps was null, so use default noteStyle texture
				{

				}
				else
					tex = globalProps.getTexture(bhilited, null);
			}
			return tex;
		}
		public System.Drawing.Color getSysColor(bool bhilited, TrackProps globalProps)
		{
			Color c = getColor(bhilited, globalProps, false);
			return System.Drawing.Color.FromArgb(c.R, c.G, c.B);
		}
		public Color getColor(bool bhilited, TrackProps globalProps, bool alpha)
		{
			double h, s, l;
			Material tp2;
			Material globalTp2;
			if (bhilited)
			{
				tp2 = hilited;
				globalTp2 = globalProps.hilited;
			}
			else
			{
				tp2 = normal;
				globalTp2 = globalProps.normal;
			}
			h = (double)(hue + globalProps.hue);
			if (h > 1) h -= 1;
			if (h < 0) h += 1;
			s = (double)(tp2.Sat * globalTp2.Sat);
			l = (double)(tp2.Lum * globalTp2.Lum);
			if (s > 1)
				s = 1;
			if (l > 1)
				l = 1;
			Color c = SongPanel.HSLA2RGBA(h, s, l, alpha ? (float)(transp * globalProps.transp) : 1);

			//c *= (transp * globalProps.transp * 255);
			return c;
		}
	}

	[Serializable()]
	public class TrackPropsTex : ISerializable
	{
		Texture2D texture = null;
		public Texture2D Texture
		{
			get { return texture; }
			set { texture = value; }
		}
		string path = "";
		public string Path
		{
			get { return path; }
			set { path = value; }
		}
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
			path = (string)info.GetValue("path", typeof(string));
			PointSmp = (bool)info.GetValue("PointSmp", typeof(bool));
			keepAspect = (bool)info.GetValue("keepAspect", typeof(bool));
			uTile = (bool)info.GetValue("uTile", typeof(bool));
			vTile = (bool)info.GetValue("vTile", typeof(bool));
			Anchor = (Point)info.GetValue("anchor", typeof(Point));
			Scroll = (Vector2)info.GetValue("scroll", typeof(Vector2));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("path", path);
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

		public bool loadTexture(string _path, FileStream stream, SongPanel songPanel)
		{
			path = _path;
			Texture2D tex = texture;
			if (tex != null)
			{
				tex.Dispose();
				tex = null;
			}
			tex = Texture2D.FromStream(songPanel.GraphicsDevice, stream);
			texture = createMipLevels(tex, songPanel);
			//trackProps[index].Texture = tex;
			return tex != null;
		}

		public bool loadTexture(string _path, SongPanel songPanel)
		{
			using (FileStream stream = File.Open(path, FileMode.Open))
			{
				return loadTexture(_path, stream, songPanel);
			}
		}
		public void unloadTexture()
		{
			path = "";
			if (texture != null)
			{
				texture.Dispose();
				texture = null;
			}
		}
    }

	[Serializable()]
	public class Material : ISerializable
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
		public Color color;
		public Color Color
		{
			get { return color; }
		}
		public System.Drawing.Color SysColor
		{
			get { return System.Drawing.Color.FromArgb(color.R, color.G, color.B); }
		}
		public Material()
		{
			sat = 1;
			lum = 1;
			texture = null;
		}
		public Material(float _sat, float _lum, Texture2D tex = null)
		{
			sat = _sat;
			lum = _lum;
			texture = tex;
		}
		public Material(SerializationInfo info, StreamingContext ctxt)
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
}
