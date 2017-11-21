using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.Interface;
using Komponent.IO;
using Komponent.CTR.Hash;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB
    {
        public List<HpiHpbAfi> Files;

        const uint HashSlotCount = 0x1000;
        const uint PathHashMagic = 0x25;

        private Stream _stream = null;

        public HPIHPB(Stream hpiInput, Stream hpbInput)
        {
            _stream = hpbInput;
            using (var br = new BinaryReaderX(hpiInput))
            {
                // HPI Header
                var header = br.ReadStruct<HpiHeader>();

                // Hash List
                br.ReadMultiple<HashEntry>(header.hashCount);

                // Entry List
                Files = br.ReadMultiple<Entry>(header.entryCount).OrderBy(e => e.stringOffset).Select(entry => new HpiHpbAfi
                {
                    Entry = entry,
                    FileName = br.ReadCStringSJIS(), // String Table
                    FileData = new SubStream(hpbInput, Math.Max(0, entry.fileOffset), entry.fileSize),
                    State = ArchiveFileState.Archived
                }).ToList();
            }
        }

        public void Save(Stream hpiOutput, Stream hpbOutput)
        {
            using (hpbOutput)
            using (var bw = new BinaryWriterX(hpiOutput))
            {
                // HPI Header
                bw.WriteStruct(new HpiHeader { hashCount = (short)HashSlotCount, entryCount = Files.Count });

                var sjis = System.Text.Encoding.GetEncoding("sjis");
                uint GetInt(byte[] ba) => ba.Aggregate(0u, (i, b) => (i << 8) | b);

                int stringOffset = 0;
                foreach (var afi in Files)
                {
                    afi.Entry.stringOffset = stringOffset;
                    if (afi.Entry.fileSize != 0) afi.WriteToHpb(hpbOutput);
                    stringOffset += sjis.GetByteCount(afi.FileName) + 1;
                }

                // Hash List
                var lookup = Files.ToLookup(e => GetInt(new SimpleHash3DS().Create(sjis.GetBytes(e.FileName), PathHashMagic)) % HashSlotCount);
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
                    bw.Write(sjis.GetBytes(afi.FileName + '\0'));
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
