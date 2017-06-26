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
	using XnaCubeMapFace = Microsoft.Xna.Framework.Graphics.CubeMapFace;
	[Serializable]
	public class Camera : ISerializable
	{
		public bool InvertY { get; set; } = false;
		public int CubeMapFace { get; set; } = -1; //-1 = normal rendering
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

		Matrix RotMat
		{
			get
			{
				Matrix rot = Matrix.CreateRotationY(angles.Y);
				if (CubeMapFace < 0)
					return rot;

				Vector3 angleOffsets = new Vector3();
				float rot90 = (float)Math.PI / 2.0f;
				XnaCubeMapFace face = (XnaCubeMapFace)Enum.ToObject(typeof(XnaCubeMapFace), CubeMapFace);
								
				switch (CubeMapFace)
				{
					case 0:
						angleOffsets.Y = 0;
						break;
					case 1:
						angleOffsets.Y = 2 * rot90;
						break;
					case 2:
						angleOffsets.X = rot90;
						break;
					case 3:
						angleOffsets.X = -rot90;
						break;
					case 4:
						angleOffsets.Y = rot90;
						break;
					case 5:
						angleOffsets.Y = -rot90;
						break;
				}
				return rot * Matrix.CreateFromYawPitchRoll(angleOffsets.Y, angleOffsets.X, angleOffsets.Z);
			}
		}
		public Matrix ViewMat
		{
			get
			{
				Matrix transMat = Matrix.CreateTranslation(pos);
				return Matrix.Invert(RotMat * transMat);
			}
		}
		public Vector2 ViewPortSize
		{
			get
			{
				//if (CubeMapFace < 0)
					return new Vector2((float)SongPanel.GraphicsDevice.Viewport.Width, (float)SongPanel.GraphicsDevice.Viewport.Height);
				//else
					//return new Vector2((float)SongPanel.GraphicsDevice.Viewport.Width, (float)SongPanel.GraphicsDevice.Viewport.Height);
			}
		}
		Matrix ViewPortMat
		{
			get
			{
				return new Matrix(2 / ViewPortSize.X, 0, 0, 0,
											0, -2 / ViewPortSize.Y * (InvertY ? -1 : 1), 0, 0,
											0, 0, 1, 0,
											0, 0, 0, 1);
			}
		}
		public Matrix ProjMat
		{
			get
			{
				float fov = CubeMapFace >= 0 ? (float)Math.PI / 2.0f : Fov;
				return Matrix.CreatePerspectiveFieldOfView(fov, 1/*ViewPortSize.X / ViewPortSize.Y*/, 0.001f, 100000);
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
			if (SongPanel != null)
				SongPanel.Invalidate();
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

		public bool control(Keys key, bool isKeyDown)
		{
			float startOrStop = isKeyDown ? 1 : 0;
			bool keyMatch = false;
			if (key == Keys.Q)
			{
				rotVel.Y = rotSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.E)
			{
				rotVel.Y = -rotSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.W)
			{
				moveVel = Vector3.Forward * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.S)
			{
				moveVel = -Vector3.Forward * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.A)
			{
				moveVel = Vector3.Left * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.D)
			{
				moveVel = -Vector3.Left * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.R)
			{
				moveVel = Vector3.Up * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.F)
			{
				moveVel = -Vector3.Up * moveSpeed * startOrStop;
				keyMatch = true;
			}
			return keyMatch;
		}
		
		
		//public void reset()
		//{
		//	Pos = new Vector3(0, 0, 0);
		//	Angles = new Vector3(0, 0, 0);
		//	Fov = (float)Math.PI / 4.0f;
		//}
		//public bool intersectsFrustum(BoundingSphere sphere)
		//{
		//	return new BoundingFrustum(VpMat).Intersects(sphere);
		//}
	}
}
