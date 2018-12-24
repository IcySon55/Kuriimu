using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Interface;
using Kontract.Image.Format;
using Kontract;
using System;
using Kontract.Image.Swizzle;
using System.Drawing;
using static Kontract.Image.Support.ETC1;

namespace image_g1t
{
    public enum Platform
    {
        PS,
        Vita,
        N3DS
    }

    public class Support
    {
        public static Dictionary<byte, IImageFormat> N3DSFormat = new Dictionary<byte, IImageFormat>
        {
            [9] = new RGBA(8, 8, 8, 8),
            [0x3B] = new RGBA(5, 6, 5),
            [0x3D] = new RGBA(4, 4, 4, 4),
            [0x44] = new LA(0, 4),
            [0x47] = new ETC1(false),
            [0x48] = new ETC1(true)
        };

        public static Dictionary<byte, IImageFormat> VitaFormat = new Dictionary<byte, IImageFormat>
        {
            [0] = new RGBA(8, 8, 8, 8),
            [1] = new RGBA(8, 8, 8, 8, Kontract.IO.ByteOrder.LittleEndian, true),
            [6] = new DXT(DXT.Version.DXT1),
            [8] = new DXT(DXT.Version.DXT5),
            [0x12] = new DXT(DXT.Version.DXT5),
        };

        public static Dictionary<byte, IImageFormat> PSFormat = new Dictionary<byte, IImageFormat>
        {
            [0x59] = new DXT(DXT.Version.DXT1),
            [0x5b] = new DXT(DXT.Version.DXT5)
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
        public byte unk2;
        public byte unk3;
        public byte unk4;
        public byte extHeader;

        public int height => (int)Math.Pow(2, dimension / 16);
        public int width => (int)Math.Pow(2, dimension % 16);
    }

    public class VitaSwizzle : IImageSwizzle
    {
        private MasterSwizzle _swizzle;

        public int Width { get; }
        public int Height { get; }

        public VitaSwizzle(int width, int height, bool block = false)
        {
            Width = (width + 3) & ~3;
            Height = (height + 3) & ~3;

            var bitField = new List<(int, int)>();
            var bitStart = block ? 4 : 1;
            if (block)
                bitField.AddRange(new List<(int, int)> { (1, 0), (2, 0), (0, 1), (0, 2) });
            for (int i = bitStart; i < Math.Min(width, height); i *= 2)
                bitField.AddRange(new List<(int, int)> { (0, i), (i, 0) });

            _swizzle = new MasterSwizzle(Width, new Point(0, 0), bitField);
        }

        public Point Get(Point point) => _swizzle.Get(point.Y * Width + point.X);
    }

    public class BlockSwizzle : IImageSwizzle
    {
        private MasterSwizzle _swizzle;

        public int Width { get; }
        public int Height { get; }

        public BlockSwizzle(int width, int height)
        {
            Width = (width + 3) & ~3;
            Height = (height + 3) & ~3;

            _swizzle = new MasterSwizzle(Width, new Point(0, 0), new[] { (1, 0), (2, 0), (0, 1), (0, 2) });
        }

        public Point Get(Point point) => _swizzle.Get(point.Y * Width + point.X);
    }
}
