using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class ImportSidForm : ImportNotesWithAudioForm
    {
        static public readonly string[] Formats = Properties.Resources.SidFormats.ToLower().Split(null);
        Forms.SubSongForm subSongForm = new Forms.SubSongForm();

        public ImportSidForm()
        {
            InitializeComponent();
        }
        public ImportSidForm(Form1 _parent) : base(_parent)
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            importFiles();

        }
        //getSongLength(subSong); //song length in seconds. 0 = NoteExtractor default
        public override void importFiles()
        {
            var options = new SidImportOptions();

            //If sid file exists, extract info about sub songs
            if (File.Exists(options.NotePath))
            {
                subSongForm.init(options.NotePath);
                if (subSongForm.NumSongs == 1 || subSongForm.NumSongs > 1 && subSongForm.ShowDialog() == DialogResult.OK)
                {
                    options.SubSong = subSongForm.SelectedSong;
                    options.SongLengthS = subSongForm.SongLengthS;
                    string mixdownPath = Path.Combine(TpartyIntegrationForm.MixdownOutputDir, Path.GetFileName(options.NotePath)) + ".wav";
                }
                else
                    return;
            }

            //If sid file didn't exist, call importFiles anyway to get error message box
            importFiles(options);
        }

        private void ImportSidForm_Shown(object sender, EventArgs e)
        {
            //if (!Form1.TpartyIntegrationForm.SidMixdown)
            //	existingAudioRbtn.Text = "Audio file";
            //else
            existingAudioRbtn.Text = "Audio file (leave empty for SID file audio)";
        }


        private void ImportSidForm_Load(object sender, EventArgs e)
        {
            createNoteFormatFilter("Sid files", Formats);
        }
    }

    [Serializable()]
    class SidImportOptions : ImportOptions
    {
        public SidImportOptions() : base(Midi.FileType.Sid)
        {
            MixdownType = Midi.MixdownType.Internal;
        }

        public SidImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }


    }
}
