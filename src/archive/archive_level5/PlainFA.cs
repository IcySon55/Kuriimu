using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_level5.PlainFA
{
    public sealed class PlainFA
    {
        public const int EntryOffset = 0x10;
        public const int EntryLength = 0x50;

        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        Stream _stream = null;

        private PlainFAHeader header;
        private List<PlainFAEntry> entries;

        public PlainFA(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                // Header
                header = br.ReadStruct<PlainFAHeader>();

                // Entries
                entries = br.ReadMultiple<PlainFAEntry>(header.fileCount);

                // Files
                Files = entries.Select(o => new ArchiveFileInfo { FileName = o.filename, FileData = new SubStream(input, o.fileOffset, o.fileSize), State = ArchiveFileState.Archived }).ToList();
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                bw.BaseStream.Position = EntryOffset + header.fileCount * EntryLength;

                int runningTotal = 0;
                entries = new List<PlainFAEntry>();
                foreach (var file in Files)
                {
                    var entry = new PlainFAEntry
                    {
                        fileOffset = (uint)bw.BaseStream.Position,
                        fileSize = (uint)file.FileSize,
                        filename = file.FileName
                    };
                    file.FileData.CopyTo(bw.BaseStream);
                    runningTotal += (int)file.FileSize;
                    entries.Add(entry);
                }

                bw.BaseStream.Position = EntryOffset;
                foreach (var entry in entries)
                    bw.WriteStruct(entry);

                header.fileSize = runningTotal + EntryOffset + header.fileCount * EntryLength;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
