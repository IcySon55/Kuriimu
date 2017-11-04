using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class MasterSwizzle
    {
        IEnumerable<(int, int)> bitFieldCoords;
        int imageWidth;

        int macroTileWidth;
        int macroTileHeight;

        public MasterSwizzle(IEnumerable<(int, int)> bitFieldCoords, int imageWidth, int macroTileWidth, int macroTileHeight)
        {
            this.bitFieldCoords = bitFieldCoords;
            this.imageWidth = imageWidth;

            this.macroTileWidth = macroTileWidth;
            this.macroTileHeight = macroTileHeight;
        }

        public Point Get(int pointCount, Point init)
        {
            //if (pointCount / (1 << bitFieldCoords.Count()) >= 1) throw new Exception("Too few bit Coordinates were given to calculate coord of given pointCount.");

            var newP = bitFieldCoords
                .Where((v, j) => (pointCount >> j) % 2 == 1)
                .Aggregate(init, (point, bitFields) =>
                    new Point(point.X ^ bitFields.Item1, point.Y ^ bitFields.Item2));

            newP.X ^= ((pointCount >> bitFieldCoords.Count()) % (imageWidth / macroTileWidth)) * macroTileWidth;
            newP.Y ^= pointCount / (imageWidth * macroTileHeight) * macroTileHeight;

            return newP;
        }
    }
}
