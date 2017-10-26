using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kontract.Interface;

namespace Kontract.Image.Swizzle
{
    public class ZOrder : IImageSwizzle
    {
        public Point Load(Point point, int width, int height)
        {
            return new Point(
                ZOrderX(width, point.Y * width + point.X),
                ZOrderY(width, point.Y * width + point.X));
        }

        int ZOrderX(int tileSize, int count)
        {
            var div = tileSize / 2;
            var x_in = count / div & div;

            while (div > 1)
            {
                div /= 2;
                x_in |= count / div & div;
            }

            return x_in;
        }
        int ZOrderY(int tileSize, int count)
        {
            var div = tileSize;
            var div2 = tileSize / 2;
            var y_in = count / div & div2;

            while (div2 > 1)
            {
                div /= 2;
                div2 /= 2;
                y_in |= count / div & div2;
            }

            return y_in;
        }

        public Point Save(int pointCount, int tileSize, int width, int height)
        {
            return new Point(0, 0);
        }
    }
}
