using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_enc
{
    public class EncFileInfo : ArchiveFileInfo
    {
        public Entry entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint size;
        public uint unk1;
        public uint offset;
    }
}
