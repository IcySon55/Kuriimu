using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_ddt_img
{
    public class DDTIMG
    {
        public List<DdtFileInfo> Files = new List<DdtFileInfo>();
        private Stream _ddtStream = null;
        private Stream _imgStream = null;

        private DdtFileEntry Header;
        private long RunningPosition;

        public DDTIMG(Stream ddtInput, Stream imgInput)
        {
            _ddtStream = ddtInput;
            _imgStream = imgInput;
            using (var br = new BinaryReaderX(ddtInput, true))
            {
                Header = new DdtFileEntry
                {
                    PathOffset = br.ReadUInt32(),
                    NextEntryOffsetOrFileID = br.ReadInt32(),
                    SubEntryCountOrFileSize = br.ReadInt32()
                };
                RunningPosition = 0;

                ReadEntries(Header, br, string.Empty);

                ReadFiles(Header, new BinaryReaderX(imgInput));
            }
        }

        private void ReadEntries(DdtFileEntry header, BinaryReaderX br, string directory)
        {
            var subEntryCount = -header.SubEntryCountOrFileSize;
            header.SubEntries = new List<DdtFileEntry>();

            br.BaseStream.Position = header.NextEntryOffsetOrFileID;

            for (var i = 0; i < subEntryCount; i++)
            {
                var entry = new DdtFileEntry()
                {
                    PathOffset = br.ReadUInt32(),
                    NextEntryOffsetOrFileID = br.ReadInt32(),
                    SubEntryCountOrFileSize = br.ReadInt32()
                };
                header.SubEntries.Add(entry);
            }

            for (var i = 0; i < subEntryCount; i++)
            {
                br.BaseStream.Position = header.SubEntries[i].PathOffset;

                var c = br.ReadByte();
                var name = new List<byte>();
                while (c != 0x0)
                {
                    name.Add(c);
                    c = br.ReadByte();
                }
                header.SubEntries[i].Name = Encoding.GetEncoding("EUC-JP").GetString(name.ToArray());
            }

            for (var i = 0; i < subEntryCount; i++)
            {
                var nextDirectory = directory;
                var entry = header.SubEntries[i];

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
                        //FileData = new SubStream(_imgStream, RunningPosition, entry.SubEntryCountOrFileSize), // Set below because reasons
                        State = ArchiveFileState.Archived
                    });
                }
            }
        }

        private void ReadFiles(DdtFileEntry header, BinaryReaderX br)
        {
            foreach (var entry in header.SubEntries.Where(s => s.SubEntryCountOrFileSize >= 0))
            {
                var fileEntry = Files.FirstOrDefault(e => e.Entry == entry);
                if (fileEntry != null) fileEntry.FileData = new SubStream(_imgStream, RunningPosition, entry.SubEntryCountOrFileSize);

                // Move to the next file
                br.BaseStream.Seek(entry.SubEntryCountOrFileSize, SeekOrigin.Current);
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var b = br.ReadByte();
                    if (b == 0x0) continue;
                    RunningPosition = br.BaseStream.Position - 1;
                    break;
                }
                br.BaseStream.Seek(-1, SeekOrigin.Current);
            }

            foreach (var entry in header.SubEntries.Where(s => s.SubEntryCountOrFileSize < 0))
                ReadFiles(entry, br);
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
