using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Linq;
using System.Text;

namespace archive_zdp
{
    public class ZDP
    {
        public List<ZDPFileInfo> Files = new List<ZDPFileInfo>();
        private Stream _stream = null;

        public PartitionHeader partHeader;
        public Header header;

        public ZDP(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                partHeader = br.ReadStruct<PartitionHeader>();
                header = br.ReadStruct<Header>();

                //Entries
                br.BaseStream.Position = header.entryListOffset;
                var entries = br.ReadMultiple<FileEntry>((int)header.fileCount);

                //Names
                br.BaseStream.Position = header.nameOffset;
                var nameOffsets = br.ReadMultiple<uint>((int)header.fileCount);
                var names = new List<string>();
                foreach (var offset in nameOffsets)
                {
                    br.BaseStream.Position = offset;
                    names.Add(br.ReadCStringA());
                }

                //Files
                for (int i = 0; i < entries.Count; i++)
                {
                    Files.Add(new ZDPFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = names[i],
                        FileData = new SubStream(br.BaseStream, entries[i].offset, entries[i].size),
                        entry = entries[i]
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //Header
                bw.WriteStruct(partHeader);
                bw.WriteStruct(header);

                //Files
                bw.BaseStream.Position = Files[0].entry.offset;
                foreach (var file in Files)
                {
                    file.entry.offset = (uint)bw.BaseStream.Position;
                    file.entry.size = (uint)file.FileSize;

                    bw.WriteAlignment();
                    file.FileData.CopyTo(bw.BaseStream);
                }

                //Entries
                bw.BaseStream.Position = header.entryListOffset;
                foreach (var file in Files) bw.WriteStruct(file.entry);

                //NameLength
                bw.BaseStream.Position = header.nameOffset;
                var nameOffset = (uint)bw.BaseStream.Length;
                foreach (var file in Files)
                {
                    bw.Write(nameOffset);
                    nameOffset += (uint)Encoding.ASCII.GetByteCount(file.FileName) + 1;
                }

                //Names
                bw.BaseStream.Position = bw.BaseStream.Length;
                foreach (var file in Files)
                {
                    bw.WriteASCII(file.FileName);
                    bw.Write((byte)0);
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
