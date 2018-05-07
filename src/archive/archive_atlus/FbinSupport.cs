using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_atlus.FBIN
{
    public class FbinFileInfo : ArchiveFileInfo
    {
        // TODO
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        // TODO
    }
}
