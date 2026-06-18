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
        DynamicVertexBuffer _vb;
        GraphicsDevice _graphicsDevice;
        PosVertex[] _verts = new PosVertex[4];
        PosVertex[] _transformedVerts = new PosVertex[4];
        Vector2 _scale = new Vector2(2, -2);
        Vector2 _offset = new Vector2(-1, 1);
        public ScreenQuad(GraphicsDevice gd)
        {
            _graphicsDevice = gd;
            VertexDeclaration vertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
            _vb = new DynamicVertexBuffer(_graphicsDevice, vertDecl, 4, BufferUsage.WriteOnly);
        }

        public void Draw()
        {
            Vector2 pos = Pos * _scale + _offset;
            Vector2 size = Size * _scale;
            for (int i = 0; i < 4; i++)
                _verts[i].pos = pos;
            _verts[1].pos.X += size.X;
            _verts[2].pos.Y += size.Y;
            _verts[3].pos += size;
            _vb.SetData(_verts, 0, 4, SetDataOptions.Discard);
            _graphicsDevice.SetVertexBuffer(_vb);
            _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }
}
