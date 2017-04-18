﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Visual_Music
{
	enum NoteStyleEnum { Default, Bar, Line };
	enum LineStyleEnum { Simple, Ribbon, Tube };
	enum LineHlStyleEnum { Arrow, Circle };

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
	public struct LineVertex : IVertexType
	{
		public Vector3 pos;
		public Vector3 normal;
		public Vector3 normal2;
		public Vector3 center;
		public Vector2 texCoords;
		public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Normal, 1), new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Position, 1), new VertexElement(48, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}
		public LineVertex(Vector3 _pos, Vector3 _normal, Vector3 _normal2, Vector3 _center, Vector2 _texCoords)
		{
		    pos = _pos;
		    normal = _normal;
			normal2 = _normal2;
		    center = _center;
			texCoords = _texCoords;
		}
		//static public DynamicVertexBuffer vertexBuffer;
		//static public void init(GraphicsDevice gfxDevice, int numVerts)
		//{
			//vertexBuffer = new DynamicVertexBuffer(gfxDevice, typeof(LineVertex), numVerts, BufferUsage.None);
		//}
	}

	[Serializable()]
	public abstract class NoteStyle : ISerializable
	{
		protected static TestVertex[] testVerts = new TestVertex[30];
		protected static LineVertex[] lineVerts = new LineVertex[30000];
		protected static LineVertex[] hLineVerts = new LineVertex[30000];
		protected static short[] lineInds = new short[lineVerts.Length];
		//protected static LineVertex[] arrowAreaVerts = new LineVertex[3];
		//protected static LineVertex[] arrowBorderVerts = new LineVertex[3];
		protected static LineVertex[] lineHlVerts = new LineVertex[4];
		public class Textures
		{
			public Texture2D hilited;
			public Texture2D normal;
		}
		protected static Textures[] textures = new Textures[Enum.GetValues(typeof(NoteStyleEnum)).GetLength(0)];
		protected static Effect circleFx;
		public static Effect CircleFx
		{
			get { return NoteStyle.circleFx; }
			set { NoteStyle.circleFx = value; }
		}
		protected static Effect lineFx;
		public static Effect LineFx
		{
			get { return NoteStyle.lineFx; }
			set { NoteStyle.lineFx = value; }
		}
		protected static Effect barFx;
		public static Effect BarFx
		{
			get { return NoteStyle.barFx; }
			set { NoteStyle.barFx = value; }
		}
		protected int index;
		public int Index
		{
			get { return index; }
			set { index = value; }
		}
		public string Name
		{
			get { return Enum.GetNames(typeof(NoteStyleEnum))[index]; }
		}
		virtual public NoteStyle Value
		{
			get { return this; }
		}
		protected TrackProps trackProps = null;
		public TrackProps TrackProps
		{
			get { return trackProps; }
			set { trackProps = value; }
		}

		//virtual public int LineWidth
		//{
		//    set { }
		//    get { return 0; }
		//}
		//public Texture2D Texture
		//{
		//    get { return textures[index]; }
		//}
		protected static SongPanel songPanel;

		public NoteStyle()
		{
		}
		public NoteStyle(TrackProps tprops)
		{
			trackProps = tprops;
		}
		public NoteStyle(SerializationInfo info, StreamingContext ctxt)
		{
			index = (int)info.GetValue("index", typeof(int));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("index", index);
		}
		public static void init(SongPanel _songPanel)
		{
			songPanel = _songPanel;
			textures[(int)NoteStyleEnum.Bar] = new Textures();
			textures[(int)NoteStyleEnum.Bar].normal = new Texture2D(songPanel.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			textures[(int)NoteStyleEnum.Bar].normal.SetData(new[] { Color.White });
			textures[(int)NoteStyleEnum.Bar].hilited = textures[(int)NoteStyleEnum.Bar].normal;

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
			textures[(int)NoteStyleEnum.Line] = new Textures();
			//textures[(int)NoteStyleEnum.Line].hilited = new Texture2D(songPanel.GraphicsDevice, radius * 2, radius * 2, false, SurfaceFormat.Color);
			//textures[(int)NoteStyleEnum.Line].hilited.SetData(texData);
			textures[(int)NoteStyleEnum.Line].hilited = textures[(int)NoteStyleEnum.Bar].normal;
			textures[(int)NoteStyleEnum.Line].normal = textures[(int)NoteStyleEnum.Bar].normal;

			for (short i = 0; i < lineInds.Length; i++)
				lineInds[i] = i;
			//LineVertex.init(songPanel.GraphicsDevice, lineInds.Length);
		}
		protected void getMaterial(SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, int x1, int x2, out Color color, out Texture2D texture)
		{
			bool bHilited = false;
			if (x1 < songDrawProps.viewportSize.X / 2 && x2 > songDrawProps.viewportSize.X / 2)
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
					texture = textures[index].hilited;
				else
					texture = textures[index].normal;
			}
		}
		protected List<Midi.Note> getNotes(int leftMargin, Midi.Track track, SongDrawProps songDrawProps)
		{
			return track.getNotes(songDrawProps.songPosT - songDrawProps.viewWidthT / 2 - leftMargin, songDrawProps.songPosT + songDrawProps.viewWidthT / 2 /*+ margin*/);
		}
		public abstract void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps);
		//public abstract void draw(NoteDrawProps drawProps, Color color, Texture2D texture, int pass);
	}

	[Serializable()]
	class NoteStyle_Default : NoteStyle
	{
		//public override NoteStyle Value
		//{
		//    get { return null; }
		//}
		public NoteStyle_Default()
		{
			index = (int)NoteStyleEnum.Default;
		}
		public NoteStyle_Default(TrackProps tprops)
			: base(tprops)
		{
			index = (int)NoteStyleEnum.Default;
		}
		public NoteStyle_Default(SerializationInfo info, StreamingContext ctxt)
			: base(info, ctxt)
		{
		}
		public override void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps)
		{
			throw new NotImplementedException();
		}
		//public override void draw(NoteDrawProps drawProps, Color color, Texture2D texture, int pass)
		//{
		//throw new NotImplementedException();
		//}
	}
	[Serializable()]
	class NoteStyle_Bar : NoteStyle
	{
		public NoteStyle_Bar()
		{
			index = (int)NoteStyleEnum.Bar;
		}
		public NoteStyle_Bar(TrackProps tprops)
			: base(tprops)
		{
			index = (int)NoteStyleEnum.Bar;
		}
		public NoteStyle_Bar(SerializationInfo info, StreamingContext ctxt)
			: base(info, ctxt)
		{
		}
		public override void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps)
		{
			List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			if (noteList.Count == 0)
				return;
			TrackProps texTrackProps = trackProps.getTexture(false, null) != null ? trackProps : globalTrackProps;
			songPanel.SpriteBatch.Begin(SpriteSortMode.Deferred, songPanel.BlendState, texTrackProps.TexProps.SamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			for (int n = 0; n < noteList.Count; n++)
			{
				Midi.Note note = noteList[n], nextNote;
				if (note.start > songDrawProps.song.SongLengthInTicks) //only  if audio ends before the notes end
					continue;

				if (n < noteList.Count - 1)
					nextNote = noteList[n + 1];
				else
					nextNote = note;

				Vector2 noteStart = songDrawProps.getScreenPosF(note.start, note.pitch);
				Vector2 noteEnd = songDrawProps.getScreenPosF(note.stop, note.pitch);

				//noteDrawProps.nextNoteX = (int)(((float)(nextNote.start - songPos) / viewWidthT + 0.5) * viewportSize.X);
				//noteDrawProps.nextNoteY = viewportSize.Y - (nextNote.pitch - notes.MinPitch) * noteHeight - noteHeight / 2 - yMargin;

				Color color;
				Texture2D texture;
				getMaterial(songDrawProps, trackProps, globalTrackProps, (int)noteStart.X, (int)noteEnd.X, out color, out texture);

				Rectangle destRect = new Rectangle((int)noteStart.X, (int)(noteStart.Y - songDrawProps.noteHeight / 2), (int)(noteEnd.X - noteStart.X + 1), (int)(songDrawProps.noteHeight - 1));
				Rectangle srcRect = destRect;

				if (texture != null) //Unnecessary because texture is never null. Can revert to default 1x1 white pixel.
				{
					setSrcRect(out srcRect.X, out srcRect.Width, texture.Width, songDrawProps.viewportSize.X, destRect.X, destRect.Width, texTrackProps.TexProps.UTile, texTrackProps.TexProps.UAnchor, songDrawProps);
					setSrcRect(out srcRect.Y, out srcRect.Height, texture.Height, songDrawProps.viewportSize.Y, destRect.Y, destRect.Height, texTrackProps.TexProps.VTile, texTrackProps.TexProps.VAnchor, songDrawProps);
					if (texTrackProps.TexProps.KeepAspect)
					{
						float uTexelsPerPixel = (float)srcRect.Width / destRect.Width;
						float vTexelsPerPixel = (float)srcRect.Height / destRect.Height;
						if (texTrackProps.TexProps.UTile && !texTrackProps.TexProps.VTile)
						{
							srcRect.X = (int)(destRect.X * vTexelsPerPixel);
							srcRect.Width = (int)(destRect.Width * vTexelsPerPixel);
						}
						else if (!texTrackProps.TexProps.UTile && texTrackProps.TexProps.VTile)
						{
							srcRect.Y = (int)(destRect.Y * uTexelsPerPixel);
							srcRect.Height = (int)(destRect.Height * uTexelsPerPixel);
						}
					}
					Vector2 texScroll = songDrawProps.songPosS * texTrackProps.TexProps.Scroll;
					srcRect.X -= (int)(texScroll.X * texture.Width);
					srcRect.Y -= (int)(texScroll.Y * texture.Height);
				}
				songPanel.SpriteBatch.Draw(texture, destRect, srcRect,color);
			}
			songPanel.SpriteBatch.End();
		}
		void setSrcRect(out int pos, out int size, int texSize, int vpSize, int notePos, int noteSize, bool tile, TexAnchorEnum anchor, SongDrawProps songDrawProps)
		{
			if (anchor == TexAnchorEnum.Screen)
			{ 
				if (!tile)
				{ 
					float f = (float)texSize / vpSize;
					pos = (int)(notePos * f);
					size = (int)(noteSize * f);
				}
				else
				{ 
					pos = notePos;
					size = noteSize;
				}
			}
			else if (anchor == TexAnchorEnum.Note)
			{
				pos = 0;
				if (!tile)
				{ 
					size = texSize;
				}
				else
				{ 
					size = noteSize;
				}
			}
			else //anchor at song start	
			{
				//tile
				pos = (int)songDrawProps.getSongPosP((float)notePos);
				size = noteSize;
				if (!tile)
				{
					float songLengthP = songDrawProps.getSongLengthP();
					pos = (int)((pos * texSize) / songLengthP);
					size = (int)((size * texSize) / songLengthP);
				}
			}
		}
		//public override void draw(NoteDrawProps drawProps, Color color, Texture2D texture, int pass)
		//{
		//if (texture == null)
		//    texture = textures[index];
		//songPanel.SpriteBatch.Draw(texture, new Rectangle(drawProps.x1, drawProps.y - drawProps.noteHeight / 2, drawProps.x2 - drawProps.x1 + 1, drawProps.noteHeight), color);
		//}
	}

	[Serializable()]
	class NoteStyleProps_Line : ISerializable
	{
		public float qn_gapThreshold = 5;
		public int lineWidth = 5;
		public float fadeOut = 1;
		public int blurredEdge = 2;
		public float shapePower = 1;
		public LineStyleEnum style = LineStyleEnum.Simple;
		public LineHlStyleEnum hlStyle = LineHlStyleEnum.Arrow;
		public int hlSize = 25;
		public bool movingHl = false;
		public bool shrinkingHl = false;
		public bool hlBorder = false;
		public NoteStyleProps_Line()
		{
		}
		public NoteStyleProps_Line(SerializationInfo info, StreamingContext ctxt)
		{
			qn_gapThreshold = (float)info.GetValue("qn_gapThreshold", typeof(float));
			lineWidth = (int)info.GetValue("lineWidth", typeof(int));
			fadeOut = (float)info.GetValue("fadeOutFromCenter", typeof(float));
			shapePower = (float)info.GetValue("shapePower", typeof(float));
			blurredEdge = (int)info.GetValue("blurredEdge", typeof(int));
			style = (LineStyleEnum)info.GetValue("style", typeof(LineStyleEnum));
			hlStyle = (LineHlStyleEnum)info.GetValue("hlStyle", typeof(LineHlStyleEnum));
			hlSize = (int)info.GetValue("hlSize", typeof(int));
			movingHl = (bool)info.GetValue("movingHl", typeof(bool));
			shrinkingHl = (bool)info.GetValue("shrinkingHl", typeof(bool));
			hlBorder = (bool)info.GetValue("hlBorder", typeof(bool));
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("qn_gapThreshold", qn_gapThreshold);
			info.AddValue("lineWidth", lineWidth);
			info.AddValue("fadeOutFromCenter", fadeOut);
			info.AddValue("shapePower", shapePower);
			info.AddValue("blurredEdge", blurredEdge);
			info.AddValue("style", style);
			info.AddValue("hlStyle", hlStyle);
			info.AddValue("hlSize", hlSize);
			info.AddValue("movingHl", movingHl);
			info.AddValue("shrinkingHl", shrinkingHl);
			info.AddValue("hlBorder", hlBorder);
		}

	}

	[Serializable()]
	class NoteStyle_Line : NoteStyle
	{

		float Qn_gapThreshold
		{
			get { return trackProps.LineStyleProps.qn_gapThreshold; }
			//set { qn_gapThreshold = value; }
		}
		int LineWidth
		{
			get { return TrackProps.LineStyleProps.lineWidth; }
		}

		float Fadeout
		{
			get { return TrackProps.LineStyleProps.fadeOut; }
		}
		float ShapePower
		{
			get { return TrackProps.LineStyleProps.shapePower; }
		}
		int BlurredEdge
		{
			get { return TrackProps.LineStyleProps.blurredEdge; }
		}
		LineStyleEnum Style
		{
			get { return TrackProps.LineStyleProps.style; }
		}
		LineHlStyleEnum HlStyle
		{
			get { return TrackProps.LineStyleProps.hlStyle; }
		}
		int HlSize
		{
			get { return TrackProps.LineStyleProps.hlSize; }
		}
		bool MovingHl
		{
			get { return TrackProps.LineStyleProps.movingHl; }
		}
		bool ShrinkingHl
		{
			get { return TrackProps.LineStyleProps.shrinkingHl; }
		}
		bool HlBorder
		{
			get { return TrackProps.LineStyleProps.hlBorder; }
		}
		public NoteStyle_Line()
		{
			index = (int)NoteStyleEnum.Line;
		}
		public NoteStyle_Line(TrackProps tprops)
			: base(tprops)
		{
			index = (int)NoteStyleEnum.Line;
		}
		public NoteStyle_Line(SerializationInfo info, StreamingContext ctxt)
			: base(info, ctxt)
		{
		}
		void drawTrackPass(int lineWidth, Vector4 colorMod, List<Midi.Note> noteList, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, bool bHilited, Effect effect, BlendState blendState)
		{
			Color color;
			Texture2D dummyTexture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, bHilited, out color, out dummyTexture);
			color.R = (byte)(color.R * colorMod.X); color.G = (byte)(color.G * colorMod.Y); color.B = (byte)(color.B * colorMod.Z); color.A = (byte)(color.A * colorMod.W);
			if (bHilited)
				color.A = 255;
			drawTrackPass(lineWidth, color, noteList, midiTrack, songDrawProps, trackProps, globalTrackProps, bHilited, effect, blendState);
		}
		void drawTrackLine(out int vertIndex, out bool drawHlNote, int lineWidth, List<Midi.Note> noteList, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps texTrackProps, Vector2 texSize)
		{
			vertIndex = 3;
			int hLineVertIndex = 0;
			drawHlNote = false;
			int completeNoteListIndex = midiTrack.Notes.IndexOf(noteList[0]);
			for (int n = 0; n < noteList.Count; n++)
			{
				#region Get (current) note and nextNote from noteList
				Midi.Note note = noteList[n], nextNote;
				if (note.start > songDrawProps.song.SongLengthInTicks) //only  if audio ends before the notes end
					continue;

				if (n < noteList.Count - 1)
					nextNote = noteList[n + 1];
				else if (completeNoteListIndex < midiTrack.Notes.Count - 1)
					nextNote = midiTrack.Notes[completeNoteListIndex + 1];
				else
					nextNote = note;
				#endregion
				#region Calc note viewport coords (noteStart, noteEnd(x-coord only) and nextNoteStart)
				Vector2 noteStart = songDrawProps.getScreenPosF(note.start, note.pitch);
				float noteEnd = songDrawProps.getScreenPosF(note.stop, note.pitch).X;
				Vector2 nextNoteStart = songDrawProps.getScreenPosF(nextNote.start, nextNote.pitch);
				if (noteEnd > nextNoteStart.X && completeNoteListIndex < midiTrack.Notes.Count - 1)
					noteEnd = nextNoteStart.X;
				#endregion

				//noteStart.X = (int)noteStart.X; noteStart.Y = (int)noteStart.Y;
				//nextNoteStart.X = (int)nextNoteStart.X; nextNoteStart.Y = (int)nextNoteStart.Y;

				bool endOfSegment = false;
				if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * songDrawProps.song.TimeDiv || note == nextNote)
				{
					if (nextNoteStart.X != noteStart.X)
						nextNoteStart.Y = (int)MathHelper.Lerp(noteStart.Y, nextNoteStart.Y, (float)(noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X));
					nextNoteStart.X = noteEnd;
					endOfSegment = true;
				}

				#region Fill vertBuf with highlight vertices
				//Fill verrtbuf with highlight vertices
				int vpCenterX = songDrawProps.viewportSize.X/2;
				if (noteStart.X < vpCenterX && noteEnd > vpCenterX)
				{
					Vector3 noteStartVec = new Vector3(noteStart.X, noteStart.Y, 0);
					drawHlNote = true;
					if (HlStyle == LineHlStyleEnum.Arrow)
					{
						float arrowLength;
						Vector3 arrowDir;
						Vector3 arrowNormal;
						Vector3 arrowStart;
						if (!MovingHl)
						{
							Vector3 nextNoteStartVec = new Vector3(nextNoteStart.X, nextNoteStart.Y, 0);
							Vector3 nextNoteOffset = nextNoteStartVec - noteStartVec;
							float nextNoteOffsetLength = nextNoteOffset.Length();
							arrowLength = nextNoteOffsetLength * ((float)noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X);
							if (noteEnd > nextNoteStart.X)
								arrowLength = nextNoteOffsetLength;

							arrowNormal = new Vector3(-nextNoteOffset.Y, nextNoteOffset.X, 0);
							arrowNormal /= nextNoteOffsetLength;
							arrowDir = nextNoteOffset / nextNoteOffsetLength;
							arrowStart = noteStartVec;
						}
						else
						{
							float x1 = songDrawProps.viewportSize.X / 2.0f;
							float normPitch = trackProps.Curve.Evaluate((float)songDrawProps.getSongPosT((int)x1));
							float y1 = songDrawProps.getPitchScreenPos(normPitch);
							float x2 = x1 + 1;
							normPitch = trackProps.Curve.Evaluate((float)songDrawProps.getSongPosT((int)x2));
							float y2 = songDrawProps.getPitchScreenPos(normPitch);
							
							arrowDir = new Vector3(x2 - x1, y2 - y1, 0);
							arrowDir.Normalize();
							arrowNormal = new Vector3(-arrowDir.Y, arrowDir.X, 0);
							arrowNormal.Normalize();
							arrowLength = HlSize;
							arrowStart = new Vector3(x1, y1, 0);
						}
						

						float arrowWidth = HlSize * 0.5f;

						lineHlVerts[0].pos = arrowStart + arrowNormal * arrowWidth;
						lineHlVerts[1].pos = arrowStart - arrowNormal * arrowWidth;
						lineHlVerts[2].pos = arrowStart + arrowDir * arrowLength;
						
						lineFx.Parameters["ArrowDir"].SetValue(arrowDir);
						//lineFx.Parameters["ArrowLength"].SetValue(nextNoteOffsetLength);
						lineFx.Parameters["ArrowStart"].SetValue(arrowStart);
						lineFx.Parameters["ArrowEnd"].SetValue(lineHlVerts[2].pos); //Is used to calc distance from the two "sides" of the triangle (not the bottom) since they share this point
						Vector3 side1Tangent = lineHlVerts[2].pos - lineHlVerts[0].pos;
						Vector3 side1Normal = new Vector3(-side1Tangent.Y, side1Tangent.X, 0);
						side1Normal.Normalize();
						lineFx.Parameters["Side1Normal"].SetValue(side1Normal);
						Vector3 side2Tangent = lineHlVerts[2].pos - lineHlVerts[1].pos;
						Vector3 side2Normal = new Vector3(-side2Tangent.Y, side2Tangent.X, 0);
						side2Normal.Normalize();
						lineFx.Parameters["Side2Normal"].SetValue(side2Normal);
					}
					else if (HlStyle == LineHlStyleEnum.Circle && !MovingHl)
					{
						setHlCirclePos(noteStartVec);
					}
					//For shrinking highlights
					float leftLength = vpCenterX - noteStart.X - 1;
					float shrinkPercent = leftLength / (noteEnd - noteStart.X);
					if (!ShrinkingHl)
					{
						shrinkPercent = 0;
						if (HlBorder)
							shrinkPercent = 1;
					}

					lineFx.Parameters["ClipPercent"].SetValue(shrinkPercent);
					float innerHlSize = HlSize * 0.5f * (1 - shrinkPercent);
					lineFx.Parameters["InnerHlSize"].SetValue(innerHlSize);
				}
				#endregion
				//innerHlSize = HlSize / 40.0f;
				
				float startDraw = noteStart.X;
				float endDraw = nextNoteStart.X;
				if (endDraw < 0 || startDraw > songDrawProps.viewportSize.X)
				{
					completeNoteListIndex++;
					continue;
				}
				//Draw between note start and next note start
				//startDraw = 960;
				//endDraw = 1000;
				for (float x = startDraw; x < endDraw; x++)
				{
					//Vector2 scale = new Vector2(lineWidth / texture.Width, (float)curLineWidth / texture.Height);
					Vector3[] points = new Vector3[3];
					Vector3[] tangents = new Vector3[2];
					for (int i = 0; i < points.Length; i++)
					{
						points[i] = new Vector3();
						points[i].X = (float)(x + i - 1);
						points[i].Y = songDrawProps.getCurveScreenY(points[i].X, trackProps.Curve);
						points[i].Z = 0;
					}
					float hLineStart = points[1].X;
					float hLineEnd = hLineStart;
										
					for (int i = 0; i < tangents.Length; i++)
						tangents[i] = points[i + 1] - points[i];

					Vector3 normal = tangents[0] + tangents[1];
					normal = new Vector3(-normal.Y, normal.X, 0);
					normal.Normalize();

					Vector3 normal1, normal2;
					if (Style == LineStyleEnum.Ribbon)
					{
						//normal1 = normal;
						normal1 = new Vector3(1, 0, 0);
						normal2 = normal;
						lineVerts[vertIndex].normal2 = lineVerts[vertIndex + 1].normal2 = normal2;
					}
					else
						normal1 = normal;

					lineVerts[vertIndex].normal = lineVerts[vertIndex + 1].normal = normal1;

					float halfWidth = lineWidth / 2.0f;
											
					//Fill vertex buffer
					lineVerts[vertIndex].pos = new Vector3(hLineStart, points[1].Y, points[1].Z) - normal1 * halfWidth;
					lineVerts[vertIndex + 1].pos = new Vector3(hLineEnd, points[1].Y, points[1].Z) + normal1 * halfWidth;
					lineVerts[vertIndex].center = lineVerts[vertIndex + 1].center = points[1];
					//Vector2 ns = songDrawProps.getScreenPosF(note.start, note.pitch);
					//Vector2 nns = songDrawProps.getScreenPosF(nextNote.start, nextNote.pitch);
					if (texTrackProps.TexProps.Texture != null)
						calcTexCoords(out lineVerts[vertIndex].texCoords, out lineVerts[vertIndex + 1].texCoords, lineVerts[vertIndex].center, texTrackProps.TexProps, texSize, x - startDraw, (float)(x - startDraw) / (float)(nextNoteStart.X - noteStart.X), songDrawProps, lineWidth, lineVerts[vertIndex].pos, lineVerts[vertIndex+1].pos);

										
					if (Style == LineStyleEnum.Ribbon)
					{
						do
						{
							hLineEnd++;
						} while ((int)points[1].Y == (int)songDrawProps.getCurveScreenY((float)hLineEnd + 1, trackProps.Curve) && hLineEnd < endDraw);
						if (hLineEnd > hLineStart + halfWidth)
						{
							hLineVerts[hLineVertIndex++] = lineVerts[vertIndex];
							hLineVerts[hLineVertIndex++] = lineVerts[vertIndex + 1];
						}
					}

					vertIndex += 2;
					
					if (x > songDrawProps.viewportSize.X)
						break;
				}
				if (endOfSegment)
					drawLineSegment(ref vertIndex, ref hLineVertIndex);
				completeNoteListIndex++;
				//break;
			}
			drawLineSegment(ref vertIndex, ref hLineVertIndex);
		}
		void calcTexCoords(out Vector2 vert1TC, out Vector2 vert2TC, Vector3 lineCenter, TrackPropsTex texProps, Vector2 texSize, float stepFromNoteStart, float normStepFromNoteStart, SongDrawProps songDrawProps, float lineWidth, Vector3 pos1, Vector3 pos2)
		{
			TexAnchorEnum texUAnchor = texProps.UAnchor;
			TexAnchorEnum texVAnchor = texProps.VAnchor;
			bool tileU = texProps.UTile;
			bool tileV = texProps.VTile;
			if (texUAnchor == TexAnchorEnum.Note)
			{
				if (!tileU)
					vert1TC.X = vert2TC.X = normStepFromNoteStart;
				else
					vert1TC.X = vert2TC.X = stepFromNoteStart / texSize.X;
			}
			else if (texUAnchor == TexAnchorEnum.Screen)
			{
				//if (!tileU)
				//    vert1TC.X = vert2TC.X = lineCenter.X / songDrawProps.viewportSize.X;
				//else
				//    vert1TC.X = vert2TC.X = lineCenter.X / texSize.X;
				if (!tileU)
				{
					vert1TC.X = pos1.X / songDrawProps.viewportSize.X;
					vert2TC.X = pos2.X / songDrawProps.viewportSize.X;
				}
				else
				{
					vert1TC.X = pos1.X / texSize.X;
					vert2TC.X = pos2.X / texSize.X;
				}
			}
			else
			{
				if (!tileU)
					vert1TC.X = vert2TC.X = songDrawProps.getSongPosP(lineCenter.X) / songDrawProps.getSongLengthP();
				else
					vert1TC.X = vert2TC.X = songDrawProps.getSongPosP(lineCenter.X) / texSize.X;
			}

			if (texVAnchor == TexAnchorEnum.Note)
			{
				vert1TC.Y = 0;
				if (!tileV)
					vert2TC.Y = 1;
				else
					vert2TC.Y = lineWidth / texSize.Y;
			}
			else
			{
				if (!tileV)
				{
					vert1TC.Y = pos1.Y / songDrawProps.viewportSize.Y;
					vert2TC.Y = pos2.Y / songDrawProps.viewportSize.Y;
				}
				else
				{
					vert1TC.Y = pos1.Y / texSize.Y;
					vert2TC.Y = pos2.Y / texSize.Y;
				}
			}
			if (texProps.KeepAspect)
			{
				adjustAspect(ref vert1TC, new Vector2(pos1.X, pos1.Y), texProps);
				adjustAspect(ref vert2TC, new Vector2(pos2.X, pos2.Y), texProps);
			}
			vert1TC -= songDrawProps.songPosS * texProps.Scroll;
			vert2TC -= songDrawProps.songPosS * texProps.Scroll;
		}
		void adjustAspect(ref Vector2 tc, Vector2 scrPos, TrackPropsTex texProps)
		{
			double uTexelsPerPixel = (double)tc.X * texProps.Texture.Width / scrPos.X;
			double vTexelsPerPixel = (double)tc.Y * texProps.Texture.Height / scrPos.Y;
			if (texProps.UTile && !texProps.VTile)
				tc.X = (float)(scrPos.X * vTexelsPerPixel / texProps.Texture.Width);
			else if (!texProps.UTile && texProps.VTile)
				tc.Y = (float)(scrPos.Y * uTexelsPerPixel / texProps.Texture.Height);
		}
		void setHlCirclePos(Vector3 pos)
		{
			lineFx.Parameters["WorldPos"].SetValue(pos);
			float halfHlSize = HlSize / 2.0f;
			lineHlVerts[0].pos = new Vector3(-halfHlSize, -halfHlSize, 0) + pos;
			lineHlVerts[1].pos = new Vector3(halfHlSize, - halfHlSize, 0) + pos;
			lineHlVerts[2].pos = new Vector3(-halfHlSize, halfHlSize, 0) + pos;
			lineHlVerts[3].pos = new Vector3(halfHlSize, halfHlSize, 0) + pos;
		}
		void drawTrackPass(int lineWidth, Color color, List<Midi.Note> noteList, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, bool bHilited, Effect effect, BlendState blendState)
		{
			Color dummyColor;
			Texture2D texture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, bHilited, out dummyColor, out texture);
			
			int completeNoteListIndex = midiTrack.Notes.IndexOf(noteList[0]);
			for (int n = 0; n < noteList.Count; n++)
			{
				Midi.Note note = noteList[n], nextNote;
				if (note.start > songDrawProps.song.SongLengthInTicks) //only  if audio ends before the notes end
					continue;

				if (n < noteList.Count - 1)
					nextNote = noteList[n + 1];
				else if (completeNoteListIndex < midiTrack.Notes.Count - 1)
					nextNote = midiTrack.Notes[completeNoteListIndex + 1];
				else
					nextNote = note;

				Point noteStart = songDrawProps.getScreenPos(note.start, note.pitch);
				int noteEnd = songDrawProps.getScreenPos(note.stop, note.pitch).X;
				Point nextNoteStart = songDrawProps.getScreenPos(nextNote.start, nextNote.pitch);

				if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * songDrawProps.song.TimeDiv || note == nextNote)
				{
					nextNoteStart.X = noteEnd;
					nextNoteStart.Y = noteStart.Y;
				}

				int startDraw = noteStart.X;
				int endDraw = nextNoteStart.X;
				int hlTail = 100;
				if (bHilited)
				{
					startDraw = songDrawProps.viewportSize.X / 2 - hlTail;
					if (startDraw < noteStart.X)
						startDraw = noteStart.X;
					endDraw = songDrawProps.viewportSize.X / 2 + 1;
				}
				for (int x = startDraw; x < endDraw; x++)
				{
					float curLineWidth = lineWidth;
					if (bHilited)
					{
						if (endDraw - x > 1)
							curLineWidth *= (x - startDraw) / (float)(endDraw - noteStart.X); //tail fadeout
						curLineWidth *= (noteEnd - endDraw) / (float)(noteEnd - noteStart.X); //note pos fadeout
					}
					Vector2 scale = new Vector2(curLineWidth / texture.Width, (float)curLineWidth / texture.Height);

					float nextX = (float)x + 1;
					float y, nextY;
					string interpMethod = "notcosine";
					if (interpMethod == "cosine")
					{
						float lerpFactor = calcLerpFactor(x, noteStart.X, nextNoteStart.X);
						y = MathHelper.Lerp((float)noteStart.Y, (float)nextNoteStart.Y, lerpFactor);
						float nextLerpFactor = calcLerpFactor(x + 1, noteStart.X, nextNoteStart.X);
						nextY = MathHelper.Lerp((float)noteStart.Y, (float)nextNoteStart.Y, nextLerpFactor);
					}
					else
					{
						float normPitch = trackProps.Curve.Evaluate((float)songDrawProps.getSongPosT((int)x));
						y = songDrawProps.getPitchScreenPos(normPitch);
						normPitch = trackProps.Curve.Evaluate((float)songDrawProps.getSongPosT((int)nextX));
						nextY = songDrawProps.getPitchScreenPos(normPitch);
					}

					Vector2 tangent = new Vector2((float)1, nextY - y);
					tangent.Normalize();
					//lineFx.Parameters["Normal"].SetValue(new Vector2(tangent.Y, -tangent.X));
					float angle = (float)Math.Asin((double)tangent.Y);
					angle = 0;
					
					for (float i = 0; i <= Math.Abs(nextY - y); i += 1)
					{
						//lineFx.Parameters["Center"].SetValue(new Vector2((float)x - 0.5f, y - 0.5f));
						circleFx.Parameters["Center"].SetValue(new Vector2((float)x - 0.5f, y - 0.5f));
						try
						{
							songPanel.SpriteBatch.Begin(SpriteSortMode.Deferred, blendState, null, null, null, effect);
							songPanel.SpriteBatch.Draw(texture, new Vector2((float)x - scale.X * texture.Width * 0.5f, y - scale.Y * texture.Height * 0.5f) + tangent * i, null, color, angle, new Vector2(0, 0)/*new Vector2(0, (float)texture.Height*scale/2.0f)*/, scale, SpriteEffects.None, 0);
							songPanel.SpriteBatch.End();
						}
						catch (Exception e)
						{
							MessageBox.Show(e.Message);
							throw e;
						}
					}
				}
				//if (bHilited)
				//songPanel.SpriteBatch.Draw(texture, new Vector2((float)x - scale.X * texture.Width * 0.5f, y - scale.Y * texture.Height * 0.5f) + angleVec * i, null, color, angle, new Vector2(0, 0)/*new Vector2(0, (float)texture.Height*scale/2.0f)*/, scale, SpriteEffects.None, 0);
				completeNoteListIndex++;
			}
		}
		static string GetName<T>(T item) where T : class
		{
			return typeof(T).GetProperties()[0].Name;
		}

		public override void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps)
		{
			//testVerts[0].pos = new Vector4(1, 0, 0, 0);
			//testVerts[1].pos = new Vector4(0, 2, 0, 0);
			//testVerts[2].pos = new Vector4(0, 0, 3, 0);
			//barFx.Techniques[0].Passes[0].Apply();
			//lineFx.CurrentTechnique = lineFx.Techniques["Arrow"];
			//lineFx.CurrentTechnique.Passes["Area"].Apply();
			//songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, lineVerts, 0, 1);
			//return;
			List<Midi.Note> noteList = getNotes((int)(Qn_gapThreshold * songDrawProps.song.TimeDiv), midiTrack, songDrawProps);
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			if (noteList.Count == 0)
				return;

			//this.trackProps = trackProps;
			lineFx.Parameters["ViewportSize"].SetValue(new Vector2(songDrawProps.viewportSize.X, songDrawProps.viewportSize.Y));
			float fadeout = 0;
			if (Fadeout > 0)
				fadeout = LineWidth / (2.0f * Fadeout);
			lineFx.Parameters["FadeoutFromCenter"].SetValue(fadeout);
			lineFx.Parameters["ShapePower"].SetValue(ShapePower);
			lineFx.Parameters["BlurredEdge"].SetValue((float)BlurredEdge);
			lineFx.Parameters["Style"].SetValue((int)Style);
			lineFx.Parameters["HlSize"].SetValue((float)HlSize / 2.0f);
			//lineFx.Parameters["TexAnchor"].SetValue(new int[]{(int)trackProps.TexUAnchor, (int)trackProps.TexVAnchor});
			
			//Light props
			TrackProps lightProps = trackProps.UseGlobalLight ? globalTrackProps : trackProps;
			Vector3 normLightDir = lightProps.LightDir;
			normLightDir.Normalize();
			lineFx.Parameters["LightDir"].SetValue(normLightDir);
			lineFx.Parameters["SpecAmount"].SetValue(lightProps.SpecAmount);
			lineFx.Parameters["SpecPower"].SetValue(lightProps.SpecPower);
			float angle = lightProps.SpecFov * (float)Math.PI / (360);
			float camPosZ = (songDrawProps.viewportSize.X / 2) / (float)Math.Tan(angle);
			Vector3 specCamPos = new Vector3(songDrawProps.viewportSize.X / 2, songDrawProps.viewportSize.Y / 2, camPosZ);
			lineFx.Parameters["SpecCamPos"].SetValue(specCamPos);

			songPanel.GraphicsDevice.BlendState = songPanel.BlendState;
			float radius = LineWidth / 2.0f;
			//if (radius < 0.5f)
				//radius = 0.5f;
			lineFx.Parameters["Radius"].SetValue(radius);
			Color color;
			Texture2D texture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, false, out color, out texture);
			lineFx.Parameters["Color"].SetValue(color.ToVector4());
			
			lineFx.Parameters["InnerHlSize"].SetValue(0.0f);
			Vector2 texSize = new Vector2(texture.Width, texture.Height);
			lineFx.Parameters["TexSize"].SetValue(texSize);
			lineFx.Parameters["Texture"].SetValue(texture);
			
			TrackProps texTrackProps = trackProps.getTexture(false, null) != null ? trackProps : globalTrackProps;
			songPanel.GraphicsDevice.SamplerStates[0] = texTrackProps.TexProps.SamplerState;
			songPanel.GraphicsDevice.SamplerStates[1] = texTrackProps.HmapProps.SamplerState;
									
			int numVerts;
			bool drawHlNote=false;
			drawTrackLine(out numVerts, out drawHlNote, LineWidth, noteList, midiTrack, songDrawProps, trackProps, texTrackProps, texSize);

			Color hlColor;
			Texture2D hlTexture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, true, out hlColor, out hlTexture);
			lineFx.Parameters["HlColor"].SetValue(hlColor.ToVector4());
			if (drawHlNote)
			{
				lineFx.Parameters["Border"].SetValue(HlBorder);
				if (HlStyle == LineHlStyleEnum.Arrow)
				{
					//Calc shortest dist to incenter from border, ie. the inscribed circle's radius
					float a = (lineHlVerts[0].pos - lineHlVerts[1].pos).Length();
					float b = (lineHlVerts[0].pos - lineHlVerts[2].pos).Length();
					float c = (lineHlVerts[1].pos - lineHlVerts[2].pos).Length();
					float k = (a + b + c) / 2.0f;
					float icRadius = (float)Math.Sqrt(k * (k - a) * (k - b) * (k - c)) / k;
					lineFx.Parameters["DistToCenter"].SetValue(icRadius);

					songPanel.GraphicsDevice.BlendState = BlendState.AlphaBlend;
					lineFx.CurrentTechnique = lineFx.Techniques["Arrow"];
					lineFx.CurrentTechnique.Passes["Area"].Apply();
					songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, lineHlVerts, 0, 1);
				}
				else if (HlStyle == LineHlStyleEnum.Circle)
				{
					if (MovingHl)
					{
						float x = songDrawProps.viewportSize.X / 2.0f;
						Vector3 circlePos = new Vector3(x, songDrawProps.getCurveScreenY(x, trackProps.Curve), 0);
						setHlCirclePos(circlePos);
					}
					songPanel.GraphicsDevice.BlendState = BlendState.AlphaBlend;
					lineFx.CurrentTechnique = lineFx.Techniques["Circle"];
					lineFx.CurrentTechnique.Passes[0].Apply();
					songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, lineHlVerts, 0, 2);
				}
			}
			
			

			
			////drawTrackPass(LineWidth + 2, new Vector4(1, 1, 1, 1), noteList, midiTrack, songDrawProps, trackProps, globalTrackProps, false, lineFx, songPanel.BlendState);
			
			//noteList = midiTrack.getNotes(songDrawProps.songPosT, songDrawProps.songPosT + 1, 0, 127);
			//if (noteList.Count == 0)
			//    return;

			//circleFx.Parameters["ViewportSize"].SetValue(new Vector2(songDrawProps.viewportSize.X, songDrawProps.viewportSize.Y));
			//circleFx.Parameters["Radius"].SetValue(8);
			////songPanel.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, circleFx);
			//drawTrackPass(23, new Vector4(1, 1, 1, 1), noteList, midiTrack, songDrawProps, trackProps, globalTrackProps, true, circleFx, BlendState.AlphaBlend);
			////songPanel.SpriteBatch.End();
		}
		void drawLineSegment(ref int numVerts, ref int numHLineVerts)
		{
			if (LineWidth > 0)
			{
				if (numVerts > 5 || numHLineVerts > 1)
				{
					if (Style == LineStyleEnum.Simple)
						lineFx.CurrentTechnique = lineFx.Techniques["Simple"];
					else
						lineFx.CurrentTechnique = lineFx.Techniques["Lighting"];
					lineFx.CurrentTechnique.Passes[0].Apply();
					if (numHLineVerts > 1)
						songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, hLineVerts, 0, numHLineVerts / 2);
					if (numVerts > 5)
					    songPanel.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, lineVerts, 3, numVerts - 3, lineInds, 0, numVerts - 5);
				}
			}
			numVerts = 3;
			numHLineVerts = 0;
		}
			
		float calcLerpFactor(int x, int x1, int x2)
		{
			float f = (float)(x - x1) / (float)(x2 - x1);
			return (1 - (float)Math.Cos((double)f * Math.PI)) * 0.5f;
			//return f;
		}
	}
}