using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.X3
{
    public sealed class X3 : IDisposable
    {
        public List<X3FileInfo> Files = new List<X3FileInfo>();

        #region Instance Members

        private Stream _stream;
        private FileHeader _header;
        private List<FileEntry> _fileEntries;

        #endregion

        public X3(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                _header = br.ReadStruct<FileHeader>();

                _fileEntries = br.ReadMultiple<FileEntry>(_header.FileCount);

                foreach (var entry in _fileEntries)
                {
                    var compressed = entry.UncompressedSize != entry.CompressedSize && entry.UncompressedSize > 0;
                    br.BaseStream.Position = entry.Offset * _header.Alignment + (compressed ? 0x4 : 0);

                    byte[] firstBlock;
                    if (compressed)
                    {
                        var firstBlockLength = br.ReadInt32();
                        firstBlock = ZLib.Decompress(new MemoryStream(br.ReadBytes(firstBlockLength)));
                    }
                    else
                        firstBlock = br.ReadBytes(4);
                    var magic = Encoding.ASCII.GetString(firstBlock.Take(4).ToArray());
                    var extension = ".bin";

                    if (magic == "GT1G")
                        extension = ".g1t";
                    else if (magic == "SMDH")
                        extension = ".icn";

                    Files.Add(new X3FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = (entry.Offset * _header.Alignment).ToString("X8") + extension,
                        FileData = new SubStream(br.BaseStream, entry.Offset * _header.Alignment, entry.CompressedSize),
                        Entry = entry,
                        CompressionLevel = compressed ? CompressionLevel.Optimal : CompressionLevel.NoCompression
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {

            }
        }

        public void Close()
        {
            _stream?.Close();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }

        public void Dispose() => Close();
    }
}
