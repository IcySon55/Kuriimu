using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace Cetera.Archive
{
    public class SimpleSARC
    {
        public List<SimpleSARCFileInfo> Files;

        public class SimpleSARCFileInfo : ArchiveFileInfo
        {
            public SimpleSFATEntry Entry;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class SimpleSARCHeader
        {
            Magic magic = "SARC";
            public int nodeCount;
            public int unk1;
            int unk2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SimpleSFATEntry
        {
            public uint hash;
            public int dataStart;
            public int dataSize;
            public int zero0;
        }

        SimpleSARCHeader header;

        public SimpleSARC(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                header = br.ReadStruct<SimpleSARCHeader>();
                Files = br.ReadMultiple<SimpleSFATEntry>(header.nodeCount).Select(entry => new SimpleSARCFileInfo
                    {
                        FileName = $"0x{entry.hash:X8}.bin",
                        FileData = new SubStream(input, entry.dataStart, entry.dataSize),
                        State = ArchiveFileState.Archived,
                        Entry = entry
                    }).ToList();
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                header.nodeCount = Files.Count;
                bw.WriteStruct(header);

                int offset = (Files.Count + 1) * 0x10;
                foreach (var afi in Files)
                {
                    afi.Entry.dataStart = offset;
                    afi.Entry.dataSize = (int)afi.FileData.Length;
                    bw.WriteStruct(afi.Entry);
                    offset += afi.Entry.dataSize;
                }
                foreach (var afi in Files)
                {
                    afi.FileData.CopyTo(bw.BaseStream);
                }
            }
        }
    }
}
