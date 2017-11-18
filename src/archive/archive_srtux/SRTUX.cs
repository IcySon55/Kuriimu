using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_srtux
{
    public class SRTUX
    {
        public List<SrtuxFileInfo> Files = new List<SrtuxFileInfo>();
        private Stream _stream = null;

        List<Entry> entries = new List<Entry>();

        public SRTUX(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                var limit = br.ReadUInt32();
                var offset = limit;
                while (br.BaseStream.Position < limit)
                {

                    var offset2 = br.ReadUInt32();
                    if (offset2 == 0) break;

                    var size = offset2 - offset;

                    entries.Add(new Entry
                    {
                        offset = offset,
                        size = size,
                        comp = br.PeekString(offset, 3) == "ECD"
                    });

                    offset = offset2;
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    Files.Add(new SrtuxFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size),
                        entry = entries[i]
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = entries[0].offset;

                foreach (var file in Files)
                {
                    bw.Write(dataOffset);

                    file.Write(bw.BaseStream, dataOffset);

                    dataOffset = (uint)bw.BaseStream.Length;
                }

                bw.Write(dataOffset);
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
