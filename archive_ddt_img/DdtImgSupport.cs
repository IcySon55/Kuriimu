using System.Collections.Generic;
using Kuriimu.Contract;

namespace archive_ddt_img
{
    public class DdtFileEntry
    {
        public uint PathOffset;
        public int NextEntryOffsetOrFileID;
        public int SubEntryCountOrFileSize;

        public string Name;

        public List<DdtFileEntry> SubEntries;
    }

    public class DdtFileInfo : ArchiveFileInfo
    {
        public DdtFileEntry Entry;
    }
}
