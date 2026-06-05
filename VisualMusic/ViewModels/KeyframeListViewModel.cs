using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VisualMusic.Keyframes;

namespace VisualMusic.ViewModels
{
    // -------------------------------------------------------------------------
    // Row model for the keyframe list
    // -------------------------------------------------------------------------

    public partial class KeyframeRowViewModel : ObservableObject
    {
        [ObservableProperty] int    _tick;
        [ObservableProperty] string _description = "";
        [ObservableProperty] int    _propertyCount;

        /// <summary>
        /// Beat position string derived from the tick, used as the display label.
        /// Updated externally when the project's tick-per-beat ratio is known.
        /// </summary>
        [ObservableProperty] string _timeDisplay = "";

        public KeyframeRowViewModel() { }
        public KeyframeRowViewModel(int tick, string desc, int propCount, string timeDisplay)
        {
            _tick          = tick;
            _description   = desc;
            _propertyCount = propCount;
            _timeDisplay   = timeDisplay;
        }
    }

    // -------------------------------------------------------------------------
    // ViewModel for the full keyframe list panel
    // -------------------------------------------------------------------------

    public partial class KeyframeListViewModel : ObservableObject
    {
        public ObservableCollection<KeyframeRowViewModel> Rows { get; } = new();

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

        public void Rebuild()
        {
            Rows.Clear();
            if (_project == null) return;

            var kfs    = _project.PropertyKeyframes;
            double tpb = _project.Notes?.TicksPerBeat ?? 480;

            foreach (int tick in kfs.AllTicks())
            {
                int    count  = kfs.PropertyCountAt(tick);
                string desc   = kfs.GetDescription(tick);
                string time   = FormatTime(tick, tpb);
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

        // ---- Row edits ----

        /// <summary>Commits an in-place time edit (from the DataGrid).</summary>
        public void CommitTimeEdit(KeyframeRowViewModel row, string newTimeText)
        {
            if (_project == null) return;
            if (!TryParseTime(newTimeText, _project.Notes?.TicksPerBeat ?? 480, out int newTick))
                return;
            if (newTick == row.Tick) return;

            _project.PropertyKeyframes.MoveColumn(row.Tick, newTick);
            KeyframeService.RaiseKeyframesChanged();
        }

        /// <summary>Commits an in-place description edit.</summary>
        public void CommitDescriptionEdit(KeyframeRowViewModel row, string newDesc)
        {
            if (_project == null) return;
            _project.PropertyKeyframes.SetDescription(row.Tick, newDesc);
            row.Description = newDesc;
        }

        /// <summary>
        /// Adds an empty keyframe marker (0 properties) at the current playback position.
        /// Returns the tick it was placed at.
        /// </summary>
        public int AddEmptyAtPlayhead()
        {
            if (_project == null) return 0;
            int tick = (int)_project.SongPosT;
            _project.PropertyKeyframes.AddMarker(tick);
            KeyframeService.RaiseKeyframesChanged();
            return tick;
        }

        /// <summary>Deletes a keyframe column (removes all properties at that tick).</summary>
        public void DeleteRow(KeyframeRowViewModel row)
        {
            if (_project == null) return;
            _project.PropertyKeyframes.DeleteColumn(row.Tick);
            KeyframeService.RaiseKeyframesChanged();
        }

        /// <summary>Full property ids (with friendly labels) keyframed at a row's tick.</summary>
        public System.Collections.Generic.IEnumerable<(string id, string display)> PropertiesAt(int tick)
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
