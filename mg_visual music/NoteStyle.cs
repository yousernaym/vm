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
	public enum LineStyleEnum { Simple, Ribbon, Tube };
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
		//static string[,] SourceType =
		//{
		//	{ "DistFromLeft","DistFromTop" },
		//	{ "DistFromCenter","DistFromCenter" },
		//	{ "DistFromRight","DistFromBottom" }
		//};			
		public string Name { get; set; }
		//public bool Bypass { get; set; }
		public int XSource { get; set; }
		public int YSource { get; set; } 
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
				else if (entry.Name == "xSource")
					XSource = (int)entry.Value;
				else if (entry.Name == "ySource")
					YSource = (int)entry.Value;
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
				else if (entry.Name == "DiscardAfterStop")
					DiscardAfterStop = (bool)entry.Value;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("name", Name);
			info.AddValue("xSource", XSource);
			info.AddValue("ySource", YSource);
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

		protected TrackProps trackProps = null;
		public TrackProps TrackProps
		{
			get { return trackProps; }
			set { trackProps = value; }
		}

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
			trackProps = tprops;
		}
		public NoteStyle(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "styleType")
					styleType = (NoteStyleEnum)entry.Value;
				if (entry.Name == "modEntries")
					ModEntries = (List<NoteStyleMod>)entry.Value;
			}
		}

		virtual public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("styleType", styleType);
			info.AddValue("ModEntries", ModEntries);
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
		abstract public void loadFx();

		protected void getMaterial(SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, int x1, int x2, out Color color, out Texture2D texture)
		{
			bool bHilited = false;
			if (x1 < 0 && x2 > 0)
				bHilited = true;
			getMaterial(songDrawProps, trackProps, globalTrackProps, bHilited, out color, out texture);
		}
		protected void getMaterial(SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, bool bHilited, out Color color, out Texture2D texture)
		{
			color = trackProps.getColor(bHilited, globalTrackProps, true);
			texture = trackProps.getTexture(bHilited, globalTrackProps);
			if (texture == null)
			{
				if (bHilited)
					texture = defaultTextures[(int)styleType].hilited;
				else
					texture = defaultTextures[(int)styleType].normal;
			}
		}
		protected List<Midi.Note> getNotes(int leftMargin, Midi.Track track, SongDrawProps songDrawProps)
		{   //Get currently visible notes in specified track
			return track.getNotes(songDrawProps.songPosT - songDrawProps.viewWidthT / 2 - leftMargin, songDrawProps.songPosT + songDrawProps.viewWidthT / 2 + leftMargin);
		}
		virtual public void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, bool selectingRegion, TrackProps texTrackProps)
		{
			Camera cam = selectingRegion ? Project.DefaultCamera : Project.Camera;

			songPanel.GraphicsDevice.SamplerStates[0] = texTrackProps.TexProps.SamplerState;
			songPanel.GraphicsDevice.SamplerStates[1] = texTrackProps.HmapProps.SamplerState;
			songPanel.GraphicsDevice.RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };
			songPanel.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			fx.Parameters["ViewportSize"].SetValue(new Vector2(songDrawProps.viewportSize.X, songDrawProps.viewportSize.Y));
			fx.Parameters["VpMat"].SetValue(cam.VpMat);
			Matrix projMat = cam.ProjMat;
			fx.Parameters["ProjScale"].SetValue(new Vector2(projMat.M11, projMat.M22));

			//Common notestyle props
			//EffectParameterCollection fxModEntries = fx.Parameters["ModEntries"].Elements;
			fx.Parameters["ActiveModEntries"].SetValue(ModEntries.Count);
			//fx.Parameters["Origin"].SetValue(new Vector2[1] { new Vector2(1, 2) });
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

				//fx.Parameters["XSource"].Elements[i].SetValue(ModEntries[i].XSource);
				//fx.Parameters["YSource"].Elements[i].SetValue(ModEntries[i].YSource);
				fx.Parameters["Origin"].Elements[i].SetValue(new Vector2(0.5f, 0.5f));
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
			}

			//Light props
			TrackProps lightProps = (bool)trackProps.UseGlobalLight ? globalTrackProps : trackProps;
			Vector3 normLightDir = lightProps.LightDir;
			normLightDir.Normalize();
			fx.Parameters["LightDir"].SetValue(normLightDir);
			fx.Parameters["SpecAmount"].SetValue((float)lightProps.SpecAmount);
			fx.Parameters["SpecPower"].SetValue((float)lightProps.SpecPower);
			//float angle = lightProps.SpecFov * (float)Math.PI / (360);
			//float camPosZ = (songDrawProps.viewportSize.X / 2) / (float)Math.Tan(angle);
			//Vector3 specCamPos = new Vector3(songDrawProps.viewportSize.X / 2, songDrawProps.viewportSize.Y / 2, camPosZ);
			fx.Parameters["CamPos"].SetValue(Project.Camera.Pos);

			//Spatial props
			Vector3 posOffset = globalTrackProps.PosOffset + trackProps.PosOffset;
			posOffset *= 0.01f * songDrawProps.viewportSize.X;
			posOffset.Z -= projMat.M22 * songDrawProps.viewportSize.Y;
			fx.Parameters["PosOffset"].SetValue(posOffset);
		}

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
	}
}
	