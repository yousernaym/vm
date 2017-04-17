using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace Visual_Music
{
	public enum TexAnchorEnum { Note = 0, Screen, Song };

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
		//bool pointSmp;
		public bool PointSmp
		{
			get { return samplerStateBacking.Filter == TextureFilter.Point; }
			set { SamplerStateBacking.Filter = value ? TextureFilter.Point : TextureFilter.Linear; }
		}
		
		bool uTile = false;
		public bool UTile
		{
			get { return uTile; }
			set { uTile = value; }
		}
		bool vTile = false;
		public bool VTile
		{
			get { return vTile; }
			set { vTile = value; }
		}
		bool keepAspect = false;
		public bool KeepAspect
		{
			get { return keepAspect; }
			set { keepAspect = value; }
		}
		Point anchor;
		public TexAnchorEnum UAnchor
		{
			get { return (TexAnchorEnum)anchor.X; }
			set { anchor.X = (int)value; }
		}
		public TexAnchorEnum VAnchor
		{
			get { return (TexAnchorEnum)anchor.Y; }
			set { anchor.Y = (int)value; }
		}
		Vector2 scroll;
		public Vector2 Scroll
		{
		    get { return scroll; }
		    set { scroll = value; }
		}
		public float UScroll
		{
			get { return scroll.X; }
			set { scroll.X = value; }
		}
		public float VScroll
		{
			get { return scroll.Y; }
			set { scroll.Y = value; }
		}
		
		//Methods----------------------
		public TrackPropsTex()
		{
			samplerStateBacking.AddressU = TextureAddressMode.Wrap;
			samplerStateBacking.AddressV = TextureAddressMode.Wrap;
			anchor.X = (int)TexAnchorEnum.Note;
			anchor.Y = (int)TexAnchorEnum.Note;
		}
		public TrackPropsTex(SerializationInfo info, StreamingContext ctxt)
		{
			path = (string)info.GetValue("path", typeof(string));
			PointSmp = (bool)info.GetValue("PointSmp", typeof(bool));
			keepAspect = (bool)info.GetValue("keepAspect", typeof(bool));
			uTile = (bool)info.GetValue("uTile", typeof(bool));
			vTile = (bool)info.GetValue("vTile", typeof(bool));
			anchor = (Point)info.GetValue("anchor", typeof(Point));
			scroll = (Vector2)info.GetValue("scroll", typeof(Vector2));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("path", path);
			info.AddValue("PointSmp", PointSmp);
			info.AddValue("keepAspect", keepAspect);
			info.AddValue("uTile", uTile);
			info.AddValue("vTile", vTile);
			info.AddValue("anchor", anchor);
			info.AddValue("scroll", scroll);
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
	public class TrackProps : ISerializable
	{
		TrackPropsTex texProps = new TrackPropsTex();
		public TrackPropsTex TexProps
		{
			get { return texProps; }
		}
		TrackPropsTex hmapProps = new TrackPropsTex();
		public TrackPropsTex HmapProps
		{
			get { return hmapProps; }
		}
		public TrackPropsTex getTexProps(int selector)
		{
			if (selector == 0)
				return texProps;
			else
				return hmapProps;
		}
		
		
		static bool bgr = false;
		static public bool Bgr
		{
			get { return bgr; }
			set { bgr = value; }
		}
		int trackNumber;
		public int TrackNumber
		{
			get { return trackNumber; }
		}

		static int numTracks;
		static public int NumTracks
		{
		    get { return numTracks; }
		    set { numTracks = value; }
		}
		float transp;
		public float Transp
		{
			get { return transp; }
			set { transp = value; }
		}
		float hue;
		public float Hue
		{
			get { return hue; }
			set { hue = value; }
		}
		NoteStyle noteStyle;
		internal NoteStyle NoteStyle
		{
			get { return noteStyle; }
			set
			{
				noteStyle = value;
				noteStyle.TrackProps = this;
			}
		}
		NoteStyleProps_Line lineStyleProps = new NoteStyleProps_Line();
		internal NoteStyleProps_Line LineStyleProps
		{
			get { return lineStyleProps; }
			//set { lineStyleProps = value; }
		}
		public int NoteStyleIndex
		{
			get
			{
				if (noteStyle == null)
					return (int)NoteStyleEnum.Default;
				else
					return noteStyle.Index;
			}
		}

		TrackProps2 normal;
		internal TrackProps2 Normal
		{
			get { return normal; }
			set { normal = value; }
		}
		TrackProps2 hilited;
		internal TrackProps2 Hilited
		{
			get { return hilited; }
			set { hilited = value; }
		}
	
		Curve curve = new Curve();
		public Curve Curve
		{
			get { return curve; }
			set { curve = value; }
		}
		Midi.Track midiTrack;
		public Midi.Track MidiTrack
		{
			get { return midiTrack; }
			set { midiTrack = value; }
		}
		bool useGlobalLight;
		public bool UseGlobalLight
		{
			get { return useGlobalLight; }
			set { useGlobalLight = value; }
		}
		Vector3 lightDir;
		public Vector3 LightDir
		{
			get { return lightDir; }
			set { lightDir = value; }
		}
		public void setLightDirX(float value)
		{ lightDir.X = value; }
		public void setLightDirY(float value)
		{ lightDir.Y = value; }
		public void setLightDirZ(float value)
		{ lightDir.Z = value; }
		float specAmount;
		public float SpecAmount
		{
			get { return specAmount; }
			set { specAmount = value; }
		}
		float specPower;
		public float SpecPower
		{
			get { return specPower; }
			set { specPower = value; }
		}
		float specFov;
		public float SpecFov
		{
			get { return specFov; }
			set { specFov = value; }
		}

		public TrackProps(int _trackNumber, int _numTracks, Midi.Song song)
		{
			midiTrack = song.Tracks[_trackNumber];
			trackNumber = _trackNumber;
			numTracks = _numTracks;

			createCurve();
			resetProps();
		}
	
		public TrackProps(SerializationInfo info, StreamingContext ctxt)
		{
			trackNumber = (int)info.GetValue("trackNumber", typeof(int));
			transp = (float)info.GetValue("transp", typeof(float));
			hue = (float)info.GetValue("hue", typeof(float));
			normal = (TrackProps2)info.GetValue("normal", typeof(TrackProps2));
			hilited = (TrackProps2)info.GetValue("hilited", typeof(TrackProps2));
			texProps = (TrackPropsTex)info.GetValue("texProps", typeof(TrackPropsTex));
			hmapProps = (TrackPropsTex)info.GetValue("hmapProps", typeof(TrackPropsTex));
			NoteStyle = (NoteStyle)info.GetValue("noteStyle", typeof(NoteStyle));
			noteStyle.TrackProps = this;
			lineStyleProps = (NoteStyleProps_Line)info.GetValue("lineStyleProps", typeof(NoteStyleProps_Line));
			//curve = (Curve)info.GetValue("curve", typeof(Curve));
			lightDir = (Vector3)info.GetValue("lightDir", typeof(Vector3));
			specAmount = (float)info.GetValue("specAmount", typeof(float));
			specPower = (float)info.GetValue("specPower", typeof(float));
			specFov = (float)info.GetValue("specFov", typeof(float));
			useGlobalLight = (bool)info.GetValue("useGlobalLight", typeof(bool));
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("trackNumber", trackNumber);
			info.AddValue("transp", transp);
			info.AddValue("hue", hue);
			info.AddValue("normal", normal);
			info.AddValue("hilited", hilited);
			info.AddValue("texProps", texProps);
			info.AddValue("hmapProps", hmapProps);
			info.AddValue("noteStyle", noteStyle);
			info.AddValue("lineStyleProps", lineStyleProps);
			//info.AddValue("curve", curve);
			info.AddValue("lightDir", lightDir);
			info.AddValue("specAmount", specAmount);
			info.AddValue("specPower", specPower);
			info.AddValue("specFov", specAmount);
			info.AddValue("useGlobalLight", useGlobalLight);

		}
		public TrackProps copyTo(TrackProps dest)
		{
			Midi.Track midiTrack = dest.midiTrack;
			Curve curve = dest.curve;
			dest = this.clone();
			dest.midiTrack = midiTrack;
			dest.curve = curve;
			return dest;
		}
		TrackProps clone()
		{
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			bf.Serialize(stream, this);
			stream.Flush();
			stream.Position = 0;
			TrackProps dest = (TrackProps)bf.Deserialize(stream);
			dest.MidiTrack = midiTrack;
			//dest.NumTracks = numTracks;
			return dest;
		}
		public void createCurve(/*Midi.Song song*/)
		{
			curve = new Curve();
			foreach (Midi.Note note in midiTrack.Notes)
			{
				float pos = (float)note.start;
				//float value = (note.pitch - song.MinPitch) / (float)song.NumPitches;
				float value = note.pitch;
				curve.Keys.Add(new CurveKey(pos, value));
			}
			setCurveTangents();
		}
		public void setCurveTangents()
		{
			for (int i = 0; i < curve.Keys.Count; i++)
			{
				int prevIndex = i == 0 ? i : i - 1;
				int nextIndex = i == curve.Keys.Count - 1 ? i : i + 1;
				CurveKey prev = curve.Keys[prevIndex], cur = curve.Keys[i], next = curve.Keys[nextIndex];

				float dt = next.Position - prev.Position;
				float dv = next.Value - prev.Value;
				if (Math.Abs(dv) < float.Epsilon)
				{
					cur.TangentIn = 0;
					cur.TangentOut = 0;
				}
				else
				{
					cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
					cur.TangentOut = dv * (next.Position - cur.Position) / dt;
				}
			}
		}

		public void resetMaterial()
		{
			texProps.unloadTexture();
			if (trackNumber == 0)
			{
				transp = 1;
				hue = 0.1f;
				normal = new TrackProps2(1, 0.27f);
				hilited = new TrackProps2(0.8f, 0.75f);
			}
			else
			{
				transp = 0.5f;
				hue = (float)(trackNumber - 1) / (numTracks - 1);
				normal = new TrackProps2();
				hilited = new TrackProps2(); ;
			}
		}
		
		public void resetStyle()
		{
			if (trackNumber == 0)
			{
				noteStyle = new NoteStyle_Bar(this);
				UseGlobalLight = false;
			}
			else
			{
				noteStyle = new NoteStyle_Default(this);
				UseGlobalLight = true;
			}
		}
		public void resetLight()
		{
			if (trackNumber == 0)
				UseGlobalLight = false;
			else
				UseGlobalLight = true;
			lightDir = new Vector3(-1, -1, 1);
			specAmount = 0.75f;
			specPower = 50;
			specFov = 90;
		}
		public void resetProps()
		{
			resetMaterial();
			resetStyle();
			resetLight();
		}
		public void drawTrack(SongDrawProps songDrawProps, TrackProps globalTrackProps, bool selectingRegion)
		{
			if (selectingRegion)
			{
				NoteStyle bars = new NoteStyle_Bar();
				bars.drawTrack(midiTrack, songDrawProps, this, globalTrackProps);
			}
			else
				getNoteStyle(globalTrackProps).drawTrack(midiTrack, songDrawProps, this, globalTrackProps);
		}
		//public void drawNote(NoteDrawProps drawProps, TrackProps globalProps)
		//{
			//getNoteStyle(globalProps).draw(drawProps, getColor(drawProps.bHilited, globalProps, true), getTexture(drawProps.bHilited, globalProps));
		//}
		//public void calcColors(bool _bgr = false)
		//{
		//    bgr = _bgr;
		//    normal.calcColor(hue, bgr);
		//    hilited.calcColor(hue, bgr);
		//}
		//public void setNoteStyle(string styleString)
		//{
		//    if (styleString == NoteStyleEnum.Default.ToString())
		//        noteStyle = null;
		//    else if (styleString == NoteStyleEnum.Bar.ToString())
		//        noteStyle = new NoteStyle_Bar();
		//    else if (styleString == NoteStyleEnum.Line.ToString())
		//        noteStyle = new NoteStyle_Line();
		//}

		NoteStyle getNoteStyle(TrackProps globalProps)
		{
			NoteStyle ns = noteStyle is NoteStyle_Default ? globalProps.noteStyle : noteStyle;
			if (ns is NoteStyle_Default) //if global has notestyle set to default
				ns = new NoteStyle_Bar(this);
			return ns;
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
			TrackProps2 tp2;
			TrackProps2 globalTp2;
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
			h = hue + globalProps.hue;
			if (h > 1) h -= 1;
			if (h < 0) h += 1;
			s = tp2.Sat * globalTp2.Sat;
			l = tp2.Lum * globalTp2.Lum;
			if (s > 1)
			    s = 1;
			if (l > 1)
			    l = 1;
			Color c = SongPanel.HSLA2RGBA(h, s, l, alpha ? transp * globalProps.transp : 1, bgr);
			
			//c *= (transp * globalProps.transp * 255);
			return c;
		}
	}

	[Serializable()]
	class TrackProps2 : ISerializable
	{
		//TrackProps parent;
		float sat = 1;
		public float Sat
		{
			get { return sat; }
			set { sat = value; }
		}
		float lum = 1;
		public float Lum
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
		public TrackProps2()
		{
			sat = 1;
			lum = 1;
			texture = null;
		}
		public TrackProps2(float _sat, float _lum, Texture2D tex = null)
		{
			sat = _sat;
			lum = _lum;
			texture = tex;
		}
		public TrackProps2(SerializationInfo info, StreamingContext ctxt)
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
		//public void calcColor(float hue, bool bgr)
		//{
		//    color = SongPanel.HSLA2RGBA(hue, sat, lum, 1, bgr);
		//}
	}
	
}
