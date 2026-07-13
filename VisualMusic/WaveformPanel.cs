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
            public Rectangle Rect;
            public readonly List<Channel> Channels = new List<Channel>();

            public void Dispose()
            {
                Renderer?.Dispose();
                Tex?.Dispose();
                Renderer = null;
                Tex = null;
            }
        }

        // Master channel list in add-order (= track order).
        private readonly List<Channel> _channels = new List<Channel>();

        private Strip _leftStrip;
        private Strip _rightStrip;
        private bool _dirty = true;

        private enum Side { Left, Right }
        // Persistent per-channel side assignment: a channel keeps its side while it sounds, so greedy
        // balancing only ever places a NEWLY-active channel on the currently-emptier side.
        private readonly Dictionary<Channel, Side> _side = new Dictionary<Channel, Side>();
        private readonly List<Channel> _leftAssigned = new List<Channel>();
        private readonly List<Channel> _rightAssigned = new List<Channel>();
        private readonly List<Channel> _sideRemoveScratch = new List<Channel>();
        private readonly HashSet<Channel> _activeSetScratch = new HashSet<Channel>();

        // Cached config used to detect changes that require a Reconfigure().
        private bool _cfgLeft;
        private bool _cfgRight;
        private float _cfgWidthFrac = -1;
        private int _cfgVpW = -1;
        private int _cfgVpH = -1;

        GraphicsDevice _gfxDevice;
        SpriteBatch _spriteBatch;
        Texture2D _pixel;   // 1x1 white texture used to draw the dark backdrop as its own layer

        internal void Init(GraphicsDevice gfxDevice, SpriteBatch spriteBatch)
        {
            _gfxDevice = gfxDevice;
            _spriteBatch = spriteBatch;
            _pixel?.Dispose();
            _pixel = new Texture2D(gfxDevice, 1, 1);
            _pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            _dirty = true;
        }

        public void Resize()
        {
            _dirty = true;
        }

        internal void Draw(double songPosS, float fade, bool left, bool right, float widthFrac, float opacity)
        {
            // Note: opacity is NOT part of this guard — it only dims the backdrop, so the waveforms
            // still draw (fully opaque) even at opacity 0.
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

            // Phase 1: compute the active channel set ONCE. Both strips hold every channel, so the
            // authority strip runs activity/trigger detection and the other reuses its result — the
            // two sides can't disagree about which channels are active or where they trigger.
            var authority = _leftStrip?.Renderer ?? _rightStrip?.Renderer;
            if (authority == null)
                return;
            authority.PrepareFrame(songPosS);
            var active = authority.ActiveThisFrame;
            if (_leftStrip != null && _rightStrip != null)
                _rightStrip.Renderer.CopyPreparedFrom(_leftStrip.Renderer);

            // Phase 2: split the active channels across the two sides. A newly-appearing channel goes
            // to whichever side currently has fewer waveforms and stays there while it sounds, so the
            // sides stay balanced without existing waveforms hopping from one side to the other.
            int leftCount, rightCount;
            if (_leftStrip != null && _rightStrip != null)
            {
                AssignSides(active);
                _leftAssigned.Clear();
                _rightAssigned.Clear();
                foreach (var ch in active)
                    (_side[ch] == Side.Left ? _leftAssigned : _rightAssigned).Add(ch);
                _leftStrip.Renderer.SetRenderFilter(_leftAssigned);
                _rightStrip.Renderer.SetRenderFilter(_rightAssigned);
                leftCount = _leftAssigned.Count;
                rightCount = _rightAssigned.Count;
            }
            else
            {
                // Only one side visible: it shows every active channel, no balancing needed.
                _leftStrip?.Renderer.SetRenderFilter(null);
                _rightStrip?.Renderer.SetRenderFilter(null);
                leftCount = rightCount = active.Count;
            }

            // Give both renderers the same slot count so track heights match across the two sides
            // (the sparser side leaves its bottom slots empty). When every channel is silent this
            // frame, still lay out one slot so each strip draws its full-height dark overlay.
            int slots = System.Math.Max(leftCount, rightCount);
            if (slots == 0 && (_leftStrip != null || _rightStrip != null))
                slots = 1;
            if (_leftStrip != null)
                _leftStrip.Renderer.LayoutSlots = slots;
            if (_rightStrip != null)
                _rightStrip.Renderer.LayoutSlots = slots;

            // The backdrop is a separate layer faded by both the song fade and the AudioVisOpacity
            // slider; the waveform texture is drawn on top at full opacity (song fade only) so the
            // waveforms stay 100% opaque regardless of the opacity setting.
            float bgAlpha = fade * opacity;
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            DrawStrip(_leftStrip, fade, bgAlpha);
            DrawStrip(_rightStrip, fade, bgAlpha);
            _spriteBatch.End();
        }

        // Update _side for this frame: drop channels that went silent (freeing their slot), then give
        // each newly-active channel (in track order) to the side that currently holds fewer waveforms.
        void AssignSides(IReadOnlyList<Channel> active)
        {
            _activeSetScratch.Clear();
            for (int i = 0; i < active.Count; ++i)
                _activeSetScratch.Add(active[i]);

            int lc = 0, rc = 0;
            if (_side.Count > 0)
            {
                _sideRemoveScratch.Clear();
                foreach (var kv in _side)
                {
                    if (!_activeSetScratch.Contains(kv.Key))
                        _sideRemoveScratch.Add(kv.Key);
                    else if (kv.Value == Side.Left)
                        lc++;
                    else
                        rc++;
                }
                foreach (var ch in _sideRemoveScratch)
                    _side.Remove(ch);
            }

            for (int i = 0; i < active.Count; ++i)
            {
                var ch = active[i];
                if (_side.ContainsKey(ch))
                    continue;
                if (lc <= rc) { _side[ch] = Side.Left; lc++; }
                else { _side[ch] = Side.Right; rc++; }
            }
        }

        void DrawStrip(Strip strip, float fade, float bgAlpha)
        {
            if (strip == null || strip.Tex == null)
                return;
            // Dark backdrop behind the waveforms (the texture itself has a transparent background),
            // drawn first and dimmed by the opacity slider. Color.Black * a is premultiplied black.
            if (bgAlpha > 0)
                _spriteBatch.Draw(_pixel, strip.Rect, Microsoft.Xna.Framework.Color.Black * bgAlpha);
            var pixels = strip.Renderer.RenderPreparedFrame();
            strip.Tex.SetData(pixels);
            // Waveforms (premultiplied BGRA, transparent background) at full opacity — only the song
            // fade applies, never the opacity slider.
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

            // Both strips hold EVERY channel; which side actually draws a given active channel is
            // decided per-frame in Draw (greedy balancing). Stale side assignments from the previous
            // strip pair no longer apply.
            _side.Clear();

            // Both strips span the full viewport height; per-frame row layout (uniform track
            // heights across the two sides, empty slots at the bottom of the sparser side) is
            // handled inside WaveformRenderer via LayoutSlots — see Draw().
            if (left)
                _leftStrip = BuildStrip(_channels, new Rectangle(0, 0, stripW, vpH), stripW, vpH);
            if (right)
                _rightStrip = BuildStrip(_channels, new Rectangle(vpW - stripW, 0, stripW, vpH), stripW, vpH);
        }

        Strip BuildStrip(List<Channel> channels, Rectangle rect, int stripW, int stripH)
        {
            if (channels.Count == 0 || stripW <= 0 || stripH <= 0)
                return null;

            var strip = new Strip { Rect = rect };
            strip.Channels.AddRange(channels);

            strip.Renderer = new WaveformRenderer
            {
                Width = stripW,
                Height = stripH,
                Columns = 1,
                RenderingBounds = new System.Drawing.Rectangle(0, 0, stripW, stripH),
                // Transparent baked background: the dark backdrop is drawn as its own layer in
                // DrawStrip (dimmed by AudioVisOpacity), so the texture holds only the waveforms.
                BackgroundColor = System.Drawing.Color.Transparent,
                FramesPerSecond = UiFps,
            };
            foreach (var ch in channels)
                strip.Renderer.AddChannel(ch);
            strip.Renderer.Init();

            strip.Tex = new Texture2D(_gfxDevice, stripW, stripH, false, SurfaceFormat.Color);
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
            _pixel?.Dispose();
            _pixel = null;
        }

        internal void RemoveChannel(Channel sidWizChannel)
        {
            _channels.Remove(sidWizChannel);
            _dirty = true;
        }
    }
}
