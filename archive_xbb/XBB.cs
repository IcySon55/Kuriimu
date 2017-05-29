using System.Collections.Generic;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;
using System.Linq;
using Cetera.Hash;

namespace archive_xbb
{
    public class XBB
    {
        public List<XBBFileInfo> Files = new List<XBBFileInfo>();
        private Stream _stream = null;

        public XBB(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                var header = br.ReadStruct<XBBHeader>();
                br.BaseStream.Position = 0x20;

                List<XBBFileEntry> entries = new List<XBBFileEntry>();
                entries.AddRange(br.ReadMultiple<XBBFileEntry>(header.entryCount));

                List<XBBHashEntry> hashes = new List<XBBHashEntry>();
                hashes.AddRange(br.ReadMultiple<XBBHashEntry>(header.entryCount));

                for (int i = 0; i < header.entryCount; i++) {
                    br.BaseStream.Position = entries[i].nameOffset;
                    Files.Add(new XBBFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = br.ReadCStringA(),
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size)
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Header
                bw.WriteASCII("XBB");
                bw.Write((byte)1);
                bw.Write(Files.Count());
                bw.BaseStream.Position = 0x20;

                var offset = 0x20 + Files.Sum(e => e.FileName.Length + 1 + 0x18);
                offset = offset + 0x7f & ~0x7f;
                var nameOffset = 0x20 + Files.Count() * 0x18;

                //FileEntries
                foreach (var file in Files) {
                    bw.Write(offset);
                    bw.Write((uint)file.FileSize);
                    bw.Write(nameOffset);
                    bw.Write(XbbHash.Create(file.FileName));

                    offset += (int)file.FileSize;
                    offset = offset + 0x7f & ~0x7f;

                    nameOffset += file.FileName.Length + 1;
                }

                //Hash table
                var files = Files.OrderBy(e => XbbHash.Create(e.FileName)).ToList();
                for (int i=0;i<files.Count();i++) {
                    var hash = XbbHash.Create(files[i].FileName);
                    bw.Write(hash);
                    bw.Write(Files.FindIndex(e=> XbbHash.Create(e.FileName)==hash));
                }

                //nameList
                foreach (var file in Files)
                {
                    bw.WriteASCII(file.FileName);
                    bw.Write((byte)0);
                }

                //FileData
                foreach (var file in Files)
                {
                    bw.WritePadding(0x80);
                    file.FileData.CopyTo(bw.BaseStream);
                }

                bw.WritePadding(0x80);
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
