using LibSidWiz;
using LibSidWiz.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualMusic
{
    public class WaveformPanel
    {
        private Texture2D _waveTex;
        private Rectangle _overlayRect;
        private WaveformRenderer _renderer;
        private List<Channel> _channels = new List<Channel>();

        // Reusable CPU buffer when copying from Bitmap to Texture2D
        private byte[] _framePixels;

        // Target UI refresh rate for libSidWiz frame math. 60 is a good default.
        private const int UiFps = 60;

        GraphicsDevice _gfxDevice;
        SpriteBatch _spriteBatch;

        internal async void Init(GraphicsDevice gfxDevice, SpriteBatch spriteBatch)
        {
            _gfxDevice = gfxDevice;
            _spriteBatch = spriteBatch;

            Resize();

            _renderer = new WaveformRenderer
            {
                Width = _overlayRect.Width,
                Height = _overlayRect.Height,
                Columns = 1,
                RenderingBounds = new System.Drawing.Rectangle(0, 0, _overlayRect.Width, _overlayRect.Height),
                BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 0, 0),
                FramesPerSecond = UiFps,
            };


            // Create the GPU texture for the overlay
            _waveTex = new Texture2D(gfxDevice, _renderer.Width, _renderer.Height, false, SurfaceFormat.Color);

            // Reuse a CPU buffer for pixel copy each frame
            _framePixels = new byte[_renderer.Width * _renderer.Height * 4];
        }

        private void Resize()
        {
            var vp = _gfxDevice.Viewport;
            
            _overlayRect = new Rectangle(0, 0, (int)(vp.Width * 0.25f), vp.Height); // leftmost 25%
            if (_renderer != null)
            {
                _renderer.Width = _overlayRect.Width;
                _renderer.Height = _overlayRect.Height;
                _renderer.RenderingBounds = new System.Drawing.Rectangle(0, 0, _overlayRect.Width, _overlayRect.Height); // local in overlay. :contentReference[oaicite:9]{index=9}

                // Recreate texture if size changes
                _waveTex?.Dispose();
                _waveTex = new Texture2D(_gfxDevice, _renderer.Width, _renderer.Height, false, SurfaceFormat.Color);
                _framePixels = new byte[_renderer.Width * _renderer.Height * 4];
            }
        }

        internal void Draw(double songPosS)
        {
            if (_waveTex == null || songPosS < 0)
                return;
            var pixels = _renderer.RenderFrame(songPosS);
            _waveTex.SetData(pixels);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            _spriteBatch.Draw(_waveTex, _overlayRect, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.End();
        }

        internal void ClearChannels()
        {
            _renderer.ClearChannels();
        }

        public void AddChannel(Channel channel)
        {
            if (_renderer == null)
                throw new InvalidOperationException("Renderer not initialized. Call Init() first.");
            _renderer.AddChannel(channel);
        }

        public void Dispose()
        {
            _renderer?.Dispose();
            _waveTex.Dispose();
        }
    }
}
