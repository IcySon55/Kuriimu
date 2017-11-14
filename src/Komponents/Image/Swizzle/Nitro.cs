﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Image.Swizzle
{
    [Export("NitroSwizzle", typeof(IImageSwizzle))]
    public class NitroSwizzle : IImageSwizzle
    {
        MasterSwizzle _tiler;

        public int Width { get; }
        public int Height { get; }

        [ImportingConstructor]
        public NitroSwizzle([Import("NitroWidth")]int width, [Import("NitroHeight")]int height)
        {
            Width = width;
            Height = height;

            _tiler = new MasterSwizzle(Width, new Point(0, 0), new[] { (1, 0), (2, 0), (4, 0), (0, 1), (0, 2), (0, 4) });
        }

        public Point Get(Point point) => _tiler.Get(point.Y * Width + point.X);
    }
}
