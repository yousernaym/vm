using System;
using System.Drawing;

namespace LibSidWiz
{
    /// <summary>How a pitch-split channel arranges its per-pitch waveforms.</summary>
    public enum SplitLayout
    {
        /// <summary>Subdivide the channel's row into a vertical slice per currently-visible slot.</summary>
        Stacked = 0,
        /// <summary>Draw all slots in the channel's row, distinguished by colour/brightness.</summary>
        Overlaid = 1,
        /// <summary>Give each visible slot its own full-size row, like an independent channel.</summary>
        Separate = 2
    }

    /// <summary>
    /// Immutable per-voice separated audio for one channel, produced by MIDI-guided STFT masking and
    /// published whole by the app. The render thread latches one reference per frame and reads each
    /// voice's Q15 <see cref="VoiceAudio.Samples"/> for its slot's trigger and waveform.
    /// </summary>
    public sealed class ChannelVoiceSet
    {
        /// <summary>Opaque identity of the inputs this set was built from, compared by the app side.</summary>
        public readonly object Key;
        /// <summary>One entry per voice; length equals the split count it was built for.</summary>
        public readonly VoiceAudio[] Voices;

        public ChannelVoiceSet(object key, VoiceAudio[] voices)
        {
            Key = key;
            Voices = voices;
        }
    }

    /// <summary>
    /// One voice's separated waveform plus the sample-index spans (note intervals extended by a short
    /// release) during which it sounds. Spans are sorted and non-overlapping, so ends ascend too.
    /// </summary>
    public sealed class VoiceAudio
    {
        /// <summary>Full-track PCM as Q15 (−32767..32767 ≈ −1..1); halves memory vs float.</summary>
        public readonly short[] Samples;
        public readonly int[] SpanStarts;  // sorted ascending
        public readonly int[] SpanEnds;    // non-overlapping ⇒ ascending

        public VoiceAudio(short[] samples, int[] spanStarts, int[] spanEnds)
        {
            Samples = samples;
            SpanStarts = spanStarts;
            SpanEnds = spanEnds;
        }

        /// <summary>End (capped at <paramref name="cap"/>) of the latest span starting before cap, or int.MinValue.</summary>
        public int LastActiveEndBefore(int cap)
        {
            var starts = SpanStarts;
            if (starts.Length == 0)
                return int.MinValue;
            // Largest index with SpanStarts[i] < cap.
            int lo = 0, hi = starts.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (starts[mid] < cap)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            if (lo == 0)
                return int.MinValue;
            int end = SpanEnds[lo - 1];
            return end > cap ? cap : end;
        }

        /// <summary>Does any span overlap [start, end)?</summary>
        public bool SoundsIn(int start, int end)
        {
            var starts = SpanStarts;
            if (starts.Length == 0 || start >= end)
                return false;
            // First span that could overlap: largest index with start >= its start, then walk forward.
            int lo = 0, hi = starts.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (starts[mid] < end)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            // Spans [0, lo) all start before end; the latest one is the only candidate that can reach
            // into [start, end) (non-overlapping ⇒ earlier spans end even earlier), but a span can be
            // long, so check backward until a span ends at/before start.
            for (int i = lo - 1; i >= 0; --i)
            {
                if (SpanEnds[i] <= start)
                    break;
                if (SpanStarts[i] < end && SpanEnds[i] > start)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Per-voice rendering slot for a split channel. A slot holds its last trigger across frames
    /// while the voice stays audible but fails to re-sync; once the voice buffer is quiet the
    /// capture is cleared so silent channels do not keep a frozen oscilloscope trace.
    /// </summary>
    internal class WaveSlot
    {
        public int LastTrigger = -1;                   // absolute sample index the held curve is centred on
        public int LastUpdateFrameStart = int.MinValue; // frame-start sample of the last successful update
        public int LastActiveEndSample = int.MinValue;  // song-sample end of this voice's most recent audible energy
        public bool HasCurve;                          // a waveform has been captured and not yet cleared
        public bool VisibleThisFrame;                  // drawn this frame (recently active)
        public Rectangle Bounds;                       // set by the renderer during layout

        // Per-slot amplitude silence detection (Separate/Stacked), mirroring channel-level caches.
        public bool WasActive;                         // shown last frame — lookahead only bridges gaps while true
        public int NextActiveSample = int.MinValue;    // earliest known upcoming active sample
        public int SilentScannedUntil = int.MinValue;  // exclusive end of verified-silent stretch
    }
}
