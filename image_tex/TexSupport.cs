using System.ComponentModel;
using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace image_tex
{
    public enum Format : byte
    {
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
        RGB888 = 0x11
    }

    public enum AlphaChannelFlags : byte
    {
        Normal = 0x0,
        Unknown1 = 0x03,
        Unknown2 = 0x04,
        Mixed = 0x08,
        NormalMaps = 0x0B, // ?
        MirroredNormalMaps1 = 0x13, // ?
        MirroredNormalMaps2 = 0x1B, // ?
        CTMipTexture = 0x20 // ?
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public byte Version;
        public byte Unknown1; // 0x00, 0x20 for MM Textures, 0x40 for NUKI Textures, 0x90 for LM Textures
        public AlphaChannelFlags AlphaChannelFlag;
        public byte CMTexture1; // 0x20, 0x60 for CM Textures
        public byte MipMapCount;
        public short Width;
        public byte Height; // 0x00 == 8
        public byte CMTexture2; // 0x01, 0x06 for CM Textures
        public Format Format;
        public byte Unknown2; //0x01
        public byte Unknown3; //0x00
    }

    public sealed class TexBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        public Format Format { get; set; }
    }
}
