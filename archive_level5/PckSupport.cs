using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_level5.PCK
{
    public class PckFileInfo : ArchiveFileInfo
    {
        public Entry Entry;
        public List<uint> Hashes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint hash;
        public int fileOffset;
        public int fileLength;
    }
}
