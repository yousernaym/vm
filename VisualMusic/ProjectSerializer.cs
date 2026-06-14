using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VisualMusic.Keyframes;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace VisualMusic
{
    static class ProjectSerializer
    {
        public static readonly Type[] KnownTypes = new Type[]
        {
            typeof(TrackView), typeof(TrackProps), typeof(StyleProps), typeof(MaterialProps),
            typeof(LightProps), typeof(SpatialProps), typeof(NoteTypeMaterial), typeof(TrackPropsTex),
            typeof(Microsoft.Xna.Framework.Point), typeof(Vector2), typeof(Vector3), typeof(Vector4),
            typeof(NoteStyle_Bar), typeof(NoteStyle_Line), typeof(LineType), typeof(LineHlType),
            typeof(NoteStyle[]), typeof(NoteStyleType), typeof(List<TrackView>), typeof(FileType),
            typeof(MixdownType), typeof(Camera), typeof(List<NoteStyleMod>), typeof(SourceSongType),
            typeof(ImportOptions), typeof(MidiImportOptions), typeof(ModImportOptions), typeof(SidImportOptions),typeof(HvlImportOptions),
            typeof(Quaternion), typeof(XnaColor), typeof(BindingList<LyricsSegment>), typeof(LyricsSegment),
            typeof(ProjProps),
            typeof(List<TrackProps>), typeof(AudioProps),
            // Per-property keyframe model
            typeof(KeyframeSet),
            typeof(Dictionary<string, PropertyKeyframeTrack>),
            typeof(PropertyKeyframeTrack),
            typeof(SortedList<int, PropertyKeyframe>),
            typeof(PropertyKeyframe),
            typeof(KfInterpolation),
            typeof(Dictionary<int, string>),
            typeof(HashSet<int>),
            typeof(KfValue),
            typeof(ScalarKfValue),
            typeof(ColorKfValue),
            typeof(CameraKfValue),
            typeof(StringKfValue),
        };
    }
}
