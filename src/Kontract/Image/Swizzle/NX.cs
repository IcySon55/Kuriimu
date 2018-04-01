using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;

namespace Kontract.Image.Swizzle
{
    public class NXSwizzle : IImageSwizzle
    {
        public enum Format : byte
        {
            Empty,
            DXT5,
            DXT1,
            RGBA8888
        }

        MasterSwizzle _swizzle;

        public int Width { get; }
        public int Height { get; }

        public NXSwizzle(int width, int height, Format format, bool toPowerOf2 = true)
        {
            Width = (toPowerOf2) ? 2 << (int)Math.Log(width - 1, 2) : width;
            Height = (toPowerOf2) ? 2 << (int)Math.Log(height - 1, 2) : height;

            switch (format)
            {
                case Format.DXT5:
                    var bitField = new List<(int, int)> { (1, 0), (2, 0), (0, 1), (0, 2), (0, 4), (4, 0), (0, 8), (0, 16), (8, 0) };
                    for (int i = 32; i < Math.Min(Height, 512); i *= 2)
                        bitField.Add((0, i));
                    _swizzle = new MasterSwizzle(Width, new Point(0, 0), bitField);
                    break;
                case Format.DXT1:
                    bitField = new List<(int, int)> { (1, 0), (2, 0), (0, 1), (0, 2), (4, 0), (0, 4), (8, 0), (0, 8), (0, 16), (16, 0) };
                    for (int i = 32; i < Math.Min(Height, 512); i *= 2)
                        bitField.Add((0, i));
                    _swizzle = new MasterSwizzle(Width, new Point(0, 0), bitField);
                    break;
                case Format.RGBA8888:
                    bitField = new List<(int, int)> { (1, 0), (2, 0), (0, 1), (4, 0), (0, 2), (0, 4), (8, 0), (0, 8), (0, 16) };
                    for (int i = 32; i < Math.Min(Height, 128); i *= 2)
                        bitField.Add((0, i));
                    _swizzle = new MasterSwizzle(Width, new Point(0, 0), bitField);
                    break;
                default:
                    _swizzle = new Linear(Width, Height)._linear;
                    break;
            }
        }

        public Point Get(Point point)
        {
            int pointCount = point.Y * Width + point.X;
            return _swizzle.Get(pointCount);
        }
    }
}
