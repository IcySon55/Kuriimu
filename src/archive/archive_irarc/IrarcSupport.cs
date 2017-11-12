using System.Runtime.InteropServices;
using Kontract.Interface;
using System.IO;
using Kontract.IO;
using Kontract.Compression;

namespace archive_irarc
{
    public class IRARCFileInfo : ArchiveFileInfo
    {
        public Entry Entry;

        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;
                if (State != ArchiveFileState.Archived) return baseStream;
                using (var br = new BinaryReaderX(baseStream, true))
                {
                    var header = br.ReadStruct<CompHeader>();
                    if (header.comp == 0x18)
                    {
                        return null;
                        //Figure out compression
                        //return new MemoryStream(LZSS.Decompress(br.BaseStream, header.uncompSize));
                    }
                    else
                    {
                        return baseStream;
                    }
                }
            }
        }

        public override long? FileSize => base.FileSize;

        /*public void WriteToHpb(Stream stream)
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
        }*/
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CompHeader
    {
        public long comp;
        public uint compSize;
        public uint uncompSize;
        public long unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint id;
        public uint offset;
        public uint compSize;
        public uint flags;
    }
}
