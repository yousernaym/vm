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
	public struct LineHlVertex : IVertexType
	{
		public Vector3 pos;
		public Vector3 normal;
		public Vector3 center;
		public float normStepFromNoteStart;
		public Vector2 texCoords;
		public LineHlVertex(Vector3 _pos, Vector3 _normal, Vector3 _center, float _normStepFromNoteStart, Vector2 _texCoords)
		{
			pos = _pos;
			normal = _normal;
			center = _center;
			normStepFromNoteStart = _normStepFromNoteStart;
			texCoords = _texCoords;
		}
		public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Position, 1), new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.Position, 2), new VertexElement(40, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}
	}

	public struct LineVertex 
	{
		public Vector3 pos;
		public Vector3 normal;
		public Vector3 center;
		public float normStepFromNoteStart;
		public Vector2 texCoords;
		
		public LineVertex(Vector3 _pos, Vector3 _normal, Vector3 _center, float _normStepFromNoteStart, Vector2 _texCoords)
		{
			pos = _pos;
			normal = _normal;
			center = _center;
			normStepFromNoteStart = _normStepFromNoteStart;
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
		const int MaxLineVerts = 100000;
		const int MaxHLineVerts = 30000; 
		//static Stack vbPool = new Stack(1000)
		static VertexDeclaration lineVertDecl;
		static TestVertex[] testVerts = new TestVertex[30];
		static LineVertex[] lineVerts = new LineVertex[MaxLineVerts];
		static LineVertex[] hLineVerts = new LineVertex[MaxHLineVerts];
		//static short[] lineInds = new short[lineVerts.Length];
		//protected static LineVertex[] arrowAreaVerts = new LineVertex[3];
		//protected static LineVertex[] arrowBorderVerts = new LineVertex[3];
		protected static LineHlVertex[] lineHlVerts = new LineHlVertex[4];
		//OcTree<LineGeo> ocTree;

		public int LineWidth = 5;
		public float Qn_gapThreshold { get; set; } = 5;
		public bool Continuous { get; set; } = true;
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
			foreach (var entry in info)
			{
				if (entry.Name == "lineWidth")
					LineWidth = (int)entry.Value;
				else if (entry.Name == "qn_gapThreshold")
					Qn_gapThreshold = (float)entry.Value;
				else if (entry.Name == "continuous")
					Continuous = (bool)entry.Value;
				else if(entry.Name == "style")
					Style = (LineStyleEnum)entry.Value;
				else if (entry.Name == "hlStyle")
					HlStyle = (LineHlStyleEnum)entry.Value;
				else if (entry.Name == "hlSize")
					HlSize = (int)entry.Value;
				else if (entry.Name == "movingHl")
					MovingHl = (bool)entry.Value;
				else if (entry.Name == "shrinkingHl")
					ShrinkingHl = (bool)entry.Value;
				else if (entry.Name == "hlBorder")
					HlBorder = (bool)entry.Value;
			}
		}
		override public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			base.GetObjectData(info, ctxt);
			info.AddValue("lineWidth", LineWidth);
			info.AddValue("qn_gapThreshold", Qn_gapThreshold);
			info.AddValue("continuous", Continuous);
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

		//override public void createOcTree(Vector3 minPos, Vector3 size, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps globalTrackProps, TrackProps trackProps, TrackProps texTrackProps)
		//{
		//	if (ocTree != null)
		//		ocTree.dispose();
		//	ocTree = new OcTree<LineGeo>(minPos, size, new Vector3(1000, 1000, 1000), createGeoChunk, drawGeoChunk);
		//	ocTree.createGeo(midiTrack, songDrawProps, trackProps, globalTrackProps, texTrackProps);
		//}

		void calcRectTexCoords(out Vector2 size_tex, out Vector2 size_world, Vector2 texSize, float startDraw, float endDraw, TrackProps texTrackProps, SongDrawProps songDrawProps, TrackProps trackProps)
		{
			Vector3 pos, normal, vertexOffset;
			getCurvePoint(out pos, out normal, out vertexOffset, LineWidth, startDraw, songDrawProps, trackProps);
			Vector3 topLeft_world3 = pos - vertexOffset;
			Vector2 topLeft_world = new Vector2(topLeft_world3.X, topLeft_world3.Y);
			getCurvePoint(out pos, out normal, out vertexOffset, LineWidth, endDraw, songDrawProps, trackProps);
			Vector3 size_world3 = pos + vertexOffset - topLeft_world3;
			size_world = new Vector2(size_world3.X, size_world3.Y);
			//base.calcRectTexCoords(out topLeft_tex, out size_tex, texSize, new Vector2(topLeft_world.X, topLeft_world.Y), size_world, texTrackProps, songDrawProps);
			//topLeft_tex = base.calcTexCoords(texSize, topLeft_world, size_world, new Vector2(0, 0), texTrackProps, songDrawProps);
			//size_tex = base.calcTexCoords(texSize, topLeft_world, size_world, size_world, texTrackProps, songDrawProps) - topLeft_tex;
			Vector2 topLeft_tex, bottomLeft_tex, topRight_tex, bottomRight_tex;
			calcTexCoords(out topLeft_tex, out bottomLeft_tex, texTrackProps.TexProps, 0, 0, songDrawProps, LineWidth, topLeft_world3, topLeft_world3);
			calcTexCoords(out topRight_tex, out bottomRight_tex, texTrackProps.TexProps, endDraw - startDraw, 1, songDrawProps, LineWidth, topLeft_world3 + size_world3, topLeft_world3);
			size_tex = bottomRight_tex - topLeft_tex;
		}
		void getCurvePoint(out Vector3 pos, out Vector3 normal, out Vector3 vertexOffset, float lineWidth, float x, SongDrawProps songDrawProps, TrackProps trackProps)
		{
			Vector3[] points = new Vector3[3];
			Vector3[] tangents = new Vector3[2];
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = new Vector3();
				points[i].X = x + i - 1;
				points[i].Y = songDrawProps.getCurveScreenY(points[i].X, trackProps.TrackView.Curve);
				points[i].Z = 0;
			}
			
			for (int i = 0; i < tangents.Length; i++)
				tangents[i] = points[i + 1] - points[i];

			normal = tangents[0] + tangents[1];
			normal = new Vector3(-normal.Y, normal.X, 0);
			normal.Normalize();
			pos = points[1];

			if (Style == LineStyleEnum.Ribbon)
				vertexOffset = new Vector3(1, 0, 0);
			else
				vertexOffset = normal;
			float halfWidth = lineWidth / 2.0f;
			vertexOffset *= halfWidth;
		}

		void calcTexCoords(out Vector2 vert1TC, out Vector2 vert2TC, TrackPropsTex texProps, float stepFromNoteStart, float normStepFromNoteStart, SongDrawProps songDrawProps, float lineWidth, Vector3 worldPos1, Vector3 worldPos2, bool adjustingAspect = false)
		{
			Vector2 texSize = new Vector2(texProps.Texture.Width, texProps.Texture.Height);
			TexAnchorEnum texUAnchor = (TexAnchorEnum)texProps.UAnchor;
			TexAnchorEnum texVAnchor = (TexAnchorEnum)texProps.VAnchor;
			bool uTile = true, vTile = true;
			if (!adjustingAspect)
			{
				uTile = (bool)texProps.UTile;
				vTile = (bool)texProps.VTile;
			}

			//UAnchor
			if (texUAnchor == TexAnchorEnum.Note)
			{
				if (!uTile)
					vert1TC.X = vert2TC.X = normStepFromNoteStart;
				else
					vert1TC.X = vert2TC.X = stepFromNoteStart / texSize.X;
			}
			else if (texUAnchor == TexAnchorEnum.Screen)
			{
				worldPos1.X += songDrawProps.viewportSize.X / 2;
				worldPos2.X += songDrawProps.viewportSize.X / 2;

				//if (!tileU)
				//    vert1TC.X = vert2TC.X = lineCenter.X / songDrawProps.viewportSize.X;
				//else
				//    vert1TC.X = vert2TC.X = lineCenter.X / texSize.X;
				if (!uTile)
				{
					vert1TC.X = worldPos1.X / songDrawProps.viewportSize.X;
					vert2TC.X = worldPos2.X / songDrawProps.viewportSize.X;
				}
				else
				{
					vert1TC.X = worldPos1.X / texSize.X;
					vert2TC.X = worldPos2.X / texSize.X;
				}
			}
			else if (texUAnchor == TexAnchorEnum.Song)
			{
				if (!uTile)
				{
					vert1TC.X = songDrawProps.getSongPosP(worldPos1.X) / songDrawProps.getSongLengthP();
					vert2TC.X = songDrawProps.getSongPosP(worldPos2.X) / songDrawProps.getSongLengthP();
				}
				else
				{
					vert1TC.X = songDrawProps.getSongPosP(worldPos1.X) / texSize.X;
					vert2TC.X = songDrawProps.getSongPosP(worldPos2.X) / texSize.X;
				}
			}
			else
				throw new NotImplementedException();

			//VAnchor
			if (texVAnchor == TexAnchorEnum.Note)
			{
				vert1TC.Y = 0;
				if (!vTile)
					vert2TC.Y = 1;
				else
					vert2TC.Y = lineWidth / texSize.Y;
			}
			else if (texVAnchor == TexAnchorEnum.Screen)
			{
				worldPos1.Y += songDrawProps.viewportSize.Y / 2;
				worldPos2.Y += songDrawProps.viewportSize.Y / 2;
				if (!vTile)
				{
					vert1TC.Y = worldPos1.Y / songDrawProps.viewportSize.Y;
					vert2TC.Y = worldPos2.Y / songDrawProps.viewportSize.Y;
				}
				else
				{
					vert1TC.Y = worldPos1.Y / texSize.Y;
					vert2TC.Y = worldPos2.Y / texSize.Y;
				}
			}
			else
				throw new NotImplementedException();
			if (!adjustingAspect)
				adjustAspect(ref vert1TC, ref vert2TC, texProps, stepFromNoteStart, normStepFromNoteStart, songDrawProps, lineWidth, worldPos1, worldPos2);
			vert1TC -= songDrawProps.songPosS * texProps.Scroll;
			vert2TC -= songDrawProps.songPosS * texProps.Scroll;
		}

		void adjustAspect(ref Vector2 vert1TC, ref Vector2 vert2TC, TrackPropsTex texProps, float stepFromNoteStart, float normStepFromNoteStart, SongDrawProps songDrawProps, float lineWidth, Vector3 worldPos1, Vector3 worldPos2)
		{
			if ((bool)texProps.KeepAspect)
			{
				Vector2 tiledTc1 = new Vector2(), tiledTc2 = new Vector2();
				calcTexCoords(out tiledTc1, out tiledTc2, texProps, stepFromNoteStart, normStepFromNoteStart, songDrawProps, lineWidth, worldPos1, worldPos2, true);
				Vector2 tcDiff = vert1TC - vert2TC;
				Vector2 tiledTcDiff = tiledTc1 - tiledTc2;

				if ((bool)texProps.UTile && !(bool)texProps.VTile)
				{
					float ratio = tcDiff.Y / tiledTcDiff.Y;
					vert1TC.X *= ratio;
					vert2TC.X *= ratio;
				}
				else if (!(bool)texProps.UTile && (bool)texProps.VTile)
				{
					float ratio;
					if (tiledTcDiff.X == 0)
						ratio = vert1TC.X / tiledTc1.X;
					else
						ratio = tcDiff.X / tiledTcDiff.X;
					vert1TC.Y *= ratio;
					vert2TC.Y *= ratio;
				}
			}
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

		//static string GetName<T>(T item) where T : class
		//{
		//	return typeof(T).GetProperties()[0].Name;
		//}

		public override void createGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, TrackProps texTrackProps)
		{
			LineGeo lineGeo = new LineGeo();
			geo = lineGeo;
			List<Midi.Note> noteList = midiTrack.Notes;
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			if (noteList.Count == 0)
				return;

			Color color;
			Texture2D texture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, false, out color, out texture);
			
			Vector2 texSize = new Vector2(texture.Width, texture.Height);
			
			int vertIndex = 3;
			int hLineVertIndex = 0;
			int completeNoteListIndex = midiTrack.Notes.IndexOf(noteList[0]);
			for (int n = 0; n < noteList.Count; n++)
			{
				//Get current note
				Midi.Note note = noteList[n], nextNote;
				if (note.start > songDrawProps.song.SongLengthT) //only  if audio ends before the notes end
					continue;

				if (n < noteList.Count - 1)
					nextNote = noteList[n + 1];
				else if (completeNoteListIndex < midiTrack.Notes.Count - 1)
					nextNote = midiTrack.Notes[completeNoteListIndex + 1];
				else
					nextNote = note;

				Vector2 noteStart = songDrawProps.getScreenPosF(note.start, note.pitch);
				float noteEnd = songDrawProps.getScreenPosF(note.stop, note.pitch).X;
				
				Vector2 nextNoteStart = songDrawProps.getScreenPosF(nextNote.start, nextNote.pitch);
				if (noteEnd > nextNoteStart.X && completeNoteListIndex < midiTrack.Notes.Count - 1)
					noteEnd = nextNoteStart.X;
				
				bool endOfSegment = false;
				if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * songDrawProps.song.TicksPerBeat || note == nextNote)
				{
					if (nextNoteStart.X != noteStart.X)
						nextNoteStart.Y = (int)MathHelper.Lerp(noteStart.Y, nextNoteStart.Y, (float)(noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X));
					nextNoteStart.X = noteEnd;
					endOfSegment = true;
				}

				float startDraw = noteStart.X;
				float endDraw = nextNoteStart.X;

				float step = 1;
				
				for (float x = startDraw; x < endDraw; x += step)
				{
					Vector3 center, normal, vertexOffset;
					getCurvePoint(out center, out normal, out vertexOffset, LineWidth, x, songDrawProps, trackProps);
					lineVerts[vertIndex].normal = lineVerts[vertIndex + 1].normal = normal;

					//Fill vertex buffer
					lineVerts[vertIndex].pos = center - vertexOffset;
					lineVerts[vertIndex + 1].pos = center + vertexOffset;
					lineVerts[vertIndex].center = lineVerts[vertIndex + 1].center = center;
					float normStepFromNoteStart = (x - startDraw) / (nextNoteStart.X - noteStart.X);
					lineVerts[vertIndex].normStepFromNoteStart = lineVerts[vertIndex + 1].normStepFromNoteStart = normStepFromNoteStart;
					//Vector2 ns = songDrawProps.getScreenPosF(note.start, note.pitch);
					//Vector2 nns = songDrawProps.getScreenPosF(nextNote.start, nextNote.pitch);
					if (texTrackProps.TexProps.Texture != null)
						calcTexCoords(out lineVerts[vertIndex].texCoords, out lineVerts[vertIndex + 1].texCoords, texTrackProps.TexProps, x - startDraw, normStepFromNoteStart, songDrawProps, LineWidth, lineVerts[vertIndex].pos, lineVerts[vertIndex + 1].pos);

					if (Style == LineStyleEnum.Ribbon)
					{
						float hLineStart = center.X;
						float hLineEnd = hLineStart;
						do
						{
							hLineEnd++;
						} while ((int)center.Y == (int)songDrawProps.getCurveScreenY((float)hLineEnd + 1, trackProps.TrackView.Curve) && hLineEnd < endDraw);
						if (hLineEnd > hLineStart + LineWidth / 2)
						{
							hLineVerts[hLineVertIndex++] = lineVerts[vertIndex];
							hLineVerts[hLineVertIndex++] = lineVerts[vertIndex + 1];
						}
					}

					if (x == startDraw && vertIndex > 3)
					{
						lineVerts[vertIndex].pos = lineVerts[vertIndex - 2].pos;
						lineVerts[vertIndex + 1].pos = lineVerts[vertIndex - 1].pos;
					}
					vertIndex += 2;
					if (vertIndex >= MaxLineVerts - 2 || hLineVertIndex >= MaxHLineVerts - 2)
					{
						createLineSegment(ref vertIndex, ref hLineVertIndex, lineGeo);
						x -= step;
					}
				}

				if (!Continuous)
					endOfSegment = true; //One draw call per note. Can be used to avoid glitches between notes because of instant IN.normStepFromNoteStart interpolation from 1 to 0.
				if (endOfSegment)
					createLineSegment(ref vertIndex, ref hLineVertIndex, lineGeo);

				completeNoteListIndex++;
			}
			createLineSegment(ref vertIndex, ref hLineVertIndex, lineGeo);
		}

		public override void drawGeoChunk(Geo geo)
		{
			LineGeo lineGeo = (LineGeo)geo;
			foreach (var vb in lineGeo.lineVb)
			{
				songPanel.GraphicsDevice.SetVertexBuffer(vb);
				songPanel.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 3, vb.VertexCount - 5);
			}
			foreach (var vb in lineGeo.hLineVb)
			{
				songPanel.GraphicsDevice.SetVertexBuffer(vb);
				songPanel.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, vb.VertexCount);
			}
		}

		void createLineSegment(ref int numVerts, ref int numHLineVerts, LineGeo geo)
		{
			if (LineWidth > 0)
			{
				if (numVerts > 5 || numHLineVerts > 1)
				{
					fx.CurrentTechnique = fx.Techniques["Line"];
					fx.CurrentTechnique.Passes[0].Apply();
					if (numHLineVerts > 1)
					{
						VertexBuffer vb = new VertexBuffer(songPanel.GraphicsDevice, lineVertDecl, numHLineVerts, BufferUsage.WriteOnly);
						vb.SetData(hLineVerts, 0, numHLineVerts);
						geo.hLineVb.Add(vb);
						//songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, hLineVerts, 0, numHLineVerts / 2);
						numHLineVerts = 0;
					}
					//songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, hLineVerts, 0, numHLineVerts / 2);
					if (numVerts > 5)
					{
						VertexBuffer vb = new VertexBuffer(songPanel.GraphicsDevice, lineVertDecl, numVerts, BufferUsage.WriteOnly);
						vb.SetData(lineVerts, 0, numVerts);
						geo.lineVb.Add(vb);
						
						//songPanel.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, lineVerts, 3, numVerts - 3, lineInds, 0, numVerts - 5);
						//songPanel.GraphicsDevice.SetVertexBuffer(geo.lineVb);
						//songPanel.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 3, geo.lineVb.VertexCount-5);
						numVerts = 3;
					}
					//songPanel.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, lineVerts, 3, numVerts - 3, lineInds, 0, numVerts - 5);
				}
			}
			
			
		}

		public override void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, bool selectingRegion, TrackProps texTrackProps)
		{
			base.drawTrack(midiTrack, songDrawProps, trackProps, globalTrackProps, selectingRegion, texTrackProps);
			List<Midi.Note> noteList = midiTrack.Notes;
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			if (noteList.Count == 0)
				return;

			//this.trackProps = trackProps;

			fx.Parameters["Style"].SetValue((int)Style);
			fx.Parameters["HlSize"].SetValue((float)HlSize / 2.0f);
			
			songPanel.GraphicsDevice.BlendState = songPanel.BlendState;
			float radius = LineWidth / 2.0f;
			fx.Parameters["Radius"].SetValue(radius);
			Color color;
			Texture2D texture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, false, out color, out texture);
			fx.Parameters["Color"].SetValue(color.ToVector4());

			float songPosP = songDrawProps.getTimeTPosF(Project.SongPosT);
			fx.Parameters["SongPos"].SetValue(songPosP);
			fx.Parameters["InnerHlSize"].SetValue(0.0f);
			Vector2 texSize = new Vector2(texture.Width, texture.Height);
			fx.Parameters["TexSize"].SetValue(texSize);
			fx.Parameters["Texture"].SetValue(texture);

			fx.CurrentTechnique = fx.Techniques["Line"];
			fx.CurrentTechnique.Passes[0].Apply();
			trackProps.TrackView.ocTree.drawGeo(Project.Camera);

			drawHighLights(midiTrack, songDrawProps, trackProps, globalTrackProps);
		}

		void drawHighLights(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps)
		{
			List<Midi.Note> noteList = midiTrack.Notes;
			int hlNoteIndex = midiTrack.getLastNoteIndexAtTime(Project.SongPosT);
			if (hlNoteIndex < 0)
				return;
			Midi.Note note = noteList[hlNoteIndex], nextNote;
			if (note.start > songDrawProps.song.SongLengthT) //only  if audio ends before the notes end
				return;

			if (hlNoteIndex < noteList.Count - 1)
				nextNote = noteList[hlNoteIndex + 1];
			else
				nextNote = note;

			Vector2 noteStart = songDrawProps.getScreenPosF(note.start, note.pitch);
			float noteEnd = songDrawProps.getScreenPosF(note.stop, note.pitch).X;

			Vector2 nextNoteStart = songDrawProps.getScreenPosF(nextNote.start, nextNote.pitch);
			if (noteEnd > nextNoteStart.X && hlNoteIndex < noteList.Count - 1)
				noteEnd = nextNoteStart.X;

			if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * songDrawProps.song.TicksPerBeat || note == nextNote)
			{
				if (nextNoteStart.X != noteStart.X)
					nextNoteStart.Y = (int)MathHelper.Lerp(noteStart.Y, nextNoteStart.Y, (float)(noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X));
				nextNoteStart.X = noteEnd;
			}

			Vector3 noteStartVec = new Vector3(noteStart.X, noteStart.Y, 0);
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
					float normPitch = trackProps.TrackView.Curve.Evaluate((float)songDrawProps.getSongPosT((int)x1));
					float y1 = songDrawProps.getPitchScreenPos(normPitch);
					float x2 = x1 + 1;
					normPitch = trackProps.TrackView.Curve.Evaluate((float)songDrawProps.getSongPosT((int)x2));
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
			float leftLength = -noteStart.X - 1;
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

			Color hlColor;
			Texture2D hlTexture;
			getMaterial(songDrawProps, trackProps, globalTrackProps, true, out hlColor, out hlTexture);
			fx.Parameters["HlColor"].SetValue(hlColor.ToVector4());
			
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
					Vector3 circlePos = new Vector3(x, songDrawProps.getCurveScreenY(x, trackProps.TrackView.Curve), 0);
					setHlCirclePos(circlePos);
				}
				songPanel.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				fx.CurrentTechnique = fx.Techniques["Circle"];
				fx.CurrentTechnique.Passes[0].Apply();
				songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, lineHlVerts, 0, 2);
			}
		}

		void drawLineSegment(ref int numVerts, ref int numHLineVerts)
		{
			if (LineWidth > 0)
			{
				if (numVerts > 5 || numHLineVerts > 1)
				{
					fx.CurrentTechnique = fx.Techniques["Line"];
					fx.CurrentTechnique.Passes[0].Apply();
					//if (numHLineVerts > 1)
						//songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, hLineVerts, 0, numHLineVerts / 2);
					//if (numVerts > 5)
						//songPanel.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, lineVerts, 3, numVerts - 3, lineInds, 0, numVerts - 5);
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
			lineVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Position, 1), new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.Position, 2), new VertexElement(40, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
			//for (short i = 0; i < lineInds.Length; i++)
				//lineInds[i] = i;
		}
	}

	public class LineGeo : Geo
	{
		public List<VertexBuffer> lineVb = new List<VertexBuffer>();
		public List<VertexBuffer> hLineVb = new List<VertexBuffer>();
		public override void Dispose()
		{
			foreach (var vb in lineVb)
				vb.Dispose();
			foreach (var vb in hLineVb)
				vb.Dispose();
		}
	}

	//public class LineOcTree : OcTree<LineGeo>
	//{
		
	//}
}

