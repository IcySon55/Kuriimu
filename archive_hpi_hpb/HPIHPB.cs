using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cetera.Hash;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB : IDisposable
    {
        public List<HpiHpbAfi> Files;

        const uint HashSlotCount = 0x1000;
        const uint PathHashMagic = 0x25;

        private Stream _hpb = null;

        public HPIHPB(Stream hpi, Stream hpb)
        {
            _hpb = hpb;
            using (var br = new BinaryReaderX(hpi))
            {
                // HPI Header
                var header = br.ReadStruct<HpiHeader>();

                // Hash List
                br.ReadMultiple<HashEntry>(header.hashCount);

                // Entry List
                Files = br.ReadMultiple<Entry>(header.entryCount).OrderBy(e => e.stringOffset).Select(entry => new HpiHpbAfi
                {
                    Entry = entry,
                    FileName = br.ReadCStringA(), // String Table
                    FileData = new SubStream(hpb, Math.Max(0, entry.fileOffset), entry.fileSize),
                    State = ArchiveFileState.Archived
                }).ToList();
            }
        }

        public void Save(Stream hpi, Stream hpb)
        {
            using (hpb)
            using (var bw = new BinaryWriterX(hpi))
            {
                // HPI Header
                bw.WriteStruct(new HpiHeader { hashCount = (short)HashSlotCount, entryCount = Files.Count });

                int stringOffset = 0;
                foreach (var afi in Files)
                {
                    afi.Entry.stringOffset = stringOffset;
                    afi.WriteToHpb(hpb);
                    stringOffset += afi.FileName.Length + 1;
                }

                // Hash List
                var lookup = Files.ToLookup(e => SimpleHash.Create(e.FileName, PathHashMagic, HashSlotCount));
                for (int i = 0, offset = 0; i < HashSlotCount; i++)
                {
                    var count = lookup[(uint)i].Count();
                    bw.WriteStruct(new HashEntry { entryOffset = (short)offset, entryCount = (short)count });
                    offset += count;
                }

                // Entry List
                foreach (var afi in lookup.OrderBy(g => g.Key).SelectMany(g => g))
                    bw.WriteStruct(afi.Entry);

                // String Table
                foreach (var afi in Files)
                    bw.WriteASCII(afi.FileName + '\0');
            }
        }

        public void Dispose()
        {
            _hpb.Dispose();
        }
    }
}