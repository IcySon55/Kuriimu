using System.ComponentModel;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;
using Kontract.Image.Format;
using System.Collections.Generic;
using Kontract.Image.Swizzle;
using System.Drawing;

namespace image_mt.Mobile
{
    public enum Version : ushort
    {
        _Mobilev1 = 0x09,
    }

    public class Support
    {
        public static Dictionary<int, IImageFormat> Format = new Dictionary<int, IImageFormat>
        {
            [0x1] = new RGBA(8, 8, 8, 8, Kontract.IO.ByteOrder.BigEndian),
            [0xa] = new ETC1(false, false, Kontract.IO.ByteOrder.BigEndian)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public uint Block1;
        //8-bits - unk1
        //8-bits - unk2
        //16-bits - version

        public uint Block2;
        //24-bits - unused
        //4-bits - R1
        //4-bits - mipMapCount

        public uint Block3;
        //2-bits - unk4
        //4-bits - unk5
        //13-bits - width
        //13-bits - height
    }

    public class HeaderInfo
    {
        //Block 1
        public byte unk1;
        public byte format;
        public Version version;

        //Block 2
        public byte r1;
        public byte mipMapCount;

        //Block3
        public int unk2;
        public short width;
        public short height;
    }

    public sealed class MobileMTTexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public string Format { get; set; }
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
