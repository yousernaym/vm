using System;
using System.Collections.Generic;

namespace LibSidWiz.Triggers
{
    /// <summary>
    /// Finds the wave with the biggest positive area (= sum of positive samples). Every complete
    /// positive hump is offered as a candidate so the shared selector can keep the view advancing
    /// smoothly (see <see cref="CandidateTriggerAlgorithm"/>) instead of pinning to the single
    /// loudest hump for several frames and then leaping past it.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    internal class BiggestPositiveWaveAreaTrigger : CandidateTriggerAlgorithm
    {
        public override void CollectCandidates(Channel channel, int startIndex, int endIndex, List<TriggerCandidate> results)
        {
            int lastCrossingPoint = -1; // -1 until the first positive edge has been seen
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
                    // Positive edge - reset
                    lastCrossingPoint = i;
                    currentArea = sample;
                }
                else if (sample <= 0 && previousSample > 0)
                {
                    // Negative edge - the positive hump just ended, offer it as a candidate
                    if (lastCrossingPoint >= 0)
                    {
                        results.Add(new TriggerCandidate(lastCrossingPoint, currentArea));
                    }
                }

                previousSample = sample;
            }
        }
    }
}
