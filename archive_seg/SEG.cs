using System.Collections.Generic;
using System.IO;
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
                    var prev = Entries[Entries.IndexOf(entry) - 1];
                    prev.Size = entry.Offset - prev.Offset;
                }
            }

            // FIles
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

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                // TODO: Write out your file format
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
