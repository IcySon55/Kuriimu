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
        public Point InnerLoad(int pointCount, int tileSize)
        {
            return new Point(
                ZOrderX(tileSize, pointCount),
                ZOrderY(tileSize, pointCount));
        }

        public Point OuterLoad(int pointCount, int tileSize, int stride)
        {
            var powTileSize = (int)Math.Pow(tileSize, 2);
            return new Point(
                (pointCount / powTileSize % (stride / tileSize)) * tileSize,
                (pointCount / powTileSize / (stride / tileSize)) * tileSize);
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
