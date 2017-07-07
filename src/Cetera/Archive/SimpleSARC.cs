using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace Cetera.Archive
{
    public class SimpleSARC
    {
        public List<SimpleSARCFileInfo> Files;
        Stream _stream = null;

        public class SimpleSARCFileInfo : ArchiveFileInfo
        {
            public uint FilenameHash { get; set; }
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
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                header = br.ReadStruct<SimpleSARCHeader>();
                Files = br.ReadMultiple<SimpleSFATEntry>(header.nodeCount).OrderBy(e => e.dataStart).Select(entry => new SimpleSARCFileInfo
                {
                    FileName = $"0x{entry.hash:X8}.bin",
                    FileData = new SubStream(input, entry.dataStart, entry.dataSize),
                    State = ArchiveFileState.Archived,
                    FilenameHash = entry.hash
                }).ToList();
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                header.nodeCount = Files.Count;
                bw.WriteStruct(header);

                int offset = (Files.Count + 1) * 0x10;
                var entries = Files.Select(afi =>
                {
                    var entry = new SimpleSFATEntry
                    {
                        hash = afi.FilenameHash,
                        dataStart = offset,
                        dataSize = (int)afi.FileData.Length
                    };
                    offset += entry.dataSize;
                    return entry;
                }).ToList();
                foreach (var entry in entries.OrderBy(e => e.hash))
                {
                    bw.WriteStruct(entry);
                }
                foreach (var afi in Files)
                {
                    afi.FileData.CopyTo(bw.BaseStream);
                }
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
