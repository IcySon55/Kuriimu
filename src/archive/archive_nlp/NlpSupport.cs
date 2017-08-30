using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.IO;

namespace archive_nlp.NLP
{
    public class NLPFileInfo : ArchiveFileInfo
    {
        public MetaInfEntry metaInfEntry;
        public BlockOffsetEntry offsetEntry;

        public uint Write(Stream input, uint offset, uint blockSize)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.BaseStream.Position = offset;

                if (State != ArchiveFileState.Archived)
                {
                    if (metaInfEntry.metaInf.magic == "PAK ")
                    {
                        using (var pack = new BinaryReaderX(base.FileData, true))
                        {
                            var header = pack.ReadStruct<PACKHeader>();
                            pack.BaseStream.Position = 0;
                            metaInfEntry.metaInf.decompSize = header.decompSize;
                            metaInfEntry.metaInf.fileOffsetInPAK = header.fileOffset;
                        }
                    }
                }

                offsetEntry.blockOffset = (offset - blockSize) / blockSize;

                base.FileData.CopyTo(bw.BaseStream);
                bw.WriteAlignment((int)blockSize);

                return (uint)((offset + base.FileData.Length + (blockSize - 1)) & ~(blockSize - 1));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        // 0x800 padding used in archive
        public uint unk1;
        public uint dataBlockOffset;
        public uint unk3;
        public uint unk4;
        public uint entryCount;
        public uint metaInfEndOffset;
        public uint unk5;
        public uint unk6;
        public uint unk7;
        public uint BlockTableEndOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MetaInf
    {
        public Magic magic;
        public uint zero1;
        public uint fileOffsetInPAK;
        public uint decompSize;
        public uint unk4;
    }

    public class MetaInfEntry
    {
        public MetaInf metaInf;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BlockOffsetHeader
    {
        public uint zero1;
        public uint entryCount;
        public uint unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BlockOffsetEntry
    {
        public uint id;
        public uint blockOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKHeader
    {
        //offsets are relative to the PACK section
        public Magic magic;
        public ushort unk1;
        public ushort packFileCount;
        public uint stringSizeOffset;
        public uint stringOffset;
        public uint fileOffset;
        public uint decompSize;
        public uint compSize;
        public uint zero1;
    }
}
