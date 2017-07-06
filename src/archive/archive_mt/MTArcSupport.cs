using System.IO.Compression;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_mt
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public short version = 0x11;
        public short entryCount;
        int padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileMetadata
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string filename;
        public uint extensionHash;
        public int compressedSize;
        public int uncompressedSize;
        public int offset;
    }

    public class MTArcFileInfo : ArchiveFileInfo
    {
        public FileMetadata Metadata { get; set; }
        public CompressionLevel CompressionLevel { get; set; }
    }
}
