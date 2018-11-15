using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class CTRSwizzle : IImageSwizzle
    {
        byte _orientation;
        MasterSwizzle _zorder;

        public int Width { get; }
        public int Height { get; }

        public CTRSwizzle(int width, int height, byte orientation = 0, bool toPowerOf2 = true)
        {
            Width = (toPowerOf2) ? 2 << (int)Math.Log(width - 1, 2) : width;
            Height = (toPowerOf2) ? 2 << (int)Math.Log(height - 1, 2) : height;

            _orientation = orientation;
            //the orientation check was "orientation == 0 ? Width : Height"; based on commit 8c5e0bed for G1Ts newest changes it seems to be only Width as a stride
            _zorder = new MasterSwizzle(orientation == 0 || orientation == 2 ? Width : Height, new Point(0, 0), new[] { (1, 0), (0, 1), (2, 0), (0, 2), (4, 0), (0, 4) });
        }

        public Point Get(Point point)
        {
            int pointCount = point.Y * Width + point.X;
            var newPoint = _zorder.Get(pointCount);

            switch (_orientation)
            {
                //Transpose
                case 8: return new Point(newPoint.Y, newPoint.X);
                //Rotate90 (anti-clockwise)
                case 4: return new Point(newPoint.Y, Height - 1 - newPoint.X);
                //Y Flip (named by Neo :P)
                case 2: return new Point(newPoint.X, Height - 1 - newPoint.Y);
                default: return newPoint;
            }
        }
    }
}
