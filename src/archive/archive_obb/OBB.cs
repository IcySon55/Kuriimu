using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_obb
{
    public class OBB
    {
        public List<OBBFileInfo> Files = new List<OBBFileInfo>();
        private Stream _stream = null;

        public Header header;

        public OBB(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                var entries = br.ReadMultiple<Entry>(header.fileCount);

                //Files
                var count = 0;
                foreach (var entry in entries)
                    Files.Add(new OBBFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset, entry.size),
                        entry = entry
                    });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = Files[0].entry.offset;

                //Files
                var offset = dataOffset;
                foreach (var file in Files)
                    offset = file.Write(bw.BaseStream, offset);

                //Entries
                bw.BaseStream.Position = 0x10;
                foreach (var file in Files)
                    bw.WriteStruct(file.entry);

                //Header
                bw.BaseStream.Position = 0;
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
