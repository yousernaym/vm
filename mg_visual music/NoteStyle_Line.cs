﻿using System;
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
	public enum LineType { Standard, Ribbon };
	public enum LineHlType { Arrow, Circle };

	public struct LineHlVertex : IVertexType
	{
		public Vector3 pos;
		public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));
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
		//public float normStepFromNoteStart;
		public Vector2 normPos;
		public Vector2 texCoords;
	}

	[Serializable()]
	public class NoteStyle_Line : NoteStyle
	{
		const int MaxLineVerts = 100000;
		const int MaxHLineVerts = 30000; 
		static VertexDeclaration lineVertDecl;
		static LineVertex[] lineVerts = new LineVertex[MaxLineVerts];
		static LineVertex[] hLineVerts = new LineVertex[MaxHLineVerts];
		protected static LineHlVertex[] lineHlVerts = new LineHlVertex[4];

		float VpLineWidth => Project.normalizeVpScalar((float)LineWidth);
		public float? LineWidth { get; set; } = 5;
		public float? Qn_gapThreshold { get; set; } = 3;
		public bool? Continuous { get; set; } = true;
		public LineType? LineType { get; set; } = Visual_Music.LineType.Standard;
		public LineHlType? HlType { get; set; } = LineHlType.Arrow;
		float VpHlSize => Project.normalizeVpScalar((float)HlSize);
		public float? HlSize { get; set; } = 20;
		public bool? MovingHl { get; set; } = false;
		public bool? ShrinkingHl { get; set; } = false;
		public bool? HlBorder { get; set; } = false;

		public NoteStyle_Line()
		{
			styleType = NoteStyleType.Line;
		}
		public NoteStyle_Line(TrackProps tprops)
			: base(tprops)
		{
			styleType = NoteStyleType.Line;
		}
		public NoteStyle_Line(SerializationInfo info, StreamingContext ctxt)
			: base(info, ctxt)
		{
			foreach (var entry in info)
			{
				if (entry.Name == "lineWidth")
					LineWidth = (float)entry.Value;
				else if (entry.Name == "qn_gapThreshold")
					Qn_gapThreshold = (float)entry.Value;
				else if (entry.Name == "continuous")
					Continuous = (bool)entry.Value;
				else if(entry.Name == "lineType")
					LineType = (LineType)entry.Value;
				else if (entry.Name == "hlType")
					HlType = (LineHlType)entry.Value;
				else if (entry.Name == "hlSize")
					HlSize = (float)entry.Value;
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
			info.AddValue("LineType", LineType);
			info.AddValue("hlType", HlType);
			info.AddValue("hlSize", HlSize);
			info.AddValue("movingHl", MovingHl);
			info.AddValue("shrinkingHl", ShrinkingHl);
			info.AddValue("hlBorder", HlBorder);
		}
		override public void loadFx()
		{
			fx = songPanel.Content.Load<Effect>("Line");
		}

		void getCurvePoint(out Vector3 pos, out Vector3 normal, out Vector3 vertexOffset, float step, float x, TrackProps trackProps, float lineWidth)
		{
			Vector3[] points = new Vector3[3];
			Vector3[] tangents = new Vector3[2];
			step *= 0.1f;

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = new Vector3();
				points[i].X = x + i * step - step;
				points[i].Y = Project.getCurveScreenY(points[i].X, trackProps.TrackView.Curve);
				points[i].Z = 0;
			}
			
			for (int i = 0; i < tangents.Length; i++)
				tangents[i] = points[i + 1] - points[i];

			normal = tangents[0] + tangents[1];
			normal = new Vector3(-normal.Y, normal.X, 0);
			normal.Normalize();
			pos = points[1];

			//normal.X = 1;
			//normal.Y = Project.getScreenPosY(trackProps.TrackView.Curve.EvaluateSignedCurvatureDerivative(x));
			//normal.Z = 0;
			//normal.Normalize();

			if (LineType == Visual_Music.LineType.Ribbon)
				vertexOffset = new Vector3(1, 0, 0);
			else
				vertexOffset = normal;
			vertexOffset *= lineWidth / 2.0f;
		}

		void calcTexCoords(out Vector2 vert1TC, out Vector2 vert2TC, TrackPropsTex texProps, float stepFromNoteStart, float normStepFromNoteStart, float lineWidth, Vector3 worldPos1, Vector3 worldPos2, bool adjustingAspect = false)
		{
			Vector2 vpSize = Project.Camera.ViewportSize;
						
			Vector2 texSize = new Vector2(texProps.Texture.Width, texProps.Texture.Height) * TexTileScale;
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
				if (!uTile)
				{
					vert1TC.X = worldPos1.X / vpSize.X;
					vert2TC.X = worldPos2.X / vpSize.X;
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
					vert1TC.X = worldPos1.X / Project.SongLengthP;
					vert2TC.X = worldPos2.X / Project.SongLengthP;
				}
				else
				{
					vert1TC.X = worldPos1.X / texSize.X;
					vert2TC.X = worldPos2.X / texSize.X;
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
				worldPos1.Y += Project.Camera.ViewportSize.Y / 2;
				worldPos2.Y += Project.Camera.ViewportSize.Y / 2;
				if (!vTile)
				{
					vert1TC.Y = worldPos1.Y / Project.Camera.ViewportSize.Y;
					vert2TC.Y = worldPos2.Y / Project.Camera.ViewportSize.Y;
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
				adjustAspect(ref vert1TC, ref vert2TC, texProps, stepFromNoteStart, normStepFromNoteStart, lineWidth, worldPos1, worldPos2);
			vert1TC.Y *= -1;
			vert2TC.Y *= -1;
		}

		void adjustAspect(ref Vector2 vert1TC, ref Vector2 vert2TC, TrackPropsTex texProps, float stepFromNoteStart, float normStepFromNoteStart, float lineWidth, Vector3 worldPos1, Vector3 worldPos2)
		{
			if ((bool)texProps.KeepAspect)
			{
				Vector2 tiledTc1 = new Vector2(), tiledTc2 = new Vector2();
				calcTexCoords(out tiledTc1, out tiledTc2, texProps, stepFromNoteStart, normStepFromNoteStart, lineWidth, worldPos1, worldPos2, true);
				Vector2 tcDiff = vert1TC - vert2TC;
				Vector2 tiledTcDiff = tiledTc1 - tiledTc2;

				if ((bool)texProps.UTile && !(bool)texProps.VTile)
				{
					float ratio = -tcDiff.Y / tiledTcDiff.Y;
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
			float halfHlSize = VpHlSize / 2.0f;
			lineHlVerts[0].pos = new Vector3(-halfHlSize, -halfHlSize, 0) + pos;
			lineHlVerts[1].pos = new Vector3(halfHlSize, -halfHlSize, 0) + pos;
			lineHlVerts[2].pos = new Vector3(-halfHlSize, halfHlSize, 0) + pos;
			lineHlVerts[3].pos = new Vector3(halfHlSize, halfHlSize, 0) + pos;
		}

		//static string GetName<T>(T item) where T : class
		//{
		//	return typeof(T).GetProperties()[0].Name;
		//}

		public override void createGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
		{
			LineGeo lineGeo = new LineGeo();
			geo = lineGeo;
			List<Midi.Note> noteList = midiTrack.Notes;
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			if (noteList.Count == 0)
				return;

			Color color;
			Texture2D texture;
			getMaterial(trackProps, false, out color, out texture);
			
			Vector2 texSize = new Vector2(texture.Width, texture.Height);
			
			int vertIndex = 3;
			int hLineVertIndex = 0;
			int completeNoteListIndex = midiTrack.Notes.IndexOf(noteList[0]);
			float vpLineWidth = VpLineWidth;
			for (int n = 0; n < noteList.Count; n++)
			{
				//Get current note
				Midi.Note note = noteList[n], nextNote;
				if (note.start > Project.Notes.SongLengthT) //only  if audio ends before the notes end
					continue;

				if (n < noteList.Count - 1)
					nextNote = noteList[n + 1];
				else if (completeNoteListIndex < midiTrack.Notes.Count - 1)
					nextNote = midiTrack.Notes[completeNoteListIndex + 1];
				else
					nextNote = note;

				Vector2 noteStart = Project.getScreenPos(note.start, note.pitch);
				float noteEnd = Project.getScreenPos(note.stop, note.pitch).X;
				
				Vector2 nextNoteStart = Project.getScreenPos(nextNote.start, nextNote.pitch);
				if (noteEnd > nextNoteStart.X && completeNoteListIndex < midiTrack.Notes.Count - 1)
					noteEnd = nextNoteStart.X;
				
				bool endOfSegment = false;
				if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * Project.Notes.TicksPerBeat || note == nextNote)
				{
					if (nextNoteStart.X != noteStart.X)
						nextNoteStart.Y = (int)MathHelper.Lerp(noteStart.Y, nextNoteStart.Y, (float)(noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X));
					nextNoteStart.X = noteEnd;
					endOfSegment = true;
				}

				float startDraw = noteStart.X;
				float endDraw = nextNoteStart.X;
				float curvature = calcLinearLineAngle(n, trackProps.TrackView.Curve);
				//if (n > 0)
				//{
				//	float prevCurvature = calcLinearLineAngle(n - 1, trackProps.TrackView.Curve);
				//	if (prevCurvature > curvature)
				//		curvature = (prevCurvature + curvature) / 2;    //Prevent too big jumps between levels of tesselation which will cause sharp bends
				//}
				if (n < noteList.Count - 1)
					curvature = Math.Max(curvature, calcLinearLineAngle(n + 1, trackProps.TrackView.Curve));
				
				//curvature /= (float)Math.PI; //Map to [0,1]
				curvature = Math.Min(curvature, 0.97f * (float)Math.PI); //Clip below 180 degrees to avoid extreme tesselation
				curvature = (float)Math.Pow(curvature, 2.25) * 1000;
				float step;
				if (curvature == 0)
					step = (endDraw - startDraw) * 0.999f; //Only one point for thes note
				else
					step = 1 / curvature;
				
				if (step >= endDraw - startDraw)
					step = (endDraw - startDraw) * 0.999f;
				for (float x = startDraw; x < endDraw; x += step)
				{
					Vector3 center, normal, vertexOffset;
					
					getCurvePoint(out center, out normal, out vertexOffset, step, x, trackProps, vpLineWidth);
					//normal.X = curvature/10f;
					lineVerts[vertIndex].normal = lineVerts[vertIndex + 1].normal = normal;

					//Create vertices
					//adjustCurvePoint(center, vertexOffset, -1, trackProps, lineVerts, vertIndex, step, vpLineWidth);
					//adjustCurvePoint(center, vertexOffset, 1, trackProps, lineVerts, vertIndex + 1, step, vpLineWidth);
					
					//curvature = trackProps.TrackView.Curve.EvaluateCurvature(Project.getTimeT(x));
					//center.Y = curvature / 10;
					lineVerts[vertIndex].pos = center - vertexOffset;
					lineVerts[vertIndex + 1].pos = center + vertexOffset;
					lineVerts[vertIndex].center = lineVerts[vertIndex + 1].center = center;
					float normStepFromNoteStart = (x - startDraw) / (nextNoteStart.X - noteStart.X);
					//lineVerts[vertIndex].normStepFromNoteStart = lineVerts[vertIndex + 1].normStepFromNoteStart = normStepFromNoteStart;
					lineVerts[vertIndex].normPos = new Vector2(normStepFromNoteStart, 0);
					lineVerts[vertIndex + 1].normPos = new Vector2(normStepFromNoteStart, 1);
					
					if (texMaterial.TexProps.Texture != null)
						calcTexCoords(out lineVerts[vertIndex].texCoords, out lineVerts[vertIndex + 1].texCoords, texMaterial.TexProps, x - startDraw, normStepFromNoteStart, vpLineWidth, lineVerts[vertIndex].pos, lineVerts[vertIndex + 1].pos);

					if (LineType == Visual_Music.LineType.Ribbon)
					{
						float hLineStart = center.X;
						float hLineEnd = hLineStart;
						do
						{
							hLineEnd += step;
						} while ((int)center.Y == (int)Project.getCurveScreenY((float)hLineEnd + step, trackProps.TrackView.Curve) && hLineEnd < endDraw);
						if (hLineEnd > hLineStart + vpLineWidth / 2)
						{
							hLineVerts[hLineVertIndex++] = lineVerts[vertIndex];
							hLineVerts[hLineVertIndex++] = lineVerts[vertIndex + 1];
						}
						//float horizontalFactor = Math.Abs(normal.Y);
						//float horizontalLimit = 0.98f;
						//float minThickness = 0.001f;
						//if (horizontalFactor > horizontalLimit)
						//{ //todo use vertexoffset for non-ribbon instead of normal
						//	horizontalFactor = (horizontalFactor - horizontalLimit) / (1 - horizontalLimit);  //[horizontalFactor, 1] -> [0, 1]
						//	lineVerts[vertIndex].pos.Y -= minThickness * horizontalFactor * Math.Sign(normal.X);
						//}
					}

					if (x == startDraw && vertIndex > 3)
					{
						lineVerts[vertIndex].pos = lineVerts[vertIndex - 2].pos;
						lineVerts[vertIndex + 1].pos = lineVerts[vertIndex - 1].pos;
					}

					//Create bounding box
					Vector3 bboxCorner1 = lineVerts[vertIndex].pos;
					Vector3 bboxCorner2 = lineVerts[vertIndex + 1].pos;
					bboxCorner1.Z = bboxCorner2.Z = 0;// fx.Parameters["PosOffset"].GetValueVector3().Z;
					geo.bboxes.Add(BoundingBox.CreateFromPoints(new Vector3[2] { bboxCorner1, bboxCorner2 }));
					
					vertIndex += 2;

					if (vertIndex >= MaxLineVerts - 2 || hLineVertIndex >= MaxHLineVerts - 2)
					{
						createLineSegment(ref vertIndex, ref hLineVertIndex, lineGeo, vpLineWidth);
						x -= step;
					}
					//break;
				}

				if (!(bool)Continuous)
					endOfSegment = true; //One draw call per note. Can be used to avoid glitches between notes because of instant IN.normStepFromNoteStart interpolation from 1 to 0.
				if (endOfSegment)
					createLineSegment(ref vertIndex, ref hLineVertIndex, lineGeo, vpLineWidth);
				completeNoteListIndex++;
			}
			createLineSegment(ref vertIndex, ref hLineVertIndex, lineGeo, vpLineWidth);
		}

		float calcLinearLineAngle(int noteIndex, Curve curve)
		{
			Vector2 prevVec;
			if (noteIndex > 0)
				prevVec = curve.Keys[noteIndex - 1].NextVector;
			else
				prevVec = new Vector2(1, 0);
			prevVec.X = Project.getScreenPosX((int)prevVec.X);
			prevVec.Y = prevVec.Y * Project.NoteHeight;
			Vector2 nextVec = curve.Keys[noteIndex].NextVector;
			nextVec.X = Project.getScreenPosX((int)nextVec.X);
			nextVec.Y = nextVec.Y * Project.NoteHeight;
			nextVec.Normalize();
			prevVec.Normalize();
			float angle = Vector2.Dot(nextVec, prevVec);
			//Handle rounding errors
			if (angle > 1)
				angle = 0;
			else if (angle < -1)
				angle = (float)Math.PI;
			else
				angle = (float)Math.Acos(angle);
			return angle;
		}

		void adjustCurvePoint(Vector3 center, Vector3 vertexOffset, int side, TrackProps trackProps, LineVertex[] lineVerts, int vertIndex, float step, float lineWidth)
		{
			Vector3 curPos = center + vertexOffset * side;
			Vector3 dummyCenter, dummyNormal, newVerteexOffset;
			getCurvePoint(out dummyCenter, out dummyNormal, out newVerteexOffset, step, curPos.X, trackProps, lineWidth);
			float newPosX = curPos.X + newVerteexOffset.X * side;

			lineVerts[vertIndex].pos = curPos;
			if (curPos.X > center.X != newPosX > curPos.X)
			{
				int prevIndex = Math.Max(vertIndex - 2, 3);
				Vector3 prevPos = lineVerts[prevIndex].pos;
				lineVerts[vertIndex].pos = prevPos;
				//int keyIndex;
				//float timeT = Project.getTimeT(center.X);
				//keyIndex = trackProps.TrackView.Curve.Keys.IndexAtPosition(timeT);
				//if (curPos.X < center.X)
				//	keyIndex--;
				//if (keyIndex < trackProps.TrackView.Curve.Keys.Count)
				//{
				//	CurveKey key = trackProps.TrackView.Curve.Keys[keyIndex];
				//	lineVerts[vertIndex].pos.X = Project.getScreenPosX((int)key.Position);
				//	//lineVerts[vertIndex].pos.Y = Project.getScreenPosY(key.Value) + (float)LineWidth / 2 * side;
				//	lineVerts[vertIndex].pos.Y = lineVerts[vertIndex-2].pos.Y;
				//}
			}
			lineVerts[vertIndex].normal = (lineVerts[vertIndex].pos - center) * -side;
			lineVerts[vertIndex].normal.Normalize();
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

		void createLineSegment(ref int numVerts, ref int numHLineVerts, LineGeo geo, float lineWidth)
		{
			//Console.WriteLine(numVerts);
			if (lineWidth > 0)
			{
				if (numVerts > 5 || numHLineVerts > 1)
				{
					if (numHLineVerts > 1)
					{
						VertexBuffer vb = new VertexBuffer(songPanel.GraphicsDevice, lineVertDecl, numHLineVerts, BufferUsage.WriteOnly);
						vb.SetData(hLineVerts, 0, numHLineVerts);
						geo.hLineVb.Add(vb);
						numHLineVerts = 0;
					}
					if (numVerts > 5)
					{
						VertexBuffer vb = new VertexBuffer(songPanel.GraphicsDevice, lineVertDecl, numVerts, BufferUsage.WriteOnly);
						vb.SetData(lineVerts, 0, numVerts);
						geo.lineVb.Add(vb);
						numVerts = 3;
					}
				}
			}
		}

		public override void drawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
		{
			float songPosP;

			base.drawTrack(midiTrack, trackProps, texMaterial, out songPosP);
			List<Midi.Note> noteList = midiTrack.Notes;
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			if (noteList.Count == 0)
				return;

			//this.trackProps = trackProps;

			fx.Parameters["LineType"].SetValue((int)LineType);
			fx.Parameters["HlSize"].SetValue(VpHlSize / 2.0f);
			songPanel.GraphicsDevice.BlendState = songPanel.BlendState;
			float radius = (float)VpLineWidth / 2.0f;
			fx.Parameters["Radius"].SetValue(radius);
			fx.Parameters["InnerHlSize"].SetValue(0.0f);

			Color color;
			Texture2D texture;
			getMaterial(trackProps, false, out color, out texture);
			fx.Parameters["Color"].SetValue(color.ToVector4());
			fx.Parameters["Texture"].SetValue(texture);

			//Texture scrolling including adjustment for screen anchoring
			Vector2 texSize = new Vector2(texture.Width, texture.Height) * TexTileScale;
			TrackPropsTex texProps = texMaterial.TexProps;
			Vector2 texScrollOffset = Project.SongPosB * texProps.Scroll;
			if (texProps.UAnchor == TexAnchorEnum.Screen)
				texScrollOffset.X += (songPosP + Project.Camera.ViewportSize.X / 2) / ((bool)texProps.UTile ? texSize.X : Project.Camera.ViewportSize.X);
			fx.Parameters["TexScrollOffset"].SetValue(texScrollOffset);

			fx.CurrentTechnique = fx.Techniques["Line"];
			fx.CurrentTechnique.Passes[0].Apply();
			trackProps.TrackView.ocTree.drawGeo(Project.Camera);

			DepthStencilState oldDss = songPanel.GraphicsDevice.DepthStencilState;
			songPanel.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			drawHighLights(midiTrack, trackProps, songPosP);
			songPanel.GraphicsDevice.DepthStencilState = oldDss;
		}

		void drawHighLights(Midi.Track midiTrack, TrackProps trackProps, float songPosP)
		{
			List<Midi.Note> noteList = midiTrack.Notes;
			int hlNoteIndex = midiTrack.getLastNoteIndexAtTime(Project.SongPosT);
			if (hlNoteIndex < 0)
				return;
			Midi.Note note = noteList[hlNoteIndex], nextNote;
			if (note.start > Project.Notes.SongLengthT) //only  if audio ends before the notes end
				return;

			if (hlNoteIndex < noteList.Count - 1)
				nextNote = noteList[hlNoteIndex + 1];
			else
				nextNote = note;

			Vector3 noteStart = new Vector3(Project.getScreenPos(note.start, note.pitch), 0);
			float noteEnd = Project.getScreenPos(note.stop, note.pitch).X;

			Vector3 nextNoteStart = new Vector3(Project.getScreenPos(nextNote.start, nextNote.pitch), 0);
			if (noteEnd > nextNoteStart.X && hlNoteIndex < noteList.Count - 1)
				noteEnd = nextNoteStart.X;
			
			if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * Project.Notes.TicksPerBeat || note == nextNote)
			{
				if (nextNoteStart.X != noteStart.X)
					nextNoteStart.Y = MathHelper.Lerp(noteStart.Y, nextNoteStart.Y, (float)(noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X));
				nextNoteStart.X = noteEnd;
			}

			Vector3 hlPos = noteStart;
			if ((bool)MovingHl)
			{
				float distBetweenNotes = (nextNoteStart.X - noteStart.X);
				float normPos = (songPosP - noteStart.X) / distBetweenNotes;
				normPos = (float)Math.Pow(normPos, 2);
				hlPos.X = noteStart.X + normPos * distBetweenNotes;
				hlPos.Y = Project.getScreenPosY(trackProps.TrackView.Curve.Evaluate(Project.getTimeT(hlPos.X)));
			}

			//Set common fx params---------------------
			
			//For shrinking highlights
			float leftLength = -(noteStart.X - songPosP) - 0.0011f;
			float shrinkPercent = leftLength / (noteEnd - noteStart.X);
			if (!(bool)ShrinkingHl)
			{
				shrinkPercent = 0;
				if ((bool)HlBorder)
					shrinkPercent = 1;
			}
			fx.Parameters["ClipPercent"].SetValue(shrinkPercent);
			float innerHlSize = VpHlSize * 0.5f * (1 - shrinkPercent);
			fx.Parameters["InnerHlSize"].SetValue(innerHlSize);

			Color hlColor;
			Texture2D hlTexture;
			getMaterial(trackProps, true, out hlColor, out hlTexture);
			fx.Parameters["HlColor"].SetValue(hlColor.ToVector4());
			fx.Parameters["Border"].SetValue((bool)HlBorder);
			//-----------------------------------------------

			if (HlType == LineHlType.Arrow)
			{
				float arrowLength;
				Vector3 arrowDir;
				Vector3 arrowNormal;
				if (!(bool)MovingHl)  //Non-moving arrow
				{
					Vector3 nextNoteOffset = nextNoteStart - noteStart;
					arrowLength = nextNoteOffset.Length();
					arrowDir = nextNoteOffset;;
				}
				else //Moving arrow
				{
					float x1 = hlPos.X;
					float y1 = hlPos.Y;
					float x2 = x1 + 0.001f;
					float pitch2 = trackProps.TrackView.Curve.Evaluate(Project.getTimeT(x2));
					float y2 = Project.getScreenPosY(pitch2);
					arrowDir = new Vector3(x2 - x1, y2 - y1, 0);
					arrowLength = VpHlSize * 1.25f;  //Make arrow 25% longer than wide
				}
				arrowDir.Normalize();
				arrowNormal = new Vector3(-arrowDir.Y, arrowDir.X, 0);
				arrowNormal.Normalize();

				float halfArrowWidth = VpHlSize * 0.5f;

				lineHlVerts[0].pos = hlPos + arrowNormal * halfArrowWidth;
				lineHlVerts[1].pos = hlPos - arrowNormal * halfArrowWidth;
				lineHlVerts[2].pos = hlPos + arrowDir * arrowLength;

				fx.Parameters["ArrowDir"].SetValue(arrowDir);
				//lineFx.Parameters["ArrowLength"].SetValue(nextNoteOffsetLength);
				fx.Parameters["ArrowStart"].SetValue(hlPos);
				fx.Parameters["ArrowEnd"].SetValue(lineHlVerts[2].pos); //Is used to calc distance from the two "sides" of the triangle (not the bottom) since they share this point
				Vector3 side1Tangent = lineHlVerts[2].pos - lineHlVerts[0].pos;
				Vector3 side1Normal = new Vector3(-side1Tangent.Y, side1Tangent.X, 0);
				side1Normal.Normalize();
				fx.Parameters["Side1Normal"].SetValue(side1Normal);
				Vector3 side2Tangent = lineHlVerts[2].pos - lineHlVerts[1].pos;
				Vector3 side2Normal = new Vector3(-side2Tangent.Y, side2Tangent.X, 0);
				side2Normal.Normalize();
				fx.Parameters["Side2Normal"].SetValue(side2Normal);

				//Calc shortest dist to incenter from border, ie. the inscribed circle's radius
				float a = (lineHlVerts[0].pos - lineHlVerts[1].pos).Length();
				float b = (lineHlVerts[0].pos - lineHlVerts[2].pos).Length();
				float c = (lineHlVerts[1].pos - lineHlVerts[2].pos).Length();
				float k = (a + b + c) / 2.0f;
				float icRadius = (float)Math.Sqrt(k * (k - a) * (k - b) * (k - c)) / k;
				fx.Parameters["DistToCenter"].SetValue(icRadius);

				fx.CurrentTechnique = fx.Techniques["Arrow"];
				fx.CurrentTechnique.Passes[0].Apply();
				songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, lineHlVerts, 0, 1);
			}
			else if (HlType == LineHlType.Circle)
			{ 
				setHlCirclePos(hlPos);
			
				songPanel.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				fx.CurrentTechnique = fx.Techniques["Circle"];
				fx.CurrentTechnique.Passes[0].Apply();
				songPanel.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, lineHlVerts, 0, 2);
			}
		}

		float calcLerpFactor(int x, int x1, int x2)
		{
			float f = (float)(x - x1) / (float)(x2 - x1);
			return (1 - (float)Math.Cos((double)f * Math.PI)) * 0.5f;
		}

		public static void sInit()
		{
			lineVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Position, 1), new VertexElement(36, VertexElementFormat.Vector2, VertexElementUsage.Position, 2), new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
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
}

