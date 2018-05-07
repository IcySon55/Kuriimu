using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_anag
{
    public sealed class ANAG
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _stream = null;

        private string magic = "ANAG";

        public ANAG(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {

            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
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
