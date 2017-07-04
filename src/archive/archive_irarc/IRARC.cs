using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_irarc
{
    public sealed class IRARC : IDisposable
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

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
