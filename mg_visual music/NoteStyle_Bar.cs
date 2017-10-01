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
using Midi;

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

		public override void createGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, TrackProps texTrackProps)
		{
			geo = new BarGeo();
			List<Midi.Note> noteList = midiTrack.Notes;
			if (noteList.Count == 0)
				return;

			float halfNoteHeight = Project.NoteHeight / 2;
			for (int n = 0; n < noteList.Count; n++)
			{
				Midi.Note note = noteList[n];
				if (note.start > Project.Notes.SongLengthT) //only if audio ends before the notes end
					continue;
				Vector2 noteStart = Project.getScreenPosF(note.start, note.pitch);
				Vector2 noteEnd = Project.getScreenPosF(note.stop, note.pitch);
				float z = fx.Parameters["PosOffset"].GetValueVector3().Z;
				
				Vector3 boxMin = new Vector3(noteStart.X, noteStart.Y -  halfNoteHeight, z);
				Vector3 boxMax = new Vector3(noteEnd.X, noteEnd.Y + halfNoteHeight, z);
				geo.bboxes.Add(new BoundingBox(boxMin, boxMax));
			}
		}

		public override void drawGeoChunk(Geo geo)
		{

		}

		public override void drawTrack(Midi.Track midiTrack, TrackProps trackProps, TrackProps texTrackProps)
		{
			float songPosP;
			base.drawTrack(midiTrack, trackProps, texTrackProps, out songPosP);
			//List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
			List<Midi.Note> noteList = midiTrack.Notes;
			if (noteList.Count == 0)
				return;

			float halfNoteHeight = Project.NoteHeight / 2;
			for (int n = 0; n < noteList.Count; n++)
			{
				Midi.Note note = noteList[n];
				if (note.start > Project.Notes.SongLengthT) //only if audio ends before the notes end
					continue;

				Vector2 noteStart = Project.getScreenPosF(note.start, note.pitch);
				Vector2 noteEnd = Project.getScreenPosF(note.stop, note.pitch);

				Color color;
				Texture2D texture;

				getMaterial(trackProps, (int)(noteStart.X - songPosP), (int)(noteEnd.X - songPosP), out color, out texture);
				//fx.Parameters["Color"].SetValue(color.ToVector4());
				instanceVerts[n].color = color;
				fx.Parameters["Texture"].SetValue(texture);
				//Vector4 destRect = new Vector4(noteStart.X, noteStart.Y - songDrawProps.noteHeight / 2, noteEnd.X - noteStart.X + 1, songDrawProps.noteHeight - 1);
				//Vector4 srcRect = destRect;
				Vector2 topLeft_world = new Vector2(noteStart.X, noteStart.Y - halfNoteHeight);
				Vector2 size_world = new Vector2(noteEnd.X - noteStart.X + 0.001f, halfNoteHeight * 2 - 0.001f);
				Vector2 topLeft_tex = topLeft_world;
				Vector2 size_tex = size_world;

				if (texture != null) //Unnecessary because texture is never null. Can revert to default 1x1 white pixel.
				{
					Vector2 texSize = new Vector2(texture.Width, texture.Height);
					calcRectTexCoords(out topLeft_tex, out size_tex, texSize, topLeft_world, size_world, texTrackProps);
				}
				instanceVerts[n].destRect = new Vector4(topLeft_world.X, topLeft_world.Y, size_world.X, size_world.Y);
				instanceVerts[n].srcRect = new Vector4(topLeft_tex.X, topLeft_tex.Y, size_tex.X, size_tex.Y);
				//instanceVerts[n].srcRect.X /= texture.Width;
				//instanceVerts[n].srcRect.Z /= texture.Width;
				//instanceVerts[n].srcRect.Y /= texture.Height;
				//instanceVerts[n].srcRect.W /= texture.Height;
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

		//override public void createOcTree(Vector3 minPos, Vector3 size, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps globalTrackProps, TrackProps trackProps, TrackProps texTrackProps)
		//{

		//}
		
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

	public class BarGeo : Geo
	{
		public override void Dispose()
		{
			
		}
	}
}
