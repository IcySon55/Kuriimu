using Kuriimu.Contract;

namespace archive_srtz.SEG
{
    public sealed class SegArchiveFileInfo : ArchiveFileInfo
    {
        public SegFileEntry Entry { get; set; }

        public override long? FileSize => Entry.UncompressedSize == 0 ? base.FileSize : Entry.UncompressedSize;
    }

    public class SegFileEntry
    {
        public uint Offset = 0;
        public uint Size = 0;
        public uint UncompressedSize = 0;
    }
}
