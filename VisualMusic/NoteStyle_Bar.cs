using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VisualMusic
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
        public Vector4 srcRect2;
        public BarInstanceVertex(Vector4 _destRect, Vector4 _srcRect, Color _color)
        {
            destRect = _destRect;
            srcRect = _srcRect;
            srcRect2 = _srcRect;
        }
    }

    [Serializable()]
    public class NoteStyle_Bar : NoteStyle
    {
        static VertexBuffer s_meshVb;
        static VertexDeclaration s_meshVertDecl;
        static VertexDeclaration s_instanceVertDecl;
        const int MaxInstances = 30000;
        static BarInstanceVertex[] s_instanceVerts = new BarInstanceVertex[MaxInstances];
        static IndexBuffer s_indexBuf;
        static VertexBufferBinding s_meshBinding;

        public NoteStyle_Bar()
        {
            _styleType = NoteStyleType.Bar;
        }
        public NoteStyle_Bar(TrackProps tprops)
            : base(tprops)
        {
            _styleType = NoteStyleType.Bar;
        }
        public NoteStyle_Bar(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }
        override public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            base.GetObjectData(info, ctxt);
        }

        override public void LoadFx()
        {
            if (Content == null)
                throw new InvalidOperationException("NoteStyle.SetContent must run before LoadFx.");
            _fx = Content.Load<Effect>("Bar");
        }

        public override void CreateGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
        {
            BarGeo barGeo = new BarGeo();
            geo = barGeo;
            List<Midi.Note> noteList = midiTrack.Notes;
            if (noteList.Count == 0)
                return;

            float halfNoteHeight = Project.Props.NoteHeight / 2;
            int instanceIndex = 0;

            for (int n = 0; n < noteList.Count; n++)
            {
                Midi.Note note = noteList[n];
                if (note.start > Project.Notes.SongLengthT) //only if audio ends before the notes end
                    continue;
                Vector2 noteStart = Project.GetScreenPos(note.start, note.pitch);
                Vector2 noteEnd = Project.GetScreenPos(note.stop, note.pitch);

                //Create bounding boxes
                Vector3 boxMin = new Vector3(noteStart.X, noteStart.Y - halfNoteHeight, 0);
                Vector3 boxMax = new Vector3(noteEnd.X, noteEnd.Y + halfNoteHeight, 0);
                geo.bboxes.Add(new BoundingBoxEx(boxMin, boxMax));

                //Create inctance data
                Vector2 topLeft_world = new Vector2(noteStart.X, noteStart.Y - halfNoteHeight);
                Vector2 size_world = new Vector2(noteEnd.X - noteStart.X, halfNoteHeight * 2 - 0.001f);
                Vector2 topLeft_tex = topLeft_world;
                Vector2 size_tex = size_world;

                Texture2D texture = texMaterial.TexProps.Texture ?? texMaterial.TexProps.TransitionTexture;
                Texture2D texture2 = texMaterial.TexProps.TransitionTexture;
                Vector2 topLeft_tex2 = topLeft_tex;
                Vector2 size_tex2 = size_tex;
                if (texture != null)
                {
                    Vector2 texSize = new Vector2(texture.Width, texture.Height);
                    CalcRectTexCoords(out topLeft_tex, out size_tex, texSize, topLeft_world, size_world, texMaterial);
                }
                if (texture2 != null)
                {
                    Vector2 texSize2 = new Vector2(texture2.Width, texture2.Height);
                    CalcRectTexCoords(out topLeft_tex2, out size_tex2, texSize2, topLeft_world, size_world, texMaterial);
                }
                else
                {
                    topLeft_tex2 = topLeft_tex;
                    size_tex2 = size_tex;
                }
                s_instanceVerts[instanceIndex].destRect = new Vector4(topLeft_world.X, topLeft_world.Y, size_world.X, size_world.Y);
                s_instanceVerts[instanceIndex].srcRect = new Vector4(topLeft_tex.X, topLeft_tex.Y, size_tex.X, size_tex.Y);
                s_instanceVerts[instanceIndex].srcRect2 = new Vector4(topLeft_tex2.X, topLeft_tex2.Y, size_tex2.X, size_tex2.Y);
                if (++instanceIndex >= MaxInstances - 1)
                    CreateVb(ref instanceIndex, barGeo);
            }
            if (instanceIndex > 0)
                CreateVb(ref instanceIndex, barGeo);
        }

        void CreateVb(ref int count, BarGeo geo)
        {
            VertexBuffer vb = new VertexBuffer(GraphicsDevice, s_instanceVertDecl, count, BufferUsage.WriteOnly);
            vb.SetData(s_instanceVerts, 0, count);
            geo.instanceBindingList.Add(new VertexBufferBinding(vb, 0, 1));
            count = 0;
        }

        public override void DrawGeoChunk(Geo geo)
        {
            BarGeo barGeo = (BarGeo)geo;
            GraphicsDevice.Indices = s_indexBuf;
            _fx.CurrentTechnique = _fx.Techniques["Technique1"];
            _fx.CurrentTechnique.Passes["Pass1"].Apply();

            foreach (var instanceBinding in barGeo.instanceBindingList)
            {
                GraphicsDevice.SetVertexBuffers(s_meshBinding, instanceBinding);
                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, instanceBinding.VertexBuffer.VertexCount);
            }
        }

        public override void DrawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
        {
            float songPosP;
            base.DrawTrack(midiTrack, trackProps, texMaterial, out songPosP);
            DrawGeoChunk(trackProps.TrackView.Geo);
        }

        public static void SInit()
        {
            //Mesh vb
            s_meshVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
            s_meshVb = new VertexBuffer(GraphicsDevice, s_meshVertDecl, 4, BufferUsage.WriteOnly);
            //Instance vb
            s_instanceVertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1), new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0), new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1));
            //instanceVb = new DynamicVertexBuffer(GraphicsDevice, instanceVertDecl, NumDynamicVerts, BufferUsage.WriteOnly);
            BarMeshVertex[] meshVerts =
            {
                new BarMeshVertex(0,0),
                new BarMeshVertex(1,0),
                new BarMeshVertex(0,1),
                new BarMeshVertex(1,1)
            };
            s_meshVb.SetData(meshVerts);
            //Index buffer
            s_indexBuf = new IndexBuffer(GraphicsDevice, typeof(short), 4, BufferUsage.WriteOnly);
            short[] indices = { 0, 1, 2, 3 };
            s_indexBuf.SetData(indices);
            //Bindings
            s_meshBinding = new VertexBufferBinding(s_meshVb);
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
        protected override void ReleaseResources()
        {
            foreach (var vb in instanceBindingList)
                vb.VertexBuffer.Dispose();
        }
    }
}
