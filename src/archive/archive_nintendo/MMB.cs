using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.MMB
{
    public class MMB
    {
        public List<MmbFileInfo> Files = new List<MmbFileInfo>();
        private Stream _stream = null;

        private Header Header;
        private const int TableStart = 0xC;

        public MMB(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                Header = br.ReadStruct<Header>();
                var entries = br.ReadMultiple<MmbFileEntry>(Header.FileCount);

                foreach (var entry in entries)
                {
                    // META
                    Files.Add(new MmbFileInfo
                    {
                        FileName = $"{entry.FileName}.meta",
                        State = ArchiveFileState.Archived,
                        FileData = new SubStream(input, entry.Offset, entry.FileSize)
                    });

                    // CTPK
                    Files.Add(new MmbFileInfo
                    {
                        FileName = $"{entry.FileName}.ctpk",
                        State = ArchiveFileState.Archived,
                        FileData = new SubStream(input, entry.Offset + entry.FileSize, entry.CtpkSize)
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                bw.WriteStruct(Header);

                var entries = new List<MmbFileEntry>();

                // Files
                var runningOffset = Header.TableSize;
                for (var i = 0; i < Files.Count; i += 2)
                {
                    bw.BaseStream.Position = runningOffset;
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    Files[i + 1].FileData.CopyTo(bw.BaseStream);

                    entries.Add(new MmbFileEntry
                    {
                        FileName = Path.GetFileNameWithoutExtension(Files[i].FileName),
                        Offset = runningOffset,
                        FileSize = (int)Files[i].FileSize,
                        CtpkSize = (int)Files[i + 1].FileSize
                    });

                    runningOffset = (int)bw.BaseStream.Position;
                }

                // Etries
                bw.BaseStream.Position = TableStart;
                foreach (var entry in entries)
                {
                    bw.WriteStruct(entry);
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
