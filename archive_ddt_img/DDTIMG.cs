using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_ddt_img
{
    public class DDTIMG
    {
        private const long Alignment = 0x800;

        public List<DdtFileInfo> Files = new List<DdtFileInfo>();
        private Stream _ddtStream = null;
        private Stream _imgStream = null;

        private DdtFileEntry Header;

        public DDTIMG(Stream ddtInput, Stream imgInput)
        {
            _ddtStream = ddtInput;
            _imgStream = imgInput;
            using (var br = new BinaryReaderX(ddtInput, true))
            {
                Header = new DdtFileEntry
                {
                    PathOffset = br.ReadUInt32(),
                    NextDirectoryOffsetOrFileOffset = br.ReadInt32(),
                    SubEntryCountOrFileSize = br.ReadInt32()
                };

                ReadEntries(Header, br, string.Empty);
            }
        }

        private void ReadEntries(DdtFileEntry parent, BinaryReaderX br, string directory)
        {
            var subEntryCount = -parent.SubEntryCountOrFileSize;
            parent.SubEntries = new List<DdtFileEntry>();

            br.BaseStream.Position = parent.NextDirectoryOffsetOrFileOffset;

            for (var i = 0; i < subEntryCount; i++)
            {
                var entry = new DdtFileEntry()
                {
                    PathOffset = br.ReadUInt32(),
                    NextDirectoryOffsetOrFileOffset = br.ReadInt32(),
                    SubEntryCountOrFileSize = br.ReadInt32()
                };
                parent.SubEntries.Add(entry);
            }

            for (var i = 0; i < subEntryCount; i++)
            {
                br.BaseStream.Position = parent.SubEntries[i].PathOffset;

                var c = br.ReadByte();
                var name = new List<byte>();
                while (c != 0x0)
                {
                    name.Add(c);
                    c = br.ReadByte();
                }
                parent.SubEntries[i].Name = Encoding.GetEncoding("EUC-JP").GetString(name.ToArray());
            }

            for (var i = 0; i < subEntryCount; i++)
            {
                var nextDirectory = directory;
                var entry = parent.SubEntries[i];

                if (entry.SubEntryCountOrFileSize < 0)
                {
                    nextDirectory += "\\" + entry.Name;
                    ReadEntries(entry, br, nextDirectory);
                }
                else
                {
                    entry.Name = $"{directory}\\{entry.Name}".TrimStart('\\');

                    Files.Add(new DdtFileInfo
                    {
                        Entry = entry,
                        FileName = entry.Name,
                        FileData = new SubStream(_imgStream, entry.NextDirectoryOffsetOrFileOffset * Alignment, entry.SubEntryCountOrFileSize),
                        State = ArchiveFileState.Archived
                    });
                }
            }
        }

        public void Save(Stream ddtOutput, Stream imgOutput)
        {
            using (var bw = new BinaryWriterX(ddtOutput))
            {
                // TODO: Write out your file format
            }
        }

        public void Close()
        {
            _ddtStream?.Dispose();
            _ddtStream = null;
            _imgStream?.Dispose();
            _imgStream = null;
        }
    }
}
