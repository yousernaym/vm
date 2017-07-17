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

namespace Visual_Music
{
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
	public class NoteStyle_Line : NoteStyle
	{
		static TestVertex[] testVerts = new TestVertex[30];
		static LineVertex[] lineVerts = new LineVertex[NumDynamicVerts];
		static LineVertex[] hLineVerts = new LineVertex[NumDynamicVerts];
		static short[] lineInds = new short[lineVerts.Length];
		//protected static LineVertex[] arrowAreaVerts = new LineVertex[3];
		//protected static LineVertex[] arrowBorderVerts = new LineVertex[3];
		protected static LineVertex[] lineHlVerts = new LineVertex[4];
		public float Qn_gapThreshold { get; set; } = 5;
		public int LineWidth = 5;
		public float FadeOut = 1;
		public int BlurredEdge = 2;
		public float ShapePower = 1;
		public LineStyleEnum Style = LineStyleEnum.Simple;
		public LineHlStyleEnum HlStyle = LineHlStyleEnum.Arrow;
		public int HlSize = 25;
		public bool MovingHl = false;
		public bool ShrinkingHl = false;
		public bool HlBorder = false;

		public NoteStyle_Line()
		{
			styleType = NoteStyleEnum.Line;
		}
		public NoteStyle_Line(TrackProps tprops)
			: base(tprops)
		{
			styleType = NoteStyleEnum.Line;
		}
		public NoteStyle_Line(SerializationInfo info, StreamingContext ctxt)
			: base(info, ctxt)
		{
			Qn_gapThreshold = (float)info.GetValue("qn_gapThreshold", typeof(float));
			LineWidth = (int)info.GetValue("lineWidth", typeof(int));
			FadeOut = (float)info.GetValue("fadeOutFromCenter", typeof(float));
			ShapePower = (float)info.GetValue("shapePower", typeof(float));
			BlurredEdge = (int)info.GetValue("blurredEdge", typeof(int));
			Style = (LineStyleEnum)info.GetValue("style", typeof(LineStyleEnum));
			HlStyle = (LineHlStyleEnum)info.GetValue("hlStyle", typeof(LineHlStyleEnum));
			HlSize = (int)info.GetValue("hlSize", typeof(int));
			MovingHl = (bool)info.GetValue("movingHl", typeof(bool));
			ShrinkingHl = (bool)info.GetValue("shrinkingHl", typeof(bool));
			HlBorder = (bool)info.GetValue("hlBorder", typeof(bool));
		}
		override public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			base.GetObjectData(info, ctxt);
			info.AddValue("qn_gapThreshold", Qn_gapThreshold);
			info.AddValue("lineWidth", LineWidth);
			info.AddValue("fadeOutFromCenter", FadeOut);
			info.AddValue("shapePower", ShapePower);
			info.AddValue("blurredEdge", BlurredEdge);
			info.AddValue("style", Style);
			info.AddValue("hlStyle", HlStyle);
			info.AddValue("hlSize", HlSize);
			info.AddValue("movingHl", MovingHl);
			info.AddValue("shrinkingHl", ShrinkingHl);
			info.AddValue("hlBorder", HlBorder);
		}
		override public void loadFx()
		{
			fx = songPanel.Content.Load<Effect>("Line");
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
			int skippedNotes = 0;
			int culledNotes = 0;
			int skippedPoints = 0;
			int totalVerts = 0;

			vertIndex = 3;
			int hLineVertIndex = 0;
			drawHlNote = false;
			int completeNoteListIndex = midiTrack.Notes.IndexOf(noteList[0]);
			for (int n = 0; n < noteList.Count; n++)
			{
				//Get current note
				Midi.Note note = noteList[n], nextNote;
				if (note.start > songDrawProps.song.SongLengthT) //only  if audio ends before the notes end
					continue;
				Vector2 noteStart = songDrawProps.getScreenPosF(note.start, note.pitch);
				float noteEnd = songDrawProps.getScreenPosF(note.stop, note.pitch).X;
				float z = fx.Parameters["PosOffset"].GetValueVector3().Z;// -songPanel.Camera.ProjMat.M22 * songDrawProps.viewportSize.Y;
				Vector4 noteStartProj = Vector4.Transform(new Vector4(noteStart.X, noteStart.Y, z, 1), songPanel.Camera.VpMat);
				Vector3 noteStartScreen = new Vector3(noteStartProj.X, noteStartProj.Y, noteStartProj.Z) / noteStartProj.W;
				//---------------------------------

				Vector2 nextNoteStart;
				float screenDist;
				Vector3 nextNoteStartScreen;
				do
				{
					if (n < noteList.Count - 1)
						nextNote = noteList[n + 1];
					else if (completeNoteListIndex < midiTrack.Notes.Count - 1)
						nextNote = midiTrack.Notes[completeNoteListIndex + 1];
					else
						nextNote = note;

					nextNoteStart = songDrawProps.getScreenPosF(nextNote.start, nextNote.pitch);
					if (noteEnd > nextNoteStart.X && completeNoteListIndex < midiTrack.Notes.Count - 1)
						noteEnd = nextNoteStart.X;

					//If notes are too close after transformation, skip to next note
					Vector4 nextNoteStartProj = Vector4.Transform(new Vector4(nextNoteStart.X, nextNoteStart.Y, z, 1), songPanel.Camera.VpMat);
					nextNoteStartScreen = new Vector3(nextNoteStartProj.X, nextNoteStartProj.Y, nextNoteStartProj.Z) / nextNoteStartProj.W;
					screenDist = (noteStartScreen - nextNoteStartScreen).LengthSquared();
					n++;
					completeNoteListIndex++;
					skippedNotes++;
				} while (screenDist < 2.0f / Math.Max(songDrawProps.viewportSize.X, songDrawProps.viewportSize.Y) && note != nextNote && bSkipClose);
				n--;
				completeNoteListIndex--;
				skippedNotes--;
				//float bboxSize = Math.Max(nextNoteStartScreen.X - noteStartScreen.X, Math.Abs(nextNoteStartScreen.Y - noteStartScreen.Y);
				//float cullBorder = 1 + bboxSize;
				BoundingBox bbox = BoundingBox.CreateFromPoints(new Vector3[] { noteStartScreen, nextNoteStartScreen });
				Vector3 clipMargin = new Vector3(0.1f, 0.1f, 0.1f);
				if (!bbox.Intersects(new BoundingBox(new Vector3(-1, -1, 0) - clipMargin, new Vector3(1, 1, 1) + clipMargin)) && bCull)
				{
					completeNoteListIndex++;
					culledNotes++;
					continue;
				}

				//noteStart.X = (int)noteStart.X; noteStart.Y = (int)noteStart.Y;
				//nextNoteStart.X = (int)nextNoteStart.X; nextNoteStart.Y = (int)nextNoteStart.Y;

				bool endOfSegment = false;
				if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * songDrawProps.song.TicksPerBeat || note == nextNote)
				{
					if (nextNoteStart.X != noteStart.X)
						nextNoteStart.Y = (int)MathHelper.Lerp(noteStart.Y, nextNoteStart.Y, (float)(noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X));
					nextNoteStart.X = noteEnd;
					endOfSegment = true;
				}

				#region Fill vertBuf with highlight vertices
				//Fill verrtbuf with highlight vertices
				int vpCenterX = 0; // songDrawProps.viewportSize.X/2;
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
							float x1 = 0;// songDrawProps.viewportSize.X / 2.0f;
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

						fx.Parameters["ArrowDir"].SetValue(arrowDir);
						//lineFx.Parameters["ArrowLength"].SetValue(nextNoteOffsetLength);
						fx.Parameters["ArrowStart"].SetValue(arrowStart);
						fx.Parameters["ArrowEnd"].SetValue(lineHlVerts[2].pos); //Is used to calc distance from the two "sides" of the triangle (not the bottom) since they share this point
						Vector3 side1Tangent = lineHlVerts[2].pos - lineHlVerts[0].pos;
						Vector3 side1Normal = new Vector3(-side1Tangent.Y, side1Tangent.X, 0);
						side1Normal.Normalize();
						fx.Parameters["Side1Normal"].SetValue(side1Normal);
						Vector3 side2Tangent = lineHlVerts[2].pos - lineHlVerts[1].pos;
						Vector3 side2Normal = new Vector3(-side2Tangent.Y, side2Tangent.X, 0);
						side2Normal.Normalize();
						fx.Parameters["Side2Normal"].SetValue(side2Normal);
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

					fx.Parameters["ClipPercent"].SetValue(shrinkPercent);
					float innerHlSize = HlSize * 0.5f * (1 - shrinkPercent);
					fx.Parameters["InnerHlSize"].SetValue(innerHlSize);
				}
				#endregion
				//innerHlSize = HlSize / 40.0f;

