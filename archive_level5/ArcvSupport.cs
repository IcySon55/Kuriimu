using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_level5.ARCV
{
    public class ARCVFileInfo : ArchiveFileInfo
    {
        public uint hash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int fileCount;
        public uint fileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset;
        public int size;
        public uint hash;
    }
}
