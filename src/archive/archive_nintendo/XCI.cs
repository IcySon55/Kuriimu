using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;
using Kontract.Interface;

namespace archive_nintendo.XCI
{
    public sealed class XCI
    {
        public List<XCIFileInfo> Files = new List<XCIFileInfo>();
        Stream _stream = null;

        Header xciHeader = null;

        public XCI(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                xciHeader = br.ReadStruct<Header>();

                //Ignore cert partition for now and jump to HFS0
                var baseEntries = ParseHFS0Partition(input, xciHeader.hfs0Offset);
                var result = new List<HFS0NamedEntry>();
                foreach (var entry in baseEntries)
                    result.AddRange(ParseHFS0Partition(input, entry.entry.offset, entry.name));

                foreach (var entry in result)
                    Files.Add(new XCIFileInfo
                    {
                        FileData = new SubStream(br.BaseStream, entry.entry.offset, entry.entry.size),
                        FileName = entry.name,
                        State = ArchiveFileState.Archived
                    });
            }
        }

        private static List<HFS0NamedEntry> ParseHFS0Partition(Stream input, long hfs0_offset, string prePath = "")
        {
            var namedEntries = new List<HFS0NamedEntry>();
            long hfs0_header_size = 0x200;

            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = hfs0_offset;
                var header = br.ReadStruct<HFS0Header>();
                var entries = br.ReadMultiple<HFS0Entry>(header.fileCount);
                var stringTable = br.ReadBytes(header.stringTableSize);

                using (var nameBr = new BinaryReaderX(new MemoryStream(stringTable)))
                    foreach (var entry in entries)
                    {
                        nameBr.BaseStream.Position = entry.nameOffset;
                        var name = Path.Combine(prePath, nameBr.ReadCStringA());
                        namedEntries.Add(new HFS0NamedEntry { name = name, entry = entry });
                    }
            }

            //Absolutize offsets
            foreach (var entry in namedEntries)
                entry.entry.offset += hfs0_offset + hfs0_header_size;

            return namedEntries;
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {

            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
