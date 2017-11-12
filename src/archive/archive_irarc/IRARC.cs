using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace archive_irarc
{
    public sealed class IRARC
    {
        public List<IRARCFileInfo> Files;
        private Stream _stream = null;



        public IRARC(Stream lstInput, Stream arcInput)
        {
            _stream = arcInput;
            using (var br = new BinaryReaderX(lstInput))
            {
                var entryCount = br.ReadInt32();

                //Files
                var count = 0;
                Files = br.ReadMultiple<Entry>(entryCount).Select(entry => new IRARCFileInfo
                {
                    Entry = entry,
                    FileName = $"{count++.ToString():8}.bin",
                    FileData = new SubStream(arcInput, entry.offset, entry.compSize),
                    State = ArchiveFileState.Archived
                }).ToList();
            }
        }

        public void Save(Stream lstOutput, Stream arcOutput)
        {
            using (arcOutput)
            using (var bw = new BinaryWriterX(lstOutput))
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
