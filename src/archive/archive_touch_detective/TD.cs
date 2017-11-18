using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_td
{
    public class TD
    {
        public List<TDFileInfo> Files = new List<TDFileInfo>();
        private Stream _stream = null;

        public TD(Stream input)
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
                    Files.Add(new TDFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset * 4, entry.size),
                        entry = entry
                    });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Count
                bw.Write(Files.Count);

                //Entries
                uint dataOffset = 4 + (uint)Files.Count * 8;
                foreach (var file in Files)
                {
                    bw.Write(dataOffset / 4);
                    bw.Write((uint)file.FileSize);

                    dataOffset = (uint)((dataOffset + file.FileSize + 0x3) & ~0x3);
                }

                //Files
                foreach (var file in Files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(4);
                }
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
