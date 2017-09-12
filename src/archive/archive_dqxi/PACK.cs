using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Text;

namespace archive_dqxi
{
    public class PACK
    {
        public List<PACKFileInfo> Files = new List<PACKFileInfo>();
        private Stream _stream = null;

        public Header header;
        public List<TableEntry> entries;

        public PACK(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Table??
                entries = br.ReadMultiple<TableEntry>(header.tableSize / 8);

                //Files
                for (int i = 0; i < header.count; i++)
                {
                    var size = br.ReadInt32();

                    Files.Add(new PACKFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = br.ReadString(0x20),
                        FileData = new SubStream(br.BaseStream, br.BaseStream.Position, size)
                    });

                    br.BaseStream.Position += size;
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = 0x10 + entries.Count * 8;

                //Files
                var offset = dataOffset;
                foreach (var file in Files)
                    offset = file.Write(bw.BaseStream, offset);

                //Table
                bw.BaseStream.Position = 0x10;
                foreach (var entry in entries)
                    bw.WriteStruct(entry);

                //Header
                bw.BaseStream.Position = 0;
                header.size = (int)bw.BaseStream.Length;
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
