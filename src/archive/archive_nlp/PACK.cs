using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nlp.PACK
{
    public class PACK
    {
        public List<PACKFileInfo> Files = new List<PACKFileInfo>();
        private Stream _stream = null;
        private Import imports = new Import();

        public PACKHeader header;
        public byte[] names;

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
                var pointers = br.ReadBytes(header.packFileCount * 4);
                br.BaseStream.Position = header.stringOffOffset;
                var stringOffsets = br.ReadMultiple<int>(header.packFileCount);

                //Strings
                br.BaseStream.Position = header.stringOffset;
                names = br.ReadBytes((int)(header.stringOffOffset - br.BaseStream.Position));

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
                    using (var name = new BinaryReaderX(new MemoryStream(names)))
                    {
                        name.BaseStream.Position = stringOffsets[i];
                        var readName = name.ReadCStringA();
                        Files.Add(new PACKFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = (entries[i].magic == "TEX ") ? readName.Remove(readName.Length - 1) : readName,
                            FileData = new SubStream(
                                br.BaseStream,
                                (entries[i].compOffset == 0) ? entries[i].decompOffset : entries[i].compOffset,
                                (entries[i].compSize == 0) ? entries[i].decompSize : entries[i].compSize),
                            Entry = fileEntries[i],
                            names = (fileEntries[i].entry.magic == "TEXI") ? names : null,
                            pointers = (fileEntries[i].entry.magic == "TEXI") ? pointers : null,
                            imports = imports
                        });
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = (uint)(0x20 + Files.Count * 0x20 + ((names.Length + 0xf) & ~0xf) + ((Files.Count * 0x4 + 0xf) & ~0xf));
                bw.BaseStream.Position = dataOffset;

                //Files
                var compOffset = dataOffset;
                var decompOffset = dataOffset;
                foreach (var file in Files)
                {
                    var res = file.Write(bw.BaseStream, compOffset, decompOffset);
                    compOffset = (uint)((res.Item1 + 0xf) & ~0xf);
                    decompOffset = (uint)((res.Item2 + 0xf) & ~0xf);
                }

                //Names
                bw.BaseStream.Position = header.stringOffset;
                bw.Write(names);

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
