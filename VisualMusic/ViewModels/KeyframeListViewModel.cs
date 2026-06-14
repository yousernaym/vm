using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VisualMusic.Keyframes;

namespace VisualMusic.ViewModels
{
    // -------------------------------------------------------------------------
    // Row model for the keyframe list
    // -------------------------------------------------------------------------

    public partial class KeyframeRowViewModel : ObservableObject
    {
        [ObservableProperty] int _tick;
        [ObservableProperty] string _description = "";
        [ObservableProperty] int _propertyCount;

        /// <summary>
        /// Beat position string derived from the tick, used as the display label.
        /// Updated externally when the project's tick-per-beat ratio is known.
        /// </summary>
        [ObservableProperty] string _timeDisplay = "";

        public KeyframeRowViewModel() { }
        public KeyframeRowViewModel(int tick, string desc, int propCount, string timeDisplay)
        {
            _tick = tick;
            _description = desc;
            _propertyCount = propCount;
            _timeDisplay = timeDisplay;
        }
    }

    // -------------------------------------------------------------------------
    // Item for the property-filter dropdown
    // -------------------------------------------------------------------------

    public class KeyframePropertyViewModel
    {
        public string Id { get; }   // full property id, e.g. "track/2/LineWidth"
        public string Display { get; }

        public KeyframePropertyViewModel(string id, string display) { Id = id; Display = display; }
    }

    // -------------------------------------------------------------------------
    // ViewModel for the full keyframe list panel
    // -------------------------------------------------------------------------

    public partial class KeyframeListViewModel : ObservableObject
    {
        public ObservableCollection<KeyframeRowViewModel> Rows { get; } = new();
        public ObservableCollection<KeyframePropertyViewModel> Properties { get; } = new();

        /// <summary>
        /// Full property id to filter the list by, or null to show all keyframes.
        /// Set via <see cref="SetPropertyFilter"/>.
        /// </summary>
        public string PropertyFilterId { get; private set; }

        Project _project;

        public Project Project
        {
            get => _project;
            set
            {
                _project = value;
                Rebuild();
            }
        }

        /// <summary>Full rebuild: refreshes both the dropdown property list and the rows.</summary>
        public void Rebuild()
        {
            RebuildProperties();
            RebuildRows();
        }

        /// <summary>
        /// Repopulates the dropdown's property list from the keyframe set. Called when the keyframe
        /// SET changes (project load, add/remove) — NOT when the filter selection changes, so the
        /// dropdown's ItemsSource is never mutated from within its own selection events.
        /// </summary>
        public void RebuildProperties()
        {
            Properties.Clear();
            if (_project == null) return;

            var kfs = _project.PropertyKeyframes;
            var props = kfs.Tracks.Keys
                .Select(id => new KeyframePropertyViewModel(id, KeyframeService.GetDisplayNameForId(id)))
                .OrderBy(p => p.Display)
                .ToList();
            foreach (var p in props)
                Properties.Add(p);

            // If the active filter's property no longer exists, drop the filter.
            if (PropertyFilterId != null && !kfs.Tracks.ContainsKey(PropertyFilterId))
                PropertyFilterId = null;
        }

        /// <summary>Repopulates the row list, respecting any active property filter.</summary>
        public void RebuildRows()
        {
            Rows.Clear();
            if (_project == null) return;

            var kfs = _project.PropertyKeyframes;
            double tpb = _project.Notes?.TicksPerBeat ?? 480;

            IEnumerable<int> ticks = PropertyFilterId != null
                ? kfs.Tracks.TryGetValue(PropertyFilterId, out var track)
                    ? (IEnumerable<int>)track.Keys
                    : Enumerable.Empty<int>()
                : kfs.AllTicks();

            foreach (int tick in ticks)
            {
                int count = kfs.PropertyCountAt(tick);
                string desc = kfs.GetDescription(tick);
                string time = FormatTime(tick, tpb);
                Rows.Add(new KeyframeRowViewModel(tick, desc, count, time));
            }
        }

