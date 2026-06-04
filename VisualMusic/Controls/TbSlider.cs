using System;
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

        int _decimalScale;

        int _decimals;
        public int Decimals
        {
            get { return _decimals; }
            set
            {
                _decimals = value;
                _decimalScale = (int)Math.Pow(10, _decimals);
                trackBar1.Maximum = (int)((_max - _min) * _decimalScale);
            }
        }
        int _decimals2; //used if slider is exponential
        public int Decimals2
        {
            get { return _decimals2; }
            set { _decimals2 = value; }
        }

        double _expBase;
        public double ExpBase
        {
            get { return _expBase; }
            set { _expBase = value; }
        }

        double _min;
        public double Min
        {
            get { return _min; }
            set
            {
                _min = value;
                trackBar1.Maximum = (int)((_max - _min) * _decimalScale);
            }
        }
        double _max;
        public double Max
        {
            get { return _max; }
            set
            {
                _max = value;
                trackBar1.Maximum = (int)((_max - _min) * _decimalScale);
            }
        }
        public double Value
        {
            set
            {
                if (value != Value)
                    textBox1.Text = value.ToString("f" + (_expBase >= 0 ? _decimals2 : _decimals));
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
                return trackBar1.TickFrequency / _decimalScale;
            }
            set
            {
                trackBar1.TickFrequency = (int)(value * _decimalScale);
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

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            double value = (double)trackBar1.Value / _decimalScale + _min;
            int d = _decimals;
            if (_expBase >= 0)
            {
                value = Math.Pow(_expBase, value);
                d = _decimals2;
            }
            textBox1.Text = value.ToString("f" + d);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            int ivalue = 0;
            try
            {
                double value = Convert.ToDouble(textBox1.Text);
                if (_expBase >= 0)
                    ivalue = (int)(Math.Log(value, _expBase) * _decimalScale);
                else
                    ivalue = (int)(value * _decimalScale);
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

        private void TrackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            OnCommitChanges(this, e);
        }

        private void TrackBar1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                OnCommitChanges(this, e);
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
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
