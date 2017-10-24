using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Text;

namespace archive_vap
{
    public class VAP
    {
        public List<VAPFileInfo> Files = new List<VAPFileInfo>();
        private Stream _stream = null;

        public Header header;

        public VAP(Stream input)
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
                    Files.Add(new VAPFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++}.bin",
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
                bw.BaseStream.Position = 0xc;
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
