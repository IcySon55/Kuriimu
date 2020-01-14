using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.IO;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.Image.Format;
using System.Drawing;
using Kontract.Image.Swizzle;

namespace image_level5.imgv
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic; // IMGV
        public int const1; // 30 30 00 00
        public short const2; // 30 00
        public byte imageFormat;
        public byte const3; // 01
        public byte combineFormat;
        public byte bitDepth;
        public short bytesPerTile;
        public short width;
        public short height;
        public int const4; // 30 00 00 00
        public int const5; // 30 00 01 00
        public int tableDataOffset; // always 0x48
        public int const6; // 03 00 00 00
        public int const7; // 00 00 00 00
        public int const8; // 00 00 00 00
        public int const9; // 00 00 00 00
        public int const10; // 00 00 00 00
        public int tableSize1;
        public int tableSize2;
        public int imgDataSize;
        public int const11; // 00 00 00 00
        public int const12; // 00 00 00 00
    }

    public class Support
    {
        public static Dictionary<byte, IImageFormat> Format = new Dictionary<byte, IImageFormat>
        {
            [0x1e]=new DXT(DXT.Version.DXT1)
        };

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
}
