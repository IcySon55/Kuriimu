using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_bfp
{
    public class BFP
    {
        public List<BFPFileInfo> Files = new List<BFPFileInfo>();
        private Stream _stream = null;
        Import imports = new Import();

        public Header header;
        public List<Entry> entries = new List<Entry>();
        public List<Entry2> entries2 = new List<Entry2>();

        public BFP(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //EntryList
                br.BaseStream.Position = 0x20;
                entries = br.ReadMultiple<Entry>((int)header.entryCount);

                //EntryList2
                while (br.BaseStream.Position <= entries[0].offset - 8)
                {
                    var entry = br.ReadStruct<Entry2>();
                    entries2.Add(entry);
                }

                //Add Files
                var count = 0;
                foreach (var entry in entries)
                {
                    br.BaseStream.Position = entry.offset;
                    var compSize = br.ReadUInt32();
                    var compSizePad = br.ReadUInt32();
                    br.BaseStream.Position -= 8;

                    Files.Add(new BFPFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{count++:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, entry.offset, compSizePad + 0x20),
                        entry = entry,
                        entry2 = null,
                        compressed = (entry.uncompSize == compSize) ? false : true,
                        imports = imports
                    });
                }
                foreach (var entry in entries2)
                {
                    if (entry.offset != 0)
                    {
                        br.BaseStream.Position = entry.offset;
                        var compSize = br.ReadUInt32();
                        var compSizePad = br.ReadUInt32();
                        br.BaseStream.Position -= 8;

                        Files.Add(new BFPFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = $"{count++:00000000}.bin",
                            FileData = new SubStream(br.BaseStream, entry.offset, compSizePad + 0x20),
                            entry = null,
                            entry2 = entry,
                            compressed = (entry.uncompSize == compSize) ? false : true
                        });
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = entries[0].offset;

                //Header
                bw.WriteStruct(header);

                //EntryTable + Files
                var offset = dataOffset;
                for (int i = 0; i < header.entryCount; i++)
                {
                    bw.BaseStream.Position = 0x20 + i * 0xc;
                    Files[i].UpdateEntry(offset);
                    bw.WriteStruct(Files[i].entry);
                    bw.BaseStream.Position = Files[i].entry.offset;
                    var compSize = Files[i].Write(bw.BaseStream);
                    offset += compSize;
                }

                //EntryTable2 + Files
                var count = 0;
                for (int i = 0; i < entries2.Count; i++)
                {
                    bw.BaseStream.Position = 0x20 + header.entryCount * 0xc + i * 0x8;
                    if (entries2[i].offset != 0)
                    {
                        Files[(int)header.entryCount + count].UpdateEntry(offset);
                        bw.WriteStruct(Files[(int)header.entryCount + count].entry2);
                        bw.BaseStream.Position = offset;
                        var compSize = Files[(int)header.entryCount + count].Write(bw.BaseStream);
                        offset += compSize;

                        count++;
                    }
                    else
                    {
                        bw.WriteStruct(entries2[i]);
                    }
                }
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
