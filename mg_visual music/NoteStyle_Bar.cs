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
	public struct BarMeshVertex
	{
		public Vector2 pos;
		public BarMeshVertex(float x, float y)
		{
			pos = new Vector2();
			pos.X = x;
			pos.Y = y;
		}
	}

	public struct BarInstanceVertex
	{
		public Vector4 destRect;
		public Vector4 srcRect;
		public Color color;
		public BarInstanceVertex(Vector4 _destRect, Vector4 _srcRect, Color _color)
		{
			destRect = _destRect;
			srcRect = _srcRect;
			color = _color;
		}
	}

	[Serializable()]
	public class NoteStyle_Bar : NoteStyle
	{
		static VertexBuffer meshVb;
		static DynamicVertexBuffer instanceVb;
		static VertexDeclaration meshVertDecl;
		static VertexDeclaration instanceVertDecl;
		static BarInstanceVertex[] instanceVerts = new BarInstanceVertex[NumDynamicVerts];
		static IndexBuffer indexBuf;
		static VertexBufferBinding[] vbBindings = new VertexBufferBinding[2];

		public NoteStyle_Bar()
		{
			styleType = NoteStyleEnum.Bar;
		}
		public NoteStyle_Bar(TrackProps tprops)
			: base(tprops)
		{
			styleType = NoteStyleEnum.Bar;
		}
		public NoteStyle_Bar(SerializationInfo info, StreamingContext ctxt)
			: base(info, ctxt)
		{
		}
		override public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			base.GetObjectData(info, ctxt);
		}
		override public void loadFx()
		{
			fx = songPanel.Content.Load<Effect>("Bar");
		}
		public override void drawTrack(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, bool selectingRegion, TrackProps texTrackProps)
		{
			base.drawTrack(midiTrack, songDrawProps, trackProps, globalTrackProps, selectingRegion, texTrackProps);
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			List<Midi.Note> noteList = midiTrack.Notes;
			if (noteList.Count == 0)
				return;

			//songPanel.SpriteBatch.Begin(SpriteSortMode.Deferred, songPanel.BlendState, texTrackProps.TexProps.SamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			for (int n = 0; n < noteList.Count; n++)
			{
				Midi.Note note = noteList[n], nextNote;
				if (note.start > songDrawProps.song.SongLengthT) //only if audio ends before the notes end
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
				//fx.Parameters["Color"].SetValue(color.ToVector4());
				instanceVerts[n].color = color;
				fx.Parameters["Texture"].SetValue(texture);
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
							srcRect.X = (int)(srcRect.X * vTexelsPerPixel);
							srcRect.Width = (int)(destRect.Width * vTexelsPerPixel);
						}
						else if (!texTrackProps.TexProps.UTile && texTrackProps.TexProps.VTile)
						{
							srcRect.Y = (int)(srcRect.Y * uTexelsPerPixel);
							srcRect.Height = (int)(destRect.Height * uTexelsPerPixel);
						}
					}
					Vector2 texScroll = songDrawProps.songPosS * texTrackProps.TexProps.Scroll;
					srcRect.X -= (int)(texScroll.X * texture.Width);
					srcRect.Y -= (int)(texScroll.Y * texture.Height);
				}
				instanceVerts[n].destRect = new Vector4(destRect.X, destRect.Y, destRect.Width, destRect.Height);
				instanceVerts[n].srcRect = new Vector4(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height);
				instanceVerts[n].srcRect.X /= texture.Width;
				instanceVerts[n].srcRect.Z /= texture.Width;
				instanceVerts[n].srcRect.Y /= texture.Height;
				instanceVerts[n].srcRect.W /= texture.Height;
				//songPanel.SpriteBatch.Draw(texture, destRect, srcRect,color);
			}
			//songPanel.SpriteBatch.End();
			instanceVb.SetData(instanceVerts, 0, noteList.Count, SetDataOptions.Discard);
			songPanel.GraphicsDevice.SetVertexBuffers(vbBindings);
			songPanel.GraphicsDevice.Indices = indexBuf;
			fx.CurrentTechnique = fx.Techniques["Technique1"];
			fx.CurrentTechnique.Passes["Pass1"].Apply();
			songPanel.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, noteList.Count);
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

		public static void sInit()
		{
			//Mesh vb
			meshVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
			meshVb = new VertexBuffer(songPanel.GraphicsDevice, meshVertDecl, 4, BufferUsage.WriteOnly);
			//Instance vb
			instanceVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1), new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0), new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0));
			instanceVb = new DynamicVertexBuffer(songPanel.GraphicsDevice, instanceVertDecl, NumDynamicVerts, BufferUsage.WriteOnly);
			BarMeshVertex[] meshVerts =
			{
				new BarMeshVertex(0,0),
				new BarMeshVertex(1,0),
				new BarMeshVertex(0,1),
				new BarMeshVertex(1,1)
			};
			meshVb.SetData(meshVerts);
			//Index buffer
			indexBuf = new IndexBuffer(songPanel.GraphicsDevice, typeof(short), 4, BufferUsage.WriteOnly);
			short[] indices = { 0, 1, 2, 3 };
			indexBuf.SetData(indices);
			//Bindings
			vbBindings[0] = new VertexBufferBinding(meshVb);
			vbBindings[1] = new VertexBufferBinding(instanceVb, 0, 1);
		}
		//public override void draw(NoteDrawProps drawProps, Color color, Texture2D texture, int pass)
		//{
		//if (texture == null)
		//    texture = textures[index];
		//songPanel.SpriteBatch.Draw(texture, new Rectangle(drawProps.x1, drawProps.y - drawProps.noteHeight / 2, drawProps.x2 - drawProps.x1 + 1, drawProps.noteHeight), color);
		//}
	}
}
