using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace archive_ara
{
    public class ARA
    {
        public List<AraFileInfo> Files = new List<AraFileInfo>();
        private Stream _binStream = null;
        private Stream _arcStream = null;

        private List<AraFileEntry> Entries = new List<AraFileEntry>();
        private Header Header;

        private const uint ArcStartOffset = 0x10;

        public ARA(Stream binInput, Stream arcInput)
        {
            _binStream = binInput;
            _arcStream = arcInput;
            using (var br = new BinaryReaderX(binInput, true))
            {
                Header = br.ReadStruct<Header>();

                for (var i = 0; i < Header.FileCount; i++)
                {
                    var e = new AraFileEntry
                    {
                        FilenameOffset = br.ReadUInt32(),
                        FileSize = br.ReadUInt32(),
                        Unk1 = br.ReadUInt32(),
                        Unk2 = br.ReadUInt32()
                    };
                    Entries.Add(e);
                }

                for (var i = 0; i < Header.FileCount; i++)
                    Entries[i].Offset = br.ReadUInt32();

                foreach (var e in Entries)
                {
                    br.BaseStream.Position = e.FilenameOffset;
                    e.Filename = br.ReadCStringA();
                }

                Files = Entries.Select(o => new AraFileInfo
                {
                    Entry = o,
                    FileName = o.Filename,
                    FileData = new SubStream(arcInput, o.Offset, o.FileSize),
                    State = ArchiveFileState.Archived
                }).ToList();
            }
        }

        public void Save(Stream binOutput, Stream arcOutput)
        {
            using (var bw = new BinaryWriterX(binOutput))
            {
                bw.WriteStruct(Header);

                var filenameOffsets = bw.BaseStream.Position;
                var fileOffsets = Header.FileCount * 0x10 + filenameOffsets;
                var filenamesOffset = (Header.FileCount * 0x4 + fileOffsets + 15) & ~15;

                // Files
                using (var bw2 = new BinaryWriterX(arcOutput))
                {
                    bw2.BaseStream.Position = ArcStartOffset;
                    for (var i = 0; i < Files.Count; i++)
                    {
                        Files[i].Entry.Offset = (uint)bw2.BaseStream.Position;
                        Files[i].Entry.FileSize = (uint)Files[i].FileSize;
                        Files[i].FileData.CopyTo(bw2.BaseStream);
                        bw2.WriteAlignment();
                    }
                }

                // Filenames
                bw.BaseStream.Position = filenamesOffset;
                for (var i = 0; i < Header.FileCount; i++)
                {
                    Entries[i].FilenameOffset = (uint)bw.BaseStream.Position;
                    bw.WriteASCII(Entries[i].Filename);
                    bw.Write((byte)0x0);
                    bw.WriteAlignment();
                }

                // Filename Offsets
                bw.BaseStream.Position = filenameOffsets;
                for (var i = 0; i < Header.FileCount; i++)
                {
                    var entry = Entries[i];
                    bw.Write(entry.FilenameOffset);
                    bw.Write(entry.FileSize);
                    bw.Write(entry.Unk1);
                    bw.Write(entry.Unk2);
                }

                // File Offsets
                bw.BaseStream.Position = fileOffsets;
                for (var i = 0; i < Header.FileCount; i++)
                    bw.Write(Entries[i].Offset);
            }
        }

        public void Close()
        {
            _binStream?.Dispose();
            _binStream = null;
            _arcStream?.Dispose();
            _arcStream = null;
        }
    }
}
