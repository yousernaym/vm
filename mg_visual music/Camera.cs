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
	//using XnaCubeMapFace = Microsoft.Xna.Framework.Graphics.CubeMapFace;
	[Serializable]
	public class Camera : ISerializable
	{
		public int Eye { get; set; } = 0;   //Stereoscopic rendering: -1 = left, 0 = center(monoscopic), 1 = right
		public float EyeOffset { get; set; } = 0.25f;   //1 = 100% of viewport width
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
		
		const float rotSpeed = 0.5f;
		const float moveSpeed = 0.5f;

		Matrix NonCubeRotMat => Matrix.CreateFromYawPitchRoll(angles.Y, angles.X, angles.Z);
		Matrix RotMat
		{
			get
			{
				Matrix rot = NonCubeRotMat;
				if (CubeMapFace <= 0)
					return rot;

				Vector3 angleOffsets = new Vector3();
				float rot90 = (float)Math.PI / 2.0f;
				switch (CubeMapFace)
				{
					case 1:
						angleOffsets.Y = 2 * rot90;
						break;
					case 2:
						angleOffsets.X = rot90;
						angleOffsets.Z = -rot90;
						break;
					case 3:
						angleOffsets.X = -rot90;
						angleOffsets.Z = rot90;
						break;
					case 4:
						angleOffsets.Y = rot90;
						break;
					case 5:
						angleOffsets.Y = -rot90;
						break;
				}
				return Matrix.CreateFromYawPitchRoll(angleOffsets.Y, angleOffsets.X, angleOffsets.Z) * rot;
				
			}
		}
		public Matrix ViewMat
		{
			get
			{
				//Vector3 rotCenter = pos - NonCubeRotMat.Forward * 1;
				//Vector3 newPos = Vector3.Transform(pos - rotCenter, RotMat) + rotCenter;
				Vector3 newPos = pos;
				Vector3 LeftOffset = NonCubeRotMat.Left * EyeOffset;
				if (Eye == -1)
					newPos += LeftOffset;
				else if (Eye == 1)
					newPos -= LeftOffset;
				newPos *= ViewportSize.X;
				Matrix transMat = Matrix.CreateTranslation(newPos);
				return Matrix.Invert(RotMat * transMat);
			}
		}
		public Vector2 ViewportSize => new Vector2(SongPanel.GraphicsDevice.Viewport.Width, SongPanel.GraphicsDevice.Viewport.Height);
		Matrix ViewportMat
		{
			get
			{
				return new Matrix(2 , 0, 0, 0,
											0, -2 , 0, 0,
											0, 0, 1, 0,
											0, 0, 0, 1);
			}
		}
		public Matrix ProjMat
		{
			get
			{
				float fov = CubeMapFace >= 0 ? (float)Math.PI / 2.0f : Fov;
				Matrix mat = Matrix.CreatePerspectiveFieldOfView(fov, ViewportSize.X / ViewportSize.Y, 0.001f, 100000);
				if (InvertY)
					mat.M22 *= -1;
				return mat;
			}
		}

		public Matrix VpMat => ViewportMat * ViewMat * ProjMat;
		
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

			if (key == Keys.X)
			{
				rotVel.Z = rotSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == Keys.C)
			{
				rotVel.Z = -rotSpeed * startOrStop;
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
