using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Content;
using WinFormsGraphicsDevice;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;

namespace Visual_Music
{
	public enum NoteStyleEnum { Default, Bar, Line };
	public enum LineStyleEnum { Simple, Ribbon };
	public enum LineHlStyleEnum { Arrow, Circle };
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
		public Vector2 Origin
		{
			get => new Vector2(XOrigin, YOrigin);
			set
			{
				XOrigin = value.X;
				YOrigin = value.Y;
			}
		}
		
		public float XOrigin { get; set; } = 0.5f;
		public float YOrigin { get; set; } = 0.5f;
		public bool XOriginEnable { get; set; } = true;
		public bool YOriginEnable { get; set; } = true;

		public int CombineXY { get; set; } 
		public bool ColorDestEnable { get; set; }
		public bool AlphaDestEnable { get; set; }
		public bool AngleDestEnable { get; set; }
		public Vector4 ColorDest { get; set; } = new Vector4(1,1,1,1);
		public System.Drawing.Color SystemColorDest
		{
			get => System.Drawing.Color.FromArgb((int)(ColorDest.W * 255), (int)(ColorDest.X * 255), (int)(ColorDest.Y * 255), (int)(ColorDest.Z * 255));
			set => ColorDest = new Vector4((float)value.R / 255, (float)value.G / 255, (float)value.B / 255, (float)value.A / 255);
		}
		public int AngleDest { get; set; } = 45;
		public float RadAngleDest => AngleDest * (float)Math.PI / 180;
		public float Start { get; set; }
		public float Stop { get; set; } = 1;
		public float FadeIn { get; set; }
		public float FadeOut { get; set; }
		public float Power { get; set; } = 1;
		public bool DiscardAfterStop { get; set; } = true;
		public bool Invert { get; set; }

		public NoteStyleMod(string _name = "")
		{
			Name = _name;
		}

		public NoteStyleMod(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "name")
					Name = (string)entry.Value;
				else if (entry.Name == "origin")
					Origin = (Vector2)entry.Value;
				else if (entry.Name == "xOriginEnable")
					XOriginEnable = (bool)entry.Value;
				else if (entry.Name == "yOriginEnable")
					YOriginEnable = (bool)entry.Value;
				else if (entry.Name == "combineXY")
					CombineXY = (int)entry.Value;
				else if (entry.Name == "colorDestEnable")
					ColorDestEnable = (bool)entry.Value;
				else if (entry.Name == "angleDestEnable")
					AngleDestEnable = (bool)entry.Value;
				else if (entry.Name == "colorDest")
					ColorDest = (Vector4)entry.Value;
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
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("name", Name);
			info.AddValue("origin", Origin);
			info.AddValue("xOriginEnable", XOriginEnable);
			info.AddValue("yOriginEnable", YOriginEnable);
			info.AddValue("combineXY", CombineXY);
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

