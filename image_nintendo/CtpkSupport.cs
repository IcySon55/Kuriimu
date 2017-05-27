using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace image_nintendo.CTPK
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public short version;
        public short texCount;
        public int texSecOffset;
        public int texSecSize;
        public int crc32SecOffset;
        public int texInfoOffset;
        public int zero0;
        public int zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public int nameOffset;
        public int texDataSize;
        public int texOffset;
        public int imageFormat;
        public short width;
        public short height;
        public byte mipLvl;
        public byte type;
        public short zero0;
        public int bitmapSizeOffset;
        public uint timeStamp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HashEntry
    {
        public uint crc32;
        public int entryNr;
    }
}
