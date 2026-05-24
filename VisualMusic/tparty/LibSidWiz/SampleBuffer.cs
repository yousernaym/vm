using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Windows;

namespace LibSidWiz
{
    internal class SampleBuffer : IDisposable
    {
        private readonly WaveStream _reader;
        private readonly ISampleProvider _sampleProvider;
        private float _gain = 1.0f;
        public float TargetPeak { get; set; } = (float)Math.Pow(10.0, -1.0 / 20.0); // -1 dBFS

        private class Chunk
        {
            public long Offset;
            public float[] Buffer;

            public bool Contains(long index)
            {
                return Offset >= 0 && index >= Offset && index < Offset + ChunkSize;
            }
        }

        private readonly Chunk _chunk1;
        private readonly Chunk _chunk2;

        // 4 bytes per sample so this is 1MB
        private const int ChunkSize = 256 * 1024;

        public long Count { get; }

        public int SampleRate { get; }

        public TimeSpan Length { get; }

        public float Max { get; private set; }

        public float Min { get; private set; }

        public SampleBuffer(string filename, Channel.Sides side, bool filter)
        {
            _reader = new AudioFileReader(filename);
            Count = _reader.Length / (_reader.WaveFormat.BitsPerSample / 8) / _reader.WaveFormat.Channels;
            //Count = _reader.Length;
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
            {
                _sampleProvider = new HighPassSampleProvider(_sampleProvider);
            }

            _chunk1 = new Chunk
            {
                Buffer = new float[ChunkSize],
                Offset = -1
            };
            _chunk2 = new Chunk
            {
                Buffer = new float[ChunkSize],
                Offset = -1
            };
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public float this[long index]
        {
            get
            {
                // We may be accessed from multiple threads; we therefore need to lock access to avoid concurrent access.
                lock (this)
                {
                    // Return from an existing chunk if possible
                    if (_chunk1.Contains(index))
                    {
                        return _chunk1.Buffer[index - _chunk1.Offset];
                    }

                    if (_chunk2.Contains(index))
                    {
                        return _chunk2.Buffer[index - _chunk2.Offset];
                    }

                    // Else pick the lower index chunk to read into
                    var chunk = _chunk1.Offset < _chunk2.Offset ? _chunk1 : _chunk2;
                    // Pick the rounded offset
                    chunk.Offset = (index / ChunkSize) * ChunkSize;
                    try
                    {
                        _reader.Position = chunk.Offset * _reader.WaveFormat.BitsPerSample / 8 *
                                           _reader.WaveFormat.Channels;
                    }
                    catch (NullReferenceException ex)
                    {
                        MessageBox.Show($"{ex.ToString()} \n chunk.Offset: {chunk.Offset}\n_reader.WaveFormat.BitsPerSample: {_reader.WaveFormat.BitsPerSample}\n_reader.WaveFormat.Channels: {_reader.WaveFormat.Channels}");
                    }
                    _sampleProvider.Read(chunk.Buffer, 0, ChunkSize);

                    for (int i = 0; i < ChunkSize; i++)
                    {
                        chunk.Buffer[i] *= _gain;
                    }

                    return chunk.Buffer[index - chunk.Offset];
                }
            }
        }

        public void Analyze()
        {
            Min = float.MaxValue;
            Max = float.MinValue;
            for (int i = 0; i < Count; ++i)
            {
                var sample = this[i];
                if (sample < Min)
                {
                    Min = sample;
                }
                if (sample > Max)
                {
                    Max = sample;
                }
            }
            var peak = Math.Max(Math.Abs(Min), Math.Abs(Max));
            _gain = peak > 0 ? TargetPeak / peak : 1.0f;
            Min *= _gain;
            Max *= _gain;
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

            // Apply the filter
            for (int i = 0; i < result; ++i)
            {
                buffer[i] = _filter.Transform(buffer[offset + i]);
            }

            return result;
        }

        public WaveFormat WaveFormat => _sampleProvider.WaveFormat;
    }
}
