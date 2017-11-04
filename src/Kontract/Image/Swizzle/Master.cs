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

    // @todo: To be consolidated with the former MasterSwizzle [Neobeo]
    public class NeoMasterSwizzle
    {
        IEnumerable<(int, int)> src;
        IEnumerable<(int, int)> srcY;
        Point init;

        public int macroTileWidth;
        public int macroTileHeight;
        int widthInTiles;

        public NeoMasterSwizzle(Point init, int width, IEnumerable<(int, int)> src, IEnumerable<(int, int)> srcY)
        {
            this.src = src;
            this.srcY = srcY;
            this.init = init;

            macroTileWidth = src.Select(p => p.Item1).Aggregate((x, y) => x | y) + 1;
            macroTileHeight = src.Select(p => p.Item2).Aggregate((x, y) => x | y) + 1;
            widthInTiles = (width + macroTileWidth - 1) / macroTileWidth;
        }

        public Point Get(int i)
        {
            var tmp = i / macroTileWidth / macroTileHeight;
            var (sx, sy) = (tmp % widthInTiles, tmp / widthInTiles);
            return new[] { (sx * macroTileWidth, sy * macroTileHeight) }
                .Concat(src.Where((v, j) => (i >> j) % 2 == 1))
                .Concat(srcY.Where((v, j) => (sy >> j) % 2 == 1))
                .Aggregate(init, (a, b) => new Point(a.X ^ b.Item1, a.Y ^ b.Item2));
        }
    }
}
