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
		public BarInstanceVertex(Vector4 _destRect, Vector4 _srcRect, Color _color)
		{
			destRect = _destRect;
			srcRect = _srcRect;
		}
	}

	[Serializable()]
	public class NoteStyle_Bar : NoteStyle
	{
		static VertexBuffer meshVb;
		static VertexDeclaration meshVertDecl;
		static VertexDeclaration instanceVertDecl;
		const int MaxInstances = 30000;
		static BarInstanceVertex[] instanceVerts = new BarInstanceVertex[MaxInstances];
		static IndexBuffer indexBuf;
		static VertexBufferBinding meshBinding;

		public NoteStyle_Bar()
		{
			styleType = NoteStyleType.Bar;
		}
		public NoteStyle_Bar(TrackProps tprops)
			: base(tprops)
		{
			styleType = NoteStyleType.Bar;
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

		public override void createGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
		{
			BarGeo barGeo = new BarGeo();
			geo = barGeo;
			List<Midi.Note> noteList = midiTrack.Notes;
			if (noteList.Count == 0)
				return;

			float halfNoteHeight = Project.NoteHeight / 2;
			int instanceIndex = 0;
			float songPosP = Project.SongPosP;
			
			for (int n = 0; n < noteList.Count; n++)
			{
				Midi.Note note = noteList[n];
				if (note.start > Project.Notes.SongLengthT) //only if audio ends before the notes end
					continue;
				Vector2 noteStart = Project.getScreenPos(note.start, note.pitch);
				Vector2 noteEnd = Project.getScreenPos(note.stop, note.pitch);

				//Create bounding boxes
				Vector3 boxMin = new Vector3(noteStart.X, noteStart.Y -  halfNoteHeight, 0);
				Vector3 boxMax = new Vector3(noteEnd.X, noteEnd.Y + halfNoteHeight, 0);
				geo.bboxes.Add(new BoundingBox(boxMin, boxMax));

				//Create inctance data
				Vector2 topLeft_world = new Vector2(noteStart.X, noteStart.Y - halfNoteHeight);
				Vector2 size_world = new Vector2(noteEnd.X - noteStart.X + 0.001f, halfNoteHeight * 2 - 0.001f);
				Vector2 topLeft_tex = topLeft_world;
				Vector2 size_tex = size_world;

				Texture2D texture = texMaterial.TexProps.Texture;
				if (texture != null)
				{
					Vector2 texSize = new Vector2(texture.Width, texture.Height);
					calcRectTexCoords(out topLeft_tex, out size_tex, texSize, topLeft_world, size_world, texMaterial);
				}
				instanceVerts[instanceIndex].destRect = new Vector4(topLeft_world.X, topLeft_world.Y, size_world.X, size_world.Y);
				instanceVerts[instanceIndex].srcRect = new Vector4(topLeft_tex.X, topLeft_tex.Y, size_tex.X, size_tex.Y);
				if (++instanceIndex >= MaxInstances - 1)
					createVb(ref instanceIndex, barGeo);
			}
			if (instanceIndex > 0)
				createVb(ref instanceIndex, barGeo);
		}

		void createVb(ref int count, BarGeo geo)
		{
			VertexBuffer vb = new VertexBuffer(songPanel.GraphicsDevice, instanceVertDecl, count, BufferUsage.WriteOnly);
			vb.SetData(instanceVerts, 0, count);
			geo.instanceBindingList.Add(new VertexBufferBinding(vb, 0, 1));
			count = 0;
		}

		public override void drawGeoChunk(Geo geo)
		{
			BarGeo barGeo = (BarGeo)geo;
			songPanel.GraphicsDevice.Indices = indexBuf;
			fx.CurrentTechnique = fx.Techniques["Technique1"];
			fx.CurrentTechnique.Passes["Pass1"].Apply();
			
			foreach (var instanceBinding in barGeo.instanceBindingList)
			{
				songPanel.GraphicsDevice.SetVertexBuffers(meshBinding, instanceBinding);
				songPanel.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, instanceBinding.VertexBuffer.VertexCount);
			}
		}

		public override void drawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
		{
			float songPosP;
			base.drawTrack(midiTrack, trackProps, texMaterial, out songPosP);
			trackProps.TrackView.ocTree.drawGeo(Project.Camera);
		}
				
		public static void sInit()
		{
			//Mesh vb
			meshVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
			meshVb = new VertexBuffer(songPanel.GraphicsDevice, meshVertDecl, 4, BufferUsage.WriteOnly);
			//Instance vb
			instanceVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1), new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0));
			//instanceVb = new DynamicVertexBuffer(songPanel.GraphicsDevice, instanceVertDecl, NumDynamicVerts, BufferUsage.WriteOnly);
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
			meshBinding = new VertexBufferBinding(meshVb);
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
		public List<VertexBufferBinding> instanceBindingList = new List<VertexBufferBinding>();
		public override void Dispose()
		{
			foreach (var vb in instanceBindingList)
				vb.VertexBuffer.Dispose();
		}
	}
}
