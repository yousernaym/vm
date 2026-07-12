using LibSidWiz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace VisualMusic
{
    public class WaveformPanel
    {
        // Target UI refresh rate for libSidWiz frame math. 60 is a good default.
        private const int UiFps = 60;

        /// <summary>One rendered waveform strip (left or right), with its own renderer + texture.</summary>
        private class Strip
        {
            public WaveformRenderer Renderer;
            public Texture2D Tex;
            public byte[] FramePixels;
            public Rectangle Rect;
            public readonly List<Channel> Channels = new List<Channel>();

            public void Dispose()
            {
                Renderer?.Dispose();
                Tex?.Dispose();
                Renderer = null;
                Tex = null;
                FramePixels = null;
            }
        }

        // Master channel list in add-order (= track order).
        private readonly List<Channel> _channels = new List<Channel>();

        private Strip _leftStrip;
        private Strip _rightStrip;
        private bool _dirty = true;

        // Cached config used to detect changes that require a Reconfigure().
        private bool _cfgLeft;
        private bool _cfgRight;
        private float _cfgWidthFrac = -1;
        private int _cfgVpW = -1;
        private int _cfgVpH = -1;

        GraphicsDevice _gfxDevice;
        SpriteBatch _spriteBatch;

        internal void Init(GraphicsDevice gfxDevice, SpriteBatch spriteBatch)
        {
            _gfxDevice = gfxDevice;
            _spriteBatch = spriteBatch;
            _dirty = true;
        }

        public void Resize()
        {
            _dirty = true;
        }

        internal void Draw(double songPosS, float fade, bool left, bool right, float widthFrac)
        {
            if (songPosS < 0 || fade <= 0 || (!left && !right) || widthFrac <= 0)
                return;
            if (!_channels.Exists(c => !string.IsNullOrEmpty(c.Filename)))
                return;

            var vp = _gfxDevice.Viewport;
            if (vp.Width <= 0 || vp.Height <= 0)
                return;

            if (_dirty || left != _cfgLeft || right != _cfgRight || widthFrac != _cfgWidthFrac
                || vp.Width != _cfgVpW || vp.Height != _cfgVpH)
                Reconfigure(left, right, widthFrac, vp.Width, vp.Height);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            DrawStrip(_leftStrip, songPosS, fade);
            DrawStrip(_rightStrip, songPosS, fade);
            _spriteBatch.End();
        }

        void DrawStrip(Strip strip, double songPosS, float fade)
        {
            if (strip == null || strip.Tex == null)
                return;
            var pixels = strip.Renderer.RenderFrame(songPosS);
            strip.Tex.SetData(pixels);
            // The texture is premultiplied BGRA, so scaling the whole color by the fade factor
            // fades both the waveforms and the semi-transparent background correctly.
            _spriteBatch.Draw(strip.Tex, strip.Rect, Microsoft.Xna.Framework.Color.White * fade);
        }

        void Reconfigure(bool left, bool right, float widthFrac, int vpW, int vpH)
        {
            _cfgLeft = left;
            _cfgRight = right;
            _cfgWidthFrac = widthFrac;
            _cfgVpW = vpW;
            _cfgVpH = vpH;
            _dirty = false;

            _leftStrip?.Dispose();
            _rightStrip?.Dispose();
            _leftStrip = null;
            _rightStrip = null;

            int sides = (left ? 1 : 0) + (right ? 1 : 0);
            float frac = sides == 2 ? widthFrac * 0.5f : widthFrac;  // sides SHARE total width
            int stripW = (int)(vpW * frac);
            if (stripW <= 0)
                return;

            // Distribute channels: one side visible -> all on it; both -> alternate (i even = left).
            var leftChannels = new List<Channel>();
            var rightChannels = new List<Channel>();
            for (int i = 0; i < _channels.Count; i++)
            {
                if (left && right)
                    (i % 2 == 0 ? leftChannels : rightChannels).Add(_channels[i]);
                else if (left)
                    leftChannels.Add(_channels[i]);
                else
                    rightChannels.Add(_channels[i]);
            }

            if (left)
                _leftStrip = BuildStrip(leftChannels, new Rectangle(0, 0, stripW, vpH), stripW, vpH);
            if (right)
                _rightStrip = BuildStrip(rightChannels, new Rectangle(vpW - stripW, 0, stripW, vpH), stripW, vpH);
        }

        Strip BuildStrip(List<Channel> channels, Rectangle rect, int stripW, int vpH)
        {
            if (channels.Count == 0)
                return null;

            var strip = new Strip { Rect = rect };
            strip.Channels.AddRange(channels);

            strip.Renderer = new WaveformRenderer
            {
                Width = stripW,
                Height = vpH,
                Columns = 1,
                RenderingBounds = new System.Drawing.Rectangle(0, 0, stripW, vpH),
                BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 0, 0),
                FramesPerSecond = UiFps,
            };
            foreach (var ch in channels)
                strip.Renderer.AddChannel(ch);
            strip.Renderer.Init();

            strip.Tex = new Texture2D(_gfxDevice, stripW, vpH, false, SurfaceFormat.Color);
            strip.FramePixels = new byte[stripW * vpH * 4];
            return strip;
        }

        internal void ClearChannels()
        {
            _channels.Clear();
            _dirty = true;
        }

        public void AddChannel(Channel channel)
        {
            _channels.Add(channel);
            _dirty = true;
        }

        public void Dispose()
        {
            _leftStrip?.Dispose();
            _rightStrip?.Dispose();
        }

        internal void RemoveChannel(Channel sidWizChannel)
        {
            _channels.Remove(sidWizChannel);
            _dirty = true;
        }
    }
}
