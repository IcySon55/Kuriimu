using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace archive_sony.PPVAX
{
    public sealed class PPVAX
    {
        public List<PpvaxFileInfo> Files = new List<PpvaxFileInfo>();
        private Stream _stream;

        private List<Entry> entries = new List<Entry>();
        int firstFileOffset = 0;
        bool lateStart = false;

        public PPVAX(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                while (br.PeekInt32() != 0x800)
                {
                    br.ReadInt32();
                    lateStart = true;
                }

                do
                {
                    entries.Add(br.ReadStruct<Entry>());
                    if (firstFileOffset == 0)
                        firstFileOffset = entries.FirstOrDefault()?.Offset ?? 0;
                } while (br.PeekInt32() > 0);

                br.BaseStream.Position = firstFileOffset;

                for (var i = 0; i < entries.Count; i++)
                {
                    Files.Add(new PpvaxFileInfo
                    {
                        Entry = entries[i],
                        FileName = $"{i.ToString("000")}.ppvax",
                        FileData = new SubStream(br.BaseStream, entries[i].Offset, entries[i].NextOffset - entries[i].Offset),
                        State = ArchiveFileState.Archived
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                if (lateStart)
                    bw.BaseStream.Position = 0x18;

                // DUMB REPLACE MODE
                for (var i = 0; i < entries.Count; i++)
                {
                    bw.WriteStruct(entries[i]);
                }

                for (var i = 0; i < Files.Count; i++)
                {
                    var file = Files[i];
                    bw.BaseStream.Position = file.Entry.Offset;
                    file.FileData.CopyTo(bw.BaseStream);
                }

                bw.WritePadding(entries.Last().NextOffset - (int)bw.BaseStream.Position);
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
