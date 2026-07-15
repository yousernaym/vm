using System;
using System.Collections.Generic;

namespace LibSidWiz.Triggers
{
    /// <summary>
    /// Finds the positive+negative wave with the biggest area (= sum of absolute samples). Every
    /// complete cycle is offered as a candidate so the shared selector can keep the view advancing
    /// smoothly (see <see cref="CandidateTriggerAlgorithm"/>) instead of pinning to the single
    /// loudest cycle for several frames and then leaping past it.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    internal class BiggestWaveAreaTrigger : CandidateTriggerAlgorithm
    {
        public override void CollectCandidates(Channel channel, int startIndex, int endIndex, List<TriggerCandidate> results)
        {
            int lastCrossingPoint = -1; // -1 until the first complete cycle has been seen
            float previousSample = channel.GetSample(startIndex);
            float currentArea = 0;

            // For each sample...
            for (int i = startIndex + 1; i < endIndex; ++i)
            {
                // Add on the area
                var sample = channel.GetSample(i);
                currentArea += Math.Abs(sample);
                if (sample > 0 && previousSample <= 0)
                {
                    // Positive edge - close off the cycle that just ended as a candidate
                    if (lastCrossingPoint >= 0)
                    {
                        results.Add(new TriggerCandidate(lastCrossingPoint, currentArea));
                    }

                    // And reset
                    lastCrossingPoint = i;
                    currentArea = sample;
                }

                previousSample = sample;
            }
        }
    }
}
