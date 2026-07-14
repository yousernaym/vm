using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace VisualMusic
{
    [Serializable]
    public class ProjProps : ISerializable
    {
        public const float DefaultViewWidthQn = 16; //Number of quarter notes that fits on screen with default camera
        public BindingList<LyricsSegment> LyricsSegments { get; private set; } = new BindingList<LyricsSegment>();

        /// <summary>Legacy project-level viewport width read from old files; migrated to the global track. Not serialized.</summary>
        internal float? LegacyViewWidthQn;

        public double AudioOffset { get; set; }
        public delegate void Delegate_PropertyChanged<T>(T value);
        public Action OnPlaybackOffsetSChanged;
        float _playbackOffsetS = 0;
        public float PlaybackOffsetS
        {
            get => _playbackOffsetS;
            set
            {
                _playbackOffsetS = value;
                OnPlaybackOffsetSChanged?.Invoke();
            }
        }

        public float FadeIn { get; set; } = 0;
        public float FadeOut { get; set; } = 0;

        public int MinPitch { get; set; }
        public int MaxPitch { get; set; }
        public const float NormPitchMargin = 1 / 100.0f;
        int NumPitches { get { return MaxPitch - MinPitch + 1; } }
        public float UserViewWidth = 1000f;
        public float PitchMargin => NormPitchMargin * Camera.ViewportSize.Y;
        public float NoteHeight => (Camera.ViewportSize.Y - PitchMargin * 2) / NumPitches;
        public Camera Camera { get; set; } = new Camera();
        public string BackgroundImagePath { get; set; } = "";
        public float BackgroundImageOpacity { get; set; } = 1.0f;
        public float BackgroundImageSaturation { get; set; } = 1.0f;

        public bool AudioVisLeft { get; set; } = true;
        public bool AudioVisRight { get; set; } = false;
        public float AudioVisWidth { get; set; } = 0.25f;
        public float AudioVisLineWidth { get; set; } = 3;   // matches Channel.cs default
        public float AudioVisOpacity { get; set; } = 0.75f;
        public float AudioVisFillOpacity { get; set; } = 0f;   // 0 = no fill under the waveform
        public bool AudioVisSmoothLines { get; set; } = true;  // anti-aliased vs pixelated lines
        public float AudioVisLabelScale { get; set; } = 1f;    // 0 hides the track labels
        public float AudioVisActivityThresholdDb { get; set; } = -48f; // matches the old 0.004 linear

        public ProjProps()
        {

        }
        public ProjProps(SerializationInfo info, StreamingContext ctxt) : base()
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "qn_viewWidth")
                    LegacyViewWidthQn = (float)entry.Value;
                else if (entry.Name == "audioOffset")
                    AudioOffset = (double)entry.Value;
                else if (entry.Name == "playbackOffsetS")
                    _playbackOffsetS = (float)entry.Value;
                else if (entry.Name == "fadeIn")
                    FadeIn = (float)entry.Value;
                else if (entry.Name == "fadeOut")
                    FadeOut = (float)entry.Value;
                else if (entry.Name == "maxPitch")
                    MaxPitch = (int)entry.Value;
                else if (entry.Name == "minPitch")
                    MinPitch = (int)entry.Value;
                else if (entry.Name == "lyrics")
                    LyricsSegments = (BindingList<LyricsSegment>)entry.Value;
                else if (entry.Name == "camera")
                    Camera = (Camera)entry.Value;
                else if (entry.Name == "userViewWidth")
                    UserViewWidth = (float)entry.Value;
                else if (entry.Name == "backgroundImagePath")
                    BackgroundImagePath = (string)entry.Value;
                else if (entry.Name == "backgroundImageOpacity")
                    BackgroundImageOpacity = (float)entry.Value;
                else if (entry.Name == "backgroundImageSaturation")
                    BackgroundImageSaturation = (float)entry.Value;
                else if (entry.Name == "audioVisLeft")
                    AudioVisLeft = (bool)entry.Value;
                else if (entry.Name == "audioVisRight")
                    AudioVisRight = (bool)entry.Value;
                else if (entry.Name == "audioVisWidth")
                    AudioVisWidth = (float)entry.Value;
                else if (entry.Name == "audioVisLineWidth")
                    AudioVisLineWidth = (float)entry.Value;
                else if (entry.Name == "audioVisOpacity")
                    AudioVisOpacity = (float)entry.Value;
                else if (entry.Name == "audioVisFillOpacity")
                    AudioVisFillOpacity = (float)entry.Value;
                else if (entry.Name == "audioVisSmoothLines")
                    AudioVisSmoothLines = (bool)entry.Value;
                else if (entry.Name == "audioVisLabelScale")
                    AudioVisLabelScale = (float)entry.Value;
                else if (entry.Name == "audioVisActivityThresholdDb")
                    AudioVisActivityThresholdDb = (float)entry.Value;
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("audioOffset", AudioOffset);
            info.AddValue("playbackOffsetS", _playbackOffsetS);
            info.AddValue("fadeIn", FadeIn);
            info.AddValue("fadeOut", FadeOut);
            info.AddValue("maxPitch", MaxPitch);
            info.AddValue("minPitch", MinPitch);
            info.AddValue("lyrics", LyricsSegments);
            info.AddValue("camera", Camera);
            info.AddValue("userViewWidth", UserViewWidth);
            info.AddValue("backgroundImagePath", BackgroundImagePath);
            info.AddValue("backgroundImageOpacity", BackgroundImageOpacity);
            info.AddValue("backgroundImageSaturation", BackgroundImageSaturation);
            info.AddValue("audioVisLeft", AudioVisLeft);
            info.AddValue("audioVisRight", AudioVisRight);
            info.AddValue("audioVisWidth", AudioVisWidth);
            info.AddValue("audioVisLineWidth", AudioVisLineWidth);
            info.AddValue("audioVisOpacity", AudioVisOpacity);
            info.AddValue("audioVisFillOpacity", AudioVisFillOpacity);
            info.AddValue("audioVisSmoothLines", AudioVisSmoothLines);
            info.AddValue("audioVisLabelScale", AudioVisLabelScale);
            info.AddValue("audioVisActivityThresholdDb", AudioVisActivityThresholdDb);
        }
    }
}
