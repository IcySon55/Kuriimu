using System.ComponentModel;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace image_mt
{
    public enum Format : byte
    {
        // 3DS
        RGBA4444 = 0x01,
        RGBA5551 = 0x02,
        RGBA8888 = 0x03,
        RGB565 = 0x04,
        LA88 = 0x07,
        ETC1 = 0x0B,
        ETC1A4 = 0x0C,
        A4 = 0x0E,
        L4 = 0x0F,
        L8 = 0x10,
        RGB888 = 0x11,

        // PS3
        DXT1 = 0x13,
        DXT3,
        DXT5 = 0x17,
        DXT5Other = 0x21,
        DXT5YCbCr = 0x2A
    }

    public enum Version
    {
        v154 = 154,
        v165 = 165
    }

    // This particual enum is questionable as the data space for it is only 4-bits (maybe)
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
        public Version Version;
        public int Unknown1;
        public int Unused1;
        public AlphaChannelFlags AlphaChannelFlags;

        // Block 2
        public int MipMapCount;
        public int Width;
        public int Height;

        // Block 3
        public int Unknown2;
        public Format Format;
        public int Unknown3;
    }

    public sealed class MTTexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        public Format Format { get; set; }
    }
}
