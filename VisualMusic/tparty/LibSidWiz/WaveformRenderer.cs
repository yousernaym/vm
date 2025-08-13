using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using LibSidWiz.Outputs;

namespace LibSidWiz
{
    /// <summary>
    /// Class responsible for rendering
    /// </summary>
    public class WaveformRenderer : IDisposable
    {
        private readonly List<Channel> _channels = new List<Channel>();

        // --- Backing fields so we can mark the template dirty on changes (NEW) ---
        private int _width;
        private int _height;
        private int _columns;
        private int _samplingRate;
        private int _framesPerSecond;
        private Color _backgroundColor = Color.Black;
        private Image _backgroundImage;
        private Rectangle _renderingBounds;

        public int Width { get => _width; set { _width = value; _templateDirty = true; } }                        // NEW
        public int Height { get => _height; set { _height = value; _templateDirty = true; } }                      // NEW
        public int Columns { get => _columns; set { _columns = value; _templateDirty = true; } }                   // NEW
        public int SamplingRate { get => _samplingRate; set { _samplingRate = value; } }
        public int FramesPerSecond { get => _framesPerSecond; set { _framesPerSecond = value; } }
        public Color BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; _templateDirty = true; } } // NEW
        public Image BackgroundImage { get => _backgroundImage; set { _backgroundImage = value; _templateDirty = true; } } // NEW
        public Rectangle RenderingBounds { get => _renderingBounds; set { _renderingBounds = value; _templateDirty = true; } } // NEW

        public void AddChannel(Channel channel)
        {
            _channels.Add(channel);
            _templateDirty = true; // NEW — any change to channel set requires template/layout rebuild. :contentReference[oaicite:0]{index=0}
        }

        // =====================================================================
        //                           NO-ALLOC RENDERER (NEW)
        // =====================================================================

        // Pinned, reusable buffers
        private byte[] _templateData;              // premultiplied BGRA template (bg, labels, borders, zero lines)
        private byte[] _frameData;                 // per-frame buffer to draw into
        private GCHandle _templateHandle;
        private GCHandle _frameHandle;
        private Bitmap _frameBitmap;               // GDI+ draws into _frameData directly
        private bool _templateDirty = true;

        // Reused per-channel resources
        private PointF[][] _pointsPerChannel;      // [channel][sample] to avoid per-frame allocs
        private Pen[] _pens;
        private Brush[] _brushes;
        private int[] _prevTrigger;                // previous trigger per channel
        private readonly GraphicsPath _fillPath = new GraphicsPath();

        // Dynamic active set
        private List<Channel> _visible = new List<Channel>();
        private string _visibleSignature = "";

        // Activity detection knobs (NEW)
        public float ActivityThreshold = 0.004f;   // ~ -48 dBFS; tweak
        public int ActivityWindowSamplesOverride = 0; // 0 => use ViewWidthInSamples
        public int ActivitySubsampleStride = 4;  // >=1; higher = faster

        /// <summary>Allocate and pin buffers; (re)build template.</summary>
        public void Init()
        {
            Dispose();

            if (Width <= 0 || Height <= 0) throw new InvalidOperationException("Width/Height must be set");

            BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 0, 0);

            _frameData = new byte[Width * Height * 4];
            _frameHandle = GCHandle.Alloc(_frameData, GCHandleType.Pinned);
            _frameBitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, _frameHandle.AddrOfPinnedObject());

            _templateData = new byte[_frameData.Length];
            _templateHandle = GCHandle.Alloc(_templateData, GCHandleType.Pinned);

            // Prepare per-channel caches and a baseline template for the current set
            EnsurePerFrameCachesInitialized(0, Math.Max(1, SamplingRate / Math.Max(1, FramesPerSecond))); // seed
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
            if (_templateHandle.IsAllocated) _templateHandle.Free();
            _frameData = null;
            _templateData = null;

            if (_pens != null) foreach (var p in _pens) p?.Dispose();
            if (_brushes != null) foreach (var b in _brushes) b?.Dispose();
            _pens = null; _brushes = null; _pointsPerChannel = null; _prevTrigger = null;
        }

        /// <summary>
        /// Render a frame (no allocations) at absolute time in seconds (use your SongPosS).
        /// Returns the reused pinned pixel buffer (BGRA, premultiplied).
        /// </summary>
        public byte[] RenderFrame(double seconds)
        {
            if (_frameData == null) Init();
            if (_channels.Count == 0 || SamplingRate <= 0 || FramesPerSecond <= 0) return _frameData;

            // === Time → frame/sample math (same as your original logic) ===
            var maxSamples = (int)_channels.Max(c => c.SampleCount);                     // longest stream. :contentReference[oaicite:1]{index=1}
            var totalSeconds = maxSamples / (double)SamplingRate;
            var t = Math.Max(0.0, Math.Min(seconds, Math.Max(0.0, totalSeconds - 1e-6)));
            var frameIndex = (int)(t * FramesPerSecond);
            var frameSamples = Math.Max(1, SamplingRate / FramesPerSecond);
            var frameIndexSamples = (int)((long)frameIndex * SamplingRate / FramesPerSecond); // int math. :contentReference[oaicite:2]{index=2}

            EnsurePerFrameCachesInitialized(frameIndex, frameSamples);

            // === Compute triggers; decide which channels are "active now" ===
            Span<int> triggers = stackalloc int[_channels.Count];
            var active = new List<Channel>(_channels.Count);
            for (int i = 0; i < _channels.Count; ++i)
            {
                var ch = _channels[i];
                if (ch.IsEmpty || ch.Loading || !string.IsNullOrEmpty(ch.ErrorMessage)) continue;

                int trig = ch.GetTriggerPoint(frameIndexSamples, frameSamples, _prevTrigger[i]); // phase‑coherent. :contentReference[oaicite:3]{index=3}
                _prevTrigger[i] = trig;
                triggers[i] = trig;

                if (IsChannelActive(ch, trig))
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
            Buffer.BlockCopy(_templateData, 0, _frameData, 0, _templateData.Length);     // fast copy. :contentReference[oaicite:4]{index=4}

            using (var g = Graphics.FromImage(_frameBitmap))
            {
                foreach (var ch in _visible)
                {
                    int idx = _channels.IndexOf(ch);
                    RenderWave(g, ch, triggers[idx], _pens[idx], _brushes[idx], _pointsPerChannel[idx], _fillPath, ch.FillBase); // draw. :contentReference[oaicite:6]{index=6}
                }
            }

            return _frameData;
        }

        // --- helpers ---------------------------------------------------------

        private void EnsurePerFrameCachesInitialized(int startFrame, int frameSamples)
        {
            if (_prevTrigger == null || _prevTrigger.Length != _channels.Count)
            {
                _prevTrigger = new int[_channels.Count];
                for (int i = 0; i < _channels.Count; ++i)
                    _prevTrigger[i] = (int)((long)startFrame * SamplingRate / Math.Max(1, FramesPerSecond)) - frameSamples; // seed. :contentReference[oaicite:7]{index=7}
            }

            if (_pointsPerChannel == null || _pointsPerChannel.Length != _channels.Count)
                _pointsPerChannel = _channels.Select(ch => new PointF[Math.Max(1, ch.ViewWidthInSamples)]).ToArray(); // reuse points. :contentReference[oaicite:8]{index=8}

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

        private bool IsChannelActive(Channel ch, int triggerPoint)
        {
            int width = ActivityWindowSamplesOverride > 0 ? ActivityWindowSamplesOverride : ch.ViewWidthInSamples; // :contentReference[oaicite:9]{index=9}
            int left = triggerPoint - width / 2;
            int right = left + width;

            float peak = 0f;
            int step = Math.Max(1, ActivitySubsampleStride);
            for (int s = left; s < right; s += step)
            {
                float v = ch.GetSample(s, forTrigger: false); // real drawing samples. :contentReference[oaicite:10]{index=10}
                float a = v >= 0 ? v : -v;
                if (a > peak)
                {
                    peak = a;
                    if (peak >= ActivityThreshold) return true;
                }
            }
            return peak >= ActivityThreshold;
        }

        /// <summary>Rebuild channel.Bounds layout for the given visible set and repaint the template into _templateData.</summary>
        private void RebuildLayoutAndTemplateForVisible(IReadOnlyList<Channel> visible)
        {
            // Determine rendering area
            var rb = RenderingBounds;
            if (rb.Width == 0 || rb.Height == 0) rb = new Rectangle(0, 0, Width, Height); // default. :contentReference[oaicite:11]{index=11}

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
                visible[i].Bounds = new Rectangle(x0, y0, x1 - x0, y1 - y0); // channel.Bounds drives drawing. :contentReference[oaicite:12]{index=12}
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
                        // Same edge handling as original: pull in 1px on right/bottom. :contentReference[oaicite:13]{index=13}
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

                    // Same alignment switch as original template drawing. :contentReference[oaicite:14]{index=14}
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

        // =====================================================================
        //                    Legacy batch renderer (unchanged)
        // =====================================================================

        // NOTE: Keeping your original Render(...) path for compatibility. It still uses a per-frame Bitmap.
        public void Render(IList<IGraphicsOutput> outputs)
        {
            var rawData = new byte[Width * Height * 4];
            GCHandle pinnedArray = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                using (var bm = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, pinnedArray.AddrOfPinnedObject()))
                {
                    int numFrames = (int)(_channels.Max(c => c.SampleCount) * FramesPerSecond / SamplingRate); // :contentReference[oaicite:15]{index=15}
                    int frameIndex = 0;
                    Render(bm, rawData, () =>
                    {
                        double fractionComplete = (double)++frameIndex / numFrames;
                        foreach (var output in outputs) output.Write(rawData, bm, fractionComplete);
                    }, 0, numFrames);
                }
            }
            finally { pinnedArray.Free(); }
        }

        private void Render(Image destination, byte[] imageBuffer, Action onFrame, int startFrame, int endFrame)
        {
            var renderingBounds = RenderingBounds;
            if (renderingBounds.Width == 0 || renderingBounds.Height == 0)
            {
                renderingBounds = new Rectangle(0, 0, Width, Height); // default. :contentReference[oaicite:16]{index=16}
            }

            // Compute channel bounds for a fixed layout (legacy path). :contentReference[oaicite:17]{index=17}
            var numRows = _channels.Count / Columns + (_channels.Count % Columns == 0 ? 0 : 1);
            for (int i = 0; i < _channels.Count; ++i)
            {
                int ChannelX(int c) => c * renderingBounds.Width / Columns + renderingBounds.Left;
                int ChannelY(int r) => r * renderingBounds.Height / numRows + renderingBounds.Top;

                var column = i % Columns;
                var row = i / Columns;
                var x = ChannelX(column);
                var y = ChannelY(row);
                _channels[i].Bounds = new Rectangle(x, y, ChannelX(column + 1) - x, ChannelY(row + 1) - y);
            }

            // Build template
            var templateData = new byte[Width * Height * 4];
            GCHandle pinnedArray = GCHandle.Alloc(templateData, GCHandleType.Pinned);
            try
            {
                using (var templateImage = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, pinnedArray.AddrOfPinnedObject()))
                {
                    GenerateTemplate(templateImage); // draws bg/labels/zero lines/borders. :contentReference[oaicite:18]{index=18}
                    using (var g = Graphics.FromImage(destination))
                    {
                        var pens = _channels.Select(c => c.LineColor == Color.Transparent || c.LineWidth <= 0
                            ? null
                            : new Pen(c.LineColor, c.LineWidth) { MiterLimit = c.LineWidth, LineJoin = LineJoin.Bevel }).ToList();
                        var brushes = _channels.Select(c => c.FillColor == Color.Transparent ? null : new SolidBrush(c.FillColor)).ToList();

                        var buffers = _channels.Select(ch => new PointF[ch.ViewWidthInSamples]).ToList(); // :contentReference[oaicite:19]{index=19}
                        var path = new GraphicsPath();

                        var frameSamples = SamplingRate / FramesPerSecond;

                        // Initialise previous trigger points
                        var triggerPoints = new int[_channels.Count];
                        for (int ci = 0; ci < _channels.Count; ++ci)
                            triggerPoints[ci] = (int)((long)startFrame * SamplingRate / FramesPerSecond) - frameSamples; // :contentReference[oaicite:20]{index=20}

                        var stringFormat = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

                        for (int frameIndex = startFrame; frameIndex < endFrame; ++frameIndex)
                        {
                            int frameIndexSamples = (int)((long)frameIndex * SamplingRate / FramesPerSecond);

                            if (imageBuffer == null) g.DrawImageUnscaled(templateImage, 0, 0); else Buffer.BlockCopy(templateData, 0, imageBuffer, 0, templateData.Length); // :contentReference[oaicite:21]{index=21}

                            for (int ci = 0; ci < _channels.Count; ++ci)
                            {
                                var channel = _channels[ci];
                                if (channel.IsEmpty) continue;

                                if (!string.IsNullOrEmpty(channel.ErrorMessage))
                                {
                                    g.DrawString(channel.ErrorMessage, SystemFonts.DefaultFont, Brushes.Red, channel.Bounds, stringFormat);
                                }
                                else if (channel.Loading)
                                {
                                    g.DrawString("Loading data.", SystemFonts.DefaultFont, Brushes.Green, channel.Bounds, stringFormat);
                                }
                                else if (channel.IsSilent && !channel.RenderIfSilent)
                                {
                                    g.DrawString("This channel is silent", SystemFonts.DefaultFont, Brushes.Yellow, channel.Bounds, stringFormat);
                                }
                                else
                                {
                                    var triggerPoint = channel.GetTriggerPoint(frameIndexSamples, frameSamples, triggerPoints[ci]); // :contentReference[oaicite:22]{index=22}
                                    triggerPoints[ci] = triggerPoint;

                                    RenderWave(g, channel, triggerPoint, pens[ci], brushes[ci], buffers[ci], path, channel.FillBase); // :contentReference[oaicite:23]{index=23}
                                }
                            }

                            onFrame();
                        }

                        foreach (var pen in pens) pen?.Dispose();
                        foreach (var brush in brushes) brush?.Dispose();
                    }
                }
            }
            finally { pinnedArray.Free(); }
        }

        private void GenerateTemplate(Image template)
        {
            using (var g = Graphics.FromImage(template))
            {
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

                foreach (var channel in _channels)
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
                                g.DrawRectangle(
                                    pen,
                                    channel.Bounds.Left,
                                    channel.Bounds.Top,
                                    channel.Bounds.Width - (channel.Bounds.Right == RenderingBounds.Right ? 1 : 0),
                                    channel.Bounds.Height - (channel.Bounds.Bottom == RenderingBounds.Bottom ? 1 : 0));
                            }
                            else
                            {
                                if (channel.Bounds.Left != RenderingBounds.Left)
                                    g.DrawLine(pen, channel.Bounds.Left, channel.Bounds.Top, channel.Bounds.Left, channel.Bounds.Bottom);

                                if (channel.Bounds.Top != RenderingBounds.Top)
                                    g.DrawLine(pen, channel.Bounds.Left, channel.Bounds.Top, channel.Bounds.Right, channel.Bounds.Top);

                                if (channel.Bounds.Right != RenderingBounds.Right)
                                    g.DrawLine(pen, channel.Bounds.Right, channel.Bounds.Top, channel.Bounds.Right, channel.Bounds.Bottom);

                                if (channel.Bounds.Bottom != RenderingBounds.Bottom)
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
            }
        }

        private void RenderWave(Graphics g, Channel channel, int triggerPoint, Pen pen, Brush brush, PointF[] points, GraphicsPath path, double fillBase)
        {
            // Same as your original RenderWave: compute poly points and optional fill. :contentReference[oaicite:24]{index=24}

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

            if (pen != null)
                g.DrawLines(pen, points);

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

        public Bitmap RenderFrame(float position = 0)
        {
            // legacy preview helper (kept as-is). :contentReference[oaicite:25]{index=25}
            var frameIndex = _channels.Count > 0
                ? (int)(position * _channels.Max(c => c.SampleCount) * FramesPerSecond / SamplingRate)
                : 0;
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            Render(bitmap, null, () => { }, frameIndex, frameIndex + 1);
            return bitmap;
        }
    }
}