        static string FormatTime(int tick, double tpb)
        {
            if (tpb <= 0) return tick.ToString();
            double beats = tick / tpb;
            return beats.ToString("F2");
        }

        public KeyframeRowViewModel FindRow(int tick)
        {
            foreach (var r in Rows)
                if (r.Tick == tick) return r;
            return null;
        }

        /// <summary>
        /// Sets the property filter and rebuilds only the rows. Pass null to show all keyframes.
        /// The dropdown's property list is intentionally left untouched here.
        /// </summary>
        public void SetPropertyFilter(string fullId)
        {
            PropertyFilterId = fullId;
            RebuildRows();
        }

        // ---- Row edits ----

        /// <summary>Commits an in-place time edit (from the DataGrid).</summary>
        public int? CommitTimeEdit(KeyframeRowViewModel row, string newTimeText)
        {
            if (_project == null) return null;
            if (!TryParseTime(newTimeText, _project.Notes?.TicksPerBeat ?? 480, out int newTick))
                return null;
            if (newTick == row.Tick) return null;

            _project.PropertyKeyframes.MoveColumn(row.Tick, newTick);
            KeyframeService.RaiseKeyframesChanged();
            KeyframeService.RaiseUndoSnapshot("Move keyframe");
            return newTick;
        }

        /// <summary>Commits an in-place description edit.</summary>
        public void CommitDescriptionEdit(KeyframeRowViewModel row, string newDesc)
        {
            if (_project == null) return;
            _project.PropertyKeyframes.SetDescription(row.Tick, newDesc);
            row.Description = newDesc;
            KeyframeService.RaiseUndoSnapshot("Edit keyframe label");
        }

        /// <summary>
        /// Deletes one or more keyframe rows. When a property filter is active, only that
        /// property's keyframe is removed from each tick (other properties remain). When no
        /// filter is active the entire tick column (all properties) is deleted.
        /// </summary>
        public void DeleteRows(IEnumerable<KeyframeRowViewModel> rows)
        {
            if (_project == null) return;
            var kfs = _project.PropertyKeyframes;
            foreach (var row in rows)
            {
                if (PropertyFilterId != null)
                    kfs.RemovePropertyAt(PropertyFilterId, row.Tick);
                else
                    kfs.DeleteColumn(row.Tick);
            }
            KeyframeService.RaiseKeyframesChanged();
            KeyframeService.RaiseUndoSnapshot("Delete keyframe");
        }

        /// <summary>Deletes a single keyframe row (convenience wrapper for <see cref="DeleteRows"/>).</summary>
        public void DeleteRow(KeyframeRowViewModel row) => DeleteRows(new[] { row });

        /// <summary>Full property ids (with friendly labels) keyframed at a row's tick.</summary>
        public IEnumerable<(string id, string display)> PropertiesAt(int tick)
        {
            if (_project == null) yield break;
            foreach (var id in _project.PropertyKeyframes.PropertyIdsAt(tick))
                yield return (id, KeyframeService.GetDisplayNameForId(id));
        }

        /// <summary>Removes a single property's keyframe at the row's tick.</summary>
        public void RemoveProperty(KeyframeRowViewModel row, string fullId)
        {
            if (_project == null) return;
            _project.PropertyKeyframes.RemovePropertyAt(fullId, row.Tick);
            KeyframeService.RaiseKeyframesChanged();
            KeyframeService.RaiseUndoSnapshot("Remove keyframe property");
        }

        /// <summary>Seeks to the keyframe at the given row's tick position.</summary>
        public void SeekToRow(KeyframeRowViewModel row)
        {
            _project?.GoToTick(row.Tick);
        }

        static bool TryParseTime(string text, double tpb, out int tick)
        {
            tick = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            // Try beats format e.g. "4.00 b" or plain beat number
            string cleaned = text.Replace("b", "").Trim();
            if (double.TryParse(cleaned, out double beats) && beats >= 0)
            {
                tick = (int)(beats * tpb);
                return true;
            }
            // Fall back: raw tick integer
            return int.TryParse(text.Trim(), out tick) && tick >= 0;
        }
    }
}
