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
        /// Valid degrees are: 0, 90, 180, 270
        /// Other values will be clamped to be one of those 4
        /// </summary>
        /// 
        /// <param name="degree">
        /// Value containing the degree by which the image should be rotated
        /// </param>
        public Rotate(int degree)
        {
            this.degree = (degree < 0) ? 360 - (degree * -1) % 360 : degree % 360 / 90 * 90;
        }

        public Point Get(Point point, int width, int height)
        {
            switch (degree)
            {
                case 90:
                    return new Point(
                        width - 1 - (point.Y * width + point.X) / height,
                        (point.Y * width + point.X) % height
                        );
                case 180:
                    return new Point(
                        (width - 1) - point.X,
                        (height - 1) - point.Y);
                case 270:
                    return new Point(
                        (point.Y * width + point.X) / height,
                        height - 1 - (point.Y * width + point.X) % height);
                default:
                    return point;
            }
        }
    }

    public class Transpose : IImageSwizzle
    {
        public Point Get(Point point, int width, int height)
        {
            return new Point(
                (point.Y * width + point.X) / height,
                (point.Y * width + point.X) % height
                );
        }
    }
}