				float startDraw = noteStart.X;
				float endDraw = nextNoteStart.X;
				float startEndXDist = Math.Abs(noteStartScreen.X - nextNoteStartScreen.X) * songDrawProps.viewportSize.X / 2.0f;
				if (endDraw < -songDrawProps.viewportSize.X / 2 || startDraw > songDrawProps.viewportSize.X / 2)
				{
					//completeNoteListIndex++;
					//continue;
				}
				//Draw between note start and next note start
				//startDraw = 960;
				//endDraw = 1000;

				float step = ((endDraw - startDraw) / startEndXDist);
				if (step < 1 || !bSkipPoints)
					step = 1;

				for (float x = startDraw; x < endDraw; x += step)
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
						calcTexCoords(out lineVerts[vertIndex].texCoords, out lineVerts[vertIndex + 1].texCoords, lineVerts[vertIndex].center, texTrackProps.TexProps, texSize, x - startDraw, (float)(x - startDraw) / (float)(nextNoteStart.X - noteStart.X), songDrawProps, lineWidth, lineVerts[vertIndex].pos, lineVerts[vertIndex + 1].pos);

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
					totalVerts += 2;

					//if (x > songDrawProps.viewportSize.X)
					//break;
					skippedPoints--;
				}
				skippedPoints += (int)(endDraw - startDraw + 1);
				if (endOfSegment || vertIndex > NumDynamicVerts - 10000)
					drawLineSegment(ref vertIndex, ref hLineVertIndex);

				completeNoteListIndex++;
				//break;
			}
			drawLineSegment(ref vertIndex, ref hLineVertIndex);
			Debug.WriteLine("Skipped notes: " + skippedNotes);
			Debug.WriteLine("Culled notes: " + culledNotes);
			Debug.WriteLine("Skipped points: " + skippedPoints);
			Debug.WriteLine("Vertices: " + totalVerts);
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
			fx.Parameters["WorldPos"].SetValue(pos);
			float halfHlSize = HlSize / 2.0f;
			lineHlVerts[0].pos = new Vector3(-halfHlSize, -halfHlSize, 0) + pos;
			lineHlVerts[1].pos = new Vector3(halfHlSize, -halfHlSize, 0) + pos;
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
				if (note.start > songDrawProps.song.SongLengthT) //only  if audio ends before the notes end
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

				if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * songDrawProps.song.TicksPerBeat || note == nextNote)
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
						//circleFx.Parameters["Center"].SetValue(new Vector2((float)x - 0.5f, y - 0.5f));
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
		//static string GetName<T>(T item) where T : class
		//{
		//	return typeof(T).GetProperties()[0].Name;
		//}

		public override void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, bool selectingRegion, TrackProps texTrackProps)
		{
			base.drawTrack(midiTrack, songDrawProps, trackProps, globalTrackProps, selectingRegion, texTrackProps);
			//testVerts[0].pos = new Vector4(1, 0, 0, 0);
			//testVerts[1].pos = new Vector4(0, 2, 0, 0);
			//testVerts[2].pos = new Vector4(0, 0, 3, 0);
			//barFx.Techniques[0].Passes[0].Apply();
			//lineFx.CurrentTechnique = lineFx.Techniques["Arrow"];
			//lineFx.CurrentTechnique.Passes["Area"].Apply();
			//songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, lineVerts, 0, 1);
			//return;
			//List<Midi.Note> noteList = getNotes((int)(Qn_gapThreshold * songDrawProps.song.TicksPerBeat), midiTrack, songDrawProps);
			List<Midi.Note> noteList = midiTrack.Notes;
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			if (noteList.Count == 0)
				return;

			//this.trackProps = trackProps;

			float fadeout = 0;
			if (FadeOut > 0)
				fadeout = LineWidth / (2.0f * FadeOut);
			fx.Parameters["FadeoutFromCenter"].SetValue(fadeout);
			fx.Parameters["ShapePower"].SetValue(ShapePower);
			fx.Parameters["BlurredEdge"].SetValue((float)BlurredEdge);
			fx.Parameters["Style"].SetValue((int)Style);
			fx.Parameters["HlSize"].SetValue((float)HlSize / 2.0f);
			//lineFx.Parameters["TexAnchor"].SetValue(new int[]{(int)trackProps.TexUAnchor, (int)trackProps.TexVAnchor});


			songPanel.GraphicsDevice.BlendState = songPanel.BlendState;
			float radius = LineWidth / 2.0f;
			//if (radius < 0.5f)
			//radius = 0.5f;
			fx.Parameters["Radius"].SetValue(radius);
			Color color;
			Texture2D texture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, false, out color, out texture);
			fx.Parameters["Color"].SetValue(color.ToVector4());

			fx.Parameters["InnerHlSize"].SetValue(0.0f);
			Vector2 texSize = new Vector2(texture.Width, texture.Height);
			fx.Parameters["TexSize"].SetValue(texSize);
			fx.Parameters["Texture"].SetValue(texture);

			int numVerts;
			bool drawHlNote = false;
			drawTrackLine(out numVerts, out drawHlNote, LineWidth, noteList, midiTrack, songDrawProps, trackProps, texTrackProps, texSize);

			Color hlColor;
			Texture2D hlTexture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, true, out hlColor, out hlTexture);
			fx.Parameters["HlColor"].SetValue(hlColor.ToVector4());
			if (drawHlNote)
			{
				fx.Parameters["Border"].SetValue(HlBorder);
				if (HlStyle == LineHlStyleEnum.Arrow)
				{
					//Calc shortest dist to incenter from border, ie. the inscribed circle's radius
					float a = (lineHlVerts[0].pos - lineHlVerts[1].pos).Length();
					float b = (lineHlVerts[0].pos - lineHlVerts[2].pos).Length();
					float c = (lineHlVerts[1].pos - lineHlVerts[2].pos).Length();
					float k = (a + b + c) / 2.0f;
					float icRadius = (float)Math.Sqrt(k * (k - a) * (k - b) * (k - c)) / k;
					fx.Parameters["DistToCenter"].SetValue(icRadius);

					songPanel.GraphicsDevice.BlendState = BlendState.AlphaBlend;
					fx.CurrentTechnique = fx.Techniques["Arrow"];
					fx.CurrentTechnique.Passes["Area"].Apply();
					songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, lineHlVerts, 0, 1);
				}
				else if (HlStyle == LineHlStyleEnum.Circle)
				{
					if (MovingHl)
					{
						float x = 0;// songDrawProps.viewportSize.X / 2.0f;
						Vector3 circlePos = new Vector3(x, songDrawProps.getCurveScreenY(x, trackProps.Curve), 0);
						setHlCirclePos(circlePos);
					}
					songPanel.GraphicsDevice.BlendState = BlendState.AlphaBlend;
					fx.CurrentTechnique = fx.Techniques["Circle"];
					fx.CurrentTechnique.Passes[0].Apply();
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
						fx.CurrentTechnique = fx.Techniques["Simple"];
					else
						fx.CurrentTechnique = fx.Techniques["Lighting"];
					fx.CurrentTechnique.Passes[0].Apply();
					//if (numHLineVerts > 1)
					//songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, hLineVerts, 0, numHLineVerts / 2);
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

		public static void sInit()
		{
			for (short i = 0; i < lineInds.Length; i++)
				lineInds[i] = i;
		}
	}
}

