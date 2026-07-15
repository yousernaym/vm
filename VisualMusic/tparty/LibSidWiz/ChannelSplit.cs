using System.Collections.Generic;
using System.Drawing;

namespace LibSidWiz
{
    /// <summary>How a pitch-split channel arranges its per-pitch waveforms.</summary>
    public enum SplitLayout
    {
        /// <summary>Subdivide the channel's row into a fixed vertical slice per slot.</summary>
        Stacked = 0,
        /// <summary>Draw all slots in the channel's row, distinguished by colour/brightness.</summary>
        Overlaid = 1,
        /// <summary>Give each visible slot its own full-size row, like an independent channel.</summary>
        Separate = 2
    }

    /// <summary>
    /// A span of audio time during which a single pitch class sounds on a track. Times are in the
    /// same "audio seconds" domain the renderer is driven with (song seconds minus playback offset).
    /// <see cref="PitchId"/> is a MIDI semitone number; int.MinValue means "unpitched".
    /// </summary>
    public struct PitchSegment
    {
        public double StartSeconds;
        public double EndSeconds;
        public int PitchId;

        public const int Unpitched = int.MinValue;

        public PitchSegment(double startSeconds, double endSeconds, int pitchId)
        {
            StartSeconds = startSeconds;
            EndSeconds = endSeconds;
            PitchId = pitchId;
        }
    }

    /// <summary>
    /// Appends the pitch segments overlapping [startSeconds, endSeconds), ordered by start, to
    /// results. Implementations must be thread-safe and allocation-light (called on the render thread).
    /// </summary>
    public delegate void PitchSegmentSource(double startSeconds, double endSeconds, List<PitchSegment> results);

    /// <summary>
    /// Per-pitch rendering slot for a split channel. A slot keeps drawing its last found waveform
    /// while its pitch is silent, so alternating pitches each show a steady curve.
    /// </summary>
    internal class WaveSlot
    {
        public int PitchId = PitchSegment.Unpitched;   // pitch class currently owning this slot
        public int LastTrigger = -1;                   // absolute sample index the held curve is centred on
        public int LastUpdateFrameStart = int.MinValue; // frame-start sample of the last successful update
        public int LastActiveEndSample = int.MinValue;  // song-sample end of this pitch's most recent audible segment
        public bool HasCurve;                          // a waveform has been captured at least once
        public bool VisibleThisFrame;                  // drawn this frame (recently active)
        public Rectangle Bounds;                       // set by the renderer during layout
    }
}
