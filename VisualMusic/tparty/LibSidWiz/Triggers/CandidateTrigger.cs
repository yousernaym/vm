using System;
using System.Collections.Generic;

namespace LibSidWiz.Triggers
{
    /// <summary>
    /// A possible sync point produced by a <see cref="CandidateTriggerAlgorithm"/>. Score is the
    /// algorithm's own "goodness" measure (peak height, wave area, ...); the selector keeps the
    /// candidates whose score is near the best and then chooses between them by position.
    /// </summary>
    public struct TriggerCandidate
    {
        public int Position;
        public float Score;

        public TriggerCandidate(int position, float score)
        {
            Position = position;
            Score = score;
        }
    }

    /// <summary>
    /// Base class for triggers that work by proposing several candidate sync points and letting a
    /// shared selector pick between them. Splitting "find candidates" from "pick one" means every
    /// such algorithm gets the same continuity behaviour: among the candidates whose score is within
    /// <see cref="ScoreTolerance"/> of the best, prefer the one closest to where the previous trigger
    /// would land after advancing one frame (so the view keeps moving at playback speed instead of
    /// pinning to the same absolute cycle for several frames and then leaping), optionally re-ranked
    /// by waveform-shape similarity to the previous frame (<see cref="Channel.ShapeStabilityWeight"/>).
    /// </summary>
    public abstract class CandidateTriggerAlgorithm : ITriggerAlgorithm
    {
        // Candidates scoring at least this fraction of the window's best count as equally valid sync
        // points; the tie between them is broken by position (proximity / shape).
        protected virtual float ScoreTolerance => 0.7f;

        // Reused across frames to avoid per-frame allocations; safe because triggers run
        // sequentially on the render thread (WaveformRenderer.PrepareFrame). The pitch-split path
        // (Channel.UpdateSlots) borrows this via CandidateScratch to pool candidates across
        // several disjoint sub-ranges before a single Select call.
        private readonly List<TriggerCandidate> _candidates = new List<TriggerCandidate>();

        /// <summary>Appends this algorithm's candidate sync points in [startIndex, endIndex) to results.</summary>
        public abstract void CollectCandidates(Channel channel, int startIndex, int endIndex, List<TriggerCandidate> results);

        /// <summary>Scratch candidate list, reused for pooled multi-range selection (pitch split).</summary>
        internal List<TriggerCandidate> CandidateScratch => _candidates;

        /// <summary>Score tolerance exposed for the pitch-split path, which selects candidates directly.</summary>
        internal float SelectionTolerance => ScoreTolerance;

        public int GetTriggerPoint(Channel channel, int startIndex, int endIndex, int frameSamples, int previousIndex)
        {
            _candidates.Clear();
            CollectCandidates(channel, startIndex, endIndex, _candidates);
            return TriggerCandidateSelector.Select(
                channel, _candidates, ScoreTolerance,
                expected: (long)previousIndex + frameSamples,
                shapeWeight: channel.ShapeStabilityWeight,
                referenceCenter: previousIndex,
                windowSpan: endIndex - startIndex);
        }
    }

    /// <summary>
    /// Picks one trigger point from a list of candidates, shared by every
    /// <see cref="CandidateTriggerAlgorithm"/> and by the pitch-split path.
    /// </summary>
    public static class TriggerCandidateSelector
    {
        // Strided shape comparison never uses more than this many taps, so cost stays bounded
        // regardless of view width.
        private const int MaxShapeTaps = 128;

        [ThreadStatic] private static float[] _refTaps;

