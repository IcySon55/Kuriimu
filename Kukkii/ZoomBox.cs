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

		public delegate void ZoomChangedEventHandler(object sender, EventArgs e);

		[Category("Action")]
		public event ZoomChangedEventHandler ZoomChanged;

		public ZoomBox()
		{
			SizeMode = PictureBoxSizeMode.StretchImage;
			InterpolationMode = InterpolationMode.NearestNeighbor;
			MinimumZoomFactor = 1;
			MaximumZoomFactor = 5;

			MouseEnter += new EventHandler(PicBox_MouseEnter);
			MouseWheel += new MouseEventHandler(PicBox_MouseWheel);
		}

		protected override void OnPaint(PaintEventArgs paintEventArgs)
		{
			paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
			base.OnPaint(paintEventArgs);
		}

		/// <summary>
		/// Make the PictureBox dimensions larger to effect the Zoom.
		/// </summary>
		/// <remarks>Maximum 5 times bigger</remarks>
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

		/// <summary>
		/// Make the PictureBox dimensions smaller to effect the Zoom.
		/// </summary>
		/// <remarks>Minimum 5 times smaller</remarks>
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

		/// <summary>
		/// We use the mousewheel to zoom the picture in or out
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PicBox_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta < 0)
				ZoomOut();
			else
				ZoomIn();
		}

		/// <summary>
		/// Make sure that the PicBox have the focus, otherwise it doesn´t receive 
		/// mousewheel events !.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PicBox_MouseEnter(object sender, EventArgs e)
		{
			if (!Focused)
				Focus();
		}
	}
}