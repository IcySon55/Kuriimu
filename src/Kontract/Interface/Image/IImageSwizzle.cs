using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace Kontract.Interface
{
    public interface IImageSwizzle
    {
        Point Load(Point point, int width, int height);
        Point Save(int pointCount, int tileSize, int width, int height);
    }
}
