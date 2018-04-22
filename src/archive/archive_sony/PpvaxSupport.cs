using System.Runtime.InteropServices;
using Kontract.Interface;

namespace archive_sony.PPVAX
{
    public class PpvaxFileInfo : ArchiveFileInfo
    {
        public Entry Entry { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Entry
    {
        public int Offset;
        public int NextOffset;
    }
}
