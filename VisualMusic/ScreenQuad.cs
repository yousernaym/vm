using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VisualMusic
{
    public struct PosVertex
    {
        public Vector2 pos;
        public PosVertex(float x, float y)
        {
            pos = new Vector2();
            pos.X = x;
            pos.Y = y;
        }
    }
    class ScreenQuad
    {
        public Vector2 Pos { get; set; } = new Vector2(0, 0);
        public Vector2 Size { get; set; } = new Vector2(1, 1);
        DynamicVertexBuffer vb;
        GraphicsDevice graphicsDevice;
        PosVertex[] verts = new PosVertex[4];
        PosVertex[] transformedVerts = new PosVertex[4];
        Vector2 scale = new Vector2(2, -2);
        Vector2 offset = new Vector2(-1, 1);
        public ScreenQuad(GraphicsDevice gd)
        {
            graphicsDevice = gd;
            VertexDeclaration vertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
            vb = new DynamicVertexBuffer(graphicsDevice, vertDecl, 4, BufferUsage.WriteOnly);
        }

        public void draw()
        {
            Vector2 pos = Pos * scale + offset;
            Vector2 size = Size * scale;
            for (int i = 0; i < 4; i++)
                verts[i].pos = pos;
            verts[1].pos.X += size.X;
            verts[2].pos.Y += size.Y;
            verts[3].pos += size;
            vb.SetData(verts, 0, 4, SetDataOptions.Discard);
            graphicsDevice.SetVertexBuffer(vb);
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }
}
