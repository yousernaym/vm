using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic.Keyframes
{
    public enum KfInterpolation { Smooth, Linear, Hold }

    // ---------------------------------------------------------------------------
    // KfValue — polymorphic keyframe value (scalar, color, quaternion, …)
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Base class for a keyframe value.  Each subclass carries its own type-specific
    /// interpolation logic so callers need not switch on kind.
    /// </summary>
    [Serializable]
    public abstract class KfValue : ISerializable
    {
        protected KfValue() { }
        protected KfValue(SerializationInfo info, StreamingContext ctxt) { }
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    /// <summary>Scalar (double) keyframe value.</summary>
    [Serializable]
    public sealed class ScalarKfValue : KfValue
    {
        public double V;

        public ScalarKfValue() { }
        public ScalarKfValue(double v) { V = v; }

        public ScalarKfValue(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            foreach (SerializationEntry e in info)
                if (e.Name == "v") V = (double)e.Value;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
            => info.AddValue("v", V);
    }

    /// <summary>RGBA color keyframe value with per-channel interpolation.</summary>
    [Serializable]
    public sealed class ColorKfValue : KfValue
    {
        public XnaColor C;

        public ColorKfValue() { }
        public ColorKfValue(XnaColor c) { C = c; }

        public ColorKfValue(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            foreach (SerializationEntry e in info)
                if (e.Name == "c") C = (XnaColor)e.Value;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
            => info.AddValue("c", C);

        /// <summary>Per-channel R/G/B/A lerp with the same smoothstep convention as scalar keyframes.</summary>
        public static ColorKfValue Lerp(ColorKfValue a, ColorKfValue b, double t, KfInterpolation mode)
        {
            if (mode == KfInterpolation.Hold) return a;
            if (mode == KfInterpolation.Smooth) t = t * t * (3.0 - 2.0 * t);
            return new ColorKfValue(new XnaColor(
                (int)Math.Round(a.C.R + (b.C.R - a.C.R) * t),
                (int)Math.Round(a.C.G + (b.C.G - a.C.G) * t),
                (int)Math.Round(a.C.B + (b.C.B - a.C.B) * t),
                (int)Math.Round(a.C.A + (b.C.A - a.C.A) * t)));
        }
    }

    // ---------------------------------------------------------------------------
    // PropertyKeyframe — one keyed point on a single property track
    // ---------------------------------------------------------------------------
    [Serializable]
    public class PropertyKeyframe : ISerializable
    {
        public int Tick { get; set; }
        /// <summary>
        /// Stored value for this keyframe.  Null means "value not captured yet".
        /// May be a <see cref="ScalarKfValue"/> or a <see cref="ColorKfValue"/>.
        /// </summary>
        public KfValue Value { get; set; }
        public KfInterpolation Interpolation { get; set; } = KfInterpolation.Smooth;

        public PropertyKeyframe() { }
        public PropertyKeyframe(int tick) { Tick = tick; }

        public PropertyKeyframe(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if      (entry.Name == "tick")   Tick          = (int)entry.Value;
                else if (entry.Name == "value")
                {
                    // New files write a KfValue; legacy files wrote a raw double.
                    if (entry.Value is KfValue kfv)      Value = kfv;
                    else if (entry.Value is double d)     Value = new ScalarKfValue(d);
                }
                else if (entry.Name == "interp") Interpolation = (KfInterpolation)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("tick",   Tick);
            if (Value != null) info.AddValue("value", Value);
            info.AddValue("interp", Interpolation);
        }
    }

    // ---------------------------------------------------------------------------
    // PropertyKeyframeTrack — all keyframes for one property
    // ---------------------------------------------------------------------------
    [Serializable]
    public class PropertyKeyframeTrack : ISerializable
    {
        SortedList<int, PropertyKeyframe> _frames = new SortedList<int, PropertyKeyframe>();

        public bool HasAny => _frames.Count > 0;
        public IList<int> Keys => _frames.Keys;

        public bool HasKeyAt(int tick) => _frames.ContainsKey(tick);

        public void Add(int tick, KfInterpolation interp = KfInterpolation.Smooth, KfValue value = null)
        {
            if (!_frames.ContainsKey(tick))
                _frames.Add(tick, new PropertyKeyframe(tick) { Interpolation = interp, Value = value });
        }

        /// <summary>
        /// Finds the two keyframes bracketing <paramref name="songPosT"/> and the linear interpolant
        /// t∈[0,1] between them.  Before==null means the position is before all keyframes (snap to
        /// first); After==null means it is past the last (snap to last).
        /// </summary>
        public (PropertyKeyframe Before, PropertyKeyframe After, double T) FindBrackets(int songPosT)
        {
            if (_frames.Count == 0) return (null, null, 0);

            PropertyKeyframe before = null, after = null;
            foreach (var kf in _frames.Values)
            {
                if (kf.Tick <= songPosT) before = kf;
                else { after = kf; break; }
            }

            if (before == null) return (null, _frames.Values[0], 0);
            if (after  == null) return (_frames.Values[_frames.Count - 1], null, 0);

            double t = (after.Tick == before.Tick) ? 0.0
                       : (double)(songPosT - before.Tick) / (after.Tick - before.Tick);
            return (before, after, t);
        }

        /// <summary>
        /// Interpolates between two scalar values according to the given mode.
        /// When <paramref name="logScale"/> is true the interpolation is performed in log2 space
        /// (matches the existing <see cref="KeyFrames"/> ViewWidthQn interpolation).
        /// </summary>
        public static double InterpolateValue(double a, double b, double t,
                                              KfInterpolation mode, bool logScale)
        {
            if (mode == KfInterpolation.Hold) return a;

            if (logScale)
            {
                a = Math.Log(a, 2);
                b = Math.Log(b, 2);
            }

            if (mode == KfInterpolation.Smooth)
                t = t * t * (3.0 - 2.0 * t);

            double result = a + (b - a) * t;
            return logScale ? Math.Pow(2, result) : result;
        }

        public bool Remove(int tick) => _frames.Remove(tick);

        public KfInterpolation? GetInterpolation(int tick)
        {
            if (_frames.TryGetValue(tick, out var kf)) return kf.Interpolation;
            return null;
        }

        public void SetInterpolation(int tick, KfInterpolation interp)
        {
            if (_frames.TryGetValue(tick, out var kf)) kf.Interpolation = interp;
        }

        public void SetValue(int tick, KfValue value)
        {
            if (_frames.TryGetValue(tick, out var kf)) kf.Value = value;
        }

        public int? NextTick(int after)
        {
            foreach (var k in _frames.Keys)
                if (k > after) return k;
            return null;
        }

        public int? PrevTick(int before)
        {
            int? result = null;
            foreach (var k in _frames.Keys)
                if (k < before) result = k;
            return result;
        }

        public void Move(int oldTick, int newTick)
        {
            if (_frames.TryGetValue(oldTick, out var kf))
            {
                _frames.Remove(oldTick);
                if (!_frames.ContainsKey(newTick))
                {
                    kf.Tick = newTick;
                    _frames.Add(newTick, kf);
                }
            }
        }

        public PropertyKeyframeTrack() { }

        public PropertyKeyframeTrack(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "frames")
                    _frames = (SortedList<int, PropertyKeyframe>)entry.Value ?? new SortedList<int, PropertyKeyframe>();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
            => info.AddValue("frames", _frames);
    }

    // ---------------------------------------------------------------------------
    // KeyframeSet — all per-property keyframe tracks for a project
    // ---------------------------------------------------------------------------
    [Serializable]
    public class KeyframeSet : ISerializable
    {
        Dictionary<string, PropertyKeyframeTrack> _tracks =
            new Dictionary<string, PropertyKeyframeTrack>();

        // Per-tick descriptions (shared across all properties at that tick, for the list view)
        Dictionary<int, string> _descriptions = new Dictionary<int, string>();

        // Standalone keyframe positions that may have zero keyed properties (added via the list "+").
        HashSet<int> _markers = new HashSet<int>();

        // ---- Tracks access (read-only, for interpolation) ----

        public IReadOnlyDictionary<string, PropertyKeyframeTrack> Tracks => _tracks;

        // ---- Query ----

        public bool HasKeyAt(string propertyId, int tick)
            => _tracks.TryGetValue(propertyId, out var t) && t.HasKeyAt(tick);

        public bool HasAny(string propertyId)
            => _tracks.TryGetValue(propertyId, out var t) && t.HasAny;

        public KfInterpolation? GetInterpolation(string propertyId, int tick)
            => _tracks.TryGetValue(propertyId, out var t) ? t.GetInterpolation(tick) : null;

        // ---- Mutation ----

        public void Add(string propertyId, int tick, KfInterpolation interp = KfInterpolation.Smooth, KfValue value = null)
        {
            if (!_tracks.TryGetValue(propertyId, out var track))
                _tracks[propertyId] = track = new PropertyKeyframeTrack();
            track.Add(tick, interp, value);
        }

        public void Remove(string propertyId, int tick)
        {
            if (_tracks.TryGetValue(propertyId, out var track))
            {
                track.Remove(tick);
                if (!track.HasAny) _tracks.Remove(propertyId);
            }
        }

        public void Toggle(string propertyId, int tick)
        {
            if (HasKeyAt(propertyId, tick)) Remove(propertyId, tick);
            else                            Add(propertyId, tick);
        }

        /// <summary>Adds a standalone keyframe marker (possibly with zero properties) at a tick.</summary>
        public void AddMarker(int tick) => _markers.Add(tick);

        public void ApplyInterpolation(string propertyId, int tick, KfInterpolation interp)
        {
            if (_tracks.TryGetValue(propertyId, out var t))
                t.SetInterpolation(tick, interp);
        }

        /// <summary>Stores an edited value into an existing keyframe (no-op if none at that tick).</summary>
        public void SetValueAt(string propertyId, int tick, KfValue value)
        {
            if (_tracks.TryGetValue(propertyId, out var t))
                t.SetValue(tick, value);
        }

        /// <summary>
        /// Removes every property whose full id starts with <paramref name="prefix"/>.
        /// Used to purge orphaned keyframe tracks when a mod entry is deleted.
        /// </summary>
        public void RemovePropertiesWithPrefix(string prefix)
        {
            var keys = _tracks.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();
            foreach (var key in keys)
                _tracks.Remove(key);
        }

        /// <summary>
        /// Removes every property's keyframe at <paramref name="tick"/> and cleans up empty tracks.
        /// Used by the list view Delete action.
        /// </summary>
        public void DeleteColumn(int tick)
        {
            foreach (var track in _tracks.Values)
                track.Remove(tick);
            _descriptions.Remove(tick);
            _markers.Remove(tick);
            // Clean up tracks that became empty
            var emptyKeys = _tracks.Where(kv => !kv.Value.HasAny).Select(kv => kv.Key).ToList();
            foreach (var key in emptyKeys)
                _tracks.Remove(key);
        }

        /// <summary>
        /// Moves every property's keyframe at <paramref name="oldTick"/> to <paramref name="newTick"/>.
        /// Used by shift-drag of a diamond marker to relocate a whole time column.
        /// </summary>
        public void MoveColumn(int oldTick, int newTick)
        {
            foreach (var track in _tracks.Values)
                track.Move(oldTick, newTick);
            if (_descriptions.TryGetValue(oldTick, out var desc))
            {
                _descriptions.Remove(oldTick);
                _descriptions[newTick] = desc;
            }
            if (_markers.Remove(oldTick))
                _markers.Add(newTick);
        }

        // ---- Union across all tracks ----

        /// <summary>
        /// Sorted set of all keyframe tick positions: any tick with a keyed property, plus any
        /// standalone marker (empty keyframe).
        /// </summary>
        public IEnumerable<int> AllTicks()
        {
            var set = new SortedSet<int>();
            foreach (var track in _tracks.Values)
                foreach (var k in track.Keys)
                    set.Add(k);
            foreach (var m in _markers)
                set.Add(m);
            return set;
        }

        /// <summary>Number of distinct properties that have a keyframe at <paramref name="tick"/>.</summary>
        public int PropertyCountAt(int tick)
            => _tracks.Count(kv => kv.Value.HasKeyAt(tick));

        /// <summary>Full property ids that have a keyframe at <paramref name="tick"/>.</summary>
        public IEnumerable<string> PropertyIdsAt(int tick)
            => _tracks.Where(kv => kv.Value.HasKeyAt(tick)).Select(kv => kv.Key).ToList();

        /// <summary>Removes the keyframe for a single property id at <paramref name="tick"/>.</summary>
        public void RemovePropertyAt(string propertyId, int tick) => Remove(propertyId, tick);

        // ---- Navigation ----

        public int? NextTick(int after)
        {
            int? best = null;
            foreach (var t in _tracks.Values)
            {
                var n = t.NextTick(after);
                if (n.HasValue && (!best.HasValue || n.Value < best.Value)) best = n;
            }
            foreach (var m in _markers)
                if (m > after && (!best.HasValue || m < best.Value)) best = m;
            return best;
        }

        public int? PrevTick(int before)
        {
            int? best = null;
            foreach (var t in _tracks.Values)
            {
                var p = t.PrevTick(before);
                if (p.HasValue && (!best.HasValue || p.Value > best.Value)) best = p;
            }
            foreach (var m in _markers)
                if (m < before && (!best.HasValue || m > best.Value)) best = m;
            return best;
        }

        public int? NextTickForProperty(string propertyId, int after)
            => _tracks.TryGetValue(propertyId, out var t) ? t.NextTick(after) : null;

        public int? PrevTickForProperty(string propertyId, int before)
            => _tracks.TryGetValue(propertyId, out var t) ? t.PrevTick(before) : null;

        // ---- Descriptions (for the list view "Description" column) ----

        public string GetDescription(int tick)
            => _descriptions.TryGetValue(tick, out var d) ? d ?? "" : "";

        public void SetDescription(int tick, string desc)
        {
            if (string.IsNullOrEmpty(desc)) _descriptions.Remove(tick);
            else                             _descriptions[tick] = desc;
        }

        // ---- Serialization ----

        public KeyframeSet() { }

        public KeyframeSet(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if      (entry.Name == "tracks")       _tracks       = (Dictionary<string, PropertyKeyframeTrack>)entry.Value ?? _tracks;
                else if (entry.Name == "descriptions") _descriptions = (Dictionary<int, string>)entry.Value ?? _descriptions;
                else if (entry.Name == "markers")      _markers      = (HashSet<int>)entry.Value ?? _markers;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("tracks",       _tracks);
            info.AddValue("descriptions", _descriptions);
            info.AddValue("markers",      _markers);
        }
    }
}
