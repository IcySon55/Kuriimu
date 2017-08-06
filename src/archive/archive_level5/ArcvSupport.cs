using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_level5.ARCV
{
    public class ARCVFileInfo : ArchiveFileInfo
    {
        public FileEntry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint fileCount;
        public uint fileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint offset;
        public uint size;
        public uint crc32;
    }
}
