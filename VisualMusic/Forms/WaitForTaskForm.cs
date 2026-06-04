using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
    public partial class WaitForTaskForm : Form
    {
        static CancellationTokenSource s_cancellationTokenSource;
        public static CancellationToken CancellationToken;
        public object Result;
        public WaitForTaskForm()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(Func<object> func, string message)
        {
            messageLabel.Text = message;
            Result = null;
            s_cancellationTokenSource = new CancellationTokenSource();
            CancellationToken = s_cancellationTokenSource.Token;
            var task = Task<object>.Factory.StartNew(() =>
            {
                object result;
                try
                {
                    result = func();
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                DialogResult = DialogResult.OK;
                return result;
            }, CancellationToken);

            DialogResult = base.ShowDialog();
            Result = task.Result;
            s_cancellationTokenSource.Dispose();
            return DialogResult;
        }

        private void WaitForTaskForm_Load(object sender, EventArgs e)
        {

        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void WaitForTaskForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            s_cancellationTokenSource.Cancel();
        }
    }
}
