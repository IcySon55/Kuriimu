using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_oto3_dat
{
    public class OTO3
    {
        public List<OTO3FileInfo> Files = new List<OTO3FileInfo>();
        private Stream _stream = null;

        Header header;
        List<Entry> entries = new List<Entry>();

        public OTO3(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = 0x10;
                for (int i = 0; i < header.entryCount; i++)
                    entries.Add(new Entry(br.BaseStream, header.nameBufSize));

                //Files
                foreach (var entry in entries)
                    Files.Add(new OTO3FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = entry.fileName,
                        FileData = new SubStream(br.BaseStream, entry.fileOffset, entry.fileSize),
                        entry = entry
                    });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = (0x10 + Files.Count * (0x8 + header.nameBufSize) + 0xf) & ~0xf;

                //Write FileData
                bw.BaseStream.Position = dataOffset;
                foreach (var file in Files)
                    file.Write(bw.BaseStream);

                //Write Entries
                bw.BaseStream.Position = 0x10;
                foreach (var file in Files)
                    file.entry.Write(bw.BaseStream, header.nameBufSize);

                //Write Header
                bw.BaseStream.Position = 0;
                header.entryCount = Files.Count;
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
