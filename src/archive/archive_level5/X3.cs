using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.X3
{
    public sealed class X3 : IDisposable
    {
        public List<X3FileInfo> Files = new List<X3FileInfo>();
        Stream _stream = null;

        #region Instance Members

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
                    Files.Add(new X3FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = Files.Count.ToString("000000") + ".bin",
                        FileData = new SubStream(br.BaseStream, entry.Offset * _header.Alignment, entry.CompressedSize),
                        Entry = entry,
                        CompressionLevel = entry.CompressedSize == entry.UncompressedSize ? CompressionLevel.NoCompression : CompressionLevel.Optimal
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
