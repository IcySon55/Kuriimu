using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters;
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
                if (br.PeekString() == "\0CRA")
                {
                    br.ByteOrder = ByteOrder = ByteOrder.BigEndian;
                    HeaderLength = 8;
                    CompressionIdentifier = 0x6881;
                }

                Header = br.ReadStruct<Header>();
                if (ByteOrder == ByteOrder.LittleEndian) br.ReadInt32();
                var lst = Enumerable.Range(0, Header.entryCount).Select(_ => br.ReadStruct<FileMetadata>()).ToList();
                Files.AddRange(lst.Select(metadata =>
                {
                    br.BaseStream.Position = metadata.offset;
                    var level = br.ReadUInt16();

                    var afi = new MTArcFileInfo
                    {
                        Metadata = metadata,
                        CompressionLevel = level == CompressionIdentifier ? CompressionLevel.Optimal : CompressionLevel.NoCompression,
                        FileData = new SubStream(br.BaseStream, metadata.offset + 2, metadata.compressedSize - 2),
                        FileName = metadata.filename + (ArcShared.ExtensionMap.ContainsKey(metadata.extensionHash) ? ArcShared.ExtensionMap[metadata.extensionHash] : ".bin"),
                        State = ArchiveFileState.Archived
                    };

                    return afi;
                }));
            }
        }

        public void Save(Stream output)
        {
            Header.entryCount = (short)Files.Count;

            using (var bw = new BinaryWriterX(output, ByteOrder))
            {
                // Header
                bw.WriteStruct(Header);
                if (ByteOrder == ByteOrder.LittleEndian) bw.Write(0);

                // Files
                bw.BaseStream.Position = ByteOrder == ByteOrder.LittleEndian ? Math.Max(HeaderLength + Header.entryCount * 80, 0x8000) : HeaderLength + Header.entryCount * 80;
                foreach (var afi in Files)
                {
                    afi.Metadata.offset = (int)bw.BaseStream.Position;
                    bw.Write((short)(afi.CompressionLevel == CompressionLevel.Optimal ? CompressionIdentifier : 0x0178));
                    if (afi.State != ArchiveFileState.Archived)
                    {
                        byte[] bytes;
                        using (var ds = new DeflateStream(bw.BaseStream, afi.CompressionLevel, true))
                        {
                            using (var br = new BinaryReaderX(afi.FileData, true))
                                bytes = br.ReadBytes((int)br.BaseStream.Length);
                            ds.Write(bytes, 0, (int)afi.FileData.Length);
                        }
                        afi.Metadata.compressedSize = bytes.Length;
                        afi.Metadata.uncompressedSize = (int)afi.FileData.Length * 8;

                        bw.Write(bytes);
                    }
                    else
                        afi.CompressedFileData.CopyTo(bw.BaseStream);
                }

                // Metadata
                bw.BaseStream.Position = HeaderLength;
                foreach (var afi in Files)
                    bw.WriteStruct(afi.Metadata);
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
