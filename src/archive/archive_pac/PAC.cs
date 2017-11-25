using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_pac
{
    public sealed class PAC
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _stream = null;

        private Header header;
        private List<Entry> entries = new List<Entry>();

        public PAC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = header.entryOffset;
                entries = br.ReadMultiple<Entry>(header.fileCount);

                //There is at least one file that for some reason doesn't have enough bytes to fit into
                //this structure, every other file in the game Chase: Cold Case Investigation fits into this structure
                //the last file in this specific pac gives a filesize that in combination with the offset would
                //reach out of range of the actual file
                //The next 3 lines try to work around this issue
                if (entries[entries.Count - 1].fileOffset + entries[entries.Count - 1].fileSize > br.BaseStream.Length)
                    using (var bw = new BinaryWriterX(input, true))
                    {
                        var bk = bw.BaseStream.Position;
                        bw.BaseStream.Position = bw.BaseStream.Length;
                        bw.WritePadding((entries[entries.Count - 1].fileOffset + entries[entries.Count - 1].fileSize) - (int)br.BaseStream.Length);
                        bw.BaseStream.Position = bk;
                    }

                //Files
                foreach (var entry in entries)
                {
                    br.BaseStream.Position = entry.fileOffset;
                    Files.Add(new ArchiveFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = entry.fileName,
                        FileData = new SubStream(br.BaseStream, entry.fileOffset, entry.fileSize)
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                var dataOffset = 0x30 + entries.Count * 0x90;

                //Write files and update Entries
                var count = 0;
                bw.BaseStream.Position = dataOffset;
                foreach (var file in Files)
                {
                    entries[count].fileOffset = (int)bw.BaseStream.Position;
                    entries[count++].fileSize = (int)file.FileSize;
                    file.FileData.CopyTo(bw.BaseStream);
                }

                //Write entries
                bw.BaseStream.Position = 0x30;
                foreach (var entry in entries)
                    bw.WriteStruct(entry);

                //Write Header
                bw.BaseStream.Position = 0x0;
                bw.WriteStruct(header);
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
