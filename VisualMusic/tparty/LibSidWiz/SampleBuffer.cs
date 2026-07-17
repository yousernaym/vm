using NAudio.Dsp;
using NAudio.Wave;
using System;

namespace LibSidWiz
{
    internal class SampleBuffer : IDisposable
    {
        private readonly WaveStream _reader;
        private readonly ISampleProvider _sampleProvider;
        private float[] _decoded;

        public float TargetPeak { get; set; } = (float)Math.Pow(10.0, -1.0 / 20.0); // -1 dBFS

        public long Count { get; }
        public int SampleRate { get; }
        public TimeSpan Length { get; }
        public float Max { get; private set; }
        public float Min { get; private set; }

        // True if reading failed during Analyze() (e.g. unsupported format or corrupt file).
        public bool Failed { get; private set; }

        public SampleBuffer(string filename, Channel.Sides side, bool filter)
        {
            _reader = new AudioFileReader(filename);
            Count = _reader.Length / (_reader.WaveFormat.BitsPerSample / 8) / _reader.WaveFormat.Channels;
            SampleRate = _reader.WaveFormat.SampleRate;
            Length = _reader.TotalTime;
            switch (side)
            {
                case Channel.Sides.Left:
                    _sampleProvider = _reader.ToSampleProvider().ToMono(1.0f, 0.0f);
                    break;
                case Channel.Sides.Right:
                    _sampleProvider = _reader.ToSampleProvider().ToMono(0.0f, 1.0f);
                    break;
                case Channel.Sides.Mix:
                    _sampleProvider = _reader.ToSampleProvider().ToMono();
                    break;
            }
            if (filter)
                _sampleProvider = new HighPassSampleProvider(_sampleProvider);
        }

        // In-memory buffer from an already-decoded mono float array (e.g. the summed voice buffer of a
        // pitch-split track). Analyze() is a no-op — the samples are ready to read immediately.
        public SampleBuffer(float[] decoded, int sampleRate)
        {
            _decoded = decoded;
            Count = decoded?.LongLength ?? 0;
            SampleRate = sampleRate;
            Length = TimeSpan.FromSeconds(sampleRate > 0 ? (double)Count / sampleRate : 0.0);
            float min = 0, max = 0;
            if (decoded != null)
                for (long i = 0; i < decoded.LongLength; i++)
                {
                    if (decoded[i] < min) min = decoded[i];
                    if (decoded[i] > max) max = decoded[i];
                }
            Min = min;
            Max = max;
        }

        public void Dispose()
        {
            // Guard against cross-thread COM release errors (e.g. MP3/MediaFoundation).
            try { _reader.Dispose(); }
            catch { }
        }

        public float this[long index]
        {
            get
            {
                if (Failed || _decoded == null || index < 0 || index >= Count)
                    return 0;
                return _decoded[index];
            }
        }

        // The whole normalised sample array, or null before Analyze() ran / on failure. The array is
        // never mutated after Analyze(), so a reference held by another thread is safe to read.
        internal float[] Decoded => _decoded;

        public void Analyze() => Analyze(true);

        // normalize = false leaves the decoded samples at their raw amplitude (used when several voice
        // buffers are summed and then normalised together, so their relative levels are preserved).
        public void Analyze(bool normalize)
        {
            if (_reader == null)   // in-memory buffer: already decoded in the ctor
                return;

            // Read all samples on the current thread (same thread that created _reader and its
            // underlying COM objects). This avoids cross-apartment InvalidCastExceptions that
            // the old lazy-chunk approach triggered for MP3/MediaFoundation readers.
            var raw = new float[Count];
            var buf = new float[4096];
            long pos = 0;
            try
            {
                while (pos < Count)
                {
                    int toRead = (int)Math.Min(buf.Length, Count - pos);
                    int read = _sampleProvider.Read(buf, 0, toRead);
                    if (read == 0) break;
                    Array.Copy(buf, 0, raw, pos, read);
                    pos += read;
                }
            }
            catch
            {
                Failed = true;
                try { _reader.Dispose(); } catch { }
                return;
            }

            // Compute peak and normalisation gain.
            float min = 0, max = 0;
            for (long i = 0; i < pos; i++)
            {
                if (raw[i] < min) min = raw[i];
                if (raw[i] > max) max = raw[i];
            }
            float peak = Math.Max(Math.Abs(min), Math.Abs(max));
            float gain = (normalize && peak > 0) ? TargetPeak / peak : 1.0f;
            Min = min * gain;
            Max = max * gain;

            // Apply gain in place.
            if (gain != 1.0f)
                for (long i = 0; i < pos; i++)
                    raw[i] *= gain;

            _decoded = raw;

            // All COM objects (MP3/MediaFoundation) are released here on the same thread
            // where they were created. SampleBuffer.Dispose() may call this again — harmless.
            try { _reader.Dispose(); } catch { }
        }
    }

    internal class HighPassSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _sampleProvider;
        private readonly BiQuadFilter _filter;

        public HighPassSampleProvider(ISampleProvider sampleProvider)
        {
            _sampleProvider = sampleProvider;
            _filter = BiQuadFilter.HighPassFilter(sampleProvider.WaveFormat.SampleRate, 20, 1);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int result = _sampleProvider.Read(buffer, offset, count);
            for (int i = 0; i < result; ++i)
                buffer[i] = _filter.Transform(buffer[offset + i]);
            return result;
        }

        public WaveFormat WaveFormat => _sampleProvider.WaveFormat;
    }
}
