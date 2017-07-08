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

        private ArcHeader Header;
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

                Header = br.ReadStruct<ArcHeader>();
                if (ByteOrder == ByteOrder.LittleEndian) br.ReadInt32();
                var lst = Enumerable.Range(0, Header.entryCount).Select(_ => br.ReadStruct<FileMetadata>()).ToList();
                Files.AddRange(lst.Select(metadata =>
                {
                    // zlib header
                    br.BaseStream.Position = metadata.offset;
                    var level = br.ReadUInt16();

                    // deflate stream
                    var ms = new MemoryStream();
                    using (var ds = new DeflateStream(br.BaseStream, CompressionMode.Decompress, true))
                    {
                        ds.CopyTo(ms);
                    }

                    // ignore adler32 footer, assume checksum is correct
                    return new MTArcFileInfo
                    {
                        Metadata = metadata,
                        CompressionLevel = level == CompressionIdentifier ? CompressionLevel.Optimal : CompressionLevel.NoCompression,
                        FileData = ms,
                        FileName = metadata.filename + (ArcShared.ExtensionMap.ContainsKey(metadata.extensionHash) ? ArcShared.ExtensionMap[metadata.extensionHash] : ".bin"),
                        State = ArchiveFileState.Archived
                    };
                }));
            }
        }

        public void Save(Stream output)
        {
            Header.entryCount = (short)Files.Count;
            var compressedList = Files.Select(e =>
            {
                using (var bw = new BinaryWriterX(new MemoryStream(), ByteOrder))
                {
                    // zlib header
                    bw.Write((short)(e.CompressionLevel == CompressionLevel.Optimal ? CompressionIdentifier : 0x0178));

                    // deflate stream
                    byte[] bytes;
                    using (var ds = new DeflateStream(bw.BaseStream, e.CompressionLevel, true))
                    {
                        using (var br = new BinaryReaderX(e.FileData, true))
                            bytes = br.ReadBytes((int)br.BaseStream.Length);
                        ds.Write(bytes, 0, (int)e.FileData.Length);
                    }

                    // adler32 footer
                    var (a, b) = bytes.Aggregate((1, 0), (x, n) => ((x.Item1 + n) % 65521, (x.Item1 + x.Item2 + n) % 65521));
                    bw.Write(new[] { (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a });
                    return ((MemoryStream)bw.BaseStream).ToArray();
                }
            }).ToList();

            using (var bw = new BinaryWriterX(output, ByteOrder))
            {
                bw.WriteStruct(Header);
                if (ByteOrder == ByteOrder.LittleEndian) bw.Write(0);
                var padding = Files.Count % 2 == 0 ? 20 : 4;
                for (var i = 0; i < Files.Count; i++)
                {
                    var ext = Path.GetExtension(Files[i].FileName);
                    bw.WriteStruct(new FileMetadata
                    {
                        filename = Files[i].FileName.Remove(Files[i].FileName.Length - ext.Length),
                        extensionHash = ArcShared.ExtensionMap.ContainsValue(ext) ? ArcShared.ExtensionMap.Single(pair => pair.Value == ext).Key : Files[i].Metadata.extensionHash,
                        compressedSize = compressedList[i].Length,
                        uncompressedSize = (int)Files[i].FileData.Length | 0x40000000,
                        offset = HeaderLength + Files.Count * 80 + padding + compressedList.Take(i).Sum(bytes => bytes.Length)
                    });
                }
                bw.WritePadding(padding);
                foreach (var bytes in compressedList)
                {
                    bw.Write(bytes);
                }
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
