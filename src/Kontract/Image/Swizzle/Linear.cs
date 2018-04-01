using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class Linear : IImageSwizzle
    {
        public int Width { get; }
        public int Height { get; }

        public MasterSwizzle _linear;

        public Linear(int widthStride, int heightStride)
        {
            Width = widthStride;
            Height = heightStride;

            List<(int, int)> bitField = new List<(int, int)>();
            for (int i = 1; i < widthStride; i *= 2)
                bitField.Add((i, 0));
            for (int i = 1; i < heightStride; i *= 2)
                bitField.Add((0, i));
            _linear = new MasterSwizzle(widthStride, new Point(0, 0), bitField);
        }

        public Point Get(Point point) => _linear.Get(point.Y * Width + point.X);
    }
}
