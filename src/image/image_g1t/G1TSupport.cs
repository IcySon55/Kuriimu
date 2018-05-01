using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Interface;
using Kontract.Image.Format;
using Kontract;
using System;
using Kontract.Image.Swizzle;
using System.Drawing;

namespace image_g1t
{
    public class Support
    {
        public static Dictionary<byte, IImageFormat> Format = new Dictionary<byte, IImageFormat>
        {
            [0x12] = new DXT(DXT.Version.DXT5)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic8 magic;
        public int fileSize;
        public int dataOffset;
        public int texCount;
        public int unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Meta
    {
        public byte unk1;
        public byte format;
        public byte dimension;
        public byte zero0;
        public long unk2;
        public long unk3;

        public int height => (int)Math.Pow(2, dimension / 16);
        public int width => (int)Math.Pow(2, dimension % 16);
    }

    public class G1TSwizzle : IImageSwizzle
    {
        private MasterSwizzle _swizzle;

        public int Width { get; }
        public int Height { get; }

        public G1TSwizzle(int width, int height, bool block = false)
        {
            Width = (width + 3) & ~3;
            Height = (height + 3) & ~3;

            var bitField = new List<(int, int)>();
            var bitStart = (block) ? 4 : 1;
            if (block)
                bitField.AddRange(new List<(int, int)> { (1, 0), (2, 0), (0, 1), (0, 2) });
            for (int i = bitStart; i < Math.Min(width, height); i *= 2)
                bitField.AddRange(new List<(int, int)> { (0, i), (i, 0) });

            _swizzle = new MasterSwizzle(Width, new Point(0, 0), bitField);
        }

        public Point Get(Point point) => _swizzle.Get(point.Y * Width + point.X);
    }
}
