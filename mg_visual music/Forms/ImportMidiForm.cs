using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Visual_Music
{
    public partial class ImportMidiForm : SourceFileForm
    {
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
			importFiles(false, MixdownType.None, AudioFilePath, 0);
        }
    }
}
