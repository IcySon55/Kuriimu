using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_nintendo.NARC
{
    public class NARCFileInfo : ArchiveFileInfo
    {
        public FATEntry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GenericHeader
    {
        public Magic magic;
        public ushort byteOrder;
        public ushort const1;
        public uint secSize;
        public ushort headerSize;
        public ushort secCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FATHeader
    {
        public Magic magic;
        public uint secSize;
        public uint fileCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FATEntry
    {
        //offsets relative to GIMF section
        public uint startOffset;
        public uint endOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FNTHeader
    {
        public Magic magic;
        public uint secSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FIMGHeader
    {
        public Magic magic;
        public uint secSize;
    }
}
