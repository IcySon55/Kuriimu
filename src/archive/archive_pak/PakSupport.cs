using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_pak
{
    public class PakFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public ushort fileCount;
        public uint fileTableOffset;
        public ushort filenameTableOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Node
    {
        public ushort stringOffset;
        public ushort flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint size;
        public uint offset;
    }
}
