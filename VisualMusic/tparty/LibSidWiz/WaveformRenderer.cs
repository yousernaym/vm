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

        // Activity detection knobs
        public float ActivityThreshold = 0.004f;   // ~ -48 dBFS; tweak
        public int ActivityWindowSamplesOverride = 0; // 0 => use ViewWidthInSamples
        public int ActivitySubsampleStride = 4;  // >=1; higher = faster
        public float ActivityHoldBelowThresholdSeconds = 2.0f; // hang time after dropping below threshold
        private int[] _lastAboveThresholdSample;    // Per-channel activity state
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
            if (_lastAboveThresholdSample != null)
                Array.Fill(_lastAboveThresholdSample, int.MinValue);

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
            if (_frameData == null)
                Init();
            if (_channels.Count == 0 || SamplingRate <= 0 || FramesPerSecond <= 0)
                return _frameData;

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

            // === Compute triggers; decide which channels are "active now" ===
            Span<int> triggers = stackalloc int[_channels.Count];
            var active = new List<Channel>(_channels.Count);
            for (int i = 0; i < _channels.Count; ++i)
            {
                var ch = _channels[i];
                if (ch.IsEmpty || ch.Loading || !string.IsNullOrEmpty(ch.ErrorMessage)) continue;

                int trig = ch.GetTriggerPoint(frameIndexSamples, frameSamples, _prevTrigger[i]);
                _prevTrigger[i] = trig;
                triggers[i] = trig;

                if (IsChannelActive(i, ch, trig, frameIndexSamples, frameSamples))
                    active.Add(ch);
            }

            // === If active set changed (or template invalid), rebuild layout + template ===
            string sig = string.Join("-", active.Select(c => _channels.IndexOf(c)));
            if (_templateDirty || sig != _visibleSignature)
            {
                _visibleSignature = sig;
                _visible = active;
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
                    RenderWave(g, ch, triggers[idx], _brushes[idx], _pointsPerChannel[idx], _fillPath, ch.FillBase);
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

            if (_lastAboveThresholdSample == null || _lastAboveThresholdSample.Length != _channels.Count)
            {
                _lastAboveThresholdSample = new int[_channels.Count];
                for (int i = 0; i < _channels.Count; ++i) _lastAboveThresholdSample[i] = int.MinValue;
            }

            if (_pointsPerChannel == null || _pointsPerChannel.Length != _channels.Count)
                _pointsPerChannel = _channels.Select(ch => new PointF[Math.Max(1, ch.ViewWidthInSamples)]).ToArray();

            if (_pens == null || _pens.Length != _channels.Count)
            {
                if (_pens != null) foreach (var p in _pens) p?.Dispose();
                if (_brushes != null) foreach (var b in _brushes) b?.Dispose();

                _pens = _channels.Select(c => c.LineColor == Color.Transparent || c.LineWidth <= 0
                    ? null
                    : new Pen(c.LineColor, c.LineWidth) { MiterLimit = c.LineWidth, LineJoin = LineJoin.Bevel }).ToArray();
                _brushes = _channels.Select(c => c.FillColor == Color.Transparent ? null : new SolidBrush(c.FillColor)).ToArray();
            }
        }

        private bool IsChannelActive(int chIndex, Channel ch, int triggerPoint, int frameStartSample, int frameSamples)
        {
            int width = ActivityWindowSamplesOverride > 0 ? ActivityWindowSamplesOverride : ch.ViewWidthInSamples;
            int left = triggerPoint - width / 2;
            int right = left + width;

            float peak = 0f;
            int step = Math.Max(1, ActivitySubsampleStride);

            // Scan the window; if any sample crosses the threshold, update timestamp and return active
            for (int s = left; s < right; s += step)
            {
                float v = ch.GetSample(s, forTrigger: false);
                float a = v >= 0 ? v : -v;
                if (a > peak) peak = a;

                if (a >= ActivityThreshold)
                {
                    // mark the latest moment we observed activity; use the end of this frame as the timestamp
                    _lastAboveThresholdSample[chIndex] = frameStartSample + frameSamples - 1;
                    return true;
                }
            }

            // No sample crossed the threshold in this window — apply hold
            if (ActivityHoldBelowThresholdSeconds > 0f)
            {
                int holdSamples = Math.Max(0, (int)Math.Round(ActivityHoldBelowThresholdSeconds * SamplingRate));
                int samplesSinceHit = frameStartSample + frameSamples - 1 - _lastAboveThresholdSample[chIndex];
                if (_lastAboveThresholdSample[chIndex] != int.MinValue && samplesSinceHit <= holdSamples)
                    return true; // still within hang time
            }

            // Finally, without hold, fall back to pure threshold decision
            return peak >= ActivityThreshold;
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

            // Compute per-channel rectangles (stack vertically regardless of Columns when dynamic)
            int count = visible.Count;
            if (count == 0)
            {
                // Just background
                using (var templateBmp = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, _templateHandle.AddrOfPinnedObject()))
                using (var g = Graphics.FromImage(templateBmp))
                {
                    DrawBackground(g, rb);
                }
                return;
            }

            for (int i = 0; i < count; ++i)
            {
                int x0 = rb.Left, x1 = rb.Right;
                int y0 = rb.Top + i * rb.Height / count;
                int y1 = rb.Top + (i + 1) * rb.Height / count;
                visible[i].Bounds = new Rectangle(x0, y0, x1 - x0, y1 - y0);
            }

            using (var templateBmp = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, _templateHandle.AddrOfPinnedObject()))
            using (var g = Graphics.FromImage(templateBmp))
            {
                // Background
                DrawBackground(g, rb);

                // Per-channel static items (bg box, zero line, border, label)
                foreach (var ch in visible)
                    DrawChannelTemplate(g, ch, rb);
            }
        }

        private void DrawBackground(Graphics g, Rectangle rb)
        {
            //Clear
            var oldMode = g.CompositingMode;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.Clear(Color.Transparent);
            g.CompositingMode = oldMode;

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (BackgroundImage != null)
            {
                using (var attribute = new ImageAttributes())
                {
                    attribute.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(
                        BackgroundImage,
                        new Rectangle(0, 0, Width, Height),
                        0, 0, BackgroundImage.Width, BackgroundImage.Height,
                        GraphicsUnit.Pixel,
                        attribute);
                }
            }
            else
            {
                using (var brush = new SolidBrush(BackgroundColor))
                    g.FillRectangle(brush, -1, -1, Width + 1, Height + 1);
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
    }
}
