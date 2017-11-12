using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.UMSBT
{
    public class UMSBT
    {
        public List<UmsbtFileInfo> Files = new List<UmsbtFileInfo>();
        private Stream _stream = null;

        public UMSBT(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                uint index = 0;
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var info = new UmsbtFileInfo();
                    info.Entry = br.ReadStruct<UmsbtFileEntry>();
                    info.FileName = index.ToString("00000000") + ".msbt";
                    info.FileData = new SubStream(input, info.Entry.Offset, info.Entry.Size);
                    info.State = ArchiveFileState.Archived;

                    if (info.Entry.Offset == 0 && info.Entry.Size == 0)
                        break;
                    else
                        Files.Add(info);

                    index++;
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                uint padding = 24;
                uint headerLength = ((uint)Files.Count) * (sizeof(uint) * 2) + padding;
                uint runningTotal = 0;

                foreach (var info in Files)
                {
                    info.Entry.Offset = headerLength + runningTotal;
                    info.Entry.Size = (uint)info.FileData.Length;
                    runningTotal += info.Entry.Size;
                    bw.WriteStruct(info.Entry);
                }

                bw.Write(new byte[padding]);

                foreach (var info in Files)
                    info.FileData.CopyTo(bw.BaseStream);
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
