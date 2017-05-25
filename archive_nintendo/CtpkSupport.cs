using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_ctpk
{
    public class CTPKFileInfo : ArchiveFileInfo
    {
        public CTPKEntry Entry;
        public CTPKHashEntry hashEntry;
        public uint texInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CTPKHeader
    {
        Magic magic = "CTPK";
        short version = 0x1;
        public short texCount = 0;
        public int texSecOffset = 0;
        public int texSecSize = 0;
        public int crc32SecOffset = 0;
        public int texInfoOffset = 0;
        int zero0 = 0;
        int zero1 = 0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CTPKEntry
    {
        public int nameOffset;
        public int texDataSize;
        public int texOffset;
        public int format;
        public short width;
        public short height;
        public byte mipLvl;
        public byte type;
        public short zero0;
        public int bitmapSizeOffset;
        public uint timeStamp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CTPKHashEntry
    {
        public int crc32;
        public int entryNr;
    }
}
