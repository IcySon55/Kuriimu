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
        IEnumerable<(int, int)> _bitFieldCoords;
        IEnumerable<(int, int)> _initPointTransformOnY;

        public int MacroTileWidth { get; }
        public int MacroTileHeight { get; }

        int _widthInTiles;
        Point _init;

        public MasterSwizzle(int imageStride, Point init, IEnumerable<(int, int)> bitFieldCoords, IEnumerable<(int, int)> initPointTransformOnY = null)
        {
            _bitFieldCoords = bitFieldCoords;
            _initPointTransformOnY = initPointTransformOnY ?? Enumerable.Empty<(int, int)>();

            _init = init;

            MacroTileWidth = bitFieldCoords.Select(p => p.Item1).Aggregate((x, y) => x | y) + 1;
            MacroTileHeight = bitFieldCoords.Select(p => p.Item2).Aggregate((x, y) => x | y) + 1;
            _widthInTiles = (imageStride + MacroTileWidth - 1) / MacroTileWidth;
        }

        public Point Get(int pointCount)
        {
            var macroTileCount = pointCount / MacroTileWidth / MacroTileHeight;
            var (macroX, macroY) = (macroTileCount % _widthInTiles, macroTileCount / _widthInTiles);

            return new[] { (macroX * MacroTileWidth, macroY * MacroTileHeight) }
                .Concat(_bitFieldCoords.Where((v, j) => (pointCount >> j) % 2 == 1))
                .Concat(_initPointTransformOnY.Where((v, j) => (macroY >> j) % 2 == 1))
                .Aggregate(_init, (a, b) => new Point(a.X ^ b.Item1, a.Y ^ b.Item2));
        }
    }
}
