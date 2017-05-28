using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_srtz.MTV
{
    public sealed class MtvArchiveFileInfo : ArchiveFileInfo
    {
        public CompressionHeader Header { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CompressionHeader
    {
        public uint Index;
        public uint Length;
        public uint Padding1;
        public uint UncompressedSize;
        public uint UncompressedSize2;
        public uint Padding2;
        public uint Padding3;
        public uint Padding4;
    }
}
