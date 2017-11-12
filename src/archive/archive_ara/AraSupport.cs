using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_ara
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Header
    {
        public Magic Magic;
        public uint Padding1;
        public uint FileCount;
        public uint Unknown1;
        public uint FilenamesOffset;
        public uint Unknown2;
        public ulong Padding2;
    }

    public class AraFileEntry
    {
        public uint FilenameOffset = 0;
        public string Filename = "";
        public uint Offset = 0;
        public uint FileSize = 0;
        public uint Unk1 = 0;
        public uint Unk2 = 0;
    }

    public class AraFileInfo : ArchiveFileInfo
    {
        public AraFileEntry Entry;
    }
}
