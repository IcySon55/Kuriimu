using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

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
                uint offset = 0;
                while (true)
                {
                    offset = br.ReadUInt32();
                    if (offset == br.BaseStream.Length - 4 || offset == br.BaseStream.Length) break;

                    var size = br.ReadUInt32() - offset;
                    br.BaseStream.Position -= 4;

                    entries.Add(new Entry
                    {
                        offset = offset,
                        size = size
                    });
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

            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
