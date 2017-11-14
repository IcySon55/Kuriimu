using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Image.Swizzle
{
    [Export("Linear", typeof(IImageSwizzle))]
    public class Linear : IImageSwizzle
    {
        public int Width { get; }
        public int Height { get; }

        MasterSwizzle _linear;

        [ImportingConstructor]
        public Linear([Import("LinearWidth")]int widthStride)
        {
            Width = widthStride;
            Height = 0;

            _linear = new MasterSwizzle(widthStride, new Point(0, 0), Enumerable.Empty<(int, int)>());
        }

        public Point Get(Point point) => _linear.Get(point.Y * Width + point.X);
    }
}
