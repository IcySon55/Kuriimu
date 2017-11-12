using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_cdar
{
    public class CDAR
    {
        public List<CDARFileInfo> Files = new List<CDARFileInfo>();
        private Stream _stream = null;

        public Header header;

        public CDAR(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Hash
                var hashes = br.ReadMultiple<uint>((int)header.entryCount);

                //FileEntries
                var entries = br.ReadMultiple<Entry>((int)header.entryCount);

                //Files
                var count = 0;
                foreach (var entry in entries)
                {
                    Files.Add(new CDARFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset, entry.compSize),
                        hash = hashes[count++],
                        entry = entry
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Header
                bw.WriteStruct(header);

                //Hash
                foreach (var file in Files)
                    bw.Write(file.hash);

                //Files
                uint dataOffset = 0x10 + (uint)Files.Count * 4 + (uint)Files.Count * 0xc;
                uint offset = dataOffset;
                bw.BaseStream.Position = dataOffset;
                foreach (var file in Files)
                {
                    file.Write(bw.BaseStream, offset);
                    offset = (uint)bw.BaseStream.Position;
                }

                //Entries
                bw.BaseStream.Position = 0x10 + Files.Count * 4;
                foreach (var file in Files)
                    bw.WriteStruct(file.entry);
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
