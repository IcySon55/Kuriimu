using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_dc1
{
    public class Dc1FileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint unk1;
        public uint fileSize;
        public uint unk2;
    }

    public class Entry
    {
        public uint offset = 0;
        public uint size = 0;
    }
}
