using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
//using Microsoft.Xna.Framework.Input;

using WinKeys = System.Windows.Forms.Keys;

namespace VisualMusic
{
    [Serializable]
    public class Camera : ISerializable
    {
        float _controlScale = 0;
        public BoundingFrustum Frustum => new BoundingFrustum(VpMat);
        public int Eye { get; set; } = 0;   //Stereoscopic rendering: -1 = left, 0 = center(monoscopic), 1 = right
        public float EyeOffset { get; set; } = 0.01f;   //1 = 100% of viewport width, ie. 0.5 will put the center at the edge of an eye's view.
        public static bool InvertY { get; set; } = false;
        public int CubeMapFace { get; set; } = -1; //-1 = normal rendering
        public float Fov { get; set; } = (float)Math.PI / 4.0f;
        Vector3 _pos = new Vector3();
        public Vector3 Pos
        {
            get => _pos;
            set
            {
                if (!_pos.Equals(value))
                {
                    if (SongPanel != null)
                        SongPanel.Invalidate();
                    _pos = value;
                }
            }
        }
        Quaternion _orientation = Quaternion.Identity;
        public Quaternion Orientation
        {
            get => _orientation;
            set
            {
                if (!_orientation.Equals(value))
                {
                    if (SongPanel != null)
                        SongPanel.Invalidate();
                    _orientation = value;
                }
            }
        }
        public Matrix NonCubeRotMat => Matrix.CreateFromQuaternion(Orientation);

        public static Action OnUserUpdating;
        public static Action OnUserUpdated;

        public Vector3 ViewportPos => GetViewportPos(_pos);

        Vector3 _rotVel = new Vector3();
        Vector3 _moveVel = new Vector3();
        Vector3 _rotVelAcc = new Vector3();
        Vector3 _moveVelAcc = new Vector3();

        const float rotSpeed = 0.15f;
        const float moveSpeed = 0.5f;
        const float rotSpeedAcc = 0.05f;
        const float moveSpeedAcc = 0.05f;

