using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_nlp.PACK
{
    public class PACKFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKHeader
    {
        public Magic magic;
        public ushort unk1;
        public ushort packFileCount;
        public uint stringSizeOffset;
        public uint stringOffset;
        public uint fileOffset;
        public uint decompSize;
        public uint compSize;
        public uint zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKEntry
    {
        public Magic magic;
        public uint zero1;

    }
}
