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
        private readonly GraphicsPath _fillPath = new GraphicsPath();

        // Dynamic active set
        private List<Channel> _visible = new List<Channel>();
        private string _visibleSignature = "";

        // Per-frame state computed by PrepareFrame and consumed by RenderPreparedFrame
        private readonly List<Channel> _activeThisFrame = new List<Channel>();
        private int[] _frameTriggers;
        private bool _framePrepared;

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

                if (IsChannelActive(i, ch, trig, frameIndexSamples, frameSamples))
                    _activeThisFrame.Add(ch);
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

            // === If active set changed (or template invalid), rebuild layout + template ===
            string sig = string.Join("-", _activeThisFrame.Select(c => _channels.IndexOf(c)));
            if (_templateDirty || sig != _visibleSignature)
            {
                _visibleSignature = sig;
                _visible = _activeThisFrame.ToList();
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
                    RenderWave(g, ch, _frameTriggers[idx], _brushes[idx], _pointsPerChannel[idx], _fillPath, ch.FillBase);
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
            // With LayoutSlots > count, channels fill the top rows and the rest stays transparent
            // so two side-by-side renderers keep identical track heights.
            int count = visible.Count;
            int slots = Math.Max(count, LayoutSlots);
            // Region actually occupied by channels (used for per-channel border edge detection).
            var channelsRect = count == 0
                ? Rectangle.Empty
                : new Rectangle(rb.Left, rb.Top, rb.Width, count * rb.Height / slots);
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

            for (int i = 0; i < count; ++i)
            {
                int x0 = rb.Left, x1 = rb.Right;
                int y0 = rb.Top + i * rb.Height / slots;
                int y1 = rb.Top + (i + 1) * rb.Height / slots;
                visible[i].Bounds = new Rectangle(x0, y0, x1 - x0, y1 - y0);
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

        private void DrawChannelTemplate(Graphics g, Channel channel, Rectangle rb)
        {
            if (channel.BackgroundColor != Color.Transparent)
            {
                using (var b = new SolidBrush(channel.BackgroundColor))
                    g.FillRectangle(b, channel.Bounds);
            }

            if (channel.ZeroLineColor != Color.Transparent && channel.ZeroLineWidth > 0)
            {
                using (var pen = new Pen(channel.ZeroLineColor, channel.ZeroLineWidth))
                {
                    g.DrawLine(pen,
                        channel.Bounds.Left,
                        channel.Bounds.Top + channel.Bounds.Height / 2,
                        channel.Bounds.Right,
                        channel.Bounds.Top + channel.Bounds.Height / 2);
                }
            }

            if (channel.BorderWidth > 0 && channel.BorderColor != Color.Transparent)
            {
                using (var pen = new Pen(channel.BorderColor, channel.BorderWidth))
                {
                    if (channel.BorderEdges)
                    {
                        // Pull in 1px on right/bottom.
                        g.DrawRectangle(
                            pen,
                            channel.Bounds.Left,
                            channel.Bounds.Top,
                            channel.Bounds.Width - (channel.Bounds.Right == rb.Right ? 1 : 0),
                            channel.Bounds.Height - (channel.Bounds.Bottom == rb.Bottom ? 1 : 0));
                    }
                    else
                    {
                        if (channel.Bounds.Left != rb.Left)
                            g.DrawLine(pen, channel.Bounds.Left, channel.Bounds.Top, channel.Bounds.Left, channel.Bounds.Bottom);

                        if (channel.Bounds.Top != rb.Top)
                            g.DrawLine(pen, channel.Bounds.Left, channel.Bounds.Top, channel.Bounds.Right, channel.Bounds.Top);

                        if (channel.Bounds.Right != rb.Right)
                            g.DrawLine(pen, channel.Bounds.Right, channel.Bounds.Top, channel.Bounds.Right, channel.Bounds.Bottom);

                        if (channel.Bounds.Bottom != rb.Bottom)
                            g.DrawLine(pen, channel.Bounds.Left, channel.Bounds.Bottom, channel.Bounds.Right, channel.Bounds.Bottom);
                    }
                }
            }

            if (channel.LabelFont != null && channel.LabelColor != Color.Transparent)
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                using (var brush = new SolidBrush(channel.LabelColor))
                {
                    var stringFormat = new StringFormat();
                    var layoutRectangle = new RectangleF(
                        channel.Bounds.Left + channel.LabelMargins.Left,
                        channel.Bounds.Top + channel.LabelMargins.Top,
                        channel.Bounds.Width - channel.LabelMargins.Left - channel.LabelMargins.Right,
                        channel.Bounds.Height - channel.LabelMargins.Top - channel.LabelMargins.Bottom);

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

                    g.DrawString(channel.Label, channel.LabelFont, brush, layoutRectangle, stringFormat);
                }
            }
        }

        private void RenderWave(Graphics g, Channel channel, int triggerPoint, Brush brush, PointF[] points, GraphicsPath path, double fillBase)
        {
            var leftmostSampleIndex = triggerPoint - channel.ViewWidthInSamples / 2;

            float xOffset = channel.Bounds.Left;
            float xScale = (float)channel.Bounds.Width / channel.ViewWidthInSamples;
            float yOffset = channel.Bounds.Top + channel.Bounds.Height * 0.5f;
            float yScale = -channel.Bounds.Height * 0.5f;

            for (int i = 0; i < channel.ViewWidthInSamples; ++i)
            {
                var sampleValue = channel.GetSample(leftmostSampleIndex + i, false);
                points[i].X = xOffset + i * xScale;
                points[i].Y = yOffset + sampleValue * yScale;
            }

            if (channel.Clip)
            {
                for (int i = 0; i < channel.ViewWidthInSamples; ++i)
                    points[i].Y = Math.Min(Math.Max(points[i].Y, channel.Bounds.Top), channel.Bounds.Bottom);
            }

            g.SmoothingMode = channel.SmoothLines ? SmoothingMode.HighQuality : SmoothingMode.None;

            g.DrawLines(channel.Pen, points);

            if (brush != null)
            {
                var baseY = (float)(yOffset + channel.Bounds.Height * -0.5 * fillBase);
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
