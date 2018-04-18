using System;
using System.Collections.Generic;
using System.Linq;
//using System.Messaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music
{
	public static class SuspendPaint_extensions
	{
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

		private const int WM_SETREDRAW = 11;

		public static void SuspendPaint(this Control control)
		{
			SendMessage(control.Handle, WM_SETREDRAW, false, 0);
		}

		public static void ResumePaint(this Control control)
		{
			SendMessage(control.Handle, WM_SETREDRAW, true, 0);
			control.Invalidate();
			if (!control.Visible)
				control.Visible = false; //Otherwise the control will be redrawn even if it's supposed to be invisible.
		}
	}	
}
