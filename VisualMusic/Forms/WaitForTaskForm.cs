using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualMusic
{
	public partial class WaitForTaskForm : Form
	{
		static CancellationTokenSource cancellationTokenSource;
		public static CancellationToken CancellationToken;
		public object Result;
		public WaitForTaskForm()
		{
			InitializeComponent();
		}

		public DialogResult ShowDialog(Func<object> func)
		{
			Result = null;
			cancellationTokenSource = new CancellationTokenSource();
			CancellationToken = cancellationTokenSource.Token;
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
			cancellationTokenSource.Dispose();
			return DialogResult;
		}

		private void WaitForTaskForm_Load(object sender, EventArgs e)
		{
			
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			cancellationTokenSource.Cancel();
			DialogResult = DialogResult.Cancel;
		}
	}
}
