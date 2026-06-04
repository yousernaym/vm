using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;

namespace VisualMusic.ViewModels
{
    public partial class SongPropsViewModel : ObservableObject
    {
        Project _project;
        public Project Project
        {
            get => _project;
            set { _project = value; RefreshAll(); }
        }

        ProjProps Props => _project?.Props;

        // ---- Write-back callbacks (set by MainViewModel) ----

        public Action CreateOcTrees { get; set; }

        /// <summary>Called on slider release — rebuilds octrees and adds an undo entry.</summary>
        public Action CommitViewWidth { get; set; }

        public Action ResetPitches { get; set; }
        public Action BrowseBackground { get; set; }
        public Action UnloadBackground { get; set; }
        public Func<int?> NotesMinPitch { get; set; }
        public Func<int?> NotesMaxPitch { get; set; }
        public Func<double?> SongLengthSWithoutPbOffset { get; set; }

        void ApplyToSelectedKeyframes(Action<ProjProps> fn)
        {
            if (_project == null) return;
            foreach (var kf in _project.KeyFrames.Values.Where(kf => kf.Selected))
                fn(kf.ProjProps);
        }

        public void RefreshAll()
        {
            OnPropertyChanged(nameof(ViewWidthQn));
            OnPropertyChanged(nameof(AudioOffset));
            OnPropertyChanged(nameof(PlaybackOffsetS));
            OnPropertyChanged(nameof(FadeIn));
            OnPropertyChanged(nameof(FadeOut));
            OnPropertyChanged(nameof(MaxPitch));
            OnPropertyChanged(nameof(MinPitch));
            OnPropertyChanged(nameof(CameraText));
            OnPropertyChanged(nameof(BackgroundImageOpacity));
            OnPropertyChanged(nameof(BackgroundImageSaturation));
        }

        /// <summary>
        /// Called each frame via <see cref="MainViewModel.NotifyScrollPositionChanged"/> to keep
        /// interpolated / live values (camera, viewport width, background) in sync with playback.
        /// </summary>
        public void RefreshLiveValues()
        {
            OnPropertyChanged(nameof(ViewWidthQn));
            OnPropertyChanged(nameof(CameraText));
            OnPropertyChanged(nameof(BackgroundImageOpacity));
            OnPropertyChanged(nameof(BackgroundImageSaturation));
        }

        // =====================================================================
        // VIEWPORT WIDTH  (keyframe-bound — written to selected keyframe)
        // =====================================================================

        public double? ViewWidthQn
        {
            get => (double?)Props?.ViewWidthQn;
            set
            {
                if (value == null || _project == null) return;
                ApplyToSelectedKeyframes(pp => pp.ViewWidthQn = (float)value);
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // AUDIO OFFSET
        // =====================================================================

        public double? AudioOffset
        {
            get => Props?.AudioOffset;
            set
            {
                if (value == null || Props == null) return;
                Props.AudioOffset = value.Value;
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // PLAYBACK OFFSET
        // =====================================================================

        public double? PlaybackOffsetS
        {
            get => (double?)Props?.PlaybackOffsetS;
            set
            {
                if (value == null || Props == null) return;
                double? maxNeg = SongLengthSWithoutPbOffset?.Invoke();
                if (maxNeg.HasValue && -value > maxNeg.Value) value = -maxNeg.Value;
                Props.PlaybackOffsetS = (float)value;
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // FADE IN / FADE OUT
        // =====================================================================

        public double? FadeIn
        {
            get => (double?)Props?.FadeIn;
            set
            {
                if (value == null || Props == null) return;
                Props.FadeIn = (float)value;
                OnPropertyChanged();
            }
        }

        public double? FadeOut
        {
            get => (double?)Props?.FadeOut;
            set
            {
                if (value == null || Props == null) return;
                Props.FadeOut = (float)value;
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // PITCH RANGE
        // =====================================================================

        public double? MaxPitch
        {
            get => (double?)Props?.MaxPitch;
            set
            {
                if (value == null || Props == null) return;
                int v = (int)value;
                int? minNote = NotesMinPitch?.Invoke();
                if (minNote.HasValue && v < minNote.Value) v = minNote.Value;
                Props.MaxPitch = v;
                CreateOcTrees?.Invoke();
                OnPropertyChanged();
            }
        }

        public double? MinPitch
        {
            get => (double?)Props?.MinPitch;
            set
            {
                if (value == null || Props == null) return;
                int v = (int)value;
                int? maxNote = NotesMaxPitch?.Invoke();
                if (maxNote.HasValue && v > maxNote.Value) v = maxNote.Value;
                Props.MinPitch = v;
                CreateOcTrees?.Invoke();
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // CAMERA  (read-only display — same format as WinForms updateCamControls)
        // =====================================================================

        public string CameraText
        {
            get
            {
                var cam = Props?.Camera;
                if (cam == null) return "";
                var pos = cam.Pos;
                var orient = cam.Orientation;
                return $"{pos.X}\r\n{pos.Y}\r\n{pos.Z}\r\n\r\n{orient.X}\r\n{orient.Y}\r\n{orient.Z}\r\n{orient.W}";
            }
        }

        // =====================================================================
        // BACKGROUND OPACITY  (keyframe-bound)
        // =====================================================================

        public double? BackgroundImageOpacity
        {
            get => (double?)Props?.BackgroundImageOpacity;
            set
            {
                if (value == null) return;
                ApplyToSelectedKeyframes(pp => pp.BackgroundImageOpacity = (float)value);
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // BACKGROUND SATURATION  (keyframe-bound)
        // =====================================================================

        public double? BackgroundImageSaturation
        {
            get => (double?)Props?.BackgroundImageSaturation;
            set
            {
                if (value == null) return;
                ApplyToSelectedKeyframes(pp => pp.BackgroundImageSaturation = (float)value);
                OnPropertyChanged();
            }
        }
    }
}
