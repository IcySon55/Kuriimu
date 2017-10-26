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

        bool sameDims;

        /// <summary>
        /// Swizzles points to accomplish a rotated image
        /// Valid degree are: 0, 90, 180, 270
        /// Other values will be clamped to be one of those 4
        /// </summary>
        /// 
        /// <param name="degree">
        /// Value containing the degree by which the image should be rotated
        /// </param>
        public Rotate(int degree, bool sameDims = false)
        {
            this.degree = degree % 360 / 90 * 90;

            this.sameDims = sameDims;
        }

        public Point Get(Point point, int width, int height)
        {
            switch (degree)
            {
                case 90:
                    if (sameDims)
                    {
                        return new Point(
                            width - 1 - (point.Y * width + point.X) / height,
                            (point.Y * width + point.X) % height
                            );
                    }
                    else
                    {
                        return new Point(
                            (height - 1) - point.Y,
                            point.X);
                    }
                case 180:
                    return new Point(
                        (width - 1) - point.X,
                        (height - 1) - point.Y);
                case 270:
                    if (sameDims)
                    {
                        return new Point(
                            (point.Y * width + point.X) / height,
                            height - 1 - (point.Y * width + point.X) % height);
                    }
                    else
                    {
                        return new Point(
                            point.Y,
                            (height - 1) - point.X);
                    }
                default:
                    return point;
            }
        }
    }

    public class Transpose : IImageSwizzle
    {
        bool sameDims;

        /// <summary>
        /// Swizzles the points to accomplish a transposed image
        /// </summary>
        /// <param name="sameDims">
        /// Transposes the points in the range of the original image dimensions
        /// </param>
        public Transpose(bool sameDims = false)
        {
            this.sameDims = sameDims;
        }

        public Point Get(Point point, int width, int height)
        {
            if (sameDims)
            {
                return new Point(
                    (point.Y * width + point.X) / height,
                    (point.Y * width + point.X) % height
                    );
            }
            else
            {
                return new Point(point.Y, point.X);
            }
        }
    }
}
