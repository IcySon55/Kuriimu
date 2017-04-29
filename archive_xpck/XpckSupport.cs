using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_xpck
{
    public class XPCKFileInfo : ArchiveFileInfo
    {
        public Entry Entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public byte fileCount;
        public byte unk1;
        ushort tmp1;
        ushort tmp2;
        ushort tmp3;
        ushort tmp4;
        ushort tmp5;
        uint tmp6;

        public ushort fileInfoOffset => (ushort)(tmp1 * 4);
        public ushort filenameTableOffset => (ushort)(tmp2 * 4);
        public ushort dataOffset => (ushort)(tmp3 * 4);
        public ushort fileInfoSize => (ushort)(tmp4 * 4);
        public ushort filenameTableSize => (ushort)(tmp5 * 4);
        public uint dataSize => tmp6 * 4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public uint crc32;
        public ushort ID;
        public ushort tmp2;
        public int fileSize;

        public ushort fileOffset => (ushort)(tmp2 * 4);
    }
}
