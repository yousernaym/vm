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

namespace Visual_Music.Controls
{
	public abstract partial class TwoD : UserControl
	{
		bool mouseDown = false;
		PointF selectionPoint = new PointF();
		PointF SelectionPoint
		{
			get => selectionPoint;
			set
			{
				selectionPoint = value;
				Invalidate();
			}
		}

		Point SelectionPointCoords
		{
			get
			{
				Point p = new Point((int)(selectionPoint.X * Width), (int)(selectionPoint.Y * Height));
				if (p.X < 1) p.X = 1;
				if (p.Y < 1) p.Y = 1;
				if (p.X > Width - 5) p.X = Width - 5;
				if (p.Y > Height - 5) p.Y = Height - 5;
				return p;
			}
			set => SelectionPoint = new PointF((float)value.X / Width, (float)value.Y / Height);
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

	public class TwoDHueSat : TwoD
	{
		//LinearGradientBrush brush = new LinearGradientBrush(;
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
				Color c2 = new HslColor(hue, 0, 0.5);
				using (LinearGradientBrush brush = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Vertical))
				{
					e.Graphics.FillRectangle(brush, r);
				}
			}
			base.OnPaint(e);

		}
	}
}
