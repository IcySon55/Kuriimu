using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_nlp
{
    public class NLP
    {
        public List<NLPFileInfo> Files = new List<NLPFileInfo>();
        private Stream _stream = null;

        public NLP(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                var header = br.ReadStruct<Header>();
                uint entryTable1Offset = (uint)((br.BaseStream.Position + 0x7ff) & ~0x7ff);

                //EntryTable1
                br.BaseStream.Position = entryTable1Offset;
                var entries1 = br.ReadMultiple<Entry>((int)header.fileCount);
                var entryTable2Offset = br.BaseStream.Position;

                //EntryTable2
                br.BaseStream.Position = entryTable2Offset;
                var entry2Header = br.ReadStruct<Entry2Header>();
                var entries2 = br.ReadMultiple<Entry2>((int)entry2Header.entryCount);
                var fileOffset = (br.BaseStream.Position + 0x7ff) & ~0x7ff;

                //Files
                br.BaseStream.Position = fileOffset;
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
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
