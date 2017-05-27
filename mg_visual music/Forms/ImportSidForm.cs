using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
    public partial class ImportSidForm : ImportNotesWithAudioForm
    {
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
            importFiles(false, false, parent.tpartyIntegrationForm.SidMixdown);
        }

		private void ImportSidForm_Shown(object sender, EventArgs e)
		{
			if (!parent.tpartyIntegrationForm.SidMixdown)
				existingAudioRbtn.Text = "Audio file";
			else
				existingAudioRbtn.Text = "Audio file (leave empty for SID file audio)";
		}
	}
}
