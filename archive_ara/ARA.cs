using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_ara
{
    public class ARA
    {
        public List<AraFileInfo> Files = new List<AraFileInfo>();
        private Stream _binStream = null;
        private Stream _arcStream = null;

        private List<AraFileEntry> Entries = new List<AraFileEntry>();
        private Header Header;

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
                        Filesize = br.ReadUInt32(),
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
                    FileData = new SubStream(arcInput, o.Offset, o.Filesize),
                    State = ArchiveFileState.Archived
                }).ToList();
            }
        }

        public void Save(Stream binOutput, Stream arcOutput)
        {
            using (var bw = new BinaryWriterX(binOutput))
            {
                // TODO: Write out your file format
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
