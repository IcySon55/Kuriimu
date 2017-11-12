using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nlp.NLP
{
    public class NLP
    {
        public List<NLPFileInfo> Files = new List<NLPFileInfo>();
        private Stream _stream = null;

        public int blockSize = 0x800;

        public Header header;
        public BlockOffsetHeader blockOffsetHeader;

        public NLP(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();
                int entryTable1Offset = blockSize;

                //EntryTable1
                br.BaseStream.Position = entryTable1Offset;
                var metaInfs = br.ReadMultiple<MetaInf>((int)header.entryCount);
                var metaInfEntries = new List<MetaInfEntry>();
                uint id = 0;
                foreach (var inf in metaInfs) metaInfEntries.Add(new MetaInfEntry
                {
                    metaInf = inf,
                    id = id++
                });
                var entryTable2Offset = br.BaseStream.Position;

                //EntryTable2
                br.BaseStream.Position = entryTable2Offset;
                blockOffsetHeader = br.ReadStruct<BlockOffsetHeader>();
                var blockOffsetEntries = br.ReadMultiple<BlockOffsetEntry>((int)blockOffsetHeader.entryCount);
                var fileOffset = (br.BaseStream.Position + 0x7ff) & ~0x7ff;

                //Files
                for (int i = 0; i < blockOffsetEntries.Count; i++)
                {
                    var packOffset = blockSize + blockOffsetEntries[i].blockOffset * blockSize;
                    br.BaseStream.Position = packOffset;
                    var packHeader = br.ReadStruct<PACKHeader>();

                    var ext = (packHeader.magic == "PACK") ? ".pack" : ".bin";

                    Files.Add(new NLPFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{blockOffsetEntries[i].id:0000}{ext}",
                        FileData = new SubStream(br.BaseStream,
                            packOffset,
                            (packHeader.magic == "PACK") ?
                                packHeader.compSize : (i + 1 == blockOffsetEntries.Count) ?
                                    br.BaseStream.Length - packOffset : (blockOffsetEntries[i + 1].blockOffset * blockSize + blockSize) - packOffset),
                        metaInfEntry = metaInfEntries.Find(x => x.id == blockOffsetEntries[i].id),
                        offsetEntry = blockOffsetEntries[i]
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Files
                var dataOffset = header.dataBlockOffset * blockSize + blockSize;
                bw.BaseStream.Position = dataOffset;
                var offset = dataOffset;
                foreach (var file in Files)
                    offset = file.Write(bw.BaseStream, (uint)offset, (uint)blockSize);

                //MetaInfEntries
                bw.BaseStream.Position = blockSize;
                for (int i = 0; i < header.entryCount; i++)
                    if (Files.Find(x => x.metaInfEntry.id == i) != null)
                        bw.WriteStruct(Files.Find(x => x.metaInfEntry.id == i).metaInfEntry.metaInf);
                    else
                        bw.WriteStruct(new MetaInf
                        {
                            magic = "\0\0\0\0",
                            zero1 = 0,
                            unk4 = 0x8000000,
                            decompSize = 0,
                            fileOffsetInPAK = 0
                        });

                //BlockOffsets
                bw.WriteStruct(blockOffsetHeader);
                foreach (var file in Files) bw.WriteStruct(file.offsetEntry);

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
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
