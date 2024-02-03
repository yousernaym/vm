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
        public float ViewWidthQn { get; set; }

        public double AudioOffset { get; set; }
        public delegate void Delegate_PropertyChanged<T>(T value);
        public Action OnPlaybackOffsetSChanged;
        float playbackOffsetS = 0;
        public float PlaybackOffsetS
        {
            get => playbackOffsetS;
            set
            {
                playbackOffsetS = value;
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

        public ProjProps()
        {

        }
        public ProjProps(SerializationInfo info, StreamingContext ctxt) : base()
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "qn_viewWidth")
                    ViewWidthQn = (float)entry.Value;
                else if (entry.Name == "audioOffset")
                    AudioOffset = (double)entry.Value;
                else if (entry.Name == "playbackOffsetS")
                    playbackOffsetS = (float)entry.Value;
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
            }
        }
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("qn_viewWidth", ViewWidthQn);
            info.AddValue("audioOffset", AudioOffset);
            info.AddValue("playbackOffsetS", playbackOffsetS);
            info.AddValue("fadeIn", FadeIn);
            info.AddValue("fadeOut", FadeOut);
            info.AddValue("maxPitch", MaxPitch);
            info.AddValue("minPitch", MinPitch);
            info.AddValue("lyrics", LyricsSegments);
            info.AddValue("camera", Camera);
            info.AddValue("userViewWidth", UserViewWidth);
        }

        //internal void readOldProjectFile(SerializationEntry entry)
        //{
        //	if (entry.Name == "qn_viewWidth")
        //		ViewWidthQn = (float)entry.Value;
        //	else if (entry.Name == "audioOffset")
        //		AudioOffset = (double)entry.Value;
        //	else if (entry.Name == "playbackOffsetS")
        //		playbackOffsetS = (float)entry.Value;
        //	else if (entry.Name == "fadeIn")
        //		FadeIn = (float)entry.Value;
        //	else if (entry.Name == "fadeOut")
        //		FadeOut = (float)entry.Value;
        //	else if (entry.Name == "maxPitch")
        //		MaxPitch = (int)entry.Value;
        //	else if (entry.Name == "minPitch")
        //		MinPitch = (int)entry.Value;
        //	else if (entry.Name == "lyrics")
        //		LyricsSegments = (BindingList<LyricsSegment>)entry.Value;
        //	else if (entry.Name == "camera")
        //		Camera = (Camera)entry.Value;
        //	else if (entry.Name == "userViewWidth")
        //		UserViewWidth = (float)entry.Value;
        //}

        //public ProjProps clone()
        //{
        //	var dest = Cloning.clone(this);
        //	dest.OnPlaybackOffsetSChanged = OnPlaybackOffsetSChanged;
        //	dest.OnPlaybackOffsetSChanged?.Invoke();
        //	return dest;
        //}
    }
}