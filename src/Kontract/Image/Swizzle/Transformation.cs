using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kontract.Interface;

namespace Kontract.Image.Swizzle
{
    public class Rotate : IImageSwizzle
    {
        int degree;

        /// <summary>
        /// Swizzles points to accomplish a rotated image
        /// Valid degree are: 0, 90, 180, 270
        /// Other values will be clamped to be one of those 4
        /// </summary>
        /// 
        /// <param name="degree">
        /// Value containing the degree by which the image should be rotated
        /// </param>
        public Rotate(int degree)
        {
            this.degree = degree % 360 / 90 * 90;
        }

        public Point Load(Point point, int width, int height)
        {
            switch (degree)
            {
                case 90:
                    return new Point(
                        (height - 1) - point.Y,
                        point.X);
                case 180:
                    return new Point(
                        (width - 1) - point.X,
                        (height - 1) - point.Y);
                case 270:
                    return new Point(
                        point.Y,
                        (height - 1) - point.X);
                default:
                    return point;
            }
        }

        public Point Save(int pointCount, int tileSize, int width, int height)
        {
            return new Point(0, 0);
        }
    }

    public class Transpose : IImageSwizzle
    {
        public Point Load(Point point, int width, int height)
        {
            return new Point(point.Y, point.X);
        }

        public Point Save(int pointCount, int tileSize, int width, int height)
        {
            return new Point(0, 0);
        }
    }
}
