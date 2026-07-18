using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace LibSidWiz
{
    /// <summary>
    /// Class responsible for rendering
    /// </summary>
    public class WaveformRenderer : IDisposable
    {
        private readonly List<Channel> _channels = new List<Channel>();

        private int _width;
        private int _height;
        private int _columns;
        private int _samplingRate;
        private int _framesPerSecond;
        private Color _backgroundColor = Color.Black;
        private Image _backgroundImage;
        private Rectangle _renderingBounds;

        public int Width { get => _width; set { _width = value; _templateDirty = true; } }
        public int Height { get => _height; set { _height = value; _templateDirty = true; } }
        public int Columns { get => _columns; set { _columns = value; _templateDirty = true; } }
        public int SamplingRate { get => _samplingRate; set { _samplingRate = value; } }
        public int FramesPerSecond { get => _framesPerSecond; set { _framesPerSecond = value; } }
        public Color BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; _templateDirty = true; } }
        public Image BackgroundImage { get => _backgroundImage; set { _backgroundImage = value; _templateDirty = true; } }
        public Rectangle RenderingBounds { get => _renderingBounds; set { _renderingBounds = value; _templateDirty = true; } }

        private int _layoutSlots;
        /// <summary>
        /// Number of layout rows the rendering bounds are divided into. When greater than the
        /// visible channel count, channels stack from the top and the remaining rows stay empty
        /// (transparent). 0 = one row per visible channel (fill the whole bounds). Lets two
        /// side-by-side renderers keep their track heights in sync.
        /// </summary>
        public int LayoutSlots { get => _layoutSlots; set { if (_layoutSlots != value) { _layoutSlots = value; _templateDirty = true; } } }

        public int ChannelCount => _channels.Count;

        public void AddChannel(Channel channel)
        {
            _channels.Add(channel);
            channel.Renderer = this;
            if (channel.SampleRate != 0)
                SamplingRate = channel.SampleRate;
            _templateDirty = true;
        }

        private byte[] _templateData;              // premultiplied BGRA template (bg, labels, borders, zero lines)
        private byte[] _frameData;                 // per-frame buffer to draw into
        private GCHandle _templateHandle;
        private GCHandle _frameHandle;
        private Bitmap _frameBitmap;               // GDI+ draws into _frameData directly
        private bool _templateDirty = true;

        private PointF[][] _pointsPerChannel;      // [channel][sample] to avoid per-frame allocs
        private Pen[] _pens;
        private Brush[] _brushes;
        private int[] _prevTrigger;                // previous trigger per channel
        // Recreated in Init(): Init() starts with Dispose(), which releases the path, so a
        // once-only (readonly) instance would be dead by the time the first frame renders.
        private GraphicsPath _fillPath;

        // Dynamic active set
        private List<Channel> _visible = new List<Channel>();
        private string _visibleSignature = "";

        // Per-frame state computed by PrepareFrame and consumed by RenderPreparedFrame
        private readonly List<Channel> _activeThisFrame = new List<Channel>();
        private int[] _frameTriggers;
        private bool _framePrepared;

        /// <summary>Channels found active by the last PrepareFrame, in track order. Read-only; do not mutate.</summary>
        public IReadOnlyList<Channel> ActiveThisFrame => _activeThisFrame;

        // When set, RenderPreparedFrame draws only the active channels also in this set (used to split
        // the active channels across two side-by-side strips). null => draw every active channel.
        private HashSet<Channel> _renderFilter;
        private readonly List<Channel> _renderVisibleScratch = new List<Channel>();

        /// <summary>Restrict which active channels this renderer draws (null = all). The set is copied.</summary>
        public void SetRenderFilter(IReadOnlyList<Channel> channels)
        {
            if (channels == null) { _renderFilter = null; return; }
            if (_renderFilter == null) _renderFilter = new HashSet<Channel>();
            else _renderFilter.Clear();
            for (int i = 0; i < channels.Count; ++i)
                _renderFilter.Add(channels[i]);
        }

        /// <summary>
        /// Adopt the active set and triggers computed by another renderer over the SAME channel list
        /// (identical references, identical order), so a second strip can render without recomputing
        /// activity/triggers. Pair with <see cref="SetRenderFilter"/> to pick this strip's channels.
        /// </summary>
        public void CopyPreparedFrom(WaveformRenderer src)
        {
            if (_frameData == null)
                Init();
            EnsurePerFrameCachesInitialized(0, Math.Max(1, SamplingRate / Math.Max(1, FramesPerSecond)));
            if (_frameTriggers == null || _frameTriggers.Length != _channels.Count)
                _frameTriggers = new int[_channels.Count];
            if (src._frameTriggers != null)
                Array.Copy(src._frameTriggers, _frameTriggers, Math.Min(_frameTriggers.Length, src._frameTriggers.Length));
            _activeThisFrame.Clear();
            _activeThisFrame.AddRange(src._activeThisFrame);
            _framePrepared = true;
        }

        // Activity detection knobs
        public float ActivityThreshold = 0.004f;   // ~ -48 dBFS; tweak
        public int ActivityWindowSamplesOverride = 0; // 0 => use ViewWidthInSamples
        public int ActivitySubsampleStride = 4;  // >=1; higher = faster
        // Lookahead duration comes from each channel's ActivityLookaheadSeconds.
        // Per-channel lookahead cache: earliest known upcoming active sample (int.MinValue = unknown)
        // and the exclusive end of the contiguously verified-silent region (int.MinValue = none).
        // Valid only while playback advances forward; backward seeks reset via ResetActivityState.
        private int[] _nextActiveSample;
        private int[] _silentScannedUntil;
        // Whether the channel was shown last frame: the lookahead only bridges gaps for a visible
        // channel; a hidden one must wait for audio to actually reach the window.
        private bool[] _wasActive;
        private int _lastFrameStartSample = int.MinValue;

        /// <summary>Allocate and pin buffers; (re)build template.</summary>
        public void Init()
        {
            Dispose();

            if (Width <= 0 || Height <= 0)
                throw new InvalidOperationException("Width and Height must be set");

            _lastFrameStartSample = int.MinValue;

            _fillPath = new GraphicsPath();
            _frameData = new byte[Width * Height * 4];
            _frameHandle = GCHandle.Alloc(_frameData, GCHandleType.Pinned);
            _frameBitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, _frameHandle.AddrOfPinnedObject());

            _templateData = new byte[_frameData.Length];
            _templateHandle = GCHandle.Alloc(_templateData, GCHandleType.Pinned);

            // Prepare per-channel caches and a baseline template for the current set
            EnsurePerFrameCachesInitialized(0, Math.Max(1, SamplingRate / Math.Max(1, FramesPerSecond)));
            RebuildLayoutAndTemplateForVisible(_channels); // initial template uses all channels
            _visible = _channels.ToList();
            _visibleSignature = string.Join("-", _visible.Select(c => _channels.IndexOf(c)));
            _templateDirty = false;
            foreach (var ch in _channels)
                ch.LineWidth = ch.LineWidth;
        }

        public void Dispose()
        {
            _fillPath?.Dispose();
            _fillPath = null;

            _frameBitmap?.Dispose(); _frameBitmap = null;
            if (_frameHandle.IsAllocated) _frameHandle.Free();
            if (_templateHandle.IsAllocated)
                _templateHandle.Free();
            _frameData = null;
            _templateData = null;

            if (_pens != null)
                foreach (var p in _pens) p?.Dispose();
            if (_brushes != null)
                foreach (var b in _brushes) b?.Dispose();
            _pens = null; _brushes = null; _pointsPerChannel = null; _prevTrigger = null;
        }

        /// <summary>Reset activity/trigger history, e.g. after seeking backwards.</summary>
        public void ResetActivityState(int frameStartSample, int frameSamples)
        {
            if (_nextActiveSample != null)
                Array.Fill(_nextActiveSample, int.MinValue);
            if (_silentScannedUntil != null)
                Array.Fill(_silentScannedUntil, int.MinValue);
            if (_wasActive != null)
                Array.Fill(_wasActive, false);

            if (_prevTrigger != null)
            {
                // Reinitialize prevTrigger so GetTriggerPoint doesn't rely on "future" state
                for (int i = 0; i < _prevTrigger.Length; ++i)
                    _prevTrigger[i] = frameStartSample - frameSamples;
            }

            // Split slots hold history across frames too; clear them so seeks/export starts are clean.
            foreach (var ch in _channels)
                ch.ResetSlots(frameStartSample);

            // Also reset the "last frame" marker to the new position
            _lastFrameStartSample = frameStartSample;
        }

        /// <summary>
        /// Render a frame at absolute time in seconds
        /// Returns the reused pinned pixel buffer (BGRA, premultiplied).
        /// </summary>
        public byte[] RenderFrame(double posSeconds)
        {
            PrepareFrame(posSeconds);
            return RenderPreparedFrame();
        }

        /// <summary>
        /// Phase 1: computes triggers and the set of channels active at the given time.
        /// Returns the active channel count so the caller can set <see cref="LayoutSlots"/>
        /// (e.g. to the max of two renderers) before calling <see cref="RenderPreparedFrame"/>.
        /// </summary>
        public int PrepareFrame(double posSeconds)
        {
            if (_frameData == null)
                Init();
            _activeThisFrame.Clear();
            _framePrepared = false;
            if (_channels.Count == 0 || SamplingRate <= 0 || FramesPerSecond <= 0)
                return 0;

            // === Time → frame/sample math
            var maxSamples = (int)_channels.Max(c => c.SampleCount);
            var totalSeconds = maxSamples / (double)SamplingRate;
            var t = Math.Max(0.0, Math.Min(posSeconds, Math.Max(0.0, totalSeconds - 1e-6)));
            var frameIndex = (int)(t * FramesPerSecond);
            var frameSamples = Math.Max(1, SamplingRate / FramesPerSecond);
            var frameIndexSamples = (int)((long)frameIndex * SamplingRate / FramesPerSecond);
            if (_lastFrameStartSample != int.MinValue && frameIndexSamples < _lastFrameStartSample)
            {
                ResetActivityState(frameIndexSamples, frameSamples);
            }
            _lastFrameStartSample = frameIndexSamples;

            EnsurePerFrameCachesInitialized(frameIndex, frameSamples);
            if (_frameTriggers == null || _frameTriggers.Length != _channels.Count)
                _frameTriggers = new int[_channels.Count];

            // === Compute triggers; decide which channels are "active now" ===
            for (int i = 0; i < _channels.Count; ++i)
            {
                var ch = _channels[i];
                if (ch.IsEmpty || ch.Loading || ch.Failed || !string.IsNullOrEmpty(ch.ErrorMessage)) continue;

                int trig = ch.GetTriggerPoint(frameIndexSamples, frameSamples, _prevTrigger[i]);
                _prevTrigger[i] = trig;
                _frameTriggers[i] = trig;

                // Separate/Stacked: each split uses its own amplitude silence detection (with the
                // usual lookahead). The channel stays in the strip while any split is visible —
                // a silent split does not wait for its siblings to go quiet.
                if (ch.SplitCount > 1 &&
                    (ch.SplitLayout == SplitLayout.Separate || ch.SplitLayout == SplitLayout.Stacked))
                {
                    ch.UpdateSlots(frameIndexSamples, frameSamples, setSpanVisibility: false);
                    if (ApplySplitSlotActivity(ch, frameIndexSamples, frameSamples))
                        _activeThisFrame.Add(ch);
                    continue;
                }

                if (IsChannelActive(i, ch, trig, frameIndexSamples, frameSamples))
                {
                    // Overlaid (and any future mix-gated split): span-based slot visibility.
                    // If every split was cleared (voice buffers quiet), drop the channel so a
                    // silent multi-channel instrument does not leave an empty strip row.
                    if (ch.SplitCount > 1)
                    {
                        ch.UpdateSlots(frameIndexSamples, frameSamples);
                        if (ch.LayoutRowsThisFrame > 0)
                            _activeThisFrame.Add(ch);
                    }
                    else
                    {
                        ch.LayoutRowsThisFrame = 1;
                        _activeThisFrame.Add(ch);
                    }
                }
            }

            _framePrepared = true;
            return _activeThisFrame.Count;
        }

        /// <summary>
        /// Phase 2: renders the frame prepared by <see cref="PrepareFrame"/>, rebuilding the
        /// layout/template first if the active set or <see cref="LayoutSlots"/> changed.
        /// Returns the reused pinned pixel buffer (BGRA, premultiplied).
        /// </summary>
        public byte[] RenderPreparedFrame()
        {
            if (!_framePrepared)
                return _frameData;

            // The active set may be split across two strips; draw only our assigned channels.
            List<Channel> visibleNow;
            if (_renderFilter == null)
                visibleNow = _activeThisFrame;
            else
            {
                _renderVisibleScratch.Clear();
                foreach (var ch in _activeThisFrame)
                    if (_renderFilter.Contains(ch))
                        _renderVisibleScratch.Add(ch);
                visibleNow = _renderVisibleScratch;
            }

            // === If visible set changed (or template invalid), rebuild layout + template ===
            // Include each channel's label text, font size and colour so runtime label edits (caption,
            // size slider, track-hue changes — all baked into the template) rebuild it even while the
            // same set of channels stays visible.
            string sig = string.Join("-", visibleNow.Select(c =>
                _channels.IndexOf(c) + ":" + c.EffectiveLabel + ":" + (c.LabelFont?.Size ?? 0) + ":" + c.LabelColor.ToArgb()
                + ":" + (int)c.SplitLayout + ":" + c.SplitCount + ":" + VisibleSlotMask(c)));
            if (_templateDirty || sig != _visibleSignature)
            {
                _visibleSignature = sig;
                _visible = visibleNow.ToList();
                RebuildLayoutAndTemplateForVisible(_visible);
                _templateDirty = false;
            }

            // === Copy template, then draw visible waveforms ===
            Buffer.BlockCopy(_templateData, 0, _frameData, 0, _templateData.Length);

            using (var g = Graphics.FromImage(_frameBitmap))
            {
                foreach (var ch in _visible)
                {
                    int idx = _channels.IndexOf(ch);
                    // ViewWidthInSamples can change at runtime (waveform zoom), so keep the reused
                    // point buffer sized to it — RenderWave writes one point per sample.
                    int need = Math.Max(1, ch.ViewWidthInSamples);
                    if (_pointsPerChannel[idx].Length != need)
                        _pointsPerChannel[idx] = new PointF[need];
                    // FillColor can change at runtime (fill-opacity slider / hue keyframes), but the
                    // brush cache is only rebuilt on channel-count changes — refresh it in place here.
                    if (ch.FillColor == Color.Transparent)
                    {
                        _brushes[idx]?.Dispose();
                        _brushes[idx] = null;
                    }
                    else if (_brushes[idx] is SolidBrush sb)
                    {
                        if (sb.Color != ch.FillColor)
                            sb.Color = ch.FillColor;
                    }
                    else
                    {
                        _brushes[idx]?.Dispose();
                        _brushes[idx] = new SolidBrush(ch.FillColor);
                    }

                    if (ch.SplitPreparedThisFrame && ch.Slots != null)
                    {
                        var slots = ch.Slots;
                        for (int k = 0; k < slots.Length; ++k)
                        {
                            var slot = slots[k];
                            if (!slot.HasCurve || !slot.VisibleThisFrame || slot.Bounds.Width <= 0 || slot.Bounds.Height <= 0)
                                continue;
                            var pen = ch.SplitLayout == SplitLayout.Overlaid ? ch.GetSlotPen(k) : ch.Pen;
                            // Draw the slot from its own voice buffer (routed via GetSample).
                            ch.SetActiveVoice(ch.FrameVoiceSamples(k));
                            try { RenderWave(g, ch, slot.Bounds, slot.LastTrigger, pen, _brushes[idx], _pointsPerChannel[idx], _fillPath, ch.FillBase); }
                            finally { ch.SetActiveVoice(null); }
                        }
                    }
                    else
                    {
                        RenderWave(g, ch, ch.Bounds, _frameTriggers[idx], ch.Pen, _brushes[idx], _pointsPerChannel[idx], _fillPath, ch.FillBase);
                    }
                }
            }

            SwapRBInPlace(_frameData);
            return _frameData;
        }

        // --- helpers ---------------------------------------------------------

        static void SwapRBInPlace(byte[] data)
        {
            for (int i = 0; i < data.Length; i += 4)
            {
                byte b = data[i];       // B
                data[i] = data[i + 2]; // R -> B slot
                data[i + 2] = b;         // B -> R slot
                                         // G and A stay as-is (data[i+1], data[i+3])
            }
        }

        private void EnsurePerFrameCachesInitialized(int startFrame, int frameSamples)
        {
            if (_prevTrigger == null || _prevTrigger.Length != _channels.Count)
            {
                _prevTrigger = new int[_channels.Count];
                for (int i = 0; i < _channels.Count; ++i)
                    _prevTrigger[i] = (int)((long)startFrame * SamplingRate / Math.Max(1, FramesPerSecond)) - frameSamples;
            }

            if (_nextActiveSample == null || _nextActiveSample.Length != _channels.Count)
            {
                _nextActiveSample = new int[_channels.Count];
                _silentScannedUntil = new int[_channels.Count];
                _wasActive = new bool[_channels.Count];
                Array.Fill(_nextActiveSample, int.MinValue);
                Array.Fill(_silentScannedUntil, int.MinValue);
            }

            if (_pointsPerChannel == null || _pointsPerChannel.Length != _channels.Count)
                _pointsPerChannel = _channels.Select(ch => new PointF[Math.Max(1, ch.ViewWidthInSamples)]).ToArray();

            if (_pens == null || _pens.Length != _channels.Count)
            {
                if (_pens != null) foreach (var p in _pens) p?.Dispose();
                if (_brushes != null) foreach (var b in _brushes) b?.Dispose();

                _pens = _channels.Select(c => c.LineColor == Color.Transparent || c.LineWidth <= 0
                    ? null
                    : new Pen(c.LineColor, c.LineWidth * Width / 500) { MiterLimit = c.LineWidth, LineJoin = LineJoin.Bevel }).ToArray();
                _brushes = _channels.Select(c => c.FillColor == Color.Transparent ? null : new SolidBrush(c.FillColor)).ToArray();
            }
        }

        private bool IsChannelActive(int chIndex, Channel ch, int triggerPoint, int frameStartSample, int frameSamples)
        {
            int width = ActivityWindowSamplesOverride > 0 ? ActivityWindowSamplesOverride : ch.ViewWidthInSamples;
            int left = triggerPoint - width / 2;
            int right = left + width;

            int step = Math.Max(1, ActivitySubsampleStride);

            // Scan the current window; any sample crossing the threshold means active now
            for (int s = left; s < right; s += step)
            {
                float v = ch.GetSample(s, forTrigger: false);
                if ((v >= 0 ? v : -v) >= ActivityThreshold)
                {
                    // Lookahead cache is only valid for a contiguous silent stretch — reset it
                    _nextActiveSample[chIndex] = int.MinValue;
                    _silentScannedUntil[chIndex] = int.MinValue;
                    _wasActive[chIndex] = true;
                    return true;
                }
            }

            // Silent now — a hidden channel stays hidden until audio actually reaches the window;
            // the lookahead below only decides whether a visible channel keeps showing across a gap.
            if (!_wasActive[chIndex] || ch.ActivityLookaheadSeconds <= 0f)
            {
                _wasActive[chIndex] = false;
                return false;
            }

            int lookaheadSamples = (int)Math.Round(ch.ActivityLookaheadSeconds * SamplingRate);
            int scanTo = frameStartSample + frameSamples + lookaheadSamples;

            // Upcoming activity found on an earlier frame and not yet reached
            if (_nextActiveSample[chIndex] != int.MinValue && _nextActiveSample[chIndex] >= right)
                return _wasActive[chIndex] = _nextActiveSample[chIndex] < scanTo;
            _nextActiveSample[chIndex] = int.MinValue;

            // Scan only the stretch not already verified silent on earlier frames
            for (int s = Math.Max(right, _silentScannedUntil[chIndex]); s < scanTo; s += step)
            {
                float v = ch.GetSample(s, forTrigger: false);
                if ((v >= 0 ? v : -v) >= ActivityThreshold)
                {
                    _nextActiveSample[chIndex] = s;
                    _silentScannedUntil[chIndex] = s;
                    return true;
                }
            }
            _silentScannedUntil[chIndex] = scanTo;
            _wasActive[chIndex] = false;
            return false;
        }

        /// <summary>
        /// Per-slot amplitude silence detection for Separate/Stacked: each voice buffer is tested
        /// independently with the same window + lookahead rules as <see cref="IsChannelActive"/>.
        /// Sets <see cref="WaveSlot.VisibleThisFrame"/> and <see cref="Channel.LayoutRowsThisFrame"/>.
        /// Returns true when at least one slot should stay in the strip.
        /// </summary>
        private bool ApplySplitSlotActivity(Channel ch, int frameStartSample, int frameSamples)
        {
            var slots = ch.Slots;
            if (slots == null || !ch.SplitPreparedThisFrame)
            {
                ch.LayoutRowsThisFrame = 0;
                return false;
            }

            int visible = 0;
            for (int k = 0; k < slots.Length; ++k)
            {
                var slot = slots[k];
                ch.SetActiveVoice(ch.FrameVoiceSamples(k));
                try
                {
                    bool active = IsSlotActive(slot, ch, frameStartSample, frameSamples);
                    // HasCurve is required to occupy a row — same gate as RenderPreparedFrame.
                    slot.VisibleThisFrame = active && slot.HasCurve;
                }
                finally { ch.SetActiveVoice(null); }

                if (slot.VisibleThisFrame)
                    ++visible;
            }

            ch.LayoutRowsThisFrame = ch.SplitLayout == SplitLayout.Separate
                ? visible
                : (visible > 0 ? 1 : 0);
            return visible > 0;
        }

        /// <summary>
        /// Same amplitude + lookahead rules as <see cref="IsChannelActive"/>, but for one voice
        /// buffer (already routed via <see cref="Channel.SetActiveVoice"/>) and one slot's caches.
        /// </summary>
        private bool IsSlotActive(WaveSlot slot, Channel ch, int frameStartSample, int frameSamples)
        {
            int width = ActivityWindowSamplesOverride > 0 ? ActivityWindowSamplesOverride : ch.ViewWidthInSamples;
            // Centre on the playhead — not the held oscilloscope trigger, which would keep
            // reading the last loud cycle forever after the voice goes quiet.
            int triggerPoint = frameStartSample + frameSamples / 2;
            int left = triggerPoint - width / 2;
            int right = left + width;

            int step = Math.Max(1, ActivitySubsampleStride);

            for (int s = left; s < right; s += step)
            {
                float v = ch.GetSample(s, forTrigger: false);
                if ((v >= 0 ? v : -v) >= ActivityThreshold)
                {
                    slot.NextActiveSample = int.MinValue;
                    slot.SilentScannedUntil = int.MinValue;
                    slot.WasActive = true;
                    return true;
                }
            }

            if (!slot.WasActive || ch.ActivityLookaheadSeconds <= 0f)
            {
                slot.WasActive = false;
                return false;
            }

            int lookaheadSamples = (int)Math.Round(ch.ActivityLookaheadSeconds * SamplingRate);
            int scanTo = frameStartSample + frameSamples + lookaheadSamples;

            if (slot.NextActiveSample != int.MinValue && slot.NextActiveSample >= right)
                return slot.WasActive = slot.NextActiveSample < scanTo;
            slot.NextActiveSample = int.MinValue;

            for (int s = Math.Max(right, slot.SilentScannedUntil); s < scanTo; s += step)
            {
                float v = ch.GetSample(s, forTrigger: false);
                if ((v >= 0 ? v : -v) >= ActivityThreshold)
                {
                    slot.NextActiveSample = s;
                    slot.SilentScannedUntil = s;
                    return true;
                }
            }
            slot.SilentScannedUntil = scanTo;
            slot.WasActive = false;
            return false;
        }

        /// <summary>Rebuild channel.Bounds layout for the given visible set and repaint the template into _templateData.</summary>
        private void RebuildLayoutAndTemplateForVisible(IReadOnlyList<Channel> visible)
        {
            if (_frameBitmap == null)
                return;
            // Determine rendering area
            var rb = RenderingBounds;
            if (rb.Width == 0 || rb.Height == 0)
                rb = new Rectangle(0, 0, Width, Height);

            // Compute per-channel rectangles (stack vertically regardless of Columns when dynamic).
            // The layout unit is a "row": most channels take one row, but a Separate-mode split
            // channel takes one row per visible slot. With LayoutSlots > rows, channels fill the top
            // rows and the rest stays transparent so two side-by-side renderers keep identical heights.
            int count = visible.Count;
            int rows = 0;
            foreach (var ch in visible)
                rows += LayoutRowsOf(ch);
            int slots = Math.Max(rows, LayoutSlots);
            // Region actually occupied by channels (used for per-channel border edge detection).
            var channelsRect = count == 0
                ? Rectangle.Empty
                : new Rectangle(rb.Left, rb.Top, rb.Width, rows * rb.Height / slots);
            // The dark overlay always spans the full slot grid, so with two side-by-side strips the
            // sparser side still shows the backdrop where its extra (empty) slots are — no gap even
            // when the other side has more active tracks.
            var bgRect = slots == 0
                ? Rectangle.Empty
                : new Rectangle(rb.Left, rb.Top, rb.Width, rb.Height);

            if (count == 0)
            {
                // No visible channels → background overlay only (transparent where no waveform is drawn)
                using (var templateBmp = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, _templateHandle.AddrOfPinnedObject()))
                using (var g = Graphics.FromImage(templateBmp))
                {
                    DrawBackground(g, bgRect);
                }
                return;
            }

            int row = 0;
            for (int i = 0; i < count; ++i)
            {
                var ch = visible[i];
                int chRows = LayoutRowsOf(ch);
                int y0 = rb.Top + row * rb.Height / slots;
                int y1 = rb.Top + (row + chRows) * rb.Height / slots;
                ch.Bounds = new Rectangle(rb.Left, y0, rb.Width, y1 - y0);
                AssignSlotBounds(ch, slots, rb, row);
                row += chRows;
            }

            using (var templateBmp = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, _templateHandle.AddrOfPinnedObject()))
            using (var g = Graphics.FromImage(templateBmp))
            {
                // Background
                DrawBackground(g, bgRect);

                // Per-channel static items (bg box, zero line, border, label)
                foreach (var ch in visible)
                    DrawChannelTemplate(g, ch, channelsRect);
            }
        }

        /// <summary>Rows a channel occupies in the strip layout (Separate split = one per visible slot).</summary>
        private static int LayoutRowsOf(Channel ch)
            => ch.SplitCount > 1 ? Math.Max(1, ch.LayoutRowsThisFrame) : 1;

        /// <summary>Bitmask of a split channel's currently-visible slots (0 for non-split), for the template signature.</summary>
        private static int VisibleSlotMask(Channel ch)
        {
            if (!ch.SplitPreparedThisFrame || ch.Slots == null)
                return 0;
            int m = 0;
            for (int k = 0; k < ch.Slots.Length; ++k)
                if (ch.Slots[k].VisibleThisFrame)
                    m |= 1 << k;
            return m;
        }

        /// <summary>Assign each split slot its drawing rectangle within the channel's row(s).</summary>
        private void AssignSlotBounds(Channel ch, int slots, Rectangle rb, int startRow)
        {
            if (ch.SplitCount <= 1 || ch.Slots == null)
                return;
            var s = ch.Slots;
            switch (ch.SplitLayout)
            {
                case SplitLayout.Overlaid:
                    foreach (var slot in s)
                        slot.Bounds = ch.Bounds;
                    break;
                case SplitLayout.Separate:
                    // Each visible slot (slot-index order) gets its own full row.
                    int r = startRow;
                    foreach (var slot in s)
                    {
                        if (!slot.VisibleThisFrame)
                        {
                            slot.Bounds = Rectangle.Empty;
                            continue;
                        }
                        int y0 = rb.Top + r * rb.Height / slots;
                        int y1 = rb.Top + (r + 1) * rb.Height / slots;
                        slot.Bounds = new Rectangle(rb.Left, y0, rb.Width, y1 - y0);
                        ++r;
                    }
                    break;
                default: // Stacked: vertical slices for currently-visible slots only (reflow)
                    int n = 0;
                    foreach (var slot in s)
                        if (slot.VisibleThisFrame)
                            ++n;
                    if (n == 0)
                    {
                        foreach (var slot in s)
                            slot.Bounds = Rectangle.Empty;
                        break;
                    }
                    int vi = 0;
                    foreach (var slot in s)
                    {
                        if (!slot.VisibleThisFrame)
                        {
                            slot.Bounds = Rectangle.Empty;
                            continue;
                        }
                        int y0 = ch.Bounds.Top + vi * ch.Bounds.Height / n;
                        int y1 = ch.Bounds.Top + (vi + 1) * ch.Bounds.Height / n;
                        slot.Bounds = new Rectangle(ch.Bounds.Left, y0, ch.Bounds.Width, y1 - y0);
                        ++vi;
                    }
                    break;
            }
        }

        private void DrawBackground(Graphics g, Rectangle bgRect)
        {
            //Clear
            var oldMode = g.CompositingMode;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.Clear(Color.Transparent);
            g.CompositingMode = oldMode;

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (bgRect.Width <= 0 || bgRect.Height <= 0)
                return;

            if (BackgroundImage != null)
            {
                using (var attribute = new ImageAttributes())
                {
                    attribute.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(
                        BackgroundImage,
                        bgRect,
                        0, 0, BackgroundImage.Width, BackgroundImage.Height,
                        GraphicsUnit.Pixel,
                        attribute);
                }
            }
            else
            {
                using (var brush = new SolidBrush(BackgroundColor))
                    g.FillRectangle(brush, bgRect.Left - 1, bgRect.Top - 1, bgRect.Width + 1, bgRect.Height + 1);
            }
        }

        private static void DrawZeroLine(Graphics g, Channel channel, Rectangle bounds)
        {
            if (channel.ZeroLineColor == Color.Transparent || channel.ZeroLineWidth <= 0)
                return;
            using (var pen = new Pen(channel.ZeroLineColor, channel.ZeroLineWidth))
            {
                g.DrawLine(pen,
                    bounds.Left,
                    bounds.Top + bounds.Height / 2,
                    bounds.Right,
                    bounds.Top + bounds.Height / 2);
            }
        }

        private static void DrawBorder(Graphics g, Channel channel, Rectangle bounds, Rectangle rb)
        {
            if (channel.BorderWidth <= 0 || channel.BorderColor == Color.Transparent)
                return;
            using (var pen = new Pen(channel.BorderColor, channel.BorderWidth))
            {
                if (channel.BorderEdges)
                {
                    // Pull in 1px on right/bottom.
                    g.DrawRectangle(
                        pen,
                        bounds.Left,
                        bounds.Top,
                        bounds.Width - (bounds.Right == rb.Right ? 1 : 0),
                        bounds.Height - (bounds.Bottom == rb.Bottom ? 1 : 0));
                }
                else
                {
                    if (bounds.Left != rb.Left)
                        g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom);

                    if (bounds.Top != rb.Top)
                        g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Top);

                    if (bounds.Right != rb.Right)
                        g.DrawLine(pen, bounds.Right, bounds.Top, bounds.Right, bounds.Bottom);

                    if (bounds.Bottom != rb.Bottom)
                        g.DrawLine(pen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
                }
            }
        }

        private void DrawChannelTemplate(Graphics g, Channel channel, Rectangle rb)
        {
            if (channel.BackgroundColor != Color.Transparent)
            {
                using (var b = new SolidBrush(channel.BackgroundColor))
                    g.FillRectangle(b, channel.Bounds);
            }

            // Zero lines and borders: per sub-rect for a split channel, else the whole channel row.
            if (channel.SplitPreparedThisFrame && channel.Slots != null)
            {
                switch (channel.SplitLayout)
                {
                    case SplitLayout.Overlaid:
                        DrawZeroLine(g, channel, channel.Bounds);
                        DrawBorder(g, channel, channel.Bounds, rb);
                        break;
                    case SplitLayout.Separate:
                        foreach (var slot in channel.Slots)
                        {
                            if (!slot.VisibleThisFrame || slot.Bounds.Width <= 0 || slot.Bounds.Height <= 0)
                                continue;
                            DrawZeroLine(g, channel, slot.Bounds);
                            DrawBorder(g, channel, slot.Bounds, rb);
                        }
                        break;
                    default: // Stacked: zero line per visible slice; one border around the channel
                        foreach (var slot in channel.Slots)
                        {
                            if (!slot.VisibleThisFrame || slot.Bounds.Width <= 0 || slot.Bounds.Height <= 0)
                                continue;
                            DrawZeroLine(g, channel, slot.Bounds);
                        }
                        DrawBorder(g, channel, channel.Bounds, rb);
                        break;
                }
            }
            else
            {
                DrawZeroLine(g, channel, channel.Bounds);
                DrawBorder(g, channel, channel.Bounds, rb);
            }

            if (channel.LabelFont != null && channel.LabelColor != Color.Transparent)
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                using (var brush = new SolidBrush(channel.LabelColor))
                {
                    var stringFormat = new StringFormat();
                    switch (channel.LabelAlignment)
                    {
                        case ContentAlignment.TopLeft: stringFormat.Alignment = StringAlignment.Near; stringFormat.LineAlignment = StringAlignment.Near; break;
                        case ContentAlignment.TopCenter: stringFormat.Alignment = StringAlignment.Center; stringFormat.LineAlignment = StringAlignment.Near; break;
                        case ContentAlignment.TopRight: stringFormat.Alignment = StringAlignment.Far; stringFormat.LineAlignment = StringAlignment.Near; break;
                        case ContentAlignment.MiddleLeft: stringFormat.Alignment = StringAlignment.Near; stringFormat.LineAlignment = StringAlignment.Center; break;
                        case ContentAlignment.MiddleCenter: stringFormat.Alignment = StringAlignment.Center; stringFormat.LineAlignment = StringAlignment.Center; break;
                        case ContentAlignment.MiddleRight: stringFormat.Alignment = StringAlignment.Far; stringFormat.LineAlignment = StringAlignment.Center; break;
                        case ContentAlignment.BottomLeft: stringFormat.Alignment = StringAlignment.Near; stringFormat.LineAlignment = StringAlignment.Far; break;
                        case ContentAlignment.BottomCenter: stringFormat.Alignment = StringAlignment.Center; stringFormat.LineAlignment = StringAlignment.Far; break;
                        case ContentAlignment.BottomRight: stringFormat.Alignment = StringAlignment.Far; stringFormat.LineAlignment = StringAlignment.Far; break;
                        default: throw new ArgumentOutOfRangeException();
                    }

                    // Separate: the same track label is drawn above each visible split row.
                    if (channel.SplitLayout == SplitLayout.Separate
                        && channel.SplitPreparedThisFrame
                        && channel.Slots != null)
                    {
                        foreach (var slot in channel.Slots)
                        {
                            if (!slot.VisibleThisFrame || slot.Bounds.Width <= 0 || slot.Bounds.Height <= 0)
                                continue;
                            var layoutRectangle = new RectangleF(
                                slot.Bounds.Left + channel.LabelMargins.Left,
                                slot.Bounds.Top + channel.LabelMargins.Top,
                                slot.Bounds.Width - channel.LabelMargins.Left - channel.LabelMargins.Right,
                                slot.Bounds.Height - channel.LabelMargins.Top - channel.LabelMargins.Bottom);
                            g.DrawString(channel.EffectiveLabel, channel.LabelFont, brush, layoutRectangle, stringFormat);
                        }
                    }
                    else
                    {
                        var layoutRectangle = new RectangleF(
                            channel.Bounds.Left + channel.LabelMargins.Left,
                            channel.Bounds.Top + channel.LabelMargins.Top,
                            channel.Bounds.Width - channel.LabelMargins.Left - channel.LabelMargins.Right,
                            channel.Bounds.Height - channel.LabelMargins.Top - channel.LabelMargins.Bottom);
                        g.DrawString(channel.EffectiveLabel, channel.LabelFont, brush, layoutRectangle, stringFormat);
                    }
                }
            }
        }

        private void RenderWave(Graphics g, Channel channel, Rectangle bounds, int triggerPoint, Pen pen, Brush brush, PointF[] points, GraphicsPath path, double fillBase)
        {
            var leftmostSampleIndex = triggerPoint - channel.ViewWidthInSamples / 2;

            float xOffset = bounds.Left;
            float xScale = (float)bounds.Width / channel.ViewWidthInSamples;
            float yOffset = bounds.Top + bounds.Height * 0.5f;
            float yScale = -bounds.Height * 0.5f;

            for (int i = 0; i < channel.ViewWidthInSamples; ++i)
            {
                var sampleValue = channel.GetSample(leftmostSampleIndex + i, false);
                points[i].X = xOffset + i * xScale;
                points[i].Y = yOffset + sampleValue * yScale;
            }

            if (channel.Clip)
            {
                for (int i = 0; i < channel.ViewWidthInSamples; ++i)
                    points[i].Y = Math.Min(Math.Max(points[i].Y, bounds.Top), bounds.Bottom);
            }

            g.SmoothingMode = channel.SmoothLines ? SmoothingMode.HighQuality : SmoothingMode.None;

            g.DrawLines(pen, points);

            if (brush != null)
            {
                var baseY = (float)(yOffset + bounds.Height * -0.5 * fillBase);
                path.Reset();
                path.AddLine(points[0].X, baseY, points[0].X, points[0].Y);
                path.AddLines(points);
                path.AddLine(points[points.Length - 1].X, points[points.Length - 1].Y, points[points.Length - 1].X, baseY);
                g.FillPath(brush, path);
            }
        }

        internal void ClearChannels()
        {
            if (_channels.Count == 0)
                return;
            _channels.Clear();
        }

        internal void RemoveChannel(Channel sidWizChannel)
        {
            _channels.Remove(sidWizChannel);
        }
    }
}
