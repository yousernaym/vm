using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace Visual_Music
{
	public enum ProjType { Ortho, Perspective };
	static class Camera
	{
		static Vector3 pos = new Vector3();
		static public Vector3 Pos { get => pos; set { pos = value; if (SongPanel != null) SongPanel.Invalidate(); } }
		static Vector3 angles = new Vector3();
		static public Vector3 Angles { get => angles; set { angles = value; if (SongPanel != null) SongPanel.Invalidate(); } }
		static Vector3 moveVel = new Vector3();
		static Vector3 rotVel = new Vector3();
		const float rotSpeed = 0.2f;
		const float moveSpeed = 0.5f;

		static Matrix RotMat { get { return Matrix.CreateRotationY(angles.Y); } }
		static public Matrix ViewMat
		{
			get
			{
				if (ProjType == ProjType.Ortho)
					return Matrix.Identity;
				else
				{
					Matrix transMat = Matrix.CreateTranslation(pos);
					return Matrix.Invert(RotMat * transMat);
					//return Matrix.CreateLookAt(pos, RotMat.Forward, RotMat.Up);
				}
			}
		}
		static public Vector2 ViewPortSize { get => new Vector2((float)SongPanel.ClientRectangle.Width, (float)SongPanel.ClientRectangle.Height); }
		static public ProjType ProjType { get; set; } = ProjType.Ortho;
		static Matrix ViewPortMat
		{
			get
			{
				return new Matrix(2 / ViewPortSize.X, 0, 0, 0,
											0, -2 / ViewPortSize.Y, 0, 0,
											0, 0, 1, 0,
											0, 0, 0, 1);
				
				
			}
		}
		static public Matrix ProjMat
		{
			get
			{
				if (ProjType == ProjType.Ortho)
				{
					Matrix mat = Matrix.Identity;
					mat.M33 = 0;
					return mat;
				}
				else //if (ProjType == ProjType.Perspective)
					return Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f, 1/*ViewPortSize.X / ViewPortSize.Y*/, 0.001f, 100000);
			}
		}

		static public Matrix VpMat
		{
			get
			{
				return ViewPortMat * ViewMat * ProjMat;
			}
		}
		static public SongPanel SongPanel { get; set; }

		static public void update(float deltaTime)
		{
			//Pos += moveVel * Math.Abs(Vector3.Dot(RotMat.Forward, Vector3.Forward)) * deltaTime;
			pos += Vector3.Transform(moveVel, RotMat) * deltaTime;
			Vector3 oldAngles = Angles;
			Angles += rotVel * deltaTime;
			float Pi2 = (float)Math.PI * 2;
			if (oldAngles.Y < Pi2 && angles.Y >= Pi2)
				angles.Y -= Pi2;
			else if (oldAngles.Y > 0 && angles.Y <= 0)
				angles.Y += Pi2;
		}

		static public void control(Keys key, bool isKeyDown)
		{
			float startOrStop = isKeyDown ? 1 : 0;

			if (key == Keys.Q)
				rotVel.Y = rotSpeed * startOrStop;
			if (key == Keys.E)
				rotVel.Y = -rotSpeed * startOrStop;
			if (key == Keys.W)
				moveVel = Vector3.Forward * moveSpeed * startOrStop;
			if (key == Keys.S)
				moveVel = -Vector3.Forward * moveSpeed * startOrStop;
			if (key == Keys.A)
				moveVel = Vector3.Left * moveSpeed * startOrStop;
			if (key == Keys.D)
				moveVel = -Vector3.Left * moveSpeed * startOrStop;
		}
		static public void reset()
		{
			Pos = new Vector3(0, 0, 0);
			Angles = new Vector3(0, 0, 0);
		}
	}
}
