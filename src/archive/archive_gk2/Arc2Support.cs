using System.IO;
using System.Runtime.InteropServices;
using Kontract.Interface;

namespace archive_gk2.arc2
{
    public class Arc2FileInfo : ArchiveFileInfo
    {
        public Entry entry;
    }

    public class Entry
    {
        public uint offset;
        public uint size;
    }
}
