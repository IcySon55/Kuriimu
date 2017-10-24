using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_dpk.DG2
{
    public class DG2
    {
        public List<DG2FileInfo> Files = new List<DG2FileInfo>();
        private Stream _stream = null;

        public Header header;
        public List<Entry> entries;

        public DG2(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = header.headerSize;
                entries = br.ReadMultiple<Entry>(header.fileCount);

                //Files
                var count = 0;
                foreach (var entry in entries)
                    Files.Add(new DG2FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++:0000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset, entry.fileSize),
                        entry = entry
                    });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                int dataOffset = header.dataOffset;

                //Files
                var files = Files.OrderBy(x => x.entry.offset);
                int offset = dataOffset;
                foreach (var file in files)
                    offset = file.Write(bw.BaseStream, offset);

                //Entry
                bw.BaseStream.Position = header.headerSize;
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
