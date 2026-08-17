using Microsoft.Xna.Framework;
using System.Windows.Input;
using Xunit;

namespace VisualMusic.Tests
{
    public class SongRendererCameraInputTests
    {
        [Fact]
        public void Camera_key_up_stops_motion()
        {
            using var _ = CameraCallbackScope.Clear();
            var camera = new Camera();
            Assert.True(camera.Control(Key.W, true, ModifierKeys.None));
            Assert.NotEqual(Vector3.Zero, Step(camera));

            Assert.True(camera.Control(Key.W, false, ModifierKeys.None));
            Assert.Equal(Vector3.Zero, Step(camera));
        }

        [Fact]
        public void Focus_lost_does_not_stop_camera_motion()
        {
            using var _ = CameraCallbackScope.Clear();
            Project.SetDrawHost(new FakeSongDrawHost());
            var renderer = new SongRenderer();
            try
            {
                var project = new Project();
                renderer.ForceProjectReference(project);

                Assert.True(project.Props.Camera.Control(Key.W, true, ModifierKeys.None));
                Assert.NotEqual(Vector3.Zero, Step(project.Props.Camera));

                renderer.HandleFocusLost();
                Assert.NotEqual(Vector3.Zero, Step(project.Props.Camera));
            }
            finally
            {
                renderer.ForceProjectReference(null);
                Project.SetDrawHost(null);
            }
        }

        [Fact]
        public void Duplicate_key_up_after_stop_does_not_raise_updated()
        {
            int updated = 0;
            using var _ = CameraCallbackScope.Clear();
            Camera.OnUserUpdated = () => updated++;
            var camera = new Camera();

            Assert.True(camera.Control(Key.W, true, ModifierKeys.None));
            camera.StopKeyboardMotion();
            Assert.Equal(1, updated);

            Assert.True(camera.Control(Key.W, false, ModifierKeys.None));
            Assert.Equal(1, updated);
        }

        static Vector3 Step(Camera camera)
        {
            var before = camera.Pos;
            camera.Update(1.0);
            return camera.Pos - before;
        }

        sealed class CameraCallbackScope : System.IDisposable
        {
            readonly System.Action _updating;
            readonly System.Action _updated;

            CameraCallbackScope(System.Action updating, System.Action updated)
            {
                _updating = updating;
                _updated = updated;
            }

            public static CameraCallbackScope Clear()
            {
                var scope = new CameraCallbackScope(Camera.OnUserUpdating, Camera.OnUserUpdated);
                Camera.OnUserUpdating = null;
                Camera.OnUserUpdated = null;
                return scope;
            }

            public void Dispose()
            {
                Camera.OnUserUpdating = _updating;
                Camera.OnUserUpdated = _updated;
            }
        }
    }
}
