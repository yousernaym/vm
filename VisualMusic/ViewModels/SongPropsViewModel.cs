using CommunityToolkit.Mvvm.ComponentModel;
using System;

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

        public Action CreateGeos { get; set; }

        public Action ResetPitches { get; set; }
        public Action BrowseBackground { get; set; }
        public Action UnloadBackground { get; set; }
        public Func<int?> NotesMinPitch { get; set; }
        public Func<int?> NotesMaxPitch { get; set; }
        public Func<double?> SongLengthSWithoutPbOffset { get; set; }

        public void RefreshAll()
        {
            OnPropertyChanged(nameof(AudioOffset));
            OnPropertyChanged(nameof(PlaybackOffsetS));
            OnPropertyChanged(nameof(FadeIn));
            OnPropertyChanged(nameof(FadeOut));
            OnPropertyChanged(nameof(MaxPitch));
            OnPropertyChanged(nameof(MinPitch));
            OnPropertyChanged(nameof(CameraText));
            OnPropertyChanged(nameof(BackgroundImageOpacity));
            OnPropertyChanged(nameof(BackgroundImageSaturation));
            OnPropertyChanged(nameof(AudioVisLeft));
            OnPropertyChanged(nameof(AudioVisRight));
            OnPropertyChanged(nameof(AudioVisWidth));
            OnPropertyChanged(nameof(AudioVisLineWidth));
            OnPropertyChanged(nameof(AudioVisOpacity));
        }

        /// <summary>
        /// Refreshes only the camera readout. Called while the user moves the camera interactively
        /// (WASD / mouse-look) so the textbox tracks the live camera position.
        /// </summary>
        public void RefreshCamera() => OnPropertyChanged(nameof(CameraText));

        /// <summary>
        /// Called each frame via <see cref="MainViewModel.NotifyScrollPositionChanged"/> to keep
        /// interpolated / live values (camera, viewport width, background) in sync with playback.
        /// </summary>
        public void RefreshLiveValues()
        {
            OnPropertyChanged(nameof(AudioOffset));
            OnPropertyChanged(nameof(PlaybackOffsetS));
            OnPropertyChanged(nameof(FadeIn));
            OnPropertyChanged(nameof(FadeOut));
            OnPropertyChanged(nameof(MaxPitch));
            OnPropertyChanged(nameof(MinPitch));
            OnPropertyChanged(nameof(CameraText));
            OnPropertyChanged(nameof(BackgroundImageOpacity));
            OnPropertyChanged(nameof(BackgroundImageSaturation));
            OnPropertyChanged(nameof(AudioVisLeft));
            OnPropertyChanged(nameof(AudioVisRight));
            OnPropertyChanged(nameof(AudioVisWidth));
            OnPropertyChanged(nameof(AudioVisLineWidth));
            OnPropertyChanged(nameof(AudioVisOpacity));
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
                CreateGeos?.Invoke();
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
                CreateGeos?.Invoke();
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // CAMERA  (read-only display)
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
        // BACKGROUND OPACITY
        // =====================================================================

        public double? BackgroundImageOpacity
        {
            get => (double?)Props?.BackgroundImageOpacity;
            set
            {
                if (value == null || Props == null) return;
                Props.BackgroundImageOpacity = (float)value;
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // BACKGROUND SATURATION
        // =====================================================================

        public double? BackgroundImageSaturation
        {
            get => (double?)Props?.BackgroundImageSaturation;
            set
            {
                if (value == null || Props == null) return;
                Props.BackgroundImageSaturation = (float)value;
                OnPropertyChanged();
            }
        }

        // =====================================================================
        // AUDIO VISUALIZATION
        // =====================================================================

        public bool? AudioVisLeft
        {
            get => Props?.AudioVisLeft;
            set
            {
                if (value == null || Props == null) return;
                Props.AudioVisLeft = value.Value;
                OnPropertyChanged();
            }
        }

        public bool? AudioVisRight
        {
            get => Props?.AudioVisRight;
            set
            {
                if (value == null || Props == null) return;
                Props.AudioVisRight = value.Value;
                OnPropertyChanged();
            }
        }

        public double? AudioVisWidth
        {
            get => (double?)Props?.AudioVisWidth;
            set
            {
                if (value == null || Props == null) return;
                Props.AudioVisWidth = (float)value;
                OnPropertyChanged();
            }
        }

        public double? AudioVisLineWidth
        {
            get => (double?)Props?.AudioVisLineWidth;
            set
            {
                if (value == null || Props == null) return;
                Props.AudioVisLineWidth = (float)value;
                OnPropertyChanged();
            }
        }

        public double? AudioVisOpacity
        {
            get => (double?)Props?.AudioVisOpacity;
            set
            {
                if (value == null || Props == null) return;
                Props.AudioVisOpacity = (float)value;
                OnPropertyChanged();
            }
        }
    }
}
