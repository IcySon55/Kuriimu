using System.Collections.Generic;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_xbb
{
    public class XBB
    {
        public List<XBBFileInfo> Files = new List<XBBFileInfo>();
        private Stream _stream = null;

        public XBB(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                var header = br.ReadStruct<XBBHeader>();
                br.BaseStream.Position = 0x20;

                List<XBBFileEntry> entries = new List<XBBFileEntry>();
                entries.AddRange(br.ReadMultiple<XBBFileEntry>(header.entryCount));

                List<XBBHashEntry> hashes = new List<XBBHashEntry>();
                hashes.AddRange(br.ReadMultiple<XBBHashEntry>(header.entryCount));

                for (int i = 0; i < header.entryCount; i++) {
                    br.BaseStream.Position = entries[i].nameOffset;
                    Files.Add(new XBBFileInfo {
                        State = ArchiveFileState.Archived,
                        FileName = br.ReadCStringA(),
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size)
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                // TODO: Write out your file format
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
