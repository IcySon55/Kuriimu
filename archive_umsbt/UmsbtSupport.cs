using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_umsbt
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class UMSBTFileEntry
    {
        public uint Offset = 0;
        public uint Size = 0;
    }

    public class UMSBTFileInfo : ArchiveFileInfo
    {
        public UMSBTFileEntry Entry = null;
    }
}