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
        private Stream _stream;

        // HFS
        private HFSHeader HFSHeader;
        private int HFSHeaderLength;
        private HFSFooter HFSFooter;

        // ARC
        private Header Header;
        private int HeaderLength = 0xC;
        private const int EntryLength = 0x50;
        private ByteOrder ByteOrder = ByteOrder.LittleEndian;
        private Platform System;

        public MTARC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true, ByteOrder))
            {
                // Set endianess
                if (br.PeekString() == "\0CRA" || br.PeekString() == "\0SFH")
                {
                    br.ByteOrder = ByteOrder = ByteOrder.BigEndian;
                    HeaderLength = 8;
                    System = Platform.PS3;
                }

                // SFHHeader
                if (br.PeekString() == "\0SFH")
                {
                    HFSHeader = br.ReadStruct<HFSHeader>();
                    HFSHeaderLength = HFSHeader.Type == 0 ? 0x20000 : 0x10;
                }

                // Header
                Header = br.ReadStruct<Header>();
                if (ByteOrder == ByteOrder.LittleEndian) br.ReadInt32();

                // Files
                var entries = br.ReadMultiple<FileMetadata>(Header.EntryCount);
                Files.AddRange(entries.Select(metadata =>
                {
                    br.BaseStream.Position = metadata.Offset + HFSHeaderLength;
                    var level = (System == Platform.CTR ? metadata.CompressedSize != (metadata.UncompressedSize & 0x00FFFFFF) : metadata.CompressedSize != (metadata.UncompressedSize >> 3)) ? CompressionLevel.Optimal : CompressionLevel.NoCompression;
                    br.BaseStream.Position += metadata.CompressedSize;

                    return new MTArcFileInfo
                    {
                        Metadata = metadata,
                        CompressionLevel = level,
                        System = System,
                        FileData = new SubStream(br.BaseStream, metadata.Offset + HFSHeaderLength, metadata.CompressedSize),
                        FileName = metadata.FileName + (ArcShared.ExtensionMap.ContainsKey(metadata.ExtensionHash) ? ArcShared.ExtensionMap[metadata.ExtensionHash] : "." + metadata.ExtensionHash.ToString("X8")),
                        State = ArchiveFileState.Archived
                    };
                }));

                // HFSFooter
                if (HFSHeader != null)
                {
                    br.SeekAlignment();
                    HFSFooter = br.ReadStruct<HFSFooter>();
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, ByteOrder))
            {
                // HFSHeader
                if (HFSHeader != null)
                {
                    bw.WriteStruct(HFSHeader);
                    HeaderLength += HFSHeaderLength;
                }

                // Header
                Header.EntryCount = (short)Files.Count;
                bw.WriteStruct(Header);
                if (ByteOrder == ByteOrder.LittleEndian) bw.Write(0);

                // Files
                switch (Header.Version)
                {
                    case 0x10:
                        bw.BaseStream.Position = ByteOrder == ByteOrder.LittleEndian ? Math.Max(HeaderLength + Header.EntryCount * EntryLength, 0x8000) : HeaderLength + Header.EntryCount * EntryLength;
                        break;
                    case 0x11:
                        bw.BaseStream.Position = ByteOrder == ByteOrder.LittleEndian ? (HeaderLength + Header.EntryCount * EntryLength + 0xff) & ~0xff : HeaderLength + Header.EntryCount * EntryLength;
                        break;
                    default:
                        bw.BaseStream.Position = HeaderLength + Header.EntryCount * EntryLength;
                        break;
                }
                foreach (var afi in Files)
                    afi.Write(bw.BaseStream, bw.BaseStream.Position - HFSHeaderLength, ByteOrder);

                // HFSFooter
                if (HFSFooter != null)
                {
                    bw.WriteAlignment();
                    bw.WriteStruct(HFSFooter);
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
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }
    }
}