        public Matrix RotMat
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
                    case 1: //Back
                        angleOffsets.Y = 2 * rot90;
                        break;
                    case 2: //Down
                        angleOffsets.X = -rot90;
                        angleOffsets.Z = rot90;
                        break;
                    case 3: //Up
                        angleOffsets.X = rot90;
                        angleOffsets.Z = -rot90;
                        break;
                    case 4: //Left
                        angleOffsets.Y = rot90;
                        break;
                    case 5: //Right
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
                Vector3 newPos = _pos;
                Vector3 LeftOffset = NonCubeRotMat.Left * EyeOffset;
                if (Eye == -1)
                    newPos += LeftOffset;
                else if (Eye == 1)
                    newPos -= LeftOffset;
                newPos = GetViewportPos(newPos);
                Matrix transMat = Matrix.CreateTranslation(newPos);
                return Matrix.Invert(RotMat * transMat);
            }
        }
        float _xyRatio;
        public float XYRatio
        {
            get => _xyRatio;
            set
            {
                _xyRatio = value;
                _viewportSize = new Vector2(1, 1 / _xyRatio);
            }
        }
        Vector2 _viewportSize;
        public Vector2 ViewportSize
        {
            get => _viewportSize;
            set
            {
                _viewportSize = value;
                _xyRatio = _viewportSize.X / _viewportSize.Y;
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
        public bool RenderingToCubemap => CubeMapFace >= 0;
        public static SongPanel SongPanel => Form1.SongPanel;
        public static bool MouseRot { get; set; } = false;

        //Methods/////////////////////////////////
        public Camera()
        {
            XYRatio = 16.0f / 9;
            Vector3 newPos = _pos;
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
            info.AddValue("pos", _pos);
            info.AddValue("orientation", _orientation);
            info.AddValue("viewportSize", ViewportSize);
        }

        public void Update(double deltaTime)
        {
            Vector3 mouseRotVel = new Vector3();
            if (MouseRot)
            {
                if (SongPanel.LeftMbPressed)
                    mouseRotVel.Z = rotSpeed;
                if (SongPanel.RightMbPressed)
                    mouseRotVel.Z = -rotSpeed;
            }
            Pos += Vector3.Transform(_moveVel, RotMat) * (float)deltaTime;
            Vector3 scaledRotVel = (_rotVel + mouseRotVel) * (float)deltaTime;
            Orientation = Orientation * Quaternion.CreateFromYawPitchRoll(scaledRotVel.Y, scaledRotVel.X, scaledRotVel.Z);
            if (Vector3.Zero != _moveVel || Vector3.Zero != scaledRotVel)
                OnUserUpdating?.Invoke();
        }

        public bool Control(WinKeys key, bool keyDown, WinKeys modifiers)
        {
            bool shifting = modifiers == WinKeys.Shift;
            if (modifiers != 0 && !shifting)
                return false;
            if (key == WinKeys.ShiftKey)
            {
                //shiftPressed = keyDown;
                //MouseRot = keyDown;
                //if (keyDown)
                //{
                //	SongPanel.NormMouseX = SongPanel.NormMouseY = 0;
                //	Cursor.Position = SongPanel.PointToScreen(new GdiPoint(SongPanel.ClientRectangle.Width / 2, SongPanel.ClientRectangle.Height / 2));
                //}
                //else
                //	OnUserUpdated?.Invoke();
            }

            _controlScale = keyDown ? 1 : 0;
            bool keyMatch = false;
            float scaledRotSpeed = rotSpeed * _controlScale;
            float scaledMoveSpeed = moveSpeed * _controlScale;

            //Rotation, requiring shifting
            if (shifting || !keyDown)
            {
                if (key == WinKeys.A)
                {
                    _rotVel.Y = scaledRotSpeed;
                    keyMatch = true;
                }
                if (key == WinKeys.D)
                {
                    _rotVel.Y = -scaledRotSpeed;
                    keyMatch = true;
                }
                if (key == WinKeys.W)
                {
                    _rotVel.X = -scaledRotSpeed;
                    keyMatch = true;
                }
                if (key == WinKeys.S)
                {
                    _rotVel.X = scaledRotSpeed;
                    keyMatch = true;
                }
            }

            //Movement, requiring not shifting
            if (!shifting || !keyDown)
            {
                if (key == WinKeys.W)
                {
                    _moveVel.Z = -scaledMoveSpeed;
                    keyMatch = true;
                }
                if (key == WinKeys.S)
                {
                    _moveVel.Z = scaledMoveSpeed;
                    keyMatch = true;
                }
                if (key == WinKeys.A)
                {
                    _moveVel.X = -scaledMoveSpeed;
                    keyMatch = true;
                }
                if (key == WinKeys.D)
                {
                    _moveVel.X = scaledMoveSpeed;
                    keyMatch = true;
                }
            }

            //Keys exclusively for either movement or rotation, shifting optional
            if (key == WinKeys.R)
            {
                _moveVel.Y = scaledMoveSpeed;
                keyMatch = true;
            }
            if (key == WinKeys.F)
            {
                _moveVel.Y = -scaledMoveSpeed;
                keyMatch = true;
            }
            if (key == WinKeys.Q)
            {
                _rotVel.Z = scaledRotSpeed;
                keyMatch = true;
            }
            if (key == WinKeys.E)
            {
                _rotVel.Z = -scaledRotSpeed;
                keyMatch = true;
            }

            if (keyMatch)
            {
                OnUserUpdating?.Invoke();
                if (!keyDown)
                    OnUserUpdated?.Invoke();
            }
            return keyMatch;
        }

        public Vector3 GetViewportPos(Vector3 _pos)
        {
            return _pos * ViewportSize.X;
        }

        /// <summary>
        /// Applies a mouse-movement rotation to the camera orientation.
        /// When <paramref name="roll"/> is false (default), horizontal movement yaws and
        /// vertical movement pitches.  When <paramref name="roll"/> is true (left button held),
        /// horizontal movement rolls and vertical movement still pitches.
        /// </summary>
        internal void ApplyMouseRot(float x, float y, bool roll = false)
        {
            const float maxStep = 0.03f;
            if (Math.Abs(x) > maxStep) x *= maxStep / Math.Abs(x);
            if (Math.Abs(y) > maxStep) y *= maxStep / Math.Abs(y);
            float yaw  = roll ?  0 : -x;
            float rollZ = roll ? -x :  0;
            Orientation = Orientation * Quaternion.CreateFromYawPitchRoll(yaw, -y, rollZ);
            OnUserUpdating?.Invoke();
        }
    }
}
