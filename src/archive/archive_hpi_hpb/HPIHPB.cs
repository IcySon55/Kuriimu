using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cetera.Hash;
using Kontract.Interface;
using Kontract.IO;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB
    {
        public List<HpiHpbAfi> Files = new List<HpiHpbAfi>();

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
                var exc = new List<Entry>();
                var entryList = br.ReadMultiple<Entry>(header.entryCount).OrderBy(e => e.stringOffset);
                foreach (var entry in entryList)
                {
                    Files.Add(new HpiHpbAfi
                    {
                        Entry = entry,
                        FileName = br.ReadCStringSJIS(), // String Table
                        FileData = new SubStream(hpbInput, (entry.fileOffset >= hpbInput.Length) ? 0 : Math.Max(0, entry.fileOffset), entry.fileSize),
                        State = ArchiveFileState.Archived
                    });
                }
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
                string GetSJIS(string s) => string.Concat(sjis.GetBytes(s).Select(b => (char)(sbyte)b));

                int stringOffset = 0;
                foreach (var afi in Files)
                {
                    afi.Entry.stringOffset = stringOffset;
                    if (afi.Entry.fileSize != 0) afi.WriteToHpb(hpbOutput);
                    stringOffset += GetSJIS(afi.FileName).Length + 1;
                }

                // Hash List
                var lookup = Files.ToLookup(e => SimpleHash.Create(GetSJIS(e.FileName), PathHashMagic, HashSlotCount));
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