		public NoteStyleMod clone()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(NoteStyleMod), Form1.projectSerializationTypes);
			MemoryStream stream = new MemoryStream();
			dcs.WriteObject(stream, this);
			stream.Flush();
			stream.Position = 0;
			NoteStyleMod mod = (NoteStyleMod)dcs.ReadObject(stream);
			mod.Name += " clone";
			return mod;
		}
	}

	[Serializable()]
	abstract public class NoteStyle : ISerializable
	{
		float VertWidthScale => 16.0f / Project.ViewWidthQn;
		public float TexTileScale => Project.Camera.ViewportSize.X / 1920;
		public static bool bCull = true;
		public static bool bSkipClose = false;
		public static bool bSkipPoints = true;
		protected const int NumDynamicVerts = 30000;
		public class Textures
		{
			public Texture2D hilited;
			public Texture2D normal;
		}
		protected static Textures[] defaultTextures = new Textures[Enum.GetValues(typeof(NoteStyleEnum)).GetLength(0)];

		protected Effect fx;
		
		//protected TrackProps trackProps = null;
		//public TrackProps TrackProps
		//{
		//	get { return trackProps; }
		//	set { trackProps = value; }
		//}

		protected static SongPanel songPanel;
		protected static Project Project => songPanel.Project;
		
		//Serializable----------
		protected NoteStyleEnum styleType; //Set in constructor of inherited class
										   //public BindingList<NoteStyleMod> ModEntries { get; set; } = new BindingList<NoteStyleMod>();
		public List<NoteStyleMod> ModEntries { get; set; } = new List<NoteStyleMod>();
		public int SelectedModEntryIndex { get; set; } = -1;
		public NoteStyleMod SelectedModEntry
		{
			get
			{
				if (SelectedModEntryIndex < 0 || SelectedModEntryIndex >= ModEntries.Count)
					return null;
				else
					return ModEntries[SelectedModEntryIndex];
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
					styleType = (NoteStyleEnum)entry.Value;
				else if (entry.Name == "modEntries")
				{
					ModEntries = (List<NoteStyleMod>)entry.Value;
					if (ModEntries != null && ModEntries.Count > 0)
						SelectedModEntryIndex = 0;
				}
			}
		}

		virtual public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("styleType", styleType);
			info.AddValue("modEntries", ModEntries);
		}

		public static void sInitAllStyles(SongPanel _songPanel)
		{
			songPanel = _songPanel;
			NoteStyle_Bar.sInit();
			NoteStyle_Line.sInit();
			defaultTextures[(int)NoteStyleEnum.Bar] = new Textures();
			defaultTextures[(int)NoteStyleEnum.Bar].normal = new Texture2D(songPanel.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			defaultTextures[(int)NoteStyleEnum.Bar].normal.SetData(new[] { Color.White });
			defaultTextures[(int)NoteStyleEnum.Bar].hilited = defaultTextures[(int)NoteStyleEnum.Bar].normal;

			int radius = 20;
			Color[] texData = new Color[radius * 2 * radius * 2];
			float alphaMargin = 3;
			for (int j = 0; j < radius * 2; j++)
			{
				for (int i = 0; i < radius * 2; i++)
				{
					int i2 = i - radius, j2 = j - radius;
					Color c = Color.White;
					float distFromCenter = (float)Math.Sqrt(i2 * i2 + j2 * j2);

					if (distFromCenter >= (float)radius - alphaMargin)
					{
						float distFromOpaque = distFromCenter - (float)radius + alphaMargin;
						float alpha = (alphaMargin - distFromOpaque) / alphaMargin;
						if (alpha < 0)
							alpha = 0;
						c = Color.White * alpha;
					}
					//c = Color.White;
					texData[i + j * radius * 2] = c;
				}
			}
			defaultTextures[(int)NoteStyleEnum.Line] = new Textures();
			//textures[(int)NoteStyleEnum.Line].hilited = new Texture2D(songPanel.GraphicsDevice, radius * 2, radius * 2, false, SurfaceFormat.Color);
			//textures[(int)NoteStyleEnum.Line].hilited.SetData(texData);
			defaultTextures[(int)NoteStyleEnum.Line].hilited = defaultTextures[(int)NoteStyleEnum.Bar].normal;
			defaultTextures[(int)NoteStyleEnum.Line].normal = defaultTextures[(int)NoteStyleEnum.Bar].normal;
		}
		public abstract void loadFx();
		//abstract public void createOcTree(Vector3 minPos, Vector3 size, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps globalTrackProps, TrackProps trackProps, TrackProps texTrackProps);
		public abstract void createGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, TrackProps texTrackProps);
		public abstract void drawGeoChunk(Geo geo);

		protected void getMaterial(TrackProps trackProps, float x1, float x2, out Color color, out Texture2D texture)
		{
			bool bHilited = false;
			if (x1 < 0 && x2 > 0)
				bHilited = true;
			getMaterial(trackProps, bHilited, out color, out texture);
		}
		protected void getMaterial(TrackProps trackProps, bool bHilited, out Color color, out Texture2D texture)
		{
			color = trackProps.getColor(bHilited, Project.GlobalTrackProps, true);
			texture = trackProps.getTexture(bHilited, Project.GlobalTrackProps);
			if (texture == null)
			{
				if (bHilited)
					texture = defaultTextures[(int)styleType].hilited;
				else
					texture = defaultTextures[(int)styleType].normal;
			}
		}
		protected List<Midi.Note> getNotes(int leftMargin, Midi.Track track)
		{   //Get currently visible notes in specified track
			return track.getNotes(Project.SongPosT - Project.ViewWidthT / 2 - leftMargin, Project.SongPosT + Project.ViewWidthT / 2 + leftMargin);
		}

		abstract public void drawTrack(Midi.Track midiTrack, TrackProps trackProps, TrackProps texTrackProps);

		protected void drawTrack(Midi.Track midiTrack, TrackProps trackProps, TrackProps texTrackProps, out float songPosP)
		{
			songPanel.GraphicsDevice.SamplerStates[0] = texTrackProps.TexProps.SamplerState;
			songPanel.GraphicsDevice.SamplerStates[1] = texTrackProps.HmapProps.SamplerState;
			songPanel.GraphicsDevice.RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };
			songPanel.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

			fx.Parameters["BlurredEdge"].SetValue(Project.Camera.ViewportSize.X * 2.0f / 1000);
			songPosP = Project.getScreenPosX(Project.SongPosT);
			fx.Parameters["SongPos"].SetValue(songPosP);
			fx.Parameters["ViewportSize"].SetValue(new Vector2(Project.Camera.ViewportSize.X, Project.Camera.ViewportSize.Y));
			fx.Parameters["VpMat"].SetValue(Project.Camera.VpMat);
			fx.Parameters["ProjScale"].SetValue(new Vector2(Project.Camera.ProjMat.M11, Project.Camera.ProjMat.M22));

			fx.Parameters["VertWidthScale"].SetValue(VertWidthScale);
			//fx.Parameters["TexWidthScale"].SetValue(texTrackProps.TexProps.UAnchor == TexAnchorEnum.Screen ? VertWidthScale : 1);

			//Common notestyle props
			//EffectParameterCollection fxModEntries = fx.Parameters["ModEntries"].Elements;
			fx.Parameters["ActiveModEntries"].SetValue(ModEntries.Count);
			for (int i = 0; i < ModEntries.Count; i++)
			{
				//EffectParameterCollection fxModEntry = fxModEntries[i].StructureMembers;
				//fxModEntry["XSource"].SetValue(ModEntries[i].XSource);
				//fxModEntry["YSource"].SetValue(ModEntries[i].YSource);
				//fxModEntry["CombineXY"].SetValue(ModEntries[i].CombineXY);
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

				fx.Parameters["Origin"].Elements[i].SetValue(ModEntries[i].Origin);
				fx.Parameters["XOriginEnable"].Elements[i].SetValue(ModEntries[i].XOriginEnable);
				fx.Parameters["YOriginEnable"].Elements[i].SetValue(ModEntries[i].YOriginEnable);
				fx.Parameters["CombineXY"].Elements[i].SetValue(ModEntries[i].CombineXY);
				fx.Parameters["ColorDestEnable"].Elements[i].SetValue(ModEntries[i].ColorDestEnable);
				fx.Parameters["AngleDestEnable"].Elements[i].SetValue(ModEntries[i].AngleDestEnable);
				fx.Parameters["AlphaDestEnable"].Elements[i].SetValue(ModEntries[i].AlphaDestEnable);
				fx.Parameters["ColorDest"].Elements[i].SetValue(ModEntries[i].ColorDest);
				fx.Parameters["AngleDest"].Elements[i].SetValue(ModEntries[i].RadAngleDest);
				fx.Parameters["Start"].Elements[i].SetValue(ModEntries[i].Start);
				fx.Parameters["Stop"].Elements[i].SetValue(ModEntries[i].Stop);
				fx.Parameters["FadeIn"].Elements[i].SetValue(ModEntries[i].FadeIn);
				fx.Parameters["FadeOut"].Elements[i].SetValue(ModEntries[i].FadeOut);
				fx.Parameters["Power"].Elements[i].SetValue(ModEntries[i].Power);
				fx.Parameters["DiscardAfterStop"].Elements[i].SetValue(ModEntries[i].DiscardAfterStop);
				fx.Parameters["Invert"].Elements[i].SetValue(ModEntries[i].Invert);
			}

			//Material
			fx.Parameters["AmbientAmount"].SetValue((float)(Project.GlobalTrackProps.AmbientAmount * trackProps.AmbientAmount));
			fx.Parameters["DiffuseAmount"].SetValue((float)(Project.GlobalTrackProps.DiffuseAmount * trackProps.DiffuseAmount));
			fx.Parameters["SpecAmount"].SetValue((float)(Project.GlobalTrackProps.SpecAmount * trackProps.SpecAmount));
			fx.Parameters["SpecPower"].SetValue((float)(Project.GlobalTrackProps.SpecPower * trackProps.SpecPower));

			//Spatial props
			fx.Parameters["PosOffset"].SetValue(Project.GlobalTrackProps.PosOffset + trackProps.PosOffset);

			//Light
			TrackProps lightProps = (bool)trackProps.UseGlobalLight ? Project.GlobalTrackProps : trackProps;
			Vector3 normLightDir = lightProps.LightDir;
			normLightDir.Normalize();
			fx.Parameters["LightDir"].SetValue(normLightDir);

			fx.Parameters["CamPos"].SetValue(Project.Camera.Pos);
		}

		//public abstract void createGeoChunk(BoundingBox bbox, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, TrackProps texTrackProps);
		
		public void addModEntry(bool selectItem, string name = "")
		{
			if (string.IsNullOrWhiteSpace(name))
				name = "Entry " + ModEntries.Count;
			ModEntries.Add(new NoteStyleMod(name));
			if (selectItem)
				SelectedModEntryIndex = ModEntries.Count - 1;

		}

		public void deleteModEntry(int entry = -1)
		{
			if (entry < 0)
				entry = SelectedModEntryIndex;
			if (ModEntries.Count > 0)
			{
				ModEntries.RemoveAt(entry);
				SelectedModEntryIndex = ModEntries.Count - 1;
			}
		}

		public void cloneModEntry(bool selectItem, int entry = -1)
		{
			if (entry < 0)
				entry = SelectedModEntryIndex;
			ModEntries.Add(ModEntries[entry].clone());
			if (selectItem)
				SelectedModEntryIndex = ModEntries.Count - 1;

		}

		protected void calcRectTexCoords(out Vector2 topLeft_tex, out Vector2 size_tex, Vector2 texSize, Vector2 topLeft_world, Vector2 size_world, TrackProps texTrackProps)
		{
			topLeft_tex = calcTexCoords(texSize, topLeft_world, size_world, new Vector2(0, 0), texTrackProps);
			size_tex = calcTexCoords(texSize, topLeft_world, size_world, size_world, texTrackProps) - topLeft_tex;

			if ((bool)texTrackProps.TexProps.KeepAspect)
			{
				float uTexelsPerPixel = size_tex.X * texSize.X / size_world.X;
				float vTexelsPerPixel = size_tex.Y * texSize.Y / size_world.Y;
				float uvRatio = -uTexelsPerPixel / vTexelsPerPixel;
				if ((bool)texTrackProps.TexProps.UTile && !(bool)texTrackProps.TexProps.VTile)
				{
					topLeft_tex.X = topLeft_tex.X / uvRatio;
					size_tex.X = size_tex.X / uvRatio;
				}
				else if (!(bool)texTrackProps.TexProps.UTile && (bool)texTrackProps.TexProps.VTile)
				{
					topLeft_tex.Y = topLeft_tex.Y * uvRatio;
					size_tex.Y = size_tex.Y * uvRatio;
				}
			}
			topLeft_tex -= Project.SongPosB * texTrackProps.TexProps.Scroll;
		}

		protected Vector2 calcTexCoords(Vector2 texSize, Vector2 notePos, Vector2 noteSize, Vector2 posOffset, TrackProps texTrackProps)
		{
			Vector2 coords = new Vector2();
			float songPosP = Project.getScreenPosX(Project.SongPosT);
			coords.X = calcTexCoordComponent(texSize.X, Project.Camera.ViewportSize.X, notePos.X - songPosP, noteSize.X, posOffset.X, (bool)texTrackProps.TexProps.UTile, (TexAnchorEnum)texTrackProps.TexProps.UAnchor);
			coords.Y = calcTexCoordComponent(texSize.Y, Project.Camera.ViewportSize.Y, notePos.Y, noteSize.Y, posOffset.Y, (bool)texTrackProps.TexProps.VTile, (TexAnchorEnum)texTrackProps.TexProps.VAnchor);
			coords.Y *= -1;
			return coords;
		}

		float calcTexCoordComponent(float texSize, float vpSize, float notePos, float noteSize, float posOffset, bool tile, TexAnchorEnum anchor)
		{
			if (tile)
				texSize *= TexTileScale;

			if (anchor == TexAnchorEnum.Screen)
			{
				float screenPos = notePos + posOffset + vpSize / 2;
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
			else //anchor at song start	
			{
				//float songPos = Project.getSongPosP((float)notePos + posOffset);
				float x = Project.getScreenPosX(Project.SongPosT) + notePos + posOffset;
				if (!tile)
					return x / Project.SongLengthP;
				else
					return x / texSize;
			}
		}
	}

	public abstract class Geo
	{
		public List<BoundingBox> bboxes = new List<BoundingBox>();
		public abstract void Dispose();
	}
}
	