using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
    public partial class ImportModForm : ImportNotesWithAudioForm
    {
        public ImportModForm()
        {
            InitializeComponent();
            customInit();
        }
        public ImportModForm(Form1 _parent) : base(_parent)
        {
            InitializeComponent();
            customInit();
        }
        public void customInit()
        {
            string supportedFiles = "*.669; *.AMF; *.APUN; *.DSM; *.FAR; *.GDM; *.IT; *.IMF; *.MOD; *.MED; *.MTM; *.OKT; *.S3M; *.STM; *.STX; *.ULT; *.UNI; *.XM;";
            openNoteFileDlg.Filter = "Module files (" + supportedFiles + ") | " + supportedFiles + " | All files(*.*) | *.*";
        }
        public bool ModInsTrack
        {
            get { return modInsTrackBtn.Checked; }
            set { if (value) modInsTrackBtn.Checked = true; else modChTrackBtn.Checked = true; }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            importFiles(ModInsTrack, true);
        }
    }
}
