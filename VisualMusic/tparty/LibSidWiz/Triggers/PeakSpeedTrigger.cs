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
    public class PeakSpeedTrigger : CandidateTriggerAlgorithm
    {
        public override void CollectCandidates(Channel channel, int startIndex, int endIndex, List<TriggerCandidate> results)
        {
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

                results.Add(new TriggerCandidate(crossing, peak));
            }
        }
    }
}
