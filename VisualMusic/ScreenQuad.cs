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
        SongPanel songPanel;
        PosVertex[] verts = new PosVertex[4];
        PosVertex[] transformedVerts = new PosVertex[4];
        Vector2 scale = new Vector2(2, -2);
        Vector2 offset = new Vector2(-1, 1);
        public ScreenQuad(SongPanel _songPanel)
        {
            songPanel = _songPanel;
            VertexDeclaration vertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
            vb = new DynamicVertexBuffer(songPanel.GraphicsDevice, vertDecl, 4, BufferUsage.WriteOnly);
            //verts = new PosVertex[]
            //{
            //	new PosVertex(-1,-1),
            //	new PosVertex(1,-1),
            //	new PosVertex(-1,1),
            //	new PosVertex(1,1)
            //};
            //vb.SetData(verts);
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
            songPanel.GraphicsDevice.SetVertexBuffer(vb);
            songPanel.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }
}
