using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using System.Runtime.Serialization;

namespace Visual_Music
{
    public partial class ImportModForm : ImportNotesWithAudioForm
    {
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
			importFiles();
        }


		public override void importFiles()
		{
			importFiles(new ModImportOptions());
		}

		private void ImportModForm_Load(object sender, EventArgs e)
		{
			createFormatFilter("Mod files", Formats);
		}
	}

	[Serializable()]
	class ModImportOptions : ImportOptions
	{
		static string[] XmPlayFormats = { "IT", "XM", "S3M", "MTM", "MOD", "UMX", "MO3" };
		new ImportModForm ImportForm;
		public ModImportOptions() : base(Midi.FileType.Mod)
		{
			ImportForm = (ImportModForm)base.ImportForm;
			string ext = NotePath.Split('.').Last().ToLower();
			bool xmPlayMixdownSupported = XmPlayFormats.Contains(ext);
			MixdownType = xmPlayMixdownSupported && Form1.TpartyIntegrationForm.ModuleMixdown ? Midi.MixdownType.Tparty : Midi.MixdownType.Internal;
			InsTrack = ImportForm.InsTrack;
		}

		public ModImportOptions(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
	}
}
