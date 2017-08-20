using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace archive_mt
{
    public class MTARC
    {
        public List<MTArcFileInfo> Files = new List<MTArcFileInfo>();
        private Stream _stream = null;

        private Header Header;
        private int HeaderLength = 12;
        private ByteOrder ByteOrder = ByteOrder.LittleEndian;
        private int CompressionIdentifier = 0x9C78;

        public MTARC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true, ByteOrder))
            {
                // Set endianess
                if (br.PeekString() == "\0CRA")
                {
                    br.ByteOrder = ByteOrder = ByteOrder.BigEndian;
                    HeaderLength = 8;
                    CompressionIdentifier = 0x6881;
                }

                // Header
                Header = br.ReadStruct<Header>();
                if (ByteOrder == ByteOrder.LittleEndian) br.ReadInt32();

                // Files
                var entries = br.ReadMultiple<FileMetadata>(Header.entryCount);
                Files.AddRange(entries.Select(metadata =>
                {
                    br.BaseStream.Position = metadata.offset;
                    var level = br.ReadUInt16() == CompressionIdentifier ? CompressionLevel.Optimal : CompressionLevel.NoCompression;
                    var shift = level != CompressionLevel.NoCompression ? 2 : 0;

                    return new MTArcFileInfo
                    {
                        Metadata = metadata,
                        CompressionLevel = level,
                        FileData = new SubStream(br.BaseStream, metadata.offset + shift, metadata.compressedSize - shift),
                        FileName = metadata.filename + (ArcShared.ExtensionMap.ContainsKey(metadata.extensionHash) ? ArcShared.ExtensionMap[metadata.extensionHash] : "." + metadata.extensionHash.ToString("X8")),
                        State = ArchiveFileState.Archived
                    };
                }));
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, ByteOrder))
            {
                // Header
                Header.entryCount = (short)Files.Count;
                bw.WriteStruct(Header);
                if (ByteOrder == ByteOrder.LittleEndian) bw.Write(0);

                // Files
                bw.BaseStream.Position = ByteOrder == ByteOrder.LittleEndian ? Math.Max(HeaderLength + Header.entryCount * 80, 0x8000) : HeaderLength + Header.entryCount * 80;
                foreach (var afi in Files)
                    afi.Write(bw.BaseStream, bw.BaseStream.Position, CompressionIdentifier, ByteOrder);

                // Metadata
                bw.BaseStream.Position = HeaderLength;
                foreach (var afi in Files)
                    bw.WriteStruct(afi.Metadata);
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }
    }
}
