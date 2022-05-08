using System;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class StopRenderingMb : Form
    {
        public StopRenderingMb()
        {
            InitializeComponent();
        }

        private void StopRenderingMb_Load(object sender, EventArgs e)
        {

        }

        private void yesBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void noBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }
    }
}
