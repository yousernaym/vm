using System;
using System.Collections.Generic;
using LibSidWiz;

namespace VisualMusic
{
    /// <summary>
    /// Immutable per-track snapshot of a MIDI track's pitch segments in the SidWiz "audio seconds"
    /// domain (song seconds minus the playback offset). Built once on the UI thread and queried
    /// lock-free by the SidWiz render thread via <see cref="LibSidWiz.PitchSegmentSource"/>, so
    /// chip arpeggios (emitted by the importer as distinct rapid notes) can be split by pitch.
    /// </summary>
    class SidWizPitchSegments
    {
        private readonly PitchSegment[] _segments; // sorted by StartSeconds, non-overlapping

        private SidWizPitchSegments(PitchSegment[] segments) => _segments = segments;

        /// <summary>
        /// Builds a snapshot from a track's notes, or null if the track has no notes (letting the
        /// channel fall back to detecting pitch from the audio itself). Must run on the UI thread:
        /// it uses the stateful <see cref="Project.TicksToSeconds"/>.
        /// </summary>
        public static SidWizPitchSegments Build(Project project, Midi.Track track)
        {
            var notes = track?.Notes;
            if (notes == null || notes.Count == 0)
                return null;

            // One +1/-1 event per note start/stop; sweep them in tick order.
            var events = new List<(int Tick, int Delta, int Pitch)>(notes.Count * 2);
            foreach (var n in notes)
            {
                if (n.stop <= n.start || n.pitch < 0 || n.pitch > 127)
                    continue;
                events.Add((n.start, +1, n.pitch));
                events.Add((n.stop, -1, n.pitch));
            }
            if (events.Count == 0)
                return null;
            events.Sort((a, b) => a.Tick != b.Tick ? a.Tick.CompareTo(b.Tick) : a.Delta.CompareTo(b.Delta));

            double offsetT = project.PlaybackOffsetT;
            double offsetS = project.Props.PlaybackOffsetS;

            var counts = new int[128];
            int active = 0;
            var segs = new List<PitchSegment>();
            int lastEndTick = int.MinValue;

            int i = 0;
            int prevTick = events[0].Tick;
            while (i < events.Count)
            {
                int tick = events[i].Tick;

                // The interval [prevTick, tick) is covered by the current active set; emit it keyed
                // by the lowest sounding pitch (per-channel chip audio is monophonic in practice).
                if (tick > prevTick && active > 0)
                {
                    int pitch = MinActive(counts);
                    double startSec = project.TicksToSeconds(prevTick + offsetT) - offsetS;
                    double endSec = project.TicksToSeconds(tick + offsetT) - offsetS;
                    if (segs.Count > 0 && prevTick == lastEndTick && segs[segs.Count - 1].PitchId == pitch)
                    {
                        // Contiguous same-pitch run: extend the previous segment.
                        var last = segs[segs.Count - 1];
                        last.EndSeconds = endSec;
                        segs[segs.Count - 1] = last;
                    }
                    else
                    {
                        segs.Add(new PitchSegment(startSec, endSec, pitch));
                    }
                    lastEndTick = tick;
                }

                // Apply every event at this tick.
                while (i < events.Count && events[i].Tick == tick)
                {
                    counts[events[i].Pitch] += events[i].Delta;
                    active += events[i].Delta;
                    ++i;
                }
                prevTick = tick;
            }

            return segs.Count == 0 ? null : new SidWizPitchSegments(segs.ToArray());
        }

        private static int MinActive(int[] counts)
        {
            for (int p = 0; p < counts.Length; ++p)
                if (counts[p] > 0)
                    return p;
            return PitchSegment.Unpitched;
        }

        /// <summary>Appends the segments overlapping [startSeconds, endSeconds) (lock-free, render thread).</summary>
        public void Query(double startSeconds, double endSeconds, List<PitchSegment> results)
        {
            var segs = _segments;
            if (segs.Length == 0)
                return;

            // Binary search for the first segment ending after startSeconds (EndSeconds is ascending
            // because the segments are sorted and non-overlapping).
            int lo = 0, hi = segs.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (segs[mid].EndSeconds > startSeconds)
                    hi = mid;
                else
                    lo = mid + 1;
            }
            for (int i = lo; i < segs.Length && segs[i].StartSeconds < endSeconds; ++i)
                results.Add(segs[i]);
        }
    }
}
