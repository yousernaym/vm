using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Visual_Music
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
	class Quad
	{
		VertexBuffer vb;
		SongPanel songPanel;
		public Quad(SongPanel _songPanel)
		{
			songPanel = _songPanel;
			VertexDeclaration vertDecl = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
			vb = new VertexBuffer(songPanel.GraphicsDevice, vertDecl, 4, BufferUsage.WriteOnly);
			PosVertex[] verts =
			{
				new PosVertex(-1,-1),
				new PosVertex(1,-1),
				new PosVertex(-1,1),
				new PosVertex(1,1)
			};
			vb.SetData(verts);
		}
		public void draw()
		{
			songPanel.GraphicsDevice.SetVertexBuffer(vb);
			songPanel.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		}
	}
}
