using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace Visual_Music
{
	[Serializable]
	public class Camera : ISerializable
	{
		public float Fov { get; set; } = (float)Math.PI / 4.0f;
		Vector3 pos = new Vector3();
		public Vector3 Pos { get => pos;
			set
			{
				if (!pos.Equals(value))
				{
					SongPanel.Invalidate();
					pos = value;
				}
			}
		}
				
		Vector3 angles = new Vector3();
		public Vector3 Angles { get => angles;
			set
			{
				if (!angles.Equals(value))
				{
					SongPanel.Invalidate();
					angles = value;
				}
			}
		}
		
				
		Vector3 moveVel = new Vector3();
		Vector3 rotVel = new Vector3();
		const float rotSpeed = 0.2f;
		const float moveSpeed = 0.5f;

		Matrix RotMat { get { return Matrix.CreateRotationY(angles.Y); } }
		public Matrix ViewMat
		{
			get
			{
				Matrix transMat = Matrix.CreateTranslation(pos);
				return Matrix.Invert(RotMat * transMat);
			}
		}
		public Vector2 ViewPortSize { get => new Vector2((float)SongPanel.ClientRectangle.Width, (float)SongPanel.ClientRectangle.Height); }
		Matrix ViewPortMat
		{
			get
			{
				return new Matrix(2 / ViewPortSize.X, 0, 0, 0,
											0, -2 / ViewPortSize.Y, 0, 0,
											0, 0, 1, 0,
											0, 0, 0, 1);
			}
		}
		public Matrix ProjMat
		{
			get
			{
				return Matrix.CreatePerspectiveFieldOfView(Fov, 1/*ViewPortSize.X / ViewPortSize.Y*/, 0.001f, 100000);
			}
		}

		public Matrix VpMat
		{
			get
			{
				return ViewPortMat * ViewMat * ProjMat;
			}
		}
		public SongPanel SongPanel { get; set; }


		//Methods/////////////////////////////////
		public Camera(SongPanel spanel = null)
		{
			SongPanel = spanel;
		}

		public Camera(SerializationInfo info, StreamingContext ctxt)
		{
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "fov")
					Fov = (float)entry.Value;
				else if (entry.Name == "pos")
					pos = (Vector3)entry.Value;
				else if (entry.Name == "angles")
					angles = (Vector3)entry.Value;
			}
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("fov", Fov);
			info.AddValue("pos", pos);
			info.AddValue("angles", angles);
		}

		public void update(float deltaTime)
		{
			Pos += Vector3.Transform(moveVel, RotMat) * deltaTime;
			Vector3 oldAngles = Angles;
			Angles += rotVel * deltaTime;
			float Pi2 = (float)Math.PI * 2;
			if (oldAngles.Y < Pi2 && angles.Y >= Pi2)
				angles.Y -= Pi2;
			else if (oldAngles.Y > 0 && angles.Y <= 0)
				angles.Y += Pi2;
		}

		public void control(Keys key, bool isKeyDown)
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
		public void reset()
		{
			Pos = new Vector3(0, 0, 0);
			Angles = new Vector3(0, 0, 0);
			Fov = (float)Math.PI / 4.0f;
		}
	}
}
