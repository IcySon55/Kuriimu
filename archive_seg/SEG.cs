using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_seg
{
    public class SEG
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private List<SegFileEntry> Entries = new List<SegFileEntry>();
        private Stream _segStream = null;
        private Stream _binStream = null;

        public SEG(Stream segInput, Stream binInput)
        {
            _segStream = segInput;
            _binStream = binInput;

            // Offsets and Sizes
            using (var br = new BinaryReaderX(segInput, true))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var entry = new SegFileEntry { Offset = br.ReadUInt32() };
                    Entries.Add(entry);

                    if (Entries.IndexOf(entry) <= 0) continue;
                    if (entry.Offset == 0) {
                        Entries.Remove(Entries.Last());
                        break;
                    }

                    var prev = Entries[Entries.IndexOf(entry) - 1];
                    prev.Size = entry.Offset - prev.Offset;
                }
            }

            // Files
            using (var br = new BinaryReaderX(binInput, true))
            {
                for (int i = 0; i < Entries.Count - 1; i++)
                    Files.Add(new ArchiveFileInfo
                    {
                        FileName = i.ToString("000000") + ".bin",
                        FileData = new SubStream(binInput, Entries[i].Offset, Entries[i].Size),
                        State = ArchiveFileState.Archived
                    });
            }
        }

        public void Save(Stream segOutput, Stream binOutput)
        {
            // Offsets and Sizes
            using (var bw = new BinaryWriterX(segOutput))
            {
                bw.Write(Entries[0].Offset);

                uint runningTotal = 0;
                for (int i = 1; i < Entries.Count - 1; i++)
                {
                    Entries[i].Offset = runningTotal + (uint)Files[i - 1].FileSize;
                    runningTotal += (uint)Files[i - 1].FileSize;
                    bw.Write(Entries[i].Offset);
                }

                bw.Write(runningTotal + (uint)Files.Last().FileSize);
            }

            // Files
            using (var bw = new BinaryWriterX(binOutput))
            {
                foreach (var file in Files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                }
            }
        }

        public void Close()
        {
            _segStream?.Dispose();
            _segStream = null;
            _binStream?.Dispose();
            _binStream = null;
        }
    }
}
