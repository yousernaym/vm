using LibSidWiz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace VisualMusic
{
    public class WaveformPanel
    {
        const float NormalizedWidth = 0.25f;
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

        internal void Init(GraphicsDevice gfxDevice, SpriteBatch spriteBatch)
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


            // Defer texture creation if the viewport is not yet sized (e.g. HwndHost init fires at 1×1).
            // Draw() will trigger lazy Resize() once the real dimensions are available.
            if (_overlayRect.Width > 0 && _overlayRect.Height > 0)
            {
                _waveTex = new Texture2D(gfxDevice, _renderer.Width, _renderer.Height, false, SurfaceFormat.Color);
                _framePixels = new byte[_renderer.Width * _renderer.Height * 4];
            }
        }

        public void Resize()
        {
            var vp = _gfxDevice.Viewport;

            _overlayRect = new Rectangle(0, 0, (int)(vp.Width * NormalizedWidth), vp.Height); // leftmost 25%
            if (_renderer != null && _overlayRect.Width > 0 && _overlayRect.Height > 0)
            {
                _renderer.Width = _overlayRect.Width;
                _renderer.Height = _overlayRect.Height;
                _renderer.RenderingBounds = new System.Drawing.Rectangle(0, 0, _overlayRect.Width, _overlayRect.Height);
                _renderer.Init();
                _waveTex?.Dispose();
                _waveTex = new Texture2D(_gfxDevice, _renderer.Width, _renderer.Height, false, SurfaceFormat.Color);
                _framePixels = new byte[_renderer.Width * _renderer.Height * 4];
            }
        }

        internal void Draw(double songPosS)
        {
            if (songPosS < 0 || _renderer.ChannelCount == 0)
                return;
            if (!_channels.Exists(c => !string.IsNullOrEmpty(c.Filename)))
                return;
            if (_waveTex == null || _renderer.Width != (int)(_gfxDevice.Viewport.Width * NormalizedWidth) || _renderer.Height != (int)(_gfxDevice.Viewport.Height))
                Resize();
            if (_waveTex == null)
                return;
            var pixels = _renderer.RenderFrame(songPosS);
            _waveTex.SetData(pixels);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            _spriteBatch.Draw(_waveTex, _overlayRect, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.End();
        }

        internal void ClearChannels()
        {
            _channels.Clear();
            _renderer?.ClearChannels();
        }

        public void AddChannel(Channel channel)
        {
            _channels.Add(channel);
            _renderer?.AddChannel(channel);
        }

        public void Dispose()
        {
            _renderer?.Dispose();
            _waveTex.Dispose();
        }

        internal void RemoveChannel(Channel sidWizChannel)
        {
            _channels.Remove(sidWizChannel);
            _renderer.RemoveChannel(sidWizChannel);
        }
    }
}
