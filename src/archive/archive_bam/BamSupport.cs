using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_bam
{
    public class BAMFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint fileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileHeader
    {
        public Magic8 magic;
        public uint size;
    }
}
