using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.Compression;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_amb
{
    public class AMB
    {
        public List<AMBFileInfo> Files = new List<AMBFileInfo>();
        Stream _stream = null;

        public Header header;
        public List<FileEntry> entries = new List<FileEntry>();

        public AMB(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true, ByteOrder.BigEndian))
            {
                //Header
                header = br.ReadStruct<Header>();

                //FileEntries
                entries = br.ReadMultiple<FileEntry>((int)header.fileCount);

                //Files
                var count = 0;
                foreach (var entry in entries)
                    Files.Add(new AMBFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset, entry.size)
                    });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, ByteOrder.BigEndian))
            {
                //Header
                bw.WriteStruct(header);

                //FileEntries
                var count = 0;
                var dataOffset = header.dataOffset;
                foreach (var file in Files)
                {
                    entries[count].offset = dataOffset;
                    entries[count++].size = (uint)file.FileSize;

                    dataOffset = (uint)((dataOffset + (uint)file.FileSize + 0xf) & ~0xf);
                }

                foreach (var entry in entries)
                {
                    bw.WriteStruct(entry);
                }

                //Files
                bw.BaseStream.Position = header.dataOffset;
                foreach (var file in Files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment();
                }
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
