using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_enc
{
    public class ENC
    {
        public List<EncFileInfo> Files = new List<EncFileInfo>();
        private Stream _stream = null;

        public ENC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Entries
                var entryCount = br.ReadInt32();
                var entries = br.ReadMultiple<Entry>(entryCount);

                //Files
                var count = 0;
                foreach (var entry in entries)
                {
                    Files.Add(new EncFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset, entry.size & 0x7fffffff),
                        entry = entry
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
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }
    }
}
