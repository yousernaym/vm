using System;
using System.Runtime.Serialization;

namespace VisualMusic
{
    public partial class ImportMidiForm : ImportNotesWithAudioForm
    {
        static public readonly string[] Formats = Properties.Resources.MidiFormats.ToLower().Split(null);
        public ImportMidiForm()
        {
            InitializeComponent();
        }
        public ImportMidiForm(Form1 _parent) : base(_parent)
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            importFiles();
        }

        public override void importFiles()
        {
            importFiles(new MidiImportOptions());
        }

        private void ImportMidiForm_Load(object sender, EventArgs e)
        {
            createNoteFormatFilter("Midi files", Formats);
        }
    }

    [Serializable()]
    class MidiImportOptions : ImportOptions
    {
        public MidiImportOptions() : base(Midi.FileType.Midi)
        {
            MixdownType = MidMix.sfLoaded() ? Midi.MixdownType.Internal : Midi.MixdownType.None;
        }

        public MidiImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
