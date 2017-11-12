using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.Image.Swizzle;
using System.Drawing;
using Kontract.Image.Format;

namespace image_aif
{
    public class Support
    {
        public static Dictionary<byte, IImageFormat> Format = new Dictionary<byte, IImageFormat>
        {
            [2] = new LA(8, 0),
            [6] = new RGBA(4, 4, 4, 4),
            [7] = new LA(8, 8),
            [8] = new RGBA(8, 8, 8, 8),
            [34] = new LA(4, 4),
            [36] = new RGBA(8, 8, 8),
            [37] = new ETC1(),
            [38] = new ETC1(true)
        };
    }

    public class AIFSwizzle : IImageSwizzle
    {
        MasterSwizzle _zorder;

        public int Width { get; }
        public int Height { get; }

        public AIFSwizzle(int width, int height)
        {
            Width = 2 << (int)Math.Log(width - 1, 2);
            Height = 2 << (int)Math.Log(height - 1, 2);

            _zorder = new MasterSwizzle(Width, new Point(0, 0), new[] { (1, 0), (0, 1), (2, 0), (0, 2), (4, 0), (0, 4) });
        }

        public Point Get(Point point)
        {
            int pointCount = point.Y * Width + point.X;
            var newPoint = _zorder.Get(pointCount);

            return new Point(newPoint.X, Height - 1 - newPoint.Y);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TexInfo
    {
        public ushort width;
        public ushort height;
        public byte format;
    }
}
