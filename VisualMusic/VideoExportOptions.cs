using System;
using System.Runtime.Serialization;

namespace VisualMusic
{
    [Serializable]
    public class VideoExportOptions : ISerializable
    {
        int _width;
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                UpdateSSAAReso();
            }
        }

        int _height;
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                UpdateSSAAReso();
            }
        }

        public int SSAAWidth { get; private set; }
        public int SSAAHeight { get; private set; }

        int _ssaaFactor = 4;
        public int SSAAFactor
        {
            get => _ssaaFactor;
            set
            {
                _ssaaFactor = value;
                UpdateSSAAReso();
            }
        }

        public bool SSAAEnabled => _ssaaFactor > 1 && SSAAWidth > Width && SSAAHeight > Height;
        public bool Sphere;
        public bool SphericalStereo;
        public bool SphericalMetadata;
        public int VideoQualityLoss = 1;
        public string VideoCrf => VideoQualityLoss <= 1 ? VideoQualityLoss.ToString() : (VideoQualityLoss * 2).ToString();
        public AVCodecID VideoCodec = AVCodecID.AV_CODEC_ID_H264;
        public float Fps = 60;

        public int ResoIndex
        {
            get => Sphere ? _sphereResoIndex : _nonSphereResoIndex;
            set
            {
                if (Sphere)
                    _sphereResoIndex = value;
                else
                    _nonSphereResoIndex = value;
            }
        }

        int _sphereResoIndex = 1;
        int _nonSphereResoIndex = 1;
        public int SsaaIndex => (int)Math.Log(_ssaaFactor, 2);

        public VideoExportOptions()
        {
        }

        public VideoExportOptions(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "sphere")
                    Sphere = (bool)entry.Value;
                else if (entry.Name == "vrMeta")
                    SphericalMetadata = (bool)entry.Value;
                else if (entry.Name == "vrStereo")
                    SphericalStereo = (bool)entry.Value;
                else if (entry.Name == "sphereResoIndex")
                    _sphereResoIndex = (int)entry.Value;
                else if (entry.Name == "nonSphereResoIndex")
                    _nonSphereResoIndex = (int)entry.Value;
                else if (entry.Name == "ssaaFactor")
                    SSAAFactor = (int)entry.Value;
                else if (entry.Name == "videoQualityLoss")
                    VideoQualityLoss = (int)entry.Value;
                else if (entry.Name == "fps")
                    Fps = (float)entry.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sphere", Sphere);
            info.AddValue("vrMeta", SphericalMetadata);
            info.AddValue("vrStereo", SphericalStereo);
            info.AddValue("sphereResoIndex", Math.Max(0, _sphereResoIndex));
            info.AddValue("nonSphereResoIndex", Math.Max(0, _nonSphereResoIndex));
            info.AddValue("ssaaFactor", SSAAFactor);
            info.AddValue("videoQualityLoss", VideoQualityLoss);
            info.AddValue("fps", Fps);
        }

        void UpdateSSAAReso()
        {
            SSAAWidth = Width * _ssaaFactor;
            SSAAHeight = Height * _ssaaFactor;
            while (SSAAWidth > 16384 || SSAAHeight > 16384)
            {
                SSAAWidth /= 2;
                SSAAHeight /= 2;
            }
        }
    }
}
