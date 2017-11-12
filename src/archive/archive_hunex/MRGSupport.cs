using Kontract.IO;
using Kontract.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archive_hunex
{
    public class MRGEntry
    {
        public int Size;
        public int Offset;

        public string Name;

        public MRGEntry(ushort sectorOffset, ushort offset, ushort sectorSizeUpper, ushort size, ushort numberOfEntries)
        {
            Size = (sectorSizeUpper - 1) / 0x20 * 0x10000 + size;
            int dataStartOffset = 6 + 2 + numberOfEntries * 8;
            Offset = dataStartOffset + sectorOffset * 0x800 + offset;
        }
    }

    public class MRGFileInfo : ArchiveFileInfo
    {
        public MRGEntry Entry;
    }
}
