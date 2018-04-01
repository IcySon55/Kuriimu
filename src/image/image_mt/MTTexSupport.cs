using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract.Interface;

namespace image_mt
{
    // Format
    public enum Format : byte
    {
        // PS3
        DXT1_Remap = 0x19,
        DXT5_B = 0x21,
        DXT5_C = 0x27,
        DXT5_YCbCr = 0x2A
    }

    // This particular enum is questionable as the data space for it is only 4-bits (maybe)
    public enum AlphaChannelFlags : byte
    {
        Normal = 0x0,
        YCbCrTransform = 0x02,
        Unknown1 = 0x03,
        Unknown2 = 0x04,
        Mixed = 0x08,
        NormalMaps = 0x0B, // ?
        MirroredNormalMaps1 = 0x13, // ?
        MirroredNormalMaps2 = 0x1B, // ?
        CTMipTexture = 0x20 // ?
    }

    public enum TransformDirection
    {
        ToProperColors,
        ToOptimizedColors
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public uint Block1;
        // Version 12-bit
        // Unknown1 12-bit
        // Unused1 4-bit
        // AlphaChannelFlags 4-bit
        public uint Block2;
        // MipMapCount 6-bit
        // Width 13-bit
        // Height 13-bit
        public uint Block3;
        // Unknown2 8-bit
        // Format 8-bit
        // Unknown3 16-bit
    }

    public class HeaderInfo
    {
        // Block 1
        public int Version;
        public int Unknown1;
        public int Unused1;
        public AlphaChannelFlags AlphaChannelFlags;

        // Block 2
        public int MipMapCount;
        public int Width;
        public int Height;

        // Block 3
        public int Unknown2;
        public byte Format;
        public int Unknown3;
    }

    // Support
    public partial class MTTEX
    {
        public static Dictionary<byte, IImageFormat> Formats = new Dictionary<byte, IImageFormat>
        {
            [1] = new RGBA(4, 4, 4, 4),
            [2] = new RGBA(5, 5, 5, 1),
            [3] = new RGBA(8, 8, 8, 8),
            [4] = new RGBA(5, 6, 5),
            [7] = new RGBA(8, 8, 8, 8,Kontract.IO.ByteOrder.BigEndian),//[7] = new LA(8, 8),
            [11] = new ETC1(),
            [12] = new ETC1(true),
            [14] = new LA(0, 4),
            [15] = new LA(4, 0),
            [16] = new LA(8, 0),
            [17] = new RGBA(8, 8, 8),

            [19] = new DXT(DXT.Version.DXT1),
            [20] = new DXT(DXT.Version.DXT3),
            [23] = new DXT(DXT.Version.DXT5),
            [25] = new DXT(DXT.Version.DXT1),
            [33] = new DXT(DXT.Version.DXT5),
            [39] = new DXT(DXT.Version.DXT5),
            [42] = new DXT(DXT.Version.DXT5)
        };

        // Currently trying out YCbCr:
        // https://en.wikipedia.org/wiki/YCbCr#JPEG_conversion
        private const int CbCrThreshold = 123; // usually 128, but 123 seems to work better here

        public static Color ToNoAlpha(Color c)
        {
            return Color.FromArgb(255, c.R, c.G, c.B);
        }

        public static Color ToProperColors(Color c)
        {
            var (A, Y, Cb, Cr) = (c.G, c.A, c.B - CbCrThreshold, c.R - CbCrThreshold);
            return Color.FromArgb(A,
                Common.Clamp(Y + 1.402 * Cr),
                Common.Clamp(Y - 0.344136 * Cb - 0.714136 * Cr),
                Common.Clamp(Y + 1.772 * Cb));
        }

        public static Color ToOptimisedColors(Color c)
        {
            var (A, Y, Cb, Cr) = (c.A,
                0.299 * c.R + 0.587 * c.G + 0.114 * c.B,
                CbCrThreshold - 0.168736 * c.R - 0.331264 * c.G + 0.5 * c.B,
                CbCrThreshold + 0.5 * c.R - 0.418688 * c.G - 0.081312 * c.B);
            return Color.FromArgb(Common.Clamp(Y), Common.Clamp(Cr), A, Common.Clamp(Cb));
        }
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

    public sealed class MTTexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public string Format { get; set; }
    }
}
