using System;
using System.Drawing;
using System.Windows.Forms;

namespace Knit
{
    public partial class TransparentTextBox : TextBox
    {
        public TransparentTextBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            TextChanged += UserControl2_OnTextChanged;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var backgroundBrush = new SolidBrush(Color.Transparent);
            Graphics g = e.Graphics;
            g.FillRectangle(backgroundBrush, 0, 0, this.Width, this.Height);
            g.DrawString(Text, Font, new SolidBrush(ForeColor), new PointF(0, 0), StringFormat.GenericDefault);
        }

        public void UserControl2_OnTextChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
