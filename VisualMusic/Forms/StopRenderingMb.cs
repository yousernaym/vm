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

        private void YesBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void NoBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }
    }
}
