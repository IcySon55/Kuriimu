using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;

namespace archive_pck
{
    public class PckFileInfo : ArchiveFileInfo
    {
        public PCKEntry Entry;
        public List<uint> Hashes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PCKEntry
    {
        public uint hash;
        public int fileOffset;
        public int fileLength;
    }
}
