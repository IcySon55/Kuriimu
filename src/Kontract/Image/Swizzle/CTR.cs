using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class CTR : IImageSwizzle
    {
        byte orientation;
        MasterSwizzle zorder;
        MasterSwizzle transform;

        public int Width { get; }
        public int Height { get; }

        public CTR(int width, int height, byte orientation = 0)
        {
            Width = width;
            Height = height;

            this.orientation = orientation;
            zorder = new MasterSwizzle(new[] { (1, 0), (0, 1), (2, 0), (0, 2), (4, 0), (0, 4) }, width, 8, 8);
            transform = new MasterSwizzle(new[] { (0, 1), (0, 2), (0, 4), (1, 0), (2, 0), (4, 0) }, width, 8, 8);
        }

        public Point Get(Point point)
        {
            int pointCount = point.Y * Width + point.X;
            var newPoint = zorder.Get(pointCount, new Point(0, 0))
            ;
            if (orientation != 0)
            {
                byte count = 8;
                while (count != 0)
                {
                    switch (orientation & (int)Math.Pow(2, --count))
                    {
                        case 0x80:
                            break;
                        case 0x40:
                            break;
                        case 0x20:
                            break;
                        case 0x10:
                            break;
                        //Transpose
                        case 0x8:
                            var pointCountTrans = (newPoint.Y / 8 * (Width / 8)) + newPoint.X / 8;
                            newPoint = transform.Get(
                                newPoint.Y % 8 * 8 + newPoint.X % 8,
                                new Point(
                                    pointCountTrans / (Height / 8) * 8,
                                    (newPoint.X / 8) % (Height / 8) * 8));
                            break;
                        //Rotated by 90
                        case 0x4:
                            var pointCountRot90 = (newPoint.Y / 8 * (Width / 8)) + newPoint.X / 8;
                            newPoint = transform.Get(
                                newPoint.Y % 8 * 8 + newPoint.X % 8,
                                new Point(
                                    pointCountRot90 / (Height / 8) * 8,
                                    ((Height / 8) - 1 - (newPoint.X / 8) % (Height / 8)) * 8 + 7));
                            break;
                        case 0x2:
                            break;
                        case 0x1:
                            break;
                    }
                }
            }

            return newPoint;
        }
    }
}
