using System;
using System.Collections.Generic;

namespace LibSidWiz.Triggers
{
    /// <summary>
    /// Finds positive edges followed by a near-maximal peak, and among those picks the one closest
    /// to where the previous trigger would land after advancing one frame (previousIndex +
    /// frameSamples). Preferring the expected position keeps the view advancing at playback speed
    /// even when the search window spans several frames (trigger lookahead): selecting purely by
    /// peak height would pin the trigger to the same loud transient for several consecutive frames
    /// and then leap past it, making the animation choppy.
    /// Like the original single-pass peak-speed algorithm (based on code from オップナー2608), this
    /// stays stable for waves which cross the zero point more than once per cycle.
    /// </summary>
    public class PeakSpeedTrigger : ITriggerAlgorithm
    {
        // Crossings whose following peak is at least this fraction of the window's best peak count
        // as equally valid sync points; the tie is broken by proximity to the expected position.
        private const float PeakTolerance = 0.7f;

        // Reused across frames to avoid per-frame allocations; safe because triggers run
        // sequentially on the render thread (WaveformRenderer.PrepareFrame).
        private readonly List<KeyValuePair<int, float>> _candidates = new List<KeyValuePair<int, float>>();

        public int GetTriggerPoint(Channel channel, int startIndex, int endIndex, int frameSamples, int previousIndex)
        {
            _candidates.Clear();
            float maxPeak = float.MinValue;

            int i = startIndex;
            while (i < endIndex)
            {
                // First find a positive edge crossing zero
                while (i < endIndex && channel.GetSample(i) > 0) ++i;
                while (i < endIndex && channel.GetSample(i) <= 0) ++i;
                if (i >= endIndex)
                {
                    break;
                }

                int crossing = i;
                // Measure the peak of the positive run following the crossing
                float peak = 0;
                for (; i < endIndex; ++i)
                {
                    var sample = channel.GetSample(i);
                    if (sample <= 0)
                    {
                        break;
                    }

                    if (sample > peak)
                    {
                        peak = sample;
                    }
                }

                _candidates.Add(new KeyValuePair<int, float>(crossing, peak));
                if (peak > maxPeak)
                {
                    maxPeak = peak;
                }
            }

            if (_candidates.Count == 0)
            {
                return -1;
            }

            // Among the crossings with a near-maximal peak, pick the one nearest the expected
            // position so consecutive frames advance the view by ~frameSamples.
            int expected = previousIndex + frameSamples;
            float threshold = maxPeak * PeakTolerance;
            int result = -1;
            long bestDistance = long.MaxValue;
            foreach (var candidate in _candidates)
            {
                if (candidate.Value < threshold)
                {
                    continue;
                }

                long distance = Math.Abs((long)candidate.Key - expected);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    result = candidate.Key;
                }
            }

            return result;
        }
    }
}
