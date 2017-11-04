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
        int widthInTiles;

        public MasterSwizzle(IEnumerable<(int, int)> bitFieldCoords, int imageWidth)
        {
            this.bitFieldCoords = bitFieldCoords;
            this.imageWidth = imageWidth;

            macroTileWidth = bitFieldCoords.Select(p => p.Item1).Aggregate((x, y) => x | y) + 1;
            macroTileHeight = bitFieldCoords.Select(p => p.Item2).Aggregate((x, y) => x | y) + 1;
            widthInTiles = (imageWidth + macroTileWidth - 1) / macroTileWidth;
        }

        public Point Get(int pointCount, Point init)
        {
            var macroTileCount = pointCount / macroTileWidth / macroTileHeight;
            var (macroX, macroY) = (macroTileCount % widthInTiles, macroTileCount / widthInTiles);

            return new[] { (macroX * macroTileWidth, macroY * macroTileHeight) }
                .Concat(bitFieldCoords.Where((v, j) => (pointCount >> j) % 2 == 1))
                //.Concat(srcY.Where((v, j) => (macroY >> j) % 2 == 1))
                .Aggregate(init, (a, b) => new Point(a.X ^ b.Item1, a.Y ^ b.Item2));
        }
    }

    //Merge Neo's point creation code - Only srcY is missing to be implemented
    //Move initpoint to constructor when srcY is implemented

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
            var macroTileCount = i / macroTileWidth / macroTileHeight;
            var (macroX, macroY) = (macroTileCount % widthInTiles, macroTileCount / widthInTiles);
            return new[] { (macroX * macroTileWidth, macroY * macroTileHeight) }
                .Concat(src.Where((v, j) => (i >> j) % 2 == 1))
                .Concat(srcY.Where((v, j) => (macroY >> j) % 2 == 1))
                .Aggregate(init, (a, b) => new Point(a.X ^ b.Item1, a.Y ^ b.Item2));
        }
    }
}
