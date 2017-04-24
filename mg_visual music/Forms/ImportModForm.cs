using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
    public partial class ImportModForm : SourceFileForm
    {
        public ImportModForm()
        {
            InitializeComponent();
        }
        public ImportModForm(Form1 _parent) : base(_parent)
        {
            InitializeComponent();
        }
        public bool ModInsTrack
        {
            get { return modInsTrackBtn.Checked; }
            set { if (value) modInsTrackBtn.Checked = true; else modChTrackBtn.Checked = true; }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            importFiles(ModInsTrack);
        }
    }
}
