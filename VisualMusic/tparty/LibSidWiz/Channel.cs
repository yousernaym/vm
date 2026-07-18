using LibSidWiz.Triggers;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LibSidWiz
{
    /// <summary>
    /// One source (chip/mod) channel's WAV feeding an exact pitch split. The WAV holds the whole
    /// channel (shared by every instrument track playing on it), so OwnedStartsSec/OwnedEndsSec
    /// carry the note-ownership ranges (seconds, sorted, parallel arrays) this track owns; samples
    /// outside them are zeroed at load. Null ranges = no gating (legacy pre-spliced voice WAVs).
    /// </summary>
    public struct VoiceFile
    {
        public int SourceChannel;
        public string Path;
        public double[] OwnedStartsSec;
        public double[] OwnedEndsSec;
        public VoiceFile(int sourceChannel, string path,
            double[] ownedStartsSec = null, double[] ownedEndsSec = null)
        {
            SourceChannel = sourceChannel;
            Path = path;
            OwnedStartsSec = ownedStartsSec;
            OwnedEndsSec = ownedEndsSec;
        }
    }

    /// <summary>A decoded voice buffer (Q15) plus the source channel it came from.</summary>
    public sealed class LoadedVoice
    {
        public int SourceChannel;
        public short[] Samples;
    }

    public struct Padding
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public Padding(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    /// <summary>
    /// Wraps a single "voice", and also deals with loading the data into memory
    /// </summary>
    public class Channel : IDisposable
    {
        private readonly bool _autoReloadOnSettingChanged;
        private SampleBuffer _samples;
        private SampleBuffer _samplesForTrigger;
        // Per-voice separated audio for the pitch split, published whole by the app thread and read
        // once per frame by the render thread. null = no split ready (channel renders unsplit).
        private volatile ChannelVoiceSet _voiceSet;
        // Render-thread-only: the voice set latched for the frame currently being prepared/drawn, and
        // the Q15 voice buffer GetSample should read (null = the mixed float channel buffer).
        private ChannelVoiceSet _frameVoices;
        private short[] _activeVoiceSamples;
        private string _labelSuffix = "";
        private string _filename;
        private string _externalTriggerFilename;
        private ITriggerAlgorithm _algorithm;
        private int _triggerLookaheadFrames = 1; // Current frame plus one ahead
        private int _triggerLookaheadOnFailureFrames = 3; // Extra frames searched beyond the normal window on failure
        private float _shapeStabilityWeight;
        private int _splitCount = 1;
        private SplitLayout _splitLayout = SplitLayout.Stacked;
        internal const int MaxSplitCount = 4;
        private Color _lineColor = Color.White;
        private string _label = "";
        private float _lineWidth = 3;
        private float _scale = 1.0f;
        private int _viewWidthInSamples = 650;
        private Color _fillColor = Color.Transparent;
        private float _zeroLineWidth = 0;
        private Color _zeroLineColor = Color.Transparent;
        private Font _labelFont;
        private Color _labelColor = Color.Transparent;
        private Color _borderColor = Color.Transparent;
        private float _borderWidth;
        private ContentAlignment _labelAlignment = ContentAlignment.TopLeft;
        private Padding _labelMargins = new Padding(0, 0, 0, 0);
        private bool _invertedTrigger;
        private bool _borderEdges = true;
        private Color _backgroundColor = Color.Transparent;
        private bool _clip;
        private Sides _side = Sides.Mix;
        private bool _smoothLines = true;
        private bool _filter;
        private double _fillBase;
        static int idCounter;
        public int Id;
        public Channel(bool autoReloadOnSettingChanged)
        {
            Id = idCounter++;
            _autoReloadOnSettingChanged = autoReloadOnSettingChanged;
        }

        public enum Sides
        {
            Left,
            Right,
            Mix
        }

        public event Action<Channel, bool> Changed;

        public Task<bool> LoadDataAsync(CancellationToken token = new CancellationToken())
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    ErrorMessage = "";

                    _samples?.Dispose();

                    if (VoiceFiles != null && VoiceFiles.Count > 0)
                    {
                        LoadVoiceData(token);
                        return true;
                    }
                    LoadedVoices = null; // plain single-file track from here on

                    if (string.IsNullOrEmpty(Filename))
                    {
                        _samples = null;
                        SampleCount = 0;
                        Max = 0;
                        Length = TimeSpan.Zero;
                        Loading = false;
                        IsEmpty = true;
                        return false;
                    }

                    IsEmpty = false;
                    Loading = true;

                    Console.WriteLine($"- Reading {Filename}");
                    _samples = new SampleBuffer(Filename, Side, HighPassFilter);
                    SampleRate = _samples.SampleRate;
                    Length = _samples.Length;

                    token.ThrowIfCancellationRequested();

                    _samples.Analyze();

                    SampleCount = _samples.Count;

                    token.ThrowIfCancellationRequested();

                    Max = Math.Max(Math.Abs(_samples.Max), Math.Abs(_samples.Min));

                    Console.WriteLine($"- Peak sample amplitude for {Filename} is {Max}");

                    if (string.IsNullOrEmpty(ExternalTriggerFilename))
                    {
                        // Point at the same SampleBuffer
                        _samplesForTrigger = _samples;
                    }
                    else
                    {
                        _samplesForTrigger = new SampleBuffer(ExternalTriggerFilename, Side, HighPassFilter);
                    }

                    Loading = false;
                    return true;
                }
                catch (TaskCanceledException)
                {
                    // Blank out if cancelled
                    Max = 0;
                    SampleRate = 0;
                    Length = TimeSpan.Zero;
                    try
                    {
                        if (_samplesForTrigger != _samples)
                            _samplesForTrigger?.Dispose();
                    }
                    catch { }
                    _samplesForTrigger = null;
                    try { _samples?.Dispose(); } catch { }
                    _samples = null;
                    LoadedVoices = null;
                    Loading = false;
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.ToString();
                    Max = 0;
                    SampleRate = 0;
                    Length = TimeSpan.Zero;
                    // Dispose may itself throw (e.g. MP3 COM Release across apartments). Guard it so
                    // we don't lose the original error and crash the awaiting async void handler.
                    try
                    {
                        if (_samplesForTrigger != _samples)
                            _samplesForTrigger?.Dispose();
                    }
                    catch { }
                    _samplesForTrigger = null;
                    try { _samples?.Dispose(); } catch { }
                    _samples = null;
                    LoadedVoices = null;
                    Loading = false;
                    return false;
                }
                finally
                {
                    // Only propagate a non-zero rate; a failed load leaves SampleRate==0 and would
                    // otherwise zero the renderer's rate, halting rendering of every other channel.
                    if (Renderer != null && SampleRate != 0)
                        Renderer.SamplingRate = SampleRate;
                    Changed?.Invoke(this, false);
                }
            }, token);
        }

        // Voice-backed load: decode each voice WAV un-normalised (mono + high-pass), gate it to the
        // note ranges this track owns (the WAV holds the whole shared source channel), sum the gated
        // voices into the track's mixed buffer, normalise the sum once, and quantise every voice to
        // Q15 with that same gain — so lane levels stay comparable and the voices sum back to the
        // track's signal.
        private void LoadVoiceData(CancellationToken token)
        {
            IsEmpty = false;
            Loading = true;

            int rate = 0;
            long maxLen = 0;
            var decodedVoices = new List<(int SourceChannel, float[] Samples)>(VoiceFiles.Count);
            foreach (var vf in VoiceFiles.OrderBy(v => v.SourceChannel))
            {
                var sb = new SampleBuffer(vf.Path, Side, HighPassFilter);
                sb.Analyze(false); // raw amplitude; the sum is normalised below
                token.ThrowIfCancellationRequested();
                var dec = sb.Decoded ?? Array.Empty<float>();
                if (rate == 0) rate = sb.SampleRate;
                GateToOwnedRanges(dec, vf.OwnedStartsSec, vf.OwnedEndsSec, sb.SampleRate);
                decodedVoices.Add((vf.SourceChannel, dec));
                if (dec.LongLength > maxLen) maxLen = dec.LongLength;
            }

            var mixed = new float[maxLen];
            foreach (var (_, dec) in decodedVoices)
                for (long i = 0; i < dec.LongLength; i++)
                    mixed[i] += dec[i];

            float targetPeak = (float)Math.Pow(10.0, -1.0 / 20.0); // -1 dBFS, matches SampleBuffer
            float peak = 0;
            for (long i = 0; i < mixed.LongLength; i++)
            {
                float a = Math.Abs(mixed[i]);
                if (a > peak) peak = a;
            }
            float gain = peak > 0 ? targetPeak / peak : 1.0f;
            if (gain != 1.0f)
                for (long i = 0; i < mixed.LongLength; i++)
                    mixed[i] *= gain;

            var loaded = new LoadedVoice[decodedVoices.Count];
            for (int v = 0; v < decodedVoices.Count; v++)
            {
                var dec = decodedVoices[v].Samples;
                var q15 = new short[dec.LongLength];
                for (long i = 0; i < dec.LongLength; i++)
                {
                    int s = (int)Math.Round(dec[i] * gain * 32767f);
                    q15[i] = s > 32767 ? (short)32767 : s < -32767 ? (short)-32767 : (short)s;
                }
                loaded[v] = new LoadedVoice { SourceChannel = decodedVoices[v].SourceChannel, Samples = q15 };
            }

            _samples = new SampleBuffer(mixed, rate);
            SampleRate = rate;
            Length = _samples.Length;
            SampleCount = _samples.Count;
            Max = Math.Max(Math.Abs(_samples.Max), Math.Abs(_samples.Min));
            _samplesForTrigger = _samples;
            LoadedVoices = loaded;
            Loading = false;
        }

        // Zero every sample outside the voice's owned ranges (sorted, seconds), reducing a shared
        // whole-channel WAV to exactly this track's signal. Null ranges = keep everything (legacy
        // pre-spliced per-voice WAVs are already gated on disk).
        private static void GateToOwnedRanges(float[] samples, double[] startsSec, double[] endsSec, int rate)
        {
            if (startsSec == null || endsSec == null || rate <= 0)
                return;
            long len = samples.LongLength;
            long pos = 0; // everything before the next owned range gets zeroed
            int n = Math.Min(startsSec.Length, endsSec.Length);
            for (int r = 0; r < n && pos < len; r++)
            {
                double startF = startsSec[r] * rate;
                double endF = endsSec[r] * rate; // double.MaxValue end => to end of buffer
                long s = startF >= len ? len : Math.Max((long)startF, 0);
                long e = endF >= len ? len : Math.Max((long)endF, 0);
                for (long i = pos; i < s; i++)
                    samples[i] = 0;
                if (e > pos)
                    pos = e;
            }
            for (long i = pos; i < len; i++)
                samples[i] = 0;
        }

        [Category("Data")]
        [Description("The full text of any error message when loading the file")]
        [JsonIgnore]
        public string ErrorMessage { get; private set; }

        [Category("Data")]
        [Description("The filename to be rendered")]
        public string Filename
        {
            get => _filename;
            set
            {
                bool needReload = value != _filename;
                _filename = value;
                Changed?.Invoke(this, needReload);
                if (_filename != "" && string.IsNullOrEmpty(_label))
                {
                    Label = ""; // GuessNameFromMultidumperFilename(_filename);
                }
            }
        }

        [Category("Triggering")]
        [Description("The filename to use for oscilloscope triggering. Leave blank to use the channel's sound data.")]
        public string ExternalTriggerFilename
        {
            get => _externalTriggerFilename;
            set
            {
                bool needReload = value != _externalTriggerFilename;
                _externalTriggerFilename = value;
                // Change algorithm to RisingEdgeTrigger when using an external trigger
                _algorithm = new RisingEdgeTrigger();
                Changed?.Invoke(this, needReload);
            }
        }

        [Category("Data")]
        [Description("The channel to use from the file (if stereo)")]
        public Sides Side
        {
            get => _side;
            set
            {
                bool needReload = value != _side;
                _side = value;
                Changed?.Invoke(this, needReload);
                if (_autoReloadOnSettingChanged)
                {
                    LoadDataAsync();
                }
            }
        }

        [Category("Data")]
        [Description("If enabled, high pass filtering will be used to remove DC offsets")]
        public bool HighPassFilter
        {
            get => _filter;
            set
            {
                bool needReload = value != _filter;
                _filter = value;
                Changed?.Invoke(this, needReload);
                if (_autoReloadOnSettingChanged)
                {
                    LoadDataAsync();
                }
            }
        }

        [Category("Triggering")]
        [Description("The algorithm to use for rendering")]
        [TypeConverter(typeof(TriggerAlgorithmTypeConverter))]
        [JsonConverter(typeof(TriggerAlgorithmJsonConverter))]
        public ITriggerAlgorithm Algorithm
        {
            get => _algorithm;
            set
            {
                _algorithm = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("How many frames to allow the triggering algorithm to look ahead. Zero means only look within the current frame. Set to larger numbers to support sync to low frequencies, but too large numbers can cause erroneous matches.")]
        public int TriggerLookaheadFrames
        {
            get => _triggerLookaheadFrames;
            set
            {
                _triggerLookaheadFrames = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("How many additional frames (beyond the normal lookahead window) to search when nothing is found with the default lookahead. Zero disables the retry.")]
        public int TriggerLookaheadOnFailureFrames
        {
            get => _triggerLookaheadOnFailureFrames;
            set
            {
                _triggerLookaheadOnFailureFrames = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("0 = pick the sync point nearest the expected position; higher values instead prefer the candidate whose surrounding waveform best matches the previous frame's, steadying complex timbres that hop between similar cycles. Only affects candidate-based algorithms (Peak speed, Biggest wave area, Biggest positive area).")]
        public float ShapeStabilityWeight
        {
            get => _shapeStabilityWeight;
            set
            {
                _shapeStabilityWeight = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("Number of waveforms the channel is split into (1 = off). Driven by the number of source channels an instrument is played on (exact per-voice splitting).")]
        public int SplitCount
        {
            get => _splitCount;
            set
            {
                int clamped = value < 1 ? 1 : value; // no upper bound: as many voices as source channels
                if (clamped == _splitCount)
                    return;
                _splitCount = clamped;
                // Publish a fresh slot array (or null) as the last step so a worker reading the old
                // reference mid-frame stays consistent.
                Slots = clamped > 1 ? BuildSlots(clamped) : null;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("How the split waveforms are arranged within the channel's area.")]
        public SplitLayout SplitLayout
        {
            get => _splitLayout;
            set
            {
                _splitLayout = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("Seconds of upcoming silence after which the channel is hidden. Re-read every frame, so no change notification is needed.")]
        public float ActivityLookaheadSeconds { get; set; } = 5f;

        [Category("Appearance")]
        [Description("The line colour")]
        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                Pen.Color = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The line width, in pixels. Fractional values are supported.")]
        public float LineWidth
        {
            get => _lineWidth;
            set
            {
                _lineWidth = value;
                // Scale with the renderer height (not width) so the thickness tracks the output
                // resolution but stays constant when the overlay-width setting changes the strip
                // width. Equals the old width-based scaling for a 16:9 viewport at 25% width.
                Pen.Width = value * (Renderer?.Height ?? 1080) / 1080;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The fill colour. Set to transparent to have no fill.")]
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The base of the fill. Set to 0 for the centre line, -1 to fill from the bottom and 1 for the top. Other values also work.")]
        public double FillBase
        {
            get => _fillBase;
            set
            {
                _fillBase = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("Whether to draw lines pixelated (false) or anti-aliased (true)")]
        public bool SmoothLines
        {
            get => _smoothLines;
            set
            {
                _smoothLines = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The width of the zero line")]
        public float ZeroLineWidth
        {
            get => _zeroLineWidth;
            set
            {
                _zeroLineWidth = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The color of the zero line")]
        public Color ZeroLineColor
        {
            get => _zeroLineColor;
            set
            {
                _zeroLineColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The color of the border")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The width of the border")]
        public float BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("Whether to draw the outer edges of any border boxes")]
        public bool BorderEdges
        {
            get => _borderEdges;
            set
            {
                _borderEdges = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("A background colour for the channel. This is layered above any background image, and can be transparent.")]
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The label for the channel")]
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                Changed?.Invoke(this, false);
            }
        }

        // A transient suffix appended to the label while the pitch-split audio is still being
        // separated (e.g. " (separating…)"). Render-only, never serialised; fires Changed so the
        // renderer rebuilds its label template.
        [Browsable(false)]
        [JsonIgnore]
        public string LabelSuffix
        {
            get => _labelSuffix;
            set
            {
                value ??= "";
                if (_labelSuffix == value)
                    return;
                _labelSuffix = value;
                Changed?.Invoke(this, false);
            }
        }

        internal string EffectiveLabel => _labelSuffix.Length == 0 ? _label : _label + _labelSuffix;

        // The per-voice separated audio for the pitch split. Written by the app (UI thread in
        // preview, export thread during export); the render thread latches it once per frame.
        [Browsable(false)]
        [JsonIgnore]
        public ChannelVoiceSet VoiceSet
        {
            get => _voiceSet;
            set => _voiceSet = value;
        }

        // The channel's whole decoded, normalised sample array (null while loading / on failure).
        // Read by the separation job on a background thread; safe because the array is immutable
        // after Analyze().
        [Browsable(false)]
        [JsonIgnore]
        public float[] DecodedSamples => _samples?.Decoded;

        // Per-voice source WAVs for an exact pitch split, set before LoadDataAsync. Empty/null = a
        // plain single-file track. The app serialises this list on AudioProps, not here.
        [Browsable(false)]
        [JsonIgnore]
        public IReadOnlyList<VoiceFile> VoiceFiles { get; set; }

        // Per-voice decoded Q15 buffers produced by the voice-backed load, sorted by SourceChannel.
        // null = not a split track. The app builds a ChannelVoiceSet from these plus the MIDI spans.
        [Browsable(false)]
        [JsonIgnore]
        public LoadedVoice[] LoadedVoices { get; private set; }

        // True when this channel has any audio source assigned — a single file or per-voice files.
        // Gates that used to test Filename alone must use this, or voice-backed tracks vanish.
        [Browsable(false)]
        [JsonIgnore]
        public bool HasAudioSource => !string.IsNullOrEmpty(Filename) || (VoiceFiles != null && VoiceFiles.Count > 0);

        [Category("Appearance")]
        [Description("The font for the channel label")]
        public Font LabelFont
        {
            get => _labelFont;
            set
            {
                _labelFont = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The color for the channel label")]
        public Color LabelColor
        {
            get => _labelColor;
            set
            {
                _labelColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The alignment for the channel label")]
        public ContentAlignment LabelAlignment
        {
            get => _labelAlignment;
            set
            {
                _labelAlignment = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The margins for the chanel label")]
        public Padding LabelMargins
        {
            get => _labelMargins;
            set
            {
                _labelMargins = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Adjustment")]
        [Description("Vertical scaling. This may be set by the auto-scaler.")]
        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Adjustment")]
        [Description("Whether to constrain the waveform to its screen area when scaled past 100%")]
        public bool Clip
        {
            get => _clip;
            set
            {
                _clip = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Adjustment")]
        [Description("View width, in ms")]
        [JsonIgnore]
        public float ViewWidthInMilliseconds
        {
            get => SampleRate == 0 ? 0 : (float)_viewWidthInSamples * 1000 / SampleRate;
            set
            {
                _viewWidthInSamples = (int)(value / 1000 * SampleRate);
                Changed?.Invoke(this, false);
            }
        }

        [Category("Adjustment")]
        [Description("View width, in samples")]
        public int ViewWidthInSamples
        {
            get => _viewWidthInSamples;
            set
            {
                _viewWidthInSamples = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("Set to true to trigger in the opposite direction")]
        // ReSharper disable once MemberCanBePrivate.Global
        public bool InvertedTrigger
        {
            get => _invertedTrigger;
            set
            {
                _invertedTrigger = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Data")]
        [Description("Peak amplitude for the channel")]
        [JsonIgnore]
        public float Max { get; private set; }

        [Browsable(false)]
        [JsonIgnore]
        public long SampleCount { get; private set; }

        [Category("Data")]
        [Description("Duration of the channel")]
        [JsonIgnore]
        public TimeSpan Length { get; private set; }

        [Category("Data")]
        [Description("Sampling rate of the channel")]
        [JsonIgnore]
        public int SampleRate { get; private set; }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        [Browsable(false)]
        [JsonIgnore]
        public bool IsSilent => Max == 0.0;

        [Browsable(false)]
        [JsonIgnore]
        public bool Loading { get; private set; } = true;

        [Browsable(false)]
        [JsonIgnore]
        public bool IsEmpty { get; private set; }

        // True if a sample read has failed at any point. Renderer skips failed channels so a
        // single broken file (e.g. an MP3 whose COM reader can't be used from the UI thread)
        // can't break rendering of the others.
        [Browsable(false)]
        [JsonIgnore]
        public bool Failed => _samples != null && _samples.Failed;

        [Browsable(false)]
        [JsonIgnore]
        internal Rectangle Bounds { get; set; }
        public WaveformRenderer Renderer { get; set; }
        public Pen Pen { get; } = new Pen(Color.White, 2);

        /// <summary>
        /// Reads one sample from the calling thread. Use after LoadDataAsync to detect
        /// readers (e.g. MediaFoundation MP3) that load on a worker thread but fail on
        /// the UI thread due to COM apartment mismatch. Returns false if the read failed.
        /// </summary>
        public bool TestReadOnCurrentThread()
        {
            if (_samples == null || _samples.Count == 0)
                return _samples != null;
            _ = _samples[0];
            return !_samples.Failed;
        }

        internal float GetSample(int sampleIndex, bool forTrigger = true)
        {
            // When a voice buffer is routed in (per-slot trigger/render on a split channel), read it
            // for both trigger and display: a slot triggers and draws from its own separated audio.
            // ExternalTriggerFilename doesn't apply to slots — the voice buffer already isolates the note.
            // Voice samples are Q15; Scale is applied at read time so auto-scaler still tracks.
            var voice = _activeVoiceSamples;
            if (voice != null)
            {
                return sampleIndex < 0 || sampleIndex >= voice.Length
                    ? 0 : (voice[sampleIndex] * (1f / 32767f)) * Scale * (forTrigger && InvertedTrigger ? -1 : 1);
            }
            var source = forTrigger ? _samplesForTrigger : _samples;
            if (source == null)
                return 0;
            return sampleIndex < 0 || sampleIndex >= source.Count ? 0 : source[sampleIndex] * Scale * (forTrigger && InvertedTrigger ? -1 : 1);
        }

        // Render-thread-only: route subsequent GetSample reads to a Q15 voice buffer (null = mixed).
        internal void SetActiveVoice(short[] samples) => _activeVoiceSamples = samples;

        // The voice set latched for this frame is complete (matches the slot count).
        internal bool SplitPreparedThisFrame => SplitCount > 1 && _frameVoices != null;

        // Samples of voice k in the frame-latched voice set (render thread only).
        internal short[] FrameVoiceSamples(int k) => _frameVoices.Voices[k].Samples;

        internal int GetTriggerPoint(int frameIndexSamples, int frameSamples, int previousTriggerPoint)
        {
            // Try at default settings
            var normalWindowEnd = frameIndexSamples + frameSamples * (TriggerLookaheadFrames + 1);
            var result = Algorithm.GetTriggerPoint(this, frameIndexSamples, normalWindowEnd, frameSamples, previousTriggerPoint);

            if (result < frameIndexSamples && TriggerLookaheadOnFailureFrames > 0)
            {
                // The normal window had no trigger at all, so search only the frames after it.
                // Start one sample early so a zero crossing landing exactly on the window
                // boundary is still seen (crossing detectors compare against the previous sample).
                result = Algorithm.GetTriggerPoint(this, normalWindowEnd - 1, normalWindowEnd + frameSamples * TriggerLookaheadOnFailureFrames, frameSamples, previousTriggerPoint);
            }

            if (result < frameIndexSamples)
            {
                // Default on failure
                result = frameIndexSamples;
            }

            return result;
        }

        // ===================== Pitch split =====================
        //
        // Each slot renders one voice's exact per-channel audio (the source channel's rendered WAV,
        // gated to the notes that channel plays; built by the app in Project.BuildExactVoiceSet). The
        // slot→voice mapping is fixed 1:1, so this class only has to search each voice buffer's trigger
        // per frame and hold the curve while silent — no per-frame pitch classification or allocation.

        /// <summary>Per-voice rendering slots (null when SplitCount &lt;= 1). Read by the renderer.</summary>
        internal WaveSlot[] Slots { get; private set; }

        /// <summary>Rows this channel occupies in the strip layout this frame (Separate mode only differs from 1).</summary>
        internal int LayoutRowsThisFrame { get; set; } = 1;

        /// <summary>Public row count for side-balancing in WaveformPanel (a separate assembly).</summary>
        [JsonIgnore]
        [Browsable(false)]
        public int LayoutRows => SplitCount > 1 ? Math.Max(1, LayoutRowsThisFrame) : 1;

        private Pen[] _slotPens;
        private int _slotPenColorArgb = -1;
        private float _slotPenWidth = -1;

        private static WaveSlot[] BuildSlots(int n)
        {
            var slots = new WaveSlot[n];
            for (int i = 0; i < n; ++i)
                slots[i] = new WaveSlot();
            return slots;
        }

        /// <summary>Clears all slot history, e.g. after seeking backwards or at export start.</summary>
        internal void ResetSlots(int frameStartSample)
        {
            LayoutRowsThisFrame = 1;
            var slots = Slots;
            if (slots == null)
                return;
            foreach (var slot in slots)
            {
                slot.LastTrigger = -1;
                slot.LastUpdateFrameStart = int.MinValue;
                slot.LastActiveEndSample = int.MinValue;
                slot.HasCurve = false;
                slot.VisibleThisFrame = false;
            }
        }

        /// <summary>
        /// Per-frame split engine: for each voice, updates its slot's trigger from that voice's
        /// separated buffer (holding the last curve while silent) and its visibility. Slot k always
        /// renders voice k. Only called for channels with SplitCount &gt; 1, on the render thread
        /// (single writer). Falls back to unsplit rendering for the frame if no voice set is ready.
        /// </summary>
        internal void UpdateSlots(int frameStart, int frameSamples)
        {
            var slots = Slots;
            var vs = _voiceSet; // one volatile read per frame
            _frameVoices = (slots != null && vs != null && vs.Voices.Length == slots.Length) ? vs : null;
            if (slots == null || _frameVoices == null)
            {
                LayoutRowsThisFrame = 1; // renders unsplit this frame
                if (slots != null)
                    foreach (var s in slots)
                        s.VisibleThisFrame = false;
                return;
            }

            int normalEnd = frameStart + frameSamples * (TriggerLookaheadFrames + 1);
            int failEnd = normalEnd + frameSamples * TriggerLookaheadOnFailureFrames;
            long hideAfter = (long)(ActivityLookaheadSeconds * SampleRate);

            int visible = 0;
            for (int k = 0; k < slots.Length; ++k)
            {
                var slot = slots[k];
                var voice = _frameVoices.Voices[k];

                int lastEnd = voice.LastActiveEndBefore(failEnd);
                if (lastEnd > slot.LastActiveEndSample)
                    slot.LastActiveEndSample = lastEnd;

                if (voice.SoundsIn(frameStart, failEnd))
                {
                    SetActiveVoice(voice.Samples);
                    try { UpdateSlotTrigger(slot, frameStart, frameSamples, normalEnd, failEnd); }
                    finally { SetActiveVoice(null); }
                }

                slot.VisibleThisFrame = slot.HasCurve && (frameStart - (long)slot.LastActiveEndSample) <= hideAfter;
                if (slot.VisibleThisFrame)
                    ++visible;
            }
            LayoutRowsThisFrame = SplitLayout == SplitLayout.Separate ? Math.Max(1, visible) : 1;
        }

        /// <summary>
        /// Searches a voice buffer for the slot's next trigger. The voice buffer is silent outside
        /// its notes, so the standard trigger path works unchanged; the expected position advances
        /// with the frame so the curve stays steady. On failure the previous curve is held.
        /// The voice buffer must already be routed in via SetActiveVoice.
        /// </summary>
        private void UpdateSlotTrigger(WaveSlot slot, int frameStart, int frameSamples, int normalEnd, int failEnd)
        {
            // previousTriggerPoint such that previousTriggerPoint + frameSamples == the expected centre.
            int prev = slot.HasCurve
                ? (int)Math.Min(int.MaxValue, (long)slot.LastTrigger + (frameStart - slot.LastUpdateFrameStart) - frameSamples)
                : frameStart - frameSamples;

            int result = Algorithm.GetTriggerPoint(this, frameStart, normalEnd, frameSamples, prev);
            if (result < frameStart && TriggerLookaheadOnFailureFrames > 0)
                result = Algorithm.GetTriggerPoint(this, normalEnd - 1, failEnd, frameSamples, prev);

            if (result >= frameStart)
            {
                slot.LastTrigger = result;
                slot.LastUpdateFrameStart = frameStart;
                slot.HasCurve = true;
            }
            // else: hold the previous curve until it ages out of the visibility window.
        }

        /// <summary>Pen for a split slot in Overlaid mode; slot 0 is the base colour, others vary in brightness.</summary>
        internal Pen GetSlotPen(int slotIndex)
        {
            int argb = _lineColor.ToArgb();
            if (_slotPens == null || _slotPenColorArgb != argb || _slotPenWidth != Pen.Width)
            {
                if (_slotPens != null)
                    foreach (var p in _slotPens) p?.Dispose();
                _slotPens = new Pen[MaxSplitCount];
                for (int i = 0; i < MaxSplitCount; ++i)
                    _slotPens[i] = new Pen(SlotColor(i), Pen.Width);
                _slotPenColorArgb = argb;
                _slotPenWidth = Pen.Width;
            }
            return _slotPens[((slotIndex % MaxSplitCount) + MaxSplitCount) % MaxSplitCount];
        }

        private Color SlotColor(int i)
        {
            switch (i)
            {
                case 0: return _lineColor;
                case 1: return Lerp(_lineColor, Color.White, 0.3f);
                case 2: return Lerp(_lineColor, Color.Black, 0.3f);
                default: return Lerp(_lineColor, Color.White, 0.55f);
            }
        }

        private static Color Lerp(Color a, Color b, float t) => Color.FromArgb(
            a.A,
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));

        public static string GuessNameFromMultidumperFilename(string filename)
        {
            var namePart = Path.GetFileNameWithoutExtension(filename);
            try
            {
                if (namePart == null)
                {
                    return filename;
                }

                var index = namePart.IndexOf(" - YM2413 #", StringComparison.Ordinal);
                if (index > -1)
                {
                    index = int.Parse(namePart.Substring(index + 11));
                    if (index < 9)
                    {
                        return $"YM2413 Tone {index + 1}";
                    }

                    switch (index)
                    {
                        case 9: return "YM2413 Bass Drum";
                        case 10: return "YM2413 Snare Drum";
                        case 11: return "YM2413 Tom-Tom";
                        case 12: return "YM2413 Cymbal";
                        case 13: return "YM2413 Hi-Hat";
                    }
                }

                index = namePart.IndexOf(" - SEGA PSG #", StringComparison.Ordinal);
                if (index > -1)
                {
                    if (int.TryParse(namePart.Substring(index + 13), out index))
                    {
                        switch (index)
                        {
                            case 0:
                            case 1:
                            case 2:
                                return $"Sega PSG Square {index + 1}";
                            case 3:
                                return "Sega PSG Noise";
                        }
                    }
                }

                index = namePart.IndexOf(" - SN76489 #", StringComparison.Ordinal);
                if (index > -1)
                {
                    if (int.TryParse(namePart.Substring(index + 12), out index))
                    {
                        switch (index)
                        {
                            case 0:
                            case 1:
                            case 2:
                                return $"SN76489 Square {index + 1}";
                            case 3:
                                return "SN76489 Noise";
                        }
                    }
                }

                // Guess it's the bit after the last " - "
                index = namePart.LastIndexOf(" - ", StringComparison.Ordinal);
                if (index > -1)
                {
                    return namePart.Substring(index + 3);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error guessing channel name for {filename}: {ex}");
            }

            // Default to just the filename
            return namePart;
        }

        /// <summary>
        /// This allows us to use a property grid to select a trigger algorithm
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public class TriggerAlgorithmTypeConverter : StringConverter
        {
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => typeof(ITriggerAlgorithm).IsAssignableFrom(t) && t != typeof(ITriggerAlgorithm))
                        .Select(t => t.Name)
                        .ToList());
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    var type = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .FirstOrDefault(t => typeof(ITriggerAlgorithm).IsAssignableFrom(t) && t.Name.ToLowerInvariant().Equals(value.ToString().ToLowerInvariant()));
                    if (type != null)
                    {
                        return Activator.CreateInstance(type) as ITriggerAlgorithm;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public class TriggerAlgorithmJsonConverter : JsonConverter<ITriggerAlgorithm>
        {
            public override void WriteJson(JsonWriter writer, ITriggerAlgorithm value, JsonSerializer serializer)
            {
                writer.WriteValue(value.GetType().Name);
            }

            public override ITriggerAlgorithm ReadJson(JsonReader reader, Type objectType, ITriggerAlgorithm existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var type = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .FirstOrDefault(t =>
                        typeof(ITriggerAlgorithm).IsAssignableFrom(t) &&
                        t.Name.ToLowerInvariant().Equals(reader.Value?.ToString().ToLowerInvariant()));
                if (type != null)
                {
                    return Activator.CreateInstance(type) as ITriggerAlgorithm;
                }

                return existingValue;
            }
        }

        public void Dispose()
        {
            _samples?.Dispose();
            if (_samplesForTrigger != null && _samplesForTrigger != _samples)
            {
                _samplesForTrigger.Dispose();
            }
            _labelFont?.Dispose();
            if (_slotPens != null)
                foreach (var p in _slotPens) p?.Dispose();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            });
        }

        public void FromJson(string json, bool preserveSource)
        {
            if (preserveSource)
            {
                JsonConvert.PopulateObject(json, this, new JsonSerializerSettings
                {
                    ContractResolver = new PreservingContractResolver()
                });
            }
            else
            {
                JsonConvert.PopulateObject(json, this);
            }
        }

        private class PreservingContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (property.PropertyName == nameof(Filename) ||
                    property.PropertyName == nameof(Label) ||
                    property.PropertyName == nameof(ExternalTriggerFilename))
                {
                    property.Ignored = true;
                }
                return property;
            }
        }

        public bool IsMono()
        {
            if (Side == Sides.Left || Side == Sides.Right)
            {
                return true;
            }

            using (var reader = new WaveFileReader(_filename))
            {
                var sp = reader.ToSampleProvider().ToStereo();
                if (sp.WaveFormat.Channels == 1)
                {
                    return true;
                }

                int bufferSize = sp.WaveFormat.SampleRate * 10;
                var buffer = new float[bufferSize];
                sp.Read(buffer, 0, bufferSize);
                for (int i = 0; i < bufferSize; i += 2)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (buffer[i] != buffer[i + 1])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
