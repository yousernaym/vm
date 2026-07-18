using LibSidWiz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace VisualMusic
{
    /// <summary>
    /// Draws the SidWiz oscilloscope strips over the song view. The waveform math (trigger search,
    /// activity/lookahead scanning, GDI+ rasterization) can be arbitrarily slow — e.g. the
    /// auto-correlation trigger with a high lookahead — so it runs on a dedicated background worker
    /// with latest-wins frame requests. The UI/render thread only hands the worker the current song
    /// position and draws whatever frame the worker finished most recently, so waveform cost never
    /// stalls the UI. During video export (<see cref="Synchronous"/>) frames are instead rendered
    /// inline on the export thread so every exported frame is exact.
    /// </summary>
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

            // Latest finished frame: the rendering thread (worker, or export thread in sync mode)
            // copies the renderer's reused pixel buffer here; the UI thread uploads it to Tex.
            public readonly object PublishLock = new object();
            public byte[] Published;
            public bool PublishedDirty;
            public bool HasFrame;   // Tex holds at least one valid frame (UI thread only)

            public void Dispose()
            {
                Renderer?.Dispose();
                Tex?.Dispose();
                Renderer = null;
                Tex = null;
            }
        }

        private struct FrameRequest
        {
            public double PosSeconds;
            public float ActivityThreshold;
        }

        // Master channel list in add-order (= track order). UI thread only; the worker sees channel
        // set changes only after the next Reconfigure() rebuilds the strips under _sync.
        private readonly List<Channel> _channels = new List<Channel>();

        private Strip _leftStrip;    // accessed by the rendering thread under _sync
        private Strip _rightStrip;
        private bool _dirty = true;

        private enum Side { Left, Right }
        // Persistent per-channel side assignment: a channel keeps its side while it sounds, so greedy
        // balancing only ever places a NEWLY-active channel on the currently-emptier side.
        // Rendering-thread state (guarded by _sync).
        private readonly Dictionary<Channel, Side> _side = new Dictionary<Channel, Side>();
        private readonly List<Channel> _leftAssigned = new List<Channel>();
        private readonly List<Channel> _rightAssigned = new List<Channel>();
        private readonly List<Channel> _sideRemoveScratch = new List<Channel>();
        private readonly HashSet<Channel> _activeSetScratch = new HashSet<Channel>();

        // Cached config used to detect changes that require a Reconfigure(). UI thread only.
        private bool _cfgLeft;
        private bool _cfgRight;
        private float _cfgWidthFrac = -1;
        private int _cfgVpW = -1;
        private int _cfgVpH = -1;

        GraphicsDevice _gfxDevice;
        SpriteBatch _spriteBatch;
        Texture2D _pixel;   // 1x1 white texture used to draw the dark backdrop as its own layer

        // Serializes strip/renderer access between the UI thread (reconfigure, dispose) and the
        // frame-rendering thread (worker, or the export thread in sync mode).
        private readonly object _sync = new object();

        // Latest-wins request handed to the worker; positions the worker can't keep up with are
        // simply skipped (the on-screen waveform just updates at whatever rate the worker manages).
        private readonly object _requestLock = new object();
        private FrameRequest _request;
        private bool _requestPending;
        private readonly AutoResetEvent _workSignal = new AutoResetEvent(false);
        private Thread _worker;
        private volatile bool _workerExit;

        /// <summary>
        /// When true (video export), each Draw renders its frame inline on the calling thread so
        /// exported frames are frame-exact. The live path leaves this false (background worker).
        /// </summary>
        public bool Synchronous { get; set; }

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

        internal void Draw(double songPosS, float fade, bool left, bool right, float widthFrac, float opacity,
            float activityThreshold)
        {
            // Note: opacity is NOT part of this guard — it only dims the backdrop, so the waveforms
            // still draw (fully opaque) even at opacity 0.
            if (songPosS < 0 || fade <= 0 || (!left && !right) || widthFrac <= 0)
                return;
            if (!_channels.Exists(c => c.HasAudioSource))
                return;

            var vp = _gfxDevice.Viewport;
            if (vp.Width <= 0 || vp.Height <= 0)
                return;

            if (_dirty || left != _cfgLeft || right != _cfgRight || widthFrac != _cfgWidthFrac
                || vp.Width != _cfgVpW || vp.Height != _cfgVpH)
            {
                // May briefly wait for the worker to finish its current frame; reconfigures are
                // rare (resize / settings / track-set changes) so this doesn't affect normal frames.
                lock (_sync)
                    Reconfigure(left, right, widthFrac, vp.Width, vp.Height);
            }

            if (_leftStrip == null && _rightStrip == null)
                return;

            var request = new FrameRequest { PosSeconds = songPosS, ActivityThreshold = activityThreshold };
            if (Synchronous)
            {
                // Export: drop any stale live request so the worker can't overwrite this frame,
                // then render inline at exactly the requested position.
                lock (_requestLock)
                    _requestPending = false;
                lock (_sync)
                    RenderFrameCore(request);
            }
            else
            {
                lock (_requestLock)
                {
                    _request = request;
                    _requestPending = true;
                }
                EnsureWorkerStarted();
                _workSignal.Set();
            }

            // The backdrop is a separate layer faded by both the song fade and the AudioVisOpacity
            // slider; the waveform texture is drawn on top at full opacity (song fade only) so the
            // waveforms stay 100% opaque regardless of the opacity setting.
            float bgAlpha = fade * opacity;
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            DrawStrip(_leftStrip, fade, bgAlpha);
            DrawStrip(_rightStrip, fade, bgAlpha);
            _spriteBatch.End();
        }

        void EnsureWorkerStarted()
        {
            if (_worker != null)
                return;
            _workerExit = false;
            _worker = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "SidWiz waveform renderer",
                // Keep the UI/render thread ahead of waveform math on saturated machines.
                Priority = ThreadPriority.BelowNormal,
            };
            _worker.Start();
        }

        void WorkerLoop()
        {
            while (true)
            {
                _workSignal.WaitOne();
                if (_workerExit)
                    return;

                FrameRequest request;
                lock (_requestLock)
                {
                    if (!_requestPending)
                        continue;
                    request = _request;
                    _requestPending = false;
                }

                try
                {
                    lock (_sync)
                    {
                        // A request queued just before an export started must not render: the export
                        // thread owns frame rendering in sync mode and a stale live-position frame
                        // would disturb the renderers' trigger/activity continuity mid-export.
                        if (!Synchronous)
                            RenderFrameCore(request);
                    }
                }
                catch (Exception e)
                {
                    // Channel properties (pens, fonts, colors) are edited by the UI mid-playback and
                    // GDI+ objects aren't thread safe, so a frame can occasionally fail; drop it and
                    // render the next one rather than killing the worker.
                    System.Diagnostics.Debug.WriteLine("SidWiz worker frame failed: " + e);
                }
            }
        }

        /// <summary>
        /// Renders one waveform frame into the strips' publish buffers. Runs on the worker thread
        /// (or the export thread in sync mode) with <see cref="_sync"/> held.
        /// </summary>
        void RenderFrameCore(in FrameRequest request)
        {
            var leftR = _leftStrip?.Renderer;
            var rightR = _rightStrip?.Renderer;
            var authority = leftR ?? rightR;
            if (authority == null)
                return;

            // Silence-detection level (linear amplitude, relative to each channel's normalized peak).
            // Both strips run their own activity scan, so push it to whichever renderers exist.
            if (leftR != null)
                leftR.ActivityThreshold = request.ActivityThreshold;
            if (rightR != null)
                rightR.ActivityThreshold = request.ActivityThreshold;

            // Phase 1: compute the active channel set ONCE. Both strips hold every channel, so the
            // authority strip runs activity/trigger detection and the other reuses its result — the
            // two sides can't disagree about which channels are active or where they trigger.
            authority.PrepareFrame(request.PosSeconds);
            var active = authority.ActiveThisFrame;
            if (leftR != null && rightR != null)
                rightR.CopyPreparedFrom(leftR);

            // Phase 2: split the active channels across the two sides. A newly-appearing channel goes
            // to whichever side currently has fewer waveforms and stays there while it sounds, so the
            // sides stay balanced without existing waveforms hopping from one side to the other.
            int leftCount, rightCount;
            if (leftR != null && rightR != null)
            {
                AssignSides(active);
                _leftAssigned.Clear();
                _rightAssigned.Clear();
                foreach (var ch in active)
                    (_side[ch] == Side.Left ? _leftAssigned : _rightAssigned).Add(ch);
                leftR.SetRenderFilter(_leftAssigned);
                rightR.SetRenderFilter(_rightAssigned);
                leftCount = RowSum(_leftAssigned);
                rightCount = RowSum(_rightAssigned);
            }
            else
            {
                // Only one side visible: it shows every active channel, no balancing needed.
                leftR?.SetRenderFilter(null);
                rightR?.SetRenderFilter(null);
                leftCount = rightCount = RowSum(active);
            }

            // Give both renderers the same slot count so track heights match across the two sides
            // (the sparser side leaves its bottom slots empty). When every channel is silent this
            // frame, still lay out one slot so each strip draws its full-height dark overlay.
            int slots = System.Math.Max(leftCount, rightCount);
            if (slots == 0)
                slots = 1;
            if (leftR != null)
                leftR.LayoutSlots = slots;
            if (rightR != null)
                rightR.LayoutSlots = slots;

            Publish(_leftStrip);
            Publish(_rightStrip);
        }

        /// <summary>Rasterizes the prepared frame and copies it into the strip's publish buffer.</summary>
        static void Publish(Strip strip)
        {
            if (strip == null)
                return;
            // The renderer reuses its pixel buffer every frame, so copy it out; the UI thread
            // uploads from the copy, decoupled from the next frame being rendered.
            var pixels = strip.Renderer.RenderPreparedFrame();
            lock (strip.PublishLock)
            {
                if (strip.Published == null || strip.Published.Length != pixels.Length)
                    strip.Published = new byte[pixels.Length];
                Buffer.BlockCopy(pixels, 0, strip.Published, 0, pixels.Length);
                strip.PublishedDirty = true;
            }
        }

        // Update _side for this frame: drop channels that went silent (freeing their slot), then give
        // each newly-active channel (in track order) to the side that currently holds fewer waveforms.
        void AssignSides(IReadOnlyList<Channel> active)
        {
            _activeSetScratch.Clear();
            for (int i = 0; i < active.Count; ++i)
                _activeSetScratch.Add(active[i]);

            // Weight by the rows each channel occupies (a Separate-mode split channel is several
            // rows) so the two sides stay balanced by height, not just by channel count.
            int lc = 0, rc = 0;
            if (_side.Count > 0)
            {
                _sideRemoveScratch.Clear();
                foreach (var kv in _side)
                {
                    if (!_activeSetScratch.Contains(kv.Key))
                        _sideRemoveScratch.Add(kv.Key);
                    else if (kv.Value == Side.Left)
                        lc += kv.Key.LayoutRows;
                    else
                        rc += kv.Key.LayoutRows;
                }
                foreach (var ch in _sideRemoveScratch)
                    _side.Remove(ch);
            }

            for (int i = 0; i < active.Count; ++i)
            {
                var ch = active[i];
                if (_side.ContainsKey(ch))
                    continue;
                if (lc <= rc) { _side[ch] = Side.Left; lc += ch.LayoutRows; }
                else { _side[ch] = Side.Right; rc += ch.LayoutRows; }
            }
        }

        static int RowSum(IReadOnlyList<Channel> channels)
        {
            int sum = 0;
            for (int i = 0; i < channels.Count; ++i)
                sum += channels[i].LayoutRows;
            return sum;
        }

        void DrawStrip(Strip strip, float fade, float bgAlpha)
        {
            if (strip == null || strip.Tex == null)
                return;
            // Dark backdrop behind the waveforms (the texture itself has a transparent background),
            // drawn first and dimmed by the opacity slider. Color.Black * a is premultiplied black.
            if (bgAlpha > 0)
                _spriteBatch.Draw(_pixel, strip.Rect, Microsoft.Xna.Framework.Color.Black * bgAlpha);

            // Upload the latest finished frame, if there is a new one. Otherwise the texture just
            // keeps showing the previous frame — the UI never waits for waveform rendering.
            lock (strip.PublishLock)
            {
                if (strip.PublishedDirty
                    && strip.Published.Length == strip.Tex.Width * strip.Tex.Height * 4)
                {
                    strip.Tex.SetData(strip.Published);
                    strip.PublishedDirty = false;
                    strip.HasFrame = true;
                }
            }

            // Waveforms (premultiplied BGRA, transparent background) at full opacity — only the song
            // fade applies, never the opacity slider.
            if (strip.HasFrame)
                _spriteBatch.Draw(strip.Tex, strip.Rect, Microsoft.Xna.Framework.Color.White * fade);
        }

        /// <summary>Rebuilds the strips for a new panel configuration. Called with _sync held.</summary>
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
            // decided per-frame in RenderFrameCore (greedy balancing). Stale side assignments from
            // the previous strip pair no longer apply.
            _side.Clear();

            // Both strips span the full viewport height; per-frame row layout (uniform track
            // heights across the two sides, empty slots at the bottom of the sparser side) is
            // handled inside WaveformRenderer via LayoutSlots — see RenderFrameCore().
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
            // Stop the worker before touching the strips it renders into.
            _workerExit = true;
            _workSignal.Set();
            _worker?.Join(500);
            _worker = null;

            lock (_sync)
            {
                _leftStrip?.Dispose();
                _rightStrip?.Dispose();
                _leftStrip = null;
                _rightStrip = null;
            }
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
