using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_nlp.NLP
{
    public class NLP
    {
        public List<NLPFileInfo> Files = new List<NLPFileInfo>();
        private Stream _stream = null;

        public int blockSize = 0x800;

        public NLP(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                var header = br.ReadStruct<Header>();
                int entryTable1Offset = blockSize;

                //EntryTable1
                br.BaseStream.Position = entryTable1Offset;
                var metaInfEntries = br.ReadMultiple<MetaInfEntry>((int)header.entryCount);
                var entryTable2Offset = br.BaseStream.Position;

                //EntryTable2
                br.BaseStream.Position = entryTable2Offset;
                var blockOffsetHeader = br.ReadStruct<BlockOffsetHeader>();
                var blockOffsetEntries = br.ReadMultiple<BlockOffsetEntry>((int)blockOffsetHeader.entryCount);
                var fileOffset = (br.BaseStream.Position + 0x7ff) & ~0x7ff;

                //Files
                for (int i = 0; i < blockOffsetEntries.Count; i++)
                {
                    var packOffset = blockSize + blockOffsetEntries[i].blockOffset * blockSize;
                    br.BaseStream.Position = packOffset;
                    var packHeader = br.ReadStruct<PACKHeader>();

                    Files.Add(new NLPFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{blockOffsetEntries[i].id:0000}.bin",
                        FileData = new SubStream(br.BaseStream,
                            packOffset,
                            (packHeader.magic == "PACK") ?
                                packHeader.compSize : (i + 1 == blockOffsetEntries.Count) ?
                                    br.BaseStream.Length - packOffset : (blockOffsetEntries[i + 1].blockOffset * blockSize + blockSize) - packOffset)
                    });
                }
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
