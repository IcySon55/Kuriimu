using Kontract.Interface;

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
