using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_nintendo.PlainUMSBT
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PlainUmsbtFileEntry
    {
        public uint Offset = 0;
        public uint Size = 0;
    }

    public class PlainUmsbtFileInfo : ArchiveFileInfo
    {
        public PlainUmsbtFileEntry Entry = null;
    }
}
