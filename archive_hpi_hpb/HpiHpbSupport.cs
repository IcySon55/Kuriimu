using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_hpi_hpb
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HpiHeader
    {
        public Magic magic = "HPIH";
        public int zero0;
        public int headerSize = 0x10;  //without magic
        public int zero1;
        public short zero2;
        public short hashCount;
        public int entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HashEntry
    {
        public short entryOffset;
        public short entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public int stringOffset;
        public int fileOffset;
        public int fileSize;
        public int uncompressedSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AcmpHeader
    {
        public Magic magic = "ACMP";
        public int compressedSize;
        public int headerSize = 0x20;
        public int zero;
        public int uncompressedSize;
        public int padding0 = 0x01234567;
        public int padding1 = 0x01234567;
        public int padding2 = 0x01234567;
    }

    public class HpiHpbAfi : ArchiveFileInfo
    {
        public Entry Entry;

        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;
                if (State != ArchiveFileState.Archived || Entry.uncompressedSize == 0) return baseStream;
                using (var br = new BinaryReaderX(baseStream, true))
                {
                    var header = br.ReadStruct<AcmpHeader>();
                    return new MemoryStream(RevLZ77.Decompress(new MemoryStream(br.ReadBytes(header.compressedSize))));
                }
            }
        }

        public override long? FileSize => Entry.uncompressedSize == 0 ? base.FileSize : Entry.uncompressedSize;

        public void WriteToHpb(Stream stream)
        {
            Entry.fileOffset = (int)stream.Position;
            if (State == ArchiveFileState.Replaced && Entry.uncompressedSize != 0)
            {
                // Only here if we need to compress from a FileStream in the base.FileData
                var uncompData = new byte[_fileData.Length];
                base.FileData.Read(uncompData, 0, uncompData.Length);
                var compData = RevLZ77.Compress(new MemoryStream(uncompData));
                if (compData == null)
                {
                    Entry.fileSize = uncompData.Length;
                    Entry.uncompressedSize = 0;
                    using (var bw = new BinaryWriterX(stream, true))
                    {
                        bw.Write(uncompData);
                    }
                }
                else
                {

                    Entry.fileSize = compData.Length + 0x20;
                    Entry.uncompressedSize = uncompData.Length;
                    using (var bw = new BinaryWriterX(stream, true))
                    {
                        bw.WriteStruct(new AcmpHeader
                        {
                            compressedSize = Entry.fileSize,
                            uncompressedSize = Entry.uncompressedSize
                        });
                        bw.Write(compData);
                    }
                }
            }
            else
            {
                Entry.fileSize = (int)_fileData.Length;
                base.FileData.CopyTo(stream);
            }

            // padding
            while (stream.Position % 4 != 0)
                stream.WriteByte(0);
        }
    }
}
