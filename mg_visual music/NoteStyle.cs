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
		public Vector4 ColorDest { get; set; } = new Vector4(1,1,1,1);
		public System.Drawing.Color SystemColorDest
		{
			get
			{
				if (ColorDest == null)
					return System.Drawing.Color.Empty;
				else
					return System.Drawing.Color.FromArgb((int)(ColorDest.W * 255), (int)(ColorDest.X * 255), (int)(ColorDest.Y * 255), (int)(ColorDest.Z * 255));
			}
			set => ColorDest = new Vector4((float)value.R / 255, (float)value.G / 255, (float)value.B / 255, (float)value.A / 255);
		}
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
				else if (entry.Name == "squareAspect")
					SquareAspect = (bool)entry.Value;
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
		public float TexTileScale => Project.Camera.ViewportSize.X / 1920;
		protected const int NumDynamicVerts = 30000;
		public class Textures
		{
			public Texture2D hilited;
			public Texture2D normal;
		}
		protected static Textures[] defaultTextures = new Textures[Enum.GetValues(typeof(NoteStyleType)).GetLength(0)];

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
		protected NoteStyleType styleType; //Set in constructor of inherited class
										   //public BindingList<NoteStyleMod> ModEntries { get; set; } = new BindingList<NoteStyleMod>();
		internal List<NoteStyleMod> ModEntries { get; set; } = new List<NoteStyleMod>();
		public int? SelectedModEntryIndex { get; set; } = -1;
		internal NoteStyleMod SelectedModEntry
		{
			get
			{
				if (ModEntries == null  || SelectedModEntryIndex == null || SelectedModEntryIndex < 0 || SelectedModEntryIndex >= ModEntries.Count)
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
					styleType = (NoteStyleType)entry.Value;
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
			info.AddValue("styleType", styleType);
			info.AddValue("modEntries", ModEntries);
			info.AddValue("selectedModEntryIndex", SelectedModEntryIndex);
		}

		public static void sInitAllStyles(SongPanel _songPanel)
		{
			songPanel = _songPanel;
			NoteStyle_Bar.sInit();
			NoteStyle_Line.sInit();
			defaultTextures[(int)NoteStyleType.Bar] = new Textures();
			defaultTextures[(int)NoteStyleType.Bar].normal = new Texture2D(songPanel.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			defaultTextures[(int)NoteStyleType.Bar].normal.SetData(new[] { Color.White });
			defaultTextures[(int)NoteStyleType.Bar].hilited = defaultTextures[(int)NoteStyleType.Bar].normal;

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
			defaultTextures[(int)NoteStyleType.Line] = new Textures();
			//textures[(int)NoteStyleEnum.Line].hilited = new Texture2D(songPanel.GraphicsDevice, radius * 2, radius * 2, false, SurfaceFormat.Color);
			//textures[(int)NoteStyleEnum.Line].hilited.SetData(texData);
			defaultTextures[(int)NoteStyleType.Line].hilited = defaultTextures[(int)NoteStyleType.Bar].normal;
			defaultTextures[(int)NoteStyleType.Line].normal = defaultTextures[(int)NoteStyleType.Bar].normal;
		}
		public abstract void loadFx();
		//abstract public void createOcTree(Vector3 minPos, Vector3 size, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps globalTrackProps, TrackProps trackProps, Material texMaterial);
		public abstract void createGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial);
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
			color = trackProps.MaterialProps.getColor(bHilited, Project.GlobalTrackProps.MaterialProps, true);
			texture = trackProps.MaterialProps.getTexture(bHilited, Project.GlobalTrackProps.MaterialProps);
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

		abstract public void drawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial);

		protected void drawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial, out float songPosP)
		{
			songPanel.GraphicsDevice.SamplerStates[0] = texMaterial.TexProps.SamplerState;
			songPanel.GraphicsDevice.SamplerStates[1] = texMaterial.HmapProps.SamplerState;
			songPanel.GraphicsDevice.RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };
			songPanel.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

			fx.Parameters["BlurredEdge"].SetValue(0.002f * Project.Camera.ViewportSize.X);
			songPosP = Project.SongPosP;
			fx.Parameters["SongPos"].SetValue(songPosP);
			fx.Parameters["ViewportSize"].SetValue(new Vector2(Project.Camera.ViewportSize.X, Project.Camera.ViewportSize.Y));
			fx.Parameters["VpMat"].SetValue(Project.Camera.VpMat);
			fx.Parameters["ProjScale"].SetValue(new Vector2(Project.Camera.ProjMat.M11, Project.Camera.ProjMat.M22));

			fx.Parameters["VertWidthScale"].SetValue(Project.VertWidthScale);
			//fx.Parameters["TexWidthScale"].SetValue(texMaterial.TexProps.UAnchor == TexAnchorEnum.Screen ? VertWidthScale : 1);

			//Common notestyle props
			//EffectParameterCollection fxModEntries = fx.Parameters["ModEntries"].Elements;
			fx.Parameters["ActiveModEntries"].SetValue(ModEntries.Count);
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

				fx.Parameters["Origin"].Elements[i].SetValue(ModEntries[i].Origin);
				fx.Parameters["XOriginEnable"].Elements[i].SetValue((bool)ModEntries[i].XOriginEnable);
				fx.Parameters["YOriginEnable"].Elements[i].SetValue((bool)ModEntries[i].YOriginEnable);
				fx.Parameters["CombineXY"].Elements[i].SetValue((int)ModEntries[i].CombineXY);
				fx.Parameters["SquareAspect"].Elements[i].SetValue((bool)ModEntries[i].SquareAspect);
				fx.Parameters["ColorDestEnable"].Elements[i].SetValue((bool)ModEntries[i].ColorDestEnable);
				fx.Parameters["AngleDestEnable"].Elements[i].SetValue((bool)ModEntries[i].AngleDestEnable);
				fx.Parameters["AlphaDestEnable"].Elements[i].SetValue((bool)ModEntries[i].AlphaDestEnable);
				fx.Parameters["ColorDest"].Elements[i].SetValue(ModEntries[i].ColorDest);
				fx.Parameters["AngleDest"].Elements[i].SetValue(ModEntries[i].RadAngleDest);
				fx.Parameters["Start"].Elements[i].SetValue((float)ModEntries[i].Start);
				fx.Parameters["Stop"].Elements[i].SetValue((float)ModEntries[i].Stop);
				fx.Parameters["FadeIn"].Elements[i].SetValue((float)ModEntries[i].FadeIn);
				fx.Parameters["FadeOut"].Elements[i].SetValue((float)ModEntries[i].FadeOut);
				fx.Parameters["Power"].Elements[i].SetValue((float)ModEntries[i].Power);
				fx.Parameters["DiscardAfterStop"].Elements[i].SetValue((bool)ModEntries[i].DiscardAfterStop);
				fx.Parameters["Invert"].Elements[i].SetValue((bool)ModEntries[i].Invert);
			}

			//Material
			fx.Parameters["AmbientAmount"].SetValue((float)(Project.GlobalTrackProps.LightProps.AmbientAmount + trackProps.LightProps.AmbientAmount));
			fx.Parameters["DiffuseAmount"].SetValue((float)(Project.GlobalTrackProps.LightProps.DiffuseAmount + trackProps.LightProps.DiffuseAmount));
			fx.Parameters["SpecAmount"].SetValue((float)(Project.GlobalTrackProps.LightProps.SpecAmount + trackProps.LightProps.SpecAmount));
			fx.Parameters["SpecPower"].SetValue((float)(Project.GlobalTrackProps.LightProps.SpecPower + trackProps.LightProps.SpecPower));

			//Light
			TrackProps lightProps = (bool)trackProps.LightProps.UseGlobalLight ? Project.GlobalTrackProps : trackProps;
			Vector3 normLightDir = lightProps.LightProps.Dir;
			normLightDir.Normalize();
			fx.Parameters["LightDir"].SetValue(normLightDir);

			//Spatial props
			fx.Parameters["PosOffset"].SetValue(Project.getSpatialNormPosOffset(trackProps)); ;

			fx.Parameters["CamPos"].SetValue(Project.Camera.Pos);
		}

		//public abstract void createGeoChunk(BoundingBox bbox, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, Material texMaterial);

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
				entry = (int)SelectedModEntryIndex;
			if (ModEntries.Count > 0)
			{
				ModEntries.RemoveAt(entry);
				SelectedModEntryIndex = ModEntries.Count - 1;
			}
		}

		public void cloneModEntry(bool selectItem, int entry = -1)
		{
			if (entry < 0)
				entry = (int)SelectedModEntryIndex;
			ModEntries.Add(ModEntries[entry].clone());
			if (selectItem)
				SelectedModEntryIndex = ModEntries.Count - 1;

		}

		protected void calcRectTexCoords(out Vector2 topLeft_tex, out Vector2 size_tex, Vector2 texSize, Vector2 topLeft_world, Vector2 size_world, MaterialProps texMaterial)
		{
			topLeft_tex = calcTexCoords(texSize, topLeft_world, size_world, new Vector2(0, 0), texMaterial);
			size_tex = calcTexCoords(texSize, topLeft_world, size_world, size_world, texMaterial) - topLeft_tex;

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
			topLeft_tex -= Project.SongPosB * texMaterial.TexProps.Scroll;
		}

		protected Vector2 calcTexCoords(Vector2 texSize, Vector2 notePos, Vector2 noteSize, Vector2 posOffset, MaterialProps texMaterial)
		{
			Vector2 coords = new Vector2();
			float songPosP = Project.getScreenPosX(Project.SongPosT);
			coords.X = calcTexCoordComponent(texSize.X, Project.Camera.ViewportSize.X, notePos.X - songPosP, noteSize.X, posOffset.X, (bool)texMaterial.TexProps.UTile, (TexAnchorEnum)texMaterial.TexProps.UAnchor);
			coords.Y = calcTexCoordComponent(texSize.Y, Project.Camera.ViewportSize.Y, notePos.Y, noteSize.Y, posOffset.Y, (bool)texMaterial.TexProps.VTile, (TexAnchorEnum)texMaterial.TexProps.VAnchor);
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
	