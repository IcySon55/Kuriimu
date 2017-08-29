using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Text;

namespace archive_nlp.PACK
{
    public class PACK
    {
        public List<PACKFileInfo> Files = new List<PACKFileInfo>();
        private Stream _stream = null;

        public PACKHeader header;

        public PACK(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<PACKHeader>();

                //Entries
                var entries = br.ReadMultiple<PACKEntry>(header.packFileCount);

                //stringOffsets
                br.BaseStream.Position = header.stringOffOffset;
                var stringOffsets = br.ReadMultiple<int>(header.packFileCount);

                //Strings
                var names = new List<string>();
                foreach (var offset in stringOffsets)
                {
                    br.BaseStream.Position = header.stringOffset + offset;
                    names.Add(br.ReadCStringA());
                }

                //FileEntry
                var fileEntries = new List<FileEntry>();
                for (int i = 0; i < header.packFileCount; i++)
                    fileEntries.Add(new FileEntry
                    {
                        entry = entries[i],
                        nameOffset = stringOffsets[i]
                    });

                //Files
                for (int i = 0; i < header.packFileCount; i++)
                {
                    Files.Add(new PACKFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = names[i],
                        FileData = new SubStream(br.BaseStream, entries[i].compOffset, entries[i].compSize),
                        Entry = fileEntries[i]
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = header.fileOffset;
                bw.BaseStream.Position = dataOffset;

                //Files
                var compOffset = dataOffset;
                var uncompOffset = dataOffset;
                foreach (var file in Files)
                {
                    var res = file.Write(bw.BaseStream, compOffset, uncompOffset);
                    compOffset = (uint)((res.Item1 + 0xf) & ~0xf);
                    uncompOffset = (uint)((res.Item2 + 0xf) & ~0xf);
                }

                //Names
                foreach (var file in Files)
                {
                    bw.BaseStream.Position = header.stringOffset + file.Entry.nameOffset;
                    bw.Write(Encoding.ASCII.GetBytes(file.FileName));
                }

                //NameOffsets
                bw.BaseStream.Position = header.stringOffOffset;
                foreach (var file in Files) bw.Write(file.Entry.nameOffset);

                //Entries
                bw.BaseStream.Position = 0x20;
                foreach (var file in Files) bw.WriteStruct(file.Entry.entry);

                //Header
                bw.BaseStream.Position = 0;
                uint compSize = 0;
                uint decompSize = 0;
                foreach (var file in Files)
                {
                    compSize += file.Entry.entry.compSize;
                    compSize = (uint)((compSize + 0xf) & ~0xf);
                    decompSize += file.Entry.entry.decompSize;
                    decompSize = (uint)((decompSize + 0xf) & ~0xf);
                }
                header.compSize = compSize + header.fileOffset;
                header.decompSize = decompSize + header.fileOffset;
                bw.WriteStruct(header);
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
