using System;
using System.Runtime.Serialization;

namespace VisualMusic
{
    static class ImportHvlForm
    {
        public static readonly string[] Formats = Properties.Resources.HvlFormats.ToLower().Split(null);
    }

    [Serializable()]
    class HvlImportOptions : ImportOptions
    {
        public HvlImportOptions() : base(Midi.FileType.Hvl)
        {
            MixdownType = Midi.MixdownType.Internal;
        }

        public HvlImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
