using System;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class BaseDialog : Form
    {
        public BaseDialog()
        {
            InitializeComponent();
        }

        private void BaseDialog_Load(object sender, EventArgs e)
        {
            okBtn.Focus();
        }
    }
}
