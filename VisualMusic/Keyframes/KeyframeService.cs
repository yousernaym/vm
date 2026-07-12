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
        /// TrackNumbers of the currently selected tracks (0 = global track).
        /// Updated by MainViewModel whenever the track-list selection changes.
        /// Values are <see cref="TrackView.TrackNumber"/> (stable MIDI track indices),
        /// NOT list positions.
        /// </summary>
        public static IReadOnlyList<int> SelectedTrackIds { get; set; } = Array.Empty<int>();

        public static int CurrentTick => (int)(Project?.SongPosT ?? 0);

        // ---- Undo hook ----

        /// <summary>
        /// Requests a whole-project undo snapshot with the given description.
        /// Wired by <see cref="MainViewModel"/> to <c>AddUndoItem</c> after a project loads;
        /// null before the first project is opened.
        /// </summary>
        public static Action<string> RequestUndoSnapshot { get; set; }

        internal static void RaiseUndoSnapshot(string desc) => RequestUndoSnapshot?.Invoke(desc);

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

        /// <summary>
        /// Fired when a property's context menu requests that the keyframe list filter to that property.
        /// The argument is the full property id to show (e.g. "track/2/LineWidth").
        /// </summary>
        public static event Action<string> FilterByPropertyRequested;

        internal static void RaiseRefresh() => RefreshRequested?.Invoke();
        internal static void RaiseKeyframesChanged() => KeyframesChanged?.Invoke();
        internal static void RaiseTickSelected(int tick) => TickSelected?.Invoke(tick);
        internal static void RaiseFilterByProperty(string id) => FilterByPropertyRequested?.Invoke(id);

        // ---- Playback ----

        /// <summary>Pauses playback at the current position (no-op if already paused).</summary>
        public static void PausePlayback() => Project?.PausePlayback();

        // ---- Unified property table ----
        // Single source of truth: raw id, scope, and friendly display name for every keyframeable property.

        public static readonly (string Id, KfScope Scope, string DisplayName)[] AllKeyframeProperties =
        {
            ("MaxPitch",                  KfScope.Project,  "Max pitch"),
            ("MinPitch",                  KfScope.Project,  "Min pitch"),
            ("Camera",                    KfScope.Project,  "Camera"),
            ("BackgroundImagePath",       KfScope.Project,  "Background image"),
            ("BackgroundImageOpacity",    KfScope.Project,  "Background opacity"),
            ("BackgroundImageSaturation", KfScope.Project,  "Background saturation"),
            ("AudioVisLeft",              KfScope.Project,  "Audio vis left side"),
            ("AudioVisRight",             KfScope.Project,  "Audio vis right side"),
            ("AudioVisWidth",             KfScope.Project,  "Audio vis overlay width"),
            ("AudioVisLineWidth",         KfScope.Project,  "Audio vis line width"),
            ("StyleTypeIndex",            KfScope.Track,    "Note style"),
            ("LineTypeIndex",             KfScope.Track,    "Line type"),
            ("LineWidth",                 KfScope.Track,    "Line width"),
            ("QnGapThreshold",            KfScope.Track,    "Gap fill"),
            ("Continuous",                KfScope.Track,    "Continuous"),
            ("LineHlTypeIndex",           KfScope.Track,    "Highlight type"),
            ("HlSize",                    KfScope.Track,    "Highlight size"),
            ("HlMovementPow",             KfScope.Track,    "Movement power"),
            ("MovingHl",                  KfScope.Track,    "Moving highlight"),
            ("ShrinkingHl",               KfScope.Track,    "Shrinking highlight"),
            ("HlBorder",                  KfScope.Track,    "Highlight border"),
            ("Transp",                    KfScope.Track,    "Opacity"),
            ("MaterialHue",               KfScope.Track,    "Material hue"),
            ("NormalSat",                 KfScope.Track,    "Normal saturation"),
            ("NormalLum",                 KfScope.Track,    "Normal luminance"),
            ("HiliteSat",                 KfScope.Track,    "Highlight saturation"),
            ("HiliteLum",                 KfScope.Track,    "Highlight luminance"),
            ("TexturePath",               KfScope.Track,    "Texture"),
            ("DisableTexture",            KfScope.Track,    "Disable texture"),
            ("PointSmp",                  KfScope.Track,    "Point sampling"),
            ("TexColBlend",               KfScope.Track,    "Texture color blend"),
            ("UTile",                     KfScope.Track,    "U tile"),
            ("VTile",                     KfScope.Track,    "V tile"),
            ("KeepAspect",                KfScope.Track,    "Keep aspect"),
            ("UAnchorIndex",              KfScope.Track,    "U anchor"),
            ("VAnchorIndex",              KfScope.Track,    "V anchor"),
            ("UScroll",                   KfScope.Track,    "U scroll"),
            ("VScroll",                   KfScope.Track,    "V scroll"),
            ("UseGlobalLight",            KfScope.Track,    "Use global light"),
            ("LightDirX",                 KfScope.Track,    "Light direction X"),
            ("LightDirY",                 KfScope.Track,    "Light direction Y"),
            ("LightDirZ",                 KfScope.Track,    "Light direction Z"),
            ("AmbientAmount",             KfScope.Track,    "Ambient amount"),
            ("AmbientColor",              KfScope.Track,    "Ambient color"),
            ("DiffuseAmount",             KfScope.Track,    "Diffuse amount"),
            ("DiffuseColor",              KfScope.Track,    "Diffuse color"),
            ("SpecAmount",                KfScope.Track,    "Specular amount"),
            ("SpecColor",                 KfScope.Track,    "Specular color"),
            ("SpecPower",                 KfScope.Track,    "Specular power"),
            ("MasterAmount",              KfScope.Track,    "Master light amount"),
            ("MasterColor",               KfScope.Track,    "Master light color"),
            ("XOffset",                   KfScope.Track,    "X offset"),
            ("YOffset",                   KfScope.Track,    "Y offset"),
            ("ZOffset",                   KfScope.Track,    "Z offset"),
            ("PitchOffset",               KfScope.Track,    "Pitch offset"),
            ("ViewWidthQn",               KfScope.Track,    "Viewport width"),
            ("SilenceThreshold",          KfScope.Track,    "Silence threshold"),
            ("ModXOriginEnable",          KfScope.TrackMod, "Mod origin X enable"),
            ("ModXOrigin",                KfScope.TrackMod, "Mod origin X"),
            ("ModYOriginEnable",          KfScope.TrackMod, "Mod origin Y enable"),
            ("ModYOrigin",                KfScope.TrackMod, "Mod origin Y"),
            ("ModCombineIndex",           KfScope.TrackMod, "Mod combine"),
            ("ModSquareAspect",           KfScope.TrackMod, "Square aspect"),
            ("ModColorDestEnable",        KfScope.TrackMod, "Mod color dest enable"),
            ("ModColorDest",              KfScope.TrackMod, "Mod color dest"),
            ("ModAngleDestEnable",        KfScope.TrackMod, "Mod angle dest enable"),
            ("ModAngleDest",              KfScope.TrackMod, "Mod angle dest"),
            ("ModStart",                  KfScope.TrackMod, "Mod start"),
            ("ModStop",                   KfScope.TrackMod, "Mod stop"),
            ("ModFadeIn",                 KfScope.TrackMod, "Mod fade in"),
            ("ModFadeOut",                KfScope.TrackMod, "Mod fade out"),
            ("ModPower",                  KfScope.TrackMod, "Mod fade power"),
            ("ModDiscardAfterStop",       KfScope.TrackMod, "Discard after stop"),
            ("ModInvert",                 KfScope.TrackMod, "Mod invert"),
        };

        static readonly Dictionary<string, string> _displayNameLookup =
            AllKeyframeProperties.ToDictionary(e => e.Id, e => e.DisplayName);

        static readonly (string Id, KfScope Scope)[] StyleKeyframeProperties =
        {
            ("StyleTypeIndex",  KfScope.Track),
            ("LineTypeIndex",   KfScope.Track),
            ("LineWidth",       KfScope.Track),
            ("QnGapThreshold",  KfScope.Track),
            ("Continuous",      KfScope.Track),
            ("LineHlTypeIndex", KfScope.Track),
            ("HlSize",          KfScope.Track),
            ("HlMovementPow",   KfScope.Track),
            ("MovingHl",        KfScope.Track),
            ("ShrinkingHl",     KfScope.Track),
            ("HlBorder",        KfScope.Track),
        };

        static readonly (string Id, KfScope Scope)[] MaterialKeyframeProperties =
        {
            ("Transp",         KfScope.Track),
            ("MaterialHue",    KfScope.Track),
            ("NormalSat",      KfScope.Track),
            ("NormalLum",      KfScope.Track),
            ("HiliteSat",      KfScope.Track),
            ("HiliteLum",      KfScope.Track),
            ("TexturePath",    KfScope.Track),
            ("DisableTexture", KfScope.Track),
            ("PointSmp",       KfScope.Track),
            ("TexColBlend",    KfScope.Track),
            ("UTile",          KfScope.Track),
            ("VTile",          KfScope.Track),
            ("KeepAspect",     KfScope.Track),
            ("UAnchorIndex",   KfScope.Track),
            ("VAnchorIndex",   KfScope.Track),
            ("UScroll",        KfScope.Track),
            ("VScroll",        KfScope.Track),
        };

        static readonly (string Id, KfScope Scope)[] LightKeyframeProperties =
        {
            ("UseGlobalLight", KfScope.Track),
            ("LightDirX",      KfScope.Track),
            ("LightDirY",      KfScope.Track),
            ("LightDirZ",      KfScope.Track),
            ("AmbientAmount",  KfScope.Track),
            ("AmbientColor",   KfScope.Track),
            ("DiffuseAmount",  KfScope.Track),
            ("DiffuseColor",   KfScope.Track),
            ("SpecAmount",     KfScope.Track),
            ("SpecColor",      KfScope.Track),
            ("SpecPower",      KfScope.Track),
            ("MasterAmount",   KfScope.Track),
            ("MasterColor",    KfScope.Track),
        };

        static readonly (string Id, KfScope Scope)[] SpatialKeyframeProperties =
        {
            ("XOffset",     KfScope.Track),
            ("YOffset",     KfScope.Track),
            ("ZOffset",     KfScope.Track),
            ("PitchOffset", KfScope.Track),
            ("ViewWidthQn", KfScope.Track),
        };

        /// <summary>Returns a friendly label for a full property id (e.g. "track/2/LineWidth").</summary>
        public static string GetDisplayNameForId(string fullId)
        {
            if (string.IsNullOrEmpty(fullId)) return fullId;
            string raw = fullId.Substring(fullId.LastIndexOf('/') + 1);
            string name = _displayNameLookup.TryGetValue(raw, out var dn) ? dn : raw;
            if (fullId.StartsWith("track/"))
            {
                var parts = fullId.Split('/');
                if (parts.Length >= 3) name += $" (track {parts[1]})";
            }
            return name;
        }

        /// <summary>
        /// Returns the friendly display name for a raw property id (e.g. "LineWidth" → "Line width").
        /// The <paramref name="scope"/> parameter is accepted for API symmetry but is not used.
        /// </summary>
        public static string GetDisplayName(string propId, KfScope scope)
            => _displayNameLookup.TryGetValue(propId, out var dn) ? dn : propId;

        // ---- Property-ID helpers ----

        public enum KfScope { Project, Track, TrackMod }

        /// <summary>
        /// Returns the full property IDs for a given <paramref name="propertyId"/> and scope.
        /// <list type="bullet">
        ///   <item><c>Project</c> — emits a single <c>"proj/{id}"</c></item>
        ///   <item><c>Track</c>   — emits one <c>"track/{trackNumber}/{id}"</c> per selected track</item>
        ///   <item><c>TrackMod</c>— emits one <c>"track/{trackNumber}/mod/{entryId}/{id}"</c> per selected
        ///     track that has an active style with a selected modulation entry; emits nothing otherwise
        ///     so adorners stay dark and Add is inert when nothing is selected.</item>
        /// </list>
        /// </summary>
        public static IEnumerable<string> ResolveIds(string propertyId, KfScope scope)
        {
            if (scope == KfScope.Project)
            {
                yield return $"proj/{propertyId}";
            }
            else if (scope == KfScope.Track)
            {
                var ids = SelectedTrackIds;
                if (ids.Count == 0)
                    yield return $"track/0/{propertyId}";
                else
                    foreach (var tn in ids)
                        yield return $"track/{tn}/{propertyId}";
            }
            else  // TrackMod
            {
                var ids = SelectedTrackIds;
                var trackViews = Project?.TrackViews;
                if (trackViews == null) yield break;

                var tns = ids.Count == 0 ? new[] { 0 } : ids.ToArray();
                foreach (var tn in tns)
                {
                    // Find TrackView by TrackNumber
                    TrackView tv = null;
                    foreach (var t in trackViews)
                        if (t.TrackNumber == tn) { tv = t; break; }
                    if (tv == null) continue;

                    var entry = tv.TrackProps.ActiveNoteStyle?.SelectedModEntry;
                    if (entry == null) continue;          // no entry selected → stay inert

                    yield return $"track/{tn}/mod/{entry.Id}/{propertyId}";
                }
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
            {
                KfValue value = Project.GetCurrentValue(id);
                Project.PropertyKeyframes.Add(id, tick, KfInterpolation.Smooth, value);
            }
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
            else AddKey(propertyId, scope);
        }

        /// <summary>
        /// Stores a freshly edited control value into the keyframe(s) at the current tick. Used after a
        /// keyframeable control commits an edit: if a keyframe exists here (green, or just created via the
        /// blue prompt) its stored value is updated so playback interpolates to the edited value. No-op for
        /// properties with no keyframe at the current tick (free live edit).
        /// </summary>
        public static void SyncEditedValue(string propertyId, KfScope scope, KfValue value)
        {
            if (Project == null) return;
            int tick = CurrentTick;
            foreach (var id in ResolveIds(propertyId, scope))
                if (Project.PropertyKeyframes.HasKeyAt(id, tick))
                    Project.PropertyKeyframes.SetValueAt(id, tick, value);
        }

        /// <summary>
        /// Writes each resolved property's live value into its keyframe at the current tick.
        /// Unlike <see cref="SyncEditedValue"/>, reads a separate value per full property id
        /// (needed for multi-track edits).
        /// </summary>
        public static void SyncCurrentValues(string propertyId, KfScope scope)
        {
            if (Project == null) return;
            int tick = CurrentTick;
            foreach (var id in ResolveIds(propertyId, scope))
            {
                if (!Project.PropertyKeyframes.HasKeyAt(id, tick)) continue;
                var val = Project.GetCurrentValue(id);
                if (val != null)
                    Project.PropertyKeyframes.SetValueAt(id, tick, val);
            }
        }

        // ---- Style-tab default reset ----

        /// <summary>
        /// Non-mod Style-tab properties (for the selected tracks) that already have keyframes anywhere.
        /// Mod properties are excluded: the reset deletes every mod entry, so their keyframes are
        /// purged (see <see cref="PurgeModKeyframesForSelectedTracks"/>) rather than re-captured.
        /// </summary>
        public static List<(string Id, KfScope Scope)> GetStylePropertiesWithKeyframes()
            => GetPropertiesWithKeyframes(StyleKeyframeProperties);

        static List<(string Id, KfScope Scope)> GetPropertiesWithKeyframes(
            IEnumerable<(string Id, KfScope Scope)> properties)
        {
            var list = new List<(string, KfScope)>();
            if (Project == null) return list;
            foreach (var (id, scope) in properties)
            {
                if (HasAnyKeyForAny(id, scope))
                    list.Add((id, scope));
            }
            return list;
        }

        /// <summary>Track numbers a Style-tab action applies to (same fallback as <see cref="ResolveIds"/>).</summary>
        static int[] EffectiveTrackNumbers()
            => SelectedTrackIds.Count == 0 ? new[] { 0 } : SelectedTrackIds.ToArray();

        /// <summary>
        /// Full keyframe-set ids under <c>track/{tn}/mod/</c> for the selected tracks. Scans the
        /// keyframe set directly so it covers ALL mod entries (not just the selected one) and orphans.
        /// </summary>
        static List<string> GetModKeyframeIds()
        {
            var result = new List<string>();
            var kfs = Project?.PropertyKeyframes;
            if (kfs == null) return result;
            foreach (var tn in EffectiveTrackNumbers())
            {
                string prefix = $"track/{tn}/mod/";
                foreach (var id in kfs.Tracks.Keys)
                    if (id.StartsWith(prefix, StringComparison.Ordinal))
                        result.Add(id);
            }
            return result;
        }

        /// <summary>
        /// Removes every mod-entry keyframe track (<c>track/{tn}/mod/…</c>) for the selected tracks.
        /// Called after a Default Style reset, which deletes all mod entries, so their keyframes
        /// don't linger as orphans.
        /// </summary>
        public static void PurgeModKeyframesForSelectedTracks()
        {
            var kfs = Project?.PropertyKeyframes;
            if (kfs == null) return;
            foreach (var tn in EffectiveTrackNumbers())
                kfs.RemovePropertiesWithPrefix($"track/{tn}/mod/");
            RaiseKeyframesChanged();
        }

        /// <summary>
        /// Keyframe-set ids belonging to the selected mod entry of each selected track
        /// (prefix <c>track/{tn}/mod/{eid}/</c>) — the entries a Delete-mod-entry action removes.
        /// </summary>
        static List<string> GetSelectedModEntryKeyframeIds()
        {
            var result = new List<string>();
            var kfs = Project?.PropertyKeyframes;
            var trackViews = Project?.TrackViews;
            if (kfs == null || trackViews == null) return result;
            foreach (var tn in EffectiveTrackNumbers())
            {
                TrackView tv = null;
                foreach (var t in trackViews)
                    if (t.TrackNumber == tn) { tv = t; break; }
                var entry = tv?.TrackProps.ActiveNoteStyle?.SelectedModEntry;
                if (entry == null) continue;
                string prefix = $"track/{tn}/mod/{entry.Id}/";
                foreach (var id in kfs.Tracks.Keys)
                    if (id.StartsWith(prefix, StringComparison.Ordinal))
                        result.Add(id);
            }
            return result;
        }

        /// <summary>
        /// Prompts before deleting the selected mod entry when any of its properties have keyframes.
        /// Returns false if the user declines; true without prompting when there is nothing to warn about.
        /// </summary>
        public static bool ConfirmModEntryDelete()
        {
            var ids = GetSelectedModEntryKeyframeIds();
            if (ids.Count == 0) return true;

            var labels = ids.Select(GetDisplayNameForId).Distinct().ToList();
            labels.Sort(StringComparer.OrdinalIgnoreCase);

            string body = "The following modulation properties will be deleted "
                        + "along with all their keyframes:\n\n"
                        + string.Join("\n", labels.Select(l => "• " + l))
                        + "\n\nContinue?";

            var result = MetroMessageBox.Show(
                body,
                "Delete mod entry",
                System.Windows.MessageBoxButton.OKCancel,
                System.Windows.MessageBoxImage.Question);

            return result == System.Windows.MessageBoxResult.OK;
        }

        static List<string> GetKeyframeLabels(IEnumerable<(string Id, KfScope Scope)> properties,
            bool onlyNotAtCurrentTick = false)
        {
            var kfs = Project?.PropertyKeyframes;
            var labels = new List<string>();
            if (kfs == null) return labels;
            int tick = CurrentTick;
            foreach (var (propId, scope) in properties)
            {
                foreach (var fullId in ResolveIds(propId, scope))
                {
                    if (!kfs.HasAny(fullId)) continue;
                    if (onlyNotAtCurrentTick && kfs.HasKeyAt(fullId, tick)) continue;
                    labels.Add(GetDisplayNameForId(fullId));
                }
            }
            labels.Sort(StringComparer.OrdinalIgnoreCase);
            return labels;
        }

        static void CapturePropertiesAtCurrentTick(IEnumerable<(string Id, KfScope Scope)> affected)
        {
            if (Project == null || affected == null) return;
            foreach (var (propId, scope) in affected)
            {
                if (!HasKeyHereForAll(propId, scope))
                    AddKey(propId, scope);
                SyncCurrentValues(propId, scope);
            }
            RaiseKeyframesChanged();
        }

        /// <summary>
        /// Prompts before a Default Style reset when it would touch existing keyframes: non-mod style
        /// properties with keyframes elsewhere get new keyframes at the current playback position, and
        /// ALL mod-entry keyframes for the selected tracks are deleted (the reset removes every mod
        /// entry). Returns false if the user declines. On acceptance, pauses playback and returns true
        /// (caller should reset style, call <see cref="PurgeModKeyframesForSelectedTracks"/>, then
        /// <see cref="CaptureDefaultStyleAtCurrentTick"/>).
        /// </summary>
        public static bool ConfirmDefaultStyleReset(out List<(string Id, KfScope Scope)> affected)
        {
            affected = GetStylePropertiesWithKeyframes();
            var modIds = GetModKeyframeIds();
            if (affected.Count == 0 && modIds.Count == 0) return true;

            var labels = GetKeyframeLabels(affected, onlyNotAtCurrentTick: true);
            var modLabels = modIds.Select(GetDisplayNameForId).Distinct().ToList();
            modLabels.Sort(StringComparer.OrdinalIgnoreCase);

            if (labels.Count > 0 || modLabels.Count > 0)
            {
                var sections = new List<string>();
                if (labels.Count > 0)
                    sections.Add("The following style properties have keyframes elsewhere "
                               + "but not at the current playback position:\n\n"
                               + string.Join("\n", labels.Select(l => "• " + l))
                               + "\n\nReset to default will create keyframes at the current playback "
                               + "position for these properties.");
                if (modLabels.Count > 0)
                    sections.Add("The following modulation properties will be deleted "
                               + "along with all their keyframes:\n\n"
                               + string.Join("\n", modLabels.Select(l => "• " + l)));
                string body = string.Join("\n\n", sections) + "\n\nContinue?";

                var result = MetroMessageBox.Show(
                    body,
                    "Default style",
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Question);

                if (result != System.Windows.MessageBoxResult.OK)
                {
                    affected = null;
                    return false;
                }
            }

            PausePlayback();
            return true;
        }

        /// <summary>
        /// After <see cref="TrackProps.ResetStyle"/> has run, stores default values into keyframes
        /// at the current tick for every property returned by <see cref="ConfirmDefaultStyleReset"/>.
        /// </summary>
        public static void CaptureDefaultStyleAtCurrentTick(IEnumerable<(string Id, KfScope Scope)> affected)
            => CapturePropertiesAtCurrentTick(affected);

        static bool ConfirmDefaultTrackTabReset(string tabName, string caption,
            IEnumerable<(string Id, KfScope Scope)> properties,
            out List<(string Id, KfScope Scope)> affected)
        {
            affected = GetPropertiesWithKeyframes(properties);
            if (affected.Count == 0) return true;

            var labels = GetKeyframeLabels(affected, onlyNotAtCurrentTick: true);
            if (labels.Count > 0)
            {
                string body = $"The following {tabName} properties have keyframes elsewhere "
                            + "but not at the current playback position:\n\n"
                            + string.Join("\n", labels.Select(l => " - " + l))
                            + "\n\nReset to default will create keyframes at the current playback "
                            + "position for these properties.\n\nContinue?";

                var result = MetroMessageBox.Show(
                    body,
                    caption,
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Question);

                if (result != System.Windows.MessageBoxResult.OK)
                {
                    affected = null;
                    return false;
                }
            }

            PausePlayback();
            return true;
        }

        public static bool ConfirmDefaultMaterialReset(out List<(string Id, KfScope Scope)> affected)
            => ConfirmDefaultTrackTabReset("material", "Default material",
                MaterialKeyframeProperties, out affected);

        public static void CaptureDefaultMaterialAtCurrentTick(
            IEnumerable<(string Id, KfScope Scope)> affected)
            => CapturePropertiesAtCurrentTick(affected);

        public static bool ConfirmDefaultLightReset(out List<(string Id, KfScope Scope)> affected)
            => ConfirmDefaultTrackTabReset("light", "Default light",
                LightKeyframeProperties, out affected);

        public static void CaptureDefaultLightAtCurrentTick(
            IEnumerable<(string Id, KfScope Scope)> affected)
            => CapturePropertiesAtCurrentTick(affected);

        public static bool ConfirmDefaultSpatialReset(out List<(string Id, KfScope Scope)> affected)
            => ConfirmDefaultTrackTabReset("spatial", "Default spatial",
                SpatialKeyframeProperties, out affected);

        public static void CaptureDefaultSpatialAtCurrentTick(
            IEnumerable<(string Id, KfScope Scope)> affected)
            => CapturePropertiesAtCurrentTick(affected);

        // ---- Pitch reset ----

        static readonly (string Id, KfScope Scope)[] PitchKeyframeProperties =
        {
            ("MaxPitch", KfScope.Project),
            ("MinPitch", KfScope.Project),
        };

        /// <summary>Min/max pitch properties that already have keyframes anywhere.</summary>
        public static List<(string Id, KfScope Scope)> GetPitchPropertiesWithKeyframes()
        {
            var list = new List<(string, KfScope)>();
            if (Project == null) return list;
            foreach (var (id, scope) in PitchKeyframeProperties)
            {
                if (HasAnyKeyForAny(id, scope))
                    list.Add((id, scope));
            }
            return list;
        }

        /// <summary>
        /// When min or max pitch has keyframes, prompts if either lacks a keyframe at the current
        /// playback position. Returns false if the user declines. On acceptance, pauses playback
        /// and returns true (caller should reset pitches, then call
        /// <see cref="CapturePitchResetAtCurrentTick"/>).
        /// </summary>
        public static bool ConfirmPitchReset(out List<(string Id, KfScope Scope)> affected)
        {
            affected = GetPitchPropertiesWithKeyframes();
            if (affected.Count == 0) return true;

            var labels = GetKeyframeLabels(affected, onlyNotAtCurrentTick: true);
            if (labels.Count > 0)
            {
                string body = "The following pitch properties have keyframes elsewhere "
                            + "but not at the current playback position:\n\n"
                            + string.Join("\n", labels.Select(l => "• " + l))
                            + "\n\nReset pitches will create keyframes at the current playback position "
                            + "for these properties.\n\nContinue?";

                var result = MetroMessageBox.Show(
                    body,
                    "Reset pitches",
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Question);

                if (result != System.Windows.MessageBoxResult.OK)
                {
                    affected = null;
                    return false;
                }
            }

            PausePlayback();
            return true;
        }

        /// <summary>
        /// After <see cref="Project.ResetPitchLimits"/> has run, stores the reset values into
        /// keyframes at the current tick for every property returned by <see cref="ConfirmPitchReset"/>.
        /// </summary>
        public static void CapturePitchResetAtCurrentTick(IEnumerable<(string Id, KfScope Scope)> affected)
            => CapturePropertiesAtCurrentTick(affected);

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

        // ---- Edit gate (shared by control edits and camera input) ----

        /// <summary>
        /// Gates an edit to a keyframeable property. Returns true if the edit may proceed:
        /// when the property is green (keyframe here) or has no keyframes at all. When the property is
        /// blue (keyframes elsewhere but not here), prompts the user — Yes creates a keyframe and returns
        /// true; No returns false (the caller should cancel/revert the edit).
        /// </summary>
        public static bool EnsureKeyframeForEdit(string propertyId, KfScope scope)
        {
            if (Project == null) return true;
            if (HasKeyHereForAll(propertyId, scope)) return true;   // green → edit the existing keyframe
            if (!HasAnyKeyForAny(propertyId, scope)) return true;   // no keyframes → edit freely

            // blue → prompt
            var label = GetDisplayNameForId(ResolveIds(propertyId, scope).FirstOrDefault() ?? propertyId);
            var result = MetroMessageBox.Show(
                $"There is no keyframe for \"{label}\" at the current playback position.\nCreate one?",
                "Keyframe",
                System.Windows.MessageBoxButton.OKCancel,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.OK)
            {
                AddKey(propertyId, scope);
                return true;
            }
            return false;
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
            {
                Project.GoToTick(best.Value);
                RaiseTickSelected(best.Value);
            }
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
            {
                Project.GoToTick(best.Value);
                RaiseTickSelected(best.Value);
            }
        }

        /// <summary>Seeks to the next tick that has a keyframe for ANY property (Playback menu).</summary>
        public static void GoToNextAny()
        {
            if (Project == null) return;
            var tick = Project.PropertyKeyframes.NextTick(CurrentTick);
            if (tick.HasValue) { Project.GoToTick(tick.Value); RaiseTickSelected(tick.Value); }
        }

        /// <summary>Seeks to the previous tick that has a keyframe for ANY property (Playback menu).</summary>
        public static void GoToPrevAny()
        {
            if (Project == null) return;
            var tick = Project.PropertyKeyframes.PrevTick(CurrentTick);
            if (tick.HasValue) { Project.GoToTick(tick.Value); RaiseTickSelected(tick.Value); }
        }

        // ---- Orphan cleanup ----

        /// <summary>
        /// Removes all keyframe tracks whose property id starts with <paramref name="prefix"/>.
        /// Call this when a modulation entry is deleted, passing <c>"track/{tn}/mod/{entryId}/"</c>
        /// as the prefix, so the deleted entry's keyframes are purged rather than left as inert orphans.
        /// </summary>
        public static void RemoveKeyframesWithPrefix(string prefix)
        {
            Project?.PropertyKeyframes?.RemovePropertiesWithPrefix(prefix);
            RaiseKeyframesChanged();
        }

        // ---- Property-filter helpers (for the list-view dropdown) ----

        /// <summary>
        /// Requests the keyframe list to filter to this property's keyframes.
        /// Picks the first resolved id that actually has any keyframes and fires
        /// <see cref="FilterByPropertyRequested"/> so the list view can respond.
        /// </summary>
        public static void RequestFilterByProperty(string propertyId, KfScope scope)
        {
            var kfs = Project?.PropertyKeyframes;
            if (kfs == null) return;
            var id = ResolveIds(propertyId, scope).FirstOrDefault(i => kfs.HasAny(i));
            if (id != null) RaiseFilterByProperty(id);
        }

        /// <summary>
        /// After confirming with the user, removes ALL keyframe tracks for the given property/scope.
        /// Other properties at the same ticks are not affected.
        /// </summary>
        public static void RemoveAllKeysForProperty(string propertyId, KfScope scope)
        {
            if (Project == null) return;
            var ids = ResolveIds(propertyId, scope).ToList();
            if (ids.Count == 0) return;

            // Build a friendly label for the confirmation dialog
            string label = GetDisplayNameForId(ids[0]);
            if (ids.Count > 1) label += $" (+{ids.Count - 1} more)";

            var result = MetroMessageBox.Show(
                $"Remove all keyframes for \"{label}\"?",
                "Remove property keyframes",
                System.Windows.MessageBoxButton.OKCancel,
                System.Windows.MessageBoxImage.Question);
            if (result != System.Windows.MessageBoxResult.OK) return;

            var kfs = Project.PropertyKeyframes;
            foreach (var id in ids)
                kfs.RemoveProperty(id);
            RaiseKeyframesChanged();
            RaiseUndoSnapshot("Remove all keyframes");
        }
    }
}
