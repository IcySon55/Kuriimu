using System.Collections.Generic;
using System.Runtime.InteropServices;
using Komponent.Image.Format;
using Kontract.Interface;
using Komponent.IO;

namespace image_ctx
{
    public class Support
    {
        public static Dictionary<uint, IImageFormat> Format = new Dictionary<uint, IImageFormat>
        {
            //composed of dataType and PixelFormat
            //short+short
            [0x14016752] = new RGBA(8, 8, 8, 8),
            [0x80336752] = new RGBA(4, 4, 4, 4),
            [0x80346752] = new RGBA(5, 5, 5, 1),
            [0x14016754] = new RGBA(8, 8, 8),
            [0x83636754] = new RGBA(5, 6, 5),
            [0x14016756] = new LA(0, 8),
            [0x67616756] = new LA(0, 4),
            [0x14016757] = new LA(8, 0),
            [0x67616757] = new LA(4, 0),
            [0x14016757] = new LA(8, 8),
            [0x67606758] = new LA(4, 4),
            [0x0000675A] = new ETC1(),
            [0x0000675B] = new ETC1(true),
            [0x1401675A] = new ETC1(),
            [0x1401675B] = new ETC1(true)
        };
    }

    /*
    public enum DataTypes : ushort
    {
        Byte = 0x1400,
        UByte = 0x1401,
        Short = 0x1402,
        UShort = 0x1403,
        Int = 0x1404,
        UInt = 0x1405,
        Float = 0x1406,
        UnsignedByte44DMP = 0x6760,
        Unsigned4BitsDMP = 0x6761,
        UnsignedShort4444 = 0x8033,
        UnsignedShort5551 = 0x8034,
        UnsignedShort565 = 0x8363
    };

    public enum PixelFormats : ushort
    {
        RGBANativeDMP = 0x6752,
        RGBNativeDMP = 0x6754,
        AlphaNativeDMP = 0x6756,
        LuminanceNativeDMP = 0x6757,
        LuminanceAlphaNativeDMP = 0x6758,
        ETC1RGB8NativeDMP = 0x675A,
        ETC1AlphaRGB8A4NativeDMP = 0x675B
    };
    */

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic8 magic;
        public uint width;
        public uint height;
        public uint width2;
        public uint height2;
        public uint unk1;
        public ushort format;
        public ushort type;
        public uint dataSize;
    }
}
