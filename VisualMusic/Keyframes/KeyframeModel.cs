using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace VisualMusic.Keyframes
{
    public enum KfInterpolation { Smooth, Linear, Hold }

    // ---------------------------------------------------------------------------
    // PropertyKeyframe — one keyed point on a single property track
    // ---------------------------------------------------------------------------
    [Serializable]
    public class PropertyKeyframe : ISerializable
    {
        public int Tick { get; set; }
        /// <summary>Stored for future interpolation; not yet applied during playback.</summary>
        public double? Value { get; set; }
        public KfInterpolation Interpolation { get; set; } = KfInterpolation.Smooth;

        public PropertyKeyframe() { }
        public PropertyKeyframe(int tick) { Tick = tick; }

        public PropertyKeyframe(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if      (entry.Name == "tick")   Tick          = (int)entry.Value;
                else if (entry.Name == "value")  Value         = (double?)entry.Value;
                else if (entry.Name == "interp") Interpolation = (KfInterpolation)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("tick",   Tick);
            if (Value.HasValue) info.AddValue("value", Value.Value);
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

        public void Add(int tick, KfInterpolation interp = KfInterpolation.Smooth)
        {
            if (!_frames.ContainsKey(tick))
                _frames.Add(tick, new PropertyKeyframe(tick) { Interpolation = interp });
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

        // ---- Query ----

        public bool HasKeyAt(string propertyId, int tick)
            => _tracks.TryGetValue(propertyId, out var t) && t.HasKeyAt(tick);

        public bool HasAny(string propertyId)
            => _tracks.TryGetValue(propertyId, out var t) && t.HasAny;

        public KfInterpolation? GetInterpolation(string propertyId, int tick)
            => _tracks.TryGetValue(propertyId, out var t) ? t.GetInterpolation(tick) : null;

        // ---- Mutation ----

        public void Add(string propertyId, int tick, KfInterpolation interp = KfInterpolation.Smooth)
        {
            if (!_tracks.TryGetValue(propertyId, out var track))
                _tracks[propertyId] = track = new PropertyKeyframeTrack();
            track.Add(tick, interp);
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
