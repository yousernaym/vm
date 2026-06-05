using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualMusic.Keyframes
{
    /// <summary>
    /// Static bridge between the keyframe data model and the per-control UI behaviors.
    /// Configured once by MainViewModel after a project loads.
    /// </summary>
    public static class KeyframeService
    {
        // ---- State ----

        public static Project Project { get; set; }

        /// <summary>
        /// Indices of the currently selected tracks (0 = global track).
        /// Updated by MainViewModel whenever the track-list selection changes.
        /// </summary>
        public static IReadOnlyList<int> SelectedTrackIndices { get; set; } = Array.Empty<int>();

        public static int CurrentTick => (int)(Project?.SongPosT ?? 0);

        // ---- Events (fired on the WPF UI thread) ----

        /// <summary>
        /// Fired every frame (from <see cref="MainViewModel.NotifyScrollPositionChanged"/>).
        /// Controls update their highlight color in response.
        /// </summary>
        public static event Action RefreshRequested;

        /// <summary>
        /// Fired when the keyframe set changes (add/remove/move) or when track selection changes.
        /// The diamond panel and list view rebuild in response.
        /// </summary>
        public static event Action KeyframesChanged;

        /// <summary>
        /// Fired when a keyframe at a given tick is picked in the UI (e.g. a diamond is clicked).
        /// The list view selects the matching row in response.
        /// </summary>
        public static event Action<int> TickSelected;

        internal static void RaiseRefresh()          => RefreshRequested?.Invoke();
        internal static void RaiseKeyframesChanged() => KeyframesChanged?.Invoke();
        internal static void RaiseTickSelected(int tick) => TickSelected?.Invoke(tick);

        // ---- Playback ----

        /// <summary>Pauses playback at the current position (no-op if already paused).</summary>
        public static void PausePlayback() => Project?.PausePlayback();

        // ---- Display-name registry ----
        // Maps the raw property name (last id segment, e.g. "LineWidth") to the friendly label
        // shown in menus.  Populated by the Keyframing behavior as controls are set up.

        static readonly Dictionary<string, string> _displayNames = new();

        public static void RegisterDisplayName(string rawName, string displayName)
        {
            if (!string.IsNullOrEmpty(rawName) && !string.IsNullOrEmpty(displayName))
                _displayNames[rawName] = displayName;
        }

        /// <summary>Returns a friendly label for a full property id (e.g. "track/2/LineWidth").</summary>
        public static string GetDisplayNameForId(string fullId)
        {
            if (string.IsNullOrEmpty(fullId)) return fullId;
            string raw = fullId.Substring(fullId.LastIndexOf('/') + 1);
            string name = _displayNames.TryGetValue(raw, out var dn) ? dn : raw;
            // Annotate track-scoped ids with their track index for disambiguation
            if (fullId.StartsWith("track/"))
            {
                var parts = fullId.Split('/');
                if (parts.Length >= 3) name += $" (track {parts[1]})";
            }
            return name;
        }

        // ---- Property-ID helpers ----

        public enum KfScope { Project, Track }

        /// <summary>
        /// Returns the full property IDs for a given <paramref name="propertyId"/> and scope.
        /// For project scope returns a single "proj/id"; for track scope returns one per selected track.
        /// </summary>
        public static IEnumerable<string> ResolveIds(string propertyId, KfScope scope)
        {
            if (scope == KfScope.Project)
            {
                yield return $"proj/{propertyId}";
            }
            else
            {
                var indices = SelectedTrackIndices;
                if (indices.Count == 0)
                    yield return $"track/0/{propertyId}";
                else
                    foreach (var idx in indices)
                        yield return $"track/{idx}/{propertyId}";
            }
        }

        // ---- Color-state queries ----

        /// <summary>
        /// True (→ green) when ALL resolved property IDs have a keyframe at the current tick.
        /// </summary>
        public static bool HasKeyHereForAll(string propertyId, KfScope scope)
        {
            var kfs = Project?.PropertyKeyframes;
            if (kfs == null) return false;
            var ids = ResolveIds(propertyId, scope).ToList();
            return ids.Count > 0 && ids.All(id => kfs.HasKeyAt(id, CurrentTick));
        }

        /// <summary>
        /// True (→ blue candidate) when ANY resolved property ID has a keyframe at any tick.
        /// </summary>
        public static bool HasAnyKeyForAny(string propertyId, KfScope scope)
        {
            var kfs = Project?.PropertyKeyframes;
            if (kfs == null) return false;
            return ResolveIds(propertyId, scope).Any(id => kfs.HasAny(id));
        }

        // ---- Mutations ----

        public static void AddKey(string propertyId, KfScope scope)
        {
            if (Project == null) return;
            int tick = CurrentTick;
            foreach (var id in ResolveIds(propertyId, scope))
                Project.PropertyKeyframes.Add(id, tick);
            RaiseKeyframesChanged();
        }

        public static void RemoveKey(string propertyId, KfScope scope)
        {
            if (Project == null) return;
            int tick = CurrentTick;
            foreach (var id in ResolveIds(propertyId, scope))
                Project.PropertyKeyframes.Remove(id, tick);
            RaiseKeyframesChanged();
        }

        public static void ToggleKey(string propertyId, KfScope scope)
        {
            if (HasKeyHereForAll(propertyId, scope)) RemoveKey(propertyId, scope);
            else                                     AddKey(propertyId, scope);
        }

        public static void SetInterpolation(string propertyId, KfScope scope, KfInterpolation interp)
        {
            if (Project == null) return;
            int tick = CurrentTick;
            foreach (var id in ResolveIds(propertyId, scope))
                Project.PropertyKeyframes.ApplyInterpolation(id, tick, interp);
            RaiseKeyframesChanged();
        }

        /// <summary>Returns the interpolation mode at the current tick for the FIRST resolved id, or null.</summary>
        public static KfInterpolation? GetInterpolation(string propertyId, KfScope scope)
        {
            var kfs = Project?.PropertyKeyframes;
            if (kfs == null) return null;
            var id = ResolveIds(propertyId, scope).FirstOrDefault();
            return id == null ? null : kfs.GetInterpolation(id, CurrentTick);
        }

        // ---- Navigation ----

        /// <summary>Seeks to the next tick that has a keyframe for ANY resolved property ID.</summary>
        public static void GoToNext(string propertyId, KfScope scope)
        {
            if (Project == null) return;
            var kfs = Project.PropertyKeyframes;
            int cur = CurrentTick;
            int? best = null;
            foreach (var id in ResolveIds(propertyId, scope))
            {
                var n = kfs.NextTickForProperty(id, cur);
                if (n.HasValue && (!best.HasValue || n.Value < best.Value)) best = n;
            }
            if (best.HasValue)
                Project.GoToTick(best.Value);
        }

        /// <summary>Seeks to the previous tick that has a keyframe for ANY resolved property ID.</summary>
        public static void GoToPrev(string propertyId, KfScope scope)
        {
            if (Project == null) return;
            var kfs = Project.PropertyKeyframes;
            int cur = CurrentTick;
            int? best = null;
            foreach (var id in ResolveIds(propertyId, scope))
            {
                var p = kfs.PrevTickForProperty(id, cur);
                if (p.HasValue && (!best.HasValue || p.Value > best.Value)) best = p;
            }
            if (best.HasValue)
                Project.GoToTick(best.Value);
        }

        /// <summary>Seeks to the next tick that has a keyframe for ANY property (Playback menu).</summary>
        public static void GoToNextAny()
        {
            if (Project == null) return;
            var tick = Project.PropertyKeyframes.NextTick(CurrentTick);
            if (tick.HasValue) Project.GoToTick(tick.Value);
        }

        /// <summary>Seeks to the previous tick that has a keyframe for ANY property (Playback menu).</summary>
        public static void GoToPrevAny()
        {
            if (Project == null) return;
            var tick = Project.PropertyKeyframes.PrevTick(CurrentTick);
            if (tick.HasValue) Project.GoToTick(tick.Value);
        }
    }
}
