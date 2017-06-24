﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visual_Music
{
	public partial class BaseDialog : Form
	{
		public BaseDialog()
		{
			InitializeComponent();
		}

		private void okBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void BaseDialog_Load(object sender, EventArgs e)
		{
			okBtn.Focus();
		}
	}
}
