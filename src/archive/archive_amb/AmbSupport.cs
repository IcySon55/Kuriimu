using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_amb
{
    public class AMBFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint headerSize;
        public int zero0;
        public uint dirCount;   //??
        public uint fileCount;
        public uint fileEntryOffset;
        public uint dataOffset;
        public uint zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint offset;
        public uint size;
        public uint id;
        public uint zero0;
    }
}
