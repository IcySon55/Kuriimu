using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_zdp
{
    public class ZDPFileInfo : ArchiveFileInfo
    {
        public FileEntry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PartitionHeader
    {
        public Magic8 magic;
        public uint zero1;
        public uint unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public uint headerSize;
        public uint fileCount;
        public ushort fileCount2;
        public ushort fileCount3;
        public uint fileCount4;
        public uint entryListOffset;
        public uint nameOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileEntry
    {
        public uint offset;
        public uint size;
    }
}
