using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_srtux
{
    public class SrtuxFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset = 0;
        public uint size = 0;
    }
}
