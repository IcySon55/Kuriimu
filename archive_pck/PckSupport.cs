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
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PCKEntry
    {
        public uint crc32;
        public uint fileOffset;
        public uint fileLength;
    }
}