        /// <param name="expected">Where the trigger would land if the view advanced smoothly.</param>
        /// <param name="shapeWeight">0 = position only; up to 1 = fully prefer the shape most like last frame.</param>
        /// <param name="referenceCenter">Centre of the previous frame's window (its chosen trigger); &lt; 0 disables shape.</param>
        /// <param name="windowSpan">Search-window width, used to normalise the proximity score.</param>
        /// <returns>The chosen sample index, or -1 if there were no candidates.</returns>
        public static int Select(Channel channel, List<TriggerCandidate> candidates, float tolerance,
            long expected, float shapeWeight, int referenceCenter, int windowSpan)
        {
            if (candidates.Count == 0)
                return -1;

            float maxScore = float.MinValue;
            foreach (var c in candidates)
                if (c.Score > maxScore)
                    maxScore = c.Score;
            float threshold = maxScore * tolerance;

            // Fast path (also the exact behaviour when shape stability is off): among the near-best
            // candidates, pick the one closest to the expected position.
            if (shapeWeight <= 0f || referenceCenter < 0)
            {
                int result = -1;
                long bestDistance = long.MaxValue;
                foreach (var c in candidates)
                {
                    if (c.Score < threshold)
                        continue;
                    long distance = Math.Abs((long)c.Position - expected);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        result = c.Position;
                    }
                }
                return result;
            }

            // Shape-aware path: build a strided reference profile once, then score each near-best
            // candidate by a blend of proximity and normalised cross-correlation to that profile.
            int span = Math.Min(channel.ViewWidthInSamples, 2048);
            if (span < 2)
                span = 2;
            int stride = Math.Max(1, span / MaxShapeTaps);
            int half = span / 2;
            int tapCount = 0;
            for (int t = -half; t < half; t += stride)
                tapCount++;
            if (_refTaps == null || _refTaps.Length < tapCount)
                _refTaps = new float[Math.Max(tapCount, MaxShapeTaps)];

            float refSum = 0f;
            int idx = 0;
            for (int t = -half; t < half; t += stride)
            {
                float v = channel.GetSample(referenceCenter + t);
                _refTaps[idx++] = v;
                refSum += v;
            }
            float refMean = refSum / tapCount;
            float refSumSq = 0f;
            for (int i = 0; i < tapCount; ++i)
            {
                float d = _refTaps[i] - refMean;
                refSumSq += d * d;
            }

            // Degenerate reference (silence / flat) => nothing to match against, fall back to proximity.
            if (refSumSq <= 0f)
            {
                int result = -1;
                long bestDistance = long.MaxValue;
                foreach (var c in candidates)
                {
                    if (c.Score < threshold)
                        continue;
                    long distance = Math.Abs((long)c.Position - expected);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        result = c.Position;
                    }
                }
                return result;
            }

            float spanNorm = Math.Max(1, windowSpan);
            float best = float.MinValue;
            long bestDist = long.MaxValue;
            int chosen = -1;
            foreach (var c in candidates)
            {
                if (c.Score < threshold)
                    continue;

                // Mean of the candidate window over the same taps.
                float sumX = 0f;
                for (int t = -half; t < half; t += stride)
                    sumX += channel.GetSample(c.Position + t);
                float meanX = sumX / tapCount;

                float sumTop = 0f, sumSqX = 0f;
                idx = 0;
                for (int t = -half; t < half; t += stride)
                {
                    float x = channel.GetSample(c.Position + t) - meanX;
                    float y = _refTaps[idx++] - refMean;
                    sumTop += x * y;
                    sumSqX += x * x;
                }

                float similarity = 0f;
                if (sumSqX > 0f)
                {
                    float ncc = (float)(sumTop / Math.Sqrt((double)sumSqX * refSumSq));
                    if (ncc > 0f)
                        similarity = ncc > 1f ? 1f : ncc;
                }

                long distance = Math.Abs((long)c.Position - expected);
                float proximity = 1f - Math.Min(1f, distance / spanNorm);
                float combined = (1f - shapeWeight) * proximity + shapeWeight * similarity;

                if (combined > best || (combined == best && distance < bestDist))
                {
                    best = combined;
                    bestDist = distance;
                    chosen = c.Position;
                }
            }
            return chosen;
        }
    }
}
