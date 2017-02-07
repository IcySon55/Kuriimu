using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kukkii
{
	public class ZoomBox : PictureBox
	{
		private int _zoom = 1;

		[Category("Behavior")]
		public InterpolationMode InterpolationMode { get; set; }

		[Category("Behavior")]
		public int MinimumZoomFactor { get; set; }

		[Category("Behavior")]
		public int MaximumZoomFactor { get; set; }

		[Category("Behavior")]
		public int Zoom
		{
			get { return _zoom; }
		}

		public new Image Image
		{
			get
			{
				return base.Image;
			}
			set
			{
				base.Image = value;
				if (base.Image != null)
				{
					Width = base.Image.Width * _zoom;
					Height = base.Image.Height * _zoom;
					ZoomChanged(this, new EventArgs());
				}
			}
		}

		public delegate void ZoomChangedEventHandler(object sender, EventArgs e);

		[Category("Action")]
		public event ZoomChangedEventHandler ZoomChanged;

		public ZoomBox()
		{
			SizeMode = PictureBoxSizeMode.StretchImage;
			InterpolationMode = InterpolationMode.NearestNeighbor;
			MinimumZoomFactor = 1;
			MaximumZoomFactor = 5;

			MouseEnter += new EventHandler(ZoomBox_MouseEnter);
			MouseWheel += new MouseEventHandler(ZoomBox_MouseWheel);
		}

		protected override void OnPaint(PaintEventArgs paintEventArgs)
		{
			paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
			base.OnPaint(paintEventArgs);
		}

		private void ZoomIn()
		{
			if (Image != null)
			{
				_zoom = Math.Min(_zoom + 1, MaximumZoomFactor);
				Width = Image.Width * _zoom;
				Height = Image.Height * _zoom;
				Invalidate();
				ZoomChanged(this, new EventArgs());
			}
		}

		private void ZoomOut()
		{
			if (Image != null)
			{
				_zoom = Math.Max(_zoom - 1, MinimumZoomFactor);
				Width = Image.Width * _zoom;
				Height = Image.Height * _zoom;
				Invalidate();
				ZoomChanged(this, new EventArgs());
			}
		}

		private void ZoomBox_MouseEnter(object sender, EventArgs e)
		{
			if (!Focused)
				Focus();
		}

		private void ZoomBox_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta < 0)
				ZoomOut();
			else
				ZoomIn();
		}
	}
}