using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisualMusic
{
	public partial class TbSlider : UserControl
	{

		public TbSlider()
		{
			InitializeComponent();
			Max = 100;
			TickFreq = 10;
			ExpBase = -1;
			Value = 0;
		}
		
		public event EventHandler ValueChanged;
		public event EventHandler CommitChanges;

		int decimalScale;

		int decimals;
		public int Decimals
		{
			get { return decimals; }
			set
			{
				decimals = value;
				decimalScale = (int)Math.Pow(10, decimals);
				trackBar1.Maximum = (int)((max - min) * decimalScale);
			}
		}
		int decimals2; //used if slider is exponential
		public int Decimals2
		{
			get { return decimals2; }
			set { decimals2 = value; }
		}
		
		double expBase;
		public double ExpBase
		{
			get { return expBase; }
			set { expBase = value; }
		}
				
		double min;
		public double Min
		{
			get { return min; }
			set
			{
				min = value;
				trackBar1.Maximum = (int)((max - min) * decimalScale);
			}
		}
		double max;
		public double Max
		{
		    get { return max; }
		    set
		    {
		        max = value;
		        trackBar1.Maximum = (int)((max - min) * decimalScale);
		    }
		}
		public double Value
		{
			set 
			{
				if (value != Value)
					textBox1.Text = value.ToString("f" + (expBase >=0 ? decimals2 : decimals)); 
			}
			get 
			{
				try
				{
					return Convert.ToDouble(textBox1.Text);
				}
				catch
				{
					return 0;
				}
			}
		}

		public double TickFreq
		{
			get
			{
				return trackBar1.TickFrequency / decimalScale;
			}
			set
			{
				trackBar1.TickFrequency = (int)(value * decimalScale);
			}
		}
		public int TbWidth
		{
			get
			{
				return textBox1.Width;
			}
			set
			{
				textBox1.Width = value;
			}
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			double value = (double)trackBar1.Value / decimalScale + min;
			int d = decimals;
			if (expBase >= 0)
			{
				value = Math.Pow(expBase, value);
				d = decimals2;
			}
			textBox1.Text = value.ToString("f"+d);
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			int ivalue = 0;
			try
			{
				double value = Convert.ToDouble(textBox1.Text);
				if (expBase >= 0)
					ivalue = (int)(Math.Log(value, expBase) * decimalScale);
				else
					ivalue = (int)(value * decimalScale);
			}
			catch
			{
				return;
			}
			
			if (ivalue > trackBar1.Maximum)
				ivalue = trackBar1.Maximum;
			if (ivalue < trackBar1.Minimum)
				ivalue = trackBar1.Minimum;
			trackBar1.Value = ivalue;
			
			if (ValueChanged != null)
				ValueChanged(this, e);
		}

		private void trackBar1_MouseUp(object sender, MouseEventArgs e)
		{
			OnCommitChanges(this, e);
		}

		private void trackBar1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				OnCommitChanges(this, e);
		}

		private void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				OnCommitChanges(this, e);
		}

		protected virtual void OnCommitChanges(object sender, EventArgs e)
		{
			var commitChanges = CommitChanges;
			if (commitChanges != null)
				commitChanges(this, e);
		}
	}
}
