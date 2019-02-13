using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using System.Runtime.Serialization;
//using Microsoft.Xna.Framework.Input;

using WinKeys = System.Windows.Forms.Keys;
using GdiPoint = System.Drawing.Point;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Visual_Music
{
	//using XnaCubeMapFace = Microsoft.Xna.Framework.Graphics.CubeMapFace;
	[Serializable]
	public class Camera : ISerializable
	{
		public BoundingFrustum Frustum => new BoundingFrustum(VpMat);
		public int Eye { get; set; } = 0;   //Stereoscopic rendering: -1 = left, 0 = center(monoscopic), 1 = right
		public float EyeOffset { get; set; } = 0.02f;   //1 = 100% of viewport width, ie. 0.5 will put the center at the edge of an eye's view.
		public bool InvertY { get; set; } = false;
		public int CubeMapFace { get; set; } = -1; //-1 = normal rendering
		public float Fov { get; set; } = (float)Math.PI / 4.0f;
		Vector3 pos = new Vector3();
		public Vector3 Pos
		{
			get => pos;
			set
			{
				if (!pos.Equals(value))
				{
					if (SongPanel != null)
						SongPanel.Invalidate();
					pos = value;
					SpatialChanged?.Invoke();
				}
			}
		}
		Quaternion orientation = Quaternion.Identity;
		public Quaternion Orientation
		{
			get => orientation;
			set
			{
				if (!orientation.Equals(value))
				{
					if(SongPanel != null)
						SongPanel.Invalidate();
					orientation = value;
					SpatialChanged?.Invoke();
				}
			}
		}
		Matrix NonCubeRotMat => Matrix.CreateFromQuaternion(Orientation);

		Action spatialChanged;
		public Action SpatialChanged
		{
			get => spatialChanged;
			set
			{
				if (value != null)
					value();
				spatialChanged = value;
			}
		}

		public Vector3 ViewportPos => getViewportPos(pos);

		Vector3 moveVel = new Vector3();
		Vector3 rotVel = new Vector3();
		
		const float rotSpeed = 0.5f;
		const float moveSpeed = 0.5f;

		
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
				newPos = getViewportPos(newPos);
				Matrix transMat = Matrix.CreateTranslation(newPos);
				return Matrix.Invert(RotMat * transMat);
			}
		}
		float xyRatio;
		public float XYRatio
		{
			get => xyRatio;
			set
			{
				xyRatio = value;
				viewportSize = new Vector2(1, 1 / xyRatio); 
			}
		}
		Vector2 viewportSize;
		public Vector2 ViewportSize
		{
			get => viewportSize;
			set
			{
				viewportSize = value;
				xyRatio = viewportSize.X / viewportSize.Y;
			}
		}
		public Matrix ProjMat
		{
			get
			{
				float fov, ratio;
				if (CubeMapFace >= 0)
				{
					fov = (float)Math.PI / 2.0f;
					ratio = 1;
				}
				else
				{
					fov = Fov;
					ratio = XYRatio;
				}

				Matrix mat = Matrix.CreatePerspectiveFieldOfView(fov, ratio, 0.0001f, 1000);
				if (InvertY)
					mat.M22 *= -1;
				return mat;
			}
		}

		public Matrix VpMat => ViewMat * ProjMat;
		public SongPanel SongPanel { get; set; }

		public bool MouseRot { get; set; } = false;

		//Methods/////////////////////////////////
		public Camera(SongPanel spanel = null)
		{
			SongPanel = spanel;
			XYRatio = 16.0f / 9;
			Vector3 newPos = pos;
			newPos.Z = Math.Abs(ProjMat.M11) / 2;
			Pos = newPos;
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
					Pos = (Vector3)entry.Value;
				else if (entry.Name == "angles")
				{
					Vector3 angles = (Vector3)entry.Value;
					Orientation = Quaternion.CreateFromYawPitchRoll(angles.Y, angles.X, angles.Z);
				}
				else if (entry.Name == "camOrientation" || entry.Name == "orientation")
					Orientation = (Quaternion)entry.Value;
				else if (entry.Name == "viewportSize")
					ViewportSize = (Vector2)entry.Value;
			}
		}
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("fov", Fov);
			info.AddValue("pos", pos);
			info.AddValue("orientation", orientation);
			info.AddValue("viewportSize", ViewportSize);
		}

		public void update(double deltaTime)
		{
			Vector3 mouseRotVel = new Vector3();
			if (MouseRot)
			{
				if (SongPanel.LeftMbPressed)
					mouseRotVel.Z = rotSpeed;
				if (SongPanel.RightMbPressed)
					mouseRotVel.Z = -rotSpeed;
			}
			Pos += Vector3.Transform(moveVel, RotMat) * (float)deltaTime;
			Vector3 scaledRotVel = (rotVel + mouseRotVel) * (float)deltaTime;
			Orientation = Orientation * Quaternion.CreateFromYawPitchRoll(scaledRotVel.Y, scaledRotVel.X, scaledRotVel.Z);
		}

		public bool control(WinKeys key, bool isKeyDown)
		{
			float startOrStop = isKeyDown ? 1 : 0;
			bool keyMatch = false;
			if (key == WinKeys.Q)
			{
				rotVel.Y = rotSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.E)
			{
				rotVel.Y = -rotSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.W)
			{
				moveVel = Vector3.Forward * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.S)
			{
				moveVel = -Vector3.Forward * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.A)
			{
				moveVel = Vector3.Left * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.D)
			{
				moveVel = -Vector3.Left * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.R)
			{
				moveVel = Vector3.Up * moveSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.F)
			{
				moveVel = -Vector3.Up * moveSpeed * startOrStop;
				keyMatch = true;
			}

			if (key == WinKeys.X)
			{
				rotVel.Z = rotSpeed * startOrStop;
				keyMatch = true;
			}
			if (key == WinKeys.C)
			{
				rotVel.Z = -rotSpeed * startOrStop;
				keyMatch = true;
			}

			/*if (key == WinKeys.ShiftKey)
				shiftPressed = isKeyDown;
			if (key == WinKeys.ControlKey)
				ctrlPressed = isKeyDown;*/
			if (key == WinKeys.CapsLock && isKeyDown)
			{
				MouseRot = !MouseRot;
				SongPanel.NormMouseX = SongPanel.NormMouseY = 0;
				Cursor.Position = SongPanel.PointToScreen(new GdiPoint(SongPanel.ClientRectangle.Width / 2, SongPanel.ClientRectangle.Height / 2));
			}

			return keyMatch;
		}

		public Vector3 getViewportPos(Vector3 _pos)
		{
			return _pos * ViewportSize.X;
		}

		internal void ApplyMouseRot(float x, float y)
		{
			Orientation = Orientation * Quaternion.CreateFromYawPitchRoll(-x, -y, 0);
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
