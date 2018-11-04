using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using ColorSpaces;

namespace Visual_Music
{
	[DefaultEvent("SelectionChanged")]
	public abstract partial class TwoD : UserControl
	{
		public event EventHandler SelectionChanged;
		
		bool mouseDown = false;
		float _x;
		protected float X
		{
			get => _x;
			set
			{
				if (_x != value)
				{
					_x = value;
					Invalidate();
				}
			}
		}

		float _y;
		protected float Y
		{
			get => _y;
			set
			{
				if (_y != value)
				{
					_y = value;
					Invalidate();
				}
			}
		}
		protected PointF SelectionPoint
		{
			get => new PointF(_x, _y);
			set
			{
				_x = value.X;
				_y = value.Y;
			}
		}

		Point SelectionPointCoords
		{
			get
			{
				Point p = new Point((int)(SelectionPoint.X * Width), (int)(SelectionPoint.Y * Height));
				if (p.X < 1) p.X = 1;
				if (p.Y < 1) p.Y = 1;
				if (p.X > Width - 5) p.X = Width - 5;
				if (p.Y > Height - 5) p.Y = Height - 5;
				return p;
			}
			set
			{
				X = (float)value.X / Width;
				Y = (float)value.Y / Height;
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		}
		protected PointF origin = new PointF(0, 0);
		protected Pen SelectionPen = new Pen(Color.Black, 2);
		public Color SetSelectionColor
		{
			get => SelectionPen.Color;
			set => SelectionPen.Color = value;
		}
		public int SelectionSize { get; set; } = 20;


		public TwoD()
		{
			InitializeComponent();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.DrawLine(SelectionPen, new Point(SelectionPointCoords.X - SelectionSize / 2, SelectionPointCoords.Y), new Point(SelectionPointCoords.X + SelectionSize / 2, SelectionPointCoords.Y));
			e.Graphics.DrawLine(SelectionPen, new Point(SelectionPointCoords.X, SelectionPointCoords.Y - SelectionSize / 2), new Point(SelectionPointCoords.X, SelectionPointCoords.Y + SelectionSize / 2));
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			SelectionPointCoords = new Point(e.X, e.Y);
			mouseDown = true;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (mouseDown)
				SelectionPointCoords = new Point(e.X, e.Y);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			mouseDown = false;
		}
	}


	//TwoDHueSat-------------------------------
	public class TwoDHueSat : TwoD
	{
		public float Hue
		{
			get => X;
			set => X = value;
		}
		public float Saturation
		{
			get => 1 - Y;
			set => Y = 1 - value;
		}

		public TwoDHueSat() : base()
		{
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			for (int x = 0; x < Width; x++)
			{
				Rectangle r = new Rectangle(x, 0, 1, Height);
				double hue = (double)x / Width;
				Color c1 = new HslColor(hue, 1, 0.5);
				Color c2 = new HslColor(hue, 0, 1);
				using (LinearGradientBrush brush = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Vertical))
				{
					e.Graphics.FillRectangle(brush, r);
				}
			}
			base.OnPaint(e);

		}
	}
}
