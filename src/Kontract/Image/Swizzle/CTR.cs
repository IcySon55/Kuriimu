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
        MasterSwizzle _transform;

        public int Width { get; }
        public int Height { get; }

        public CTRSwizzle(int width, int height, byte orientation = 0)
        {
            // @todo: Add padToPowerOf2 [Neobeo]
            Width = 2 << (int)Math.Log(width - 1, 2);
            Height = 2 << (int)Math.Log(height - 1, 2);

            _orientation = orientation;
            _zorder = new MasterSwizzle(Width, new[] { (1, 0), (0, 1), (2, 0), (0, 2), (4, 0), (0, 4) });
            _transform = new MasterSwizzle(Width, new[] { (0, 1), (0, 2), (0, 4), (1, 0), (2, 0), (4, 0) });
        }

        public Point Get(Point point)
        {
            int pointCount = point.Y * Width + point.X;
            var newPoint = _zorder.Get(pointCount, new Point(0, 0));

            // @todo: Refactor this out [Neobeo]
            switch (_orientation)
            {
                case 8:
                    var pointCountTrans = (newPoint.Y / 8 * (Width / 8)) + newPoint.X / 8;
                    return _transform.Get(
                        newPoint.Y % 8 * 8 + newPoint.X % 8,
                        new Point(
                            pointCountTrans / (Height / 8) * 8,
                            (newPoint.X / 8) % (Height / 8) * 8));
                case 4:
                    var pointCountRot90 = (newPoint.Y / 8 * (Width / 8)) + newPoint.X / 8;
                    return _transform.Get(
                        newPoint.Y % 8 * 8 + newPoint.X % 8,
                        new Point(
                            pointCountRot90 / (Height / 8) * 8,
                            ((Height / 8) - 1 - (newPoint.X / 8) % (Height / 8)) * 8 + 7));
                default:
                    return newPoint;
            }
        }
    }
}
