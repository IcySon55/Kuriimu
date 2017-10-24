using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace Kuriimu.Kontract
{
    public interface IImageTransformation
    {
        Tuple<int, int> GetStride(int width, int height, int tileSize);
        Point Load(Point point, int pointCount, int tileSize);
        Point Save(Point point, int pointCount, int tileSize);
    }
}
