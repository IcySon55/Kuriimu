using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileHeader
    {
        public Magic Magic;
        public int FileCount;
        public int Alignment;
        public int Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public int Offset;
        public int Padding;
        public int CompressedSize;
        public int UncompressedSize;
    }

    public class X3FileInfo : ArchiveFileInfo
    {
        public FileEntry Entry { get; set; }
        public CompressionLevel CompressionLevel { get; set; }

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived || CompressionLevel == CompressionLevel.NoCompression) return base.FileData;

                var ms = new MemoryStream();
                using (var bw = new BinaryWriterX(ms, true))
                    using (var br = new BinaryReaderX(base.FileData, true))
                    {
                        br.BaseStream.Position = 0;
                        var uncompressedSize = br.ReadInt32();
                        while (bw.BaseStream.Length < uncompressedSize)
                        {
                            var compressedSize = br.ReadInt32();
                            bw.Write(ZLib.Decompress(new SubStream(br.BaseStream, br.BaseStream.Position, compressedSize)));
                            br.BaseStream.Position += 4; // Skip ZLib footer
                        }
                    }

                ms.Position = 0;
                return ms;
            }
        }

        public override long? FileSize => Entry.UncompressedSize;

    }
}
