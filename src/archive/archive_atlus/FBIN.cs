using Kontract.Interface;
using Kontract.IO;
using System.Collections.Generic;
using System.IO;

namespace archive_atlus.FBIN
{
    public class FBIN
    {
        public List<FbinFileInfo> Files = new List<FbinFileInfo>();
        private Stream _stream = null;

        public Header header;

        public FBIN(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                // TODO
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                // TODO
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
