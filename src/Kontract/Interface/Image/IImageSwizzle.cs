using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace Kuriimu.Kontract
{
    public interface IImageSwizzle
    {
        Point InnerLoad(int pointCount, int tileSize);
        Point OuterLoad(int pointCount, int tileSize, int stride);
        Point Save(int pointCount, int tileSize, int width, int height);
    }
}
