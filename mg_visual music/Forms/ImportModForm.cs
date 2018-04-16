using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Visual_Music
{
    public partial class ImportModForm : ImportNotesWithAudioForm
    {
		static string[] XmPlayFormats = { "IT", "XM", "S3M", "MTM", "MOD", "UMX", "MO3" };
		static public readonly string[] Formats = Properties.Resources.ModFormats.ToLower().Split(null);
		public ImportModForm()
        {
            InitializeComponent();
			//customInit();
        }
        public ImportModForm(Form1 _parent) : base(_parent)
        {
            InitializeComponent();
            //customInit();
        }
        public void customInit()
        {
			//string supportedFiles = "*.669; *.AMF; *.APUN; *.DSM; *.FAR; *.GDM; *.IT; *.IMF; *.MOD; *.MED; *.MTM; *.OKT; *.S3M; *.STM; *.STX; *.ULT; *.UNI; *.XM;";
			//openNoteFileDlg.Filter = "Module files (" + supportedFiles + ") | " + supportedFiles + " | All files(*.*) | *.*";
		}
        public bool InsTrack
        {
            get { return modInsTrackBtn.Checked; }
            set { if (value) modInsTrackBtn.Checked = true; else modChTrackBtn.Checked = true; }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
			bool xmPlayMixdownSupported = false;
			string ext = Path.GetExtension(NoteFilePath);
			if (ext.Length > 1) //'.' and more
			{
				ext = ext.Substring(1); //Remove '.'
				foreach (string f in XmPlayFormats)
					if (ext.ToLower() == f.ToLower())
						xmPlayMixdownSupported = true;
			}
			importFiles(InsTrack, true, xmPlayMixdownSupported && parent.tpartyIntegrationForm.ModuleMixdown, 0, Midi.FileType.Mod);
        }

		private void ImportModForm_Load(object sender, EventArgs e)
		{
			createFormatFilter("Mod files", Formats);
		}
	}
}
