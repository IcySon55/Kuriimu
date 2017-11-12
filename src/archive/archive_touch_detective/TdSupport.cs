using System.Runtime.InteropServices;
using Kontract.Interface;

namespace archive_td
{
    public class TDFileInfo : ArchiveFileInfo
    {
        public Entry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset;
        public uint size;
    }
}
