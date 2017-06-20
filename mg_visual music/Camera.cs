using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace Visual_Music
{
	static class Camera
	{
		public static float Fov { get; set; } = (float)Math.PI / 4.0f;
		static Vector3 pos = new Vector3();
		public static Vector3 Pos { get => pos; set { pos = value; if (SongPanel != null) SongPanel.Invalidate(); } }
		static Vector3 angles = new Vector3();
		public static Vector3 Angles { get => angles; set { angles = value; if (SongPanel != null) SongPanel.Invalidate(); } }
		static Vector3 moveVel = new Vector3();
		static Vector3 rotVel = new Vector3();
		const float rotSpeed = 0.2f;
		const float moveSpeed = 0.5f;

		static Matrix RotMat { get { return Matrix.CreateRotationY(angles.Y); } }
		public static Matrix ViewMat
		{
			get
			{
				Matrix transMat = Matrix.CreateTranslation(pos);
				return Matrix.Invert(RotMat * transMat);
			}
		}
		public static Vector2 ViewPortSize { get => new Vector2((float)SongPanel.ClientRectangle.Width, (float)SongPanel.ClientRectangle.Height); }
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
		public static Matrix ProjMat
		{
			get
			{
				return Matrix.CreatePerspectiveFieldOfView(Fov, 1/*ViewPortSize.X / ViewPortSize.Y*/, 0.001f, 100000);
			}
		}

		public static Matrix VpMat
		{
			get
			{
				return ViewPortMat * ViewMat * ProjMat;
			}
		}
		public static SongPanel SongPanel { get; set; }

		public  static void update(float deltaTime)
		{
			pos += Vector3.Transform(moveVel, RotMat) * deltaTime;
			Vector3 oldAngles = Angles;
			Angles += rotVel * deltaTime;
			float Pi2 = (float)Math.PI * 2;
			if (oldAngles.Y < Pi2 && angles.Y >= Pi2)
				angles.Y -= Pi2;
			else if (oldAngles.Y > 0 && angles.Y <= 0)
				angles.Y += Pi2;
		}

		public static void control(Keys key, bool isKeyDown)
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
		public static void reset()
		{
			Pos = new Vector3(0, 0, 0);
			Angles = new Vector3(0, 0, 0);
			Fov = (float)Math.PI / 4.0f;
		}
	}
}
