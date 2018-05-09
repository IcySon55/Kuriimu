using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_atlus.FBIN
{
    public class FbinFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int fileCount;
        public uint headerSize;
    }

}
