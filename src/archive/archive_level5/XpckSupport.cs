using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_level5.XPCK
{
    public class XPCKFileInfo : ArchiveFileInfo
    {
        public FileInfoEntry Entry;
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
        public uint tmp6;

        public ushort fileInfoOffset => (ushort)(tmp1 << 2);
        public ushort filenameTableOffset => (ushort)(tmp2 << 2);
        public ushort dataOffset => (ushort)(tmp3 << 2);
        public ushort fileInfoSize => (ushort)(tmp4 << 2);
        public ushort filenameTableSize => (ushort)(tmp5 << 2);
        public uint dataSize => tmp6 << 2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileInfoEntry
    {
        public uint crc32;
        public ushort nameOffset;
        public ushort tmp;
        public ushort tmp2;
        public byte tmpZ;
        public byte tmp2Z;

        public uint fileOffset => (((uint)tmpZ << 16) | tmp) << 2;
        public uint fileSize => ((uint)tmp2Z << 16) | tmp2;
    }
}
