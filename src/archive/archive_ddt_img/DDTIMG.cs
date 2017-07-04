using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_ddt_img
{
    public class DDTIMG
    {
        private const long Alignment = 0x800;

        public List<DdtFileInfo> Files = new List<DdtFileInfo>();
        private Stream _ddtStream;
        private Stream _imgStream;

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
                var entry = new DdtFileEntry
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
                var entry = parent.SubEntries[i];

                if (entry.SubEntryCountOrFileSize >= 0)
                {
                    Files.Add(new DdtFileInfo
                    {
                        Entry = entry,
                        FileName = $"{directory}\\{entry.Name}".TrimStart('\\'),
                        FileData = new SubStream(_imgStream, entry.NextDirectoryOffsetOrFileOffset * Alignment, entry.SubEntryCountOrFileSize),
                        State = ArchiveFileState.Archived
                    });
                }
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
            }
        }

        public void Save(Stream ddtOutput, Stream imgOutput)
        {
            using (var bwDdt = new BinaryWriterX(ddtOutput))
            {
                // Header
                bwDdt.Write(Header.PathOffset);
                bwDdt.Write(Header.NextDirectoryOffsetOrFileOffset);
                bwDdt.Write(Header.SubEntryCountOrFileSize);

                long runningOffset = 0;
                int runningFileOffset = 0;
                UpdateEntries(Header, ref runningOffset, ref runningFileOffset);

                using (var bwImg = new BinaryWriterX(imgOutput))
                {
                    runningFileOffset = 0;
                    WriteEntries(Header, bwDdt, bwImg, ref runningFileOffset);
                }
            }
        }

        private void UpdateEntries(DdtFileEntry parent, ref long runningOffset, ref int runningFileIndex)
        {
            var subEntryCount = -parent.SubEntryCountOrFileSize;

            for (var i = 0; i < subEntryCount; i++)
            {
                var entry = parent.SubEntries[i];

                if (entry.SubEntryCountOrFileSize >= 0)
                {
                    entry.NextDirectoryOffsetOrFileOffset = (int)(runningOffset / Alignment);
                    runningOffset += (long)Files[runningFileIndex].FileSize;
                    if (runningOffset % Alignment > 0)
                        runningOffset += Alignment - runningOffset % Alignment;
                    runningFileIndex++;
                }
            }
            
            for (var i = 0; i < subEntryCount; i++)
            {
                var entry = parent.SubEntries[i];

                if (entry.SubEntryCountOrFileSize < 0)
                    UpdateEntries(entry, ref runningOffset, ref runningFileIndex);
            }
        }

        private void WriteEntries(DdtFileEntry parent, BinaryWriterX bwDdt, BinaryWriterX bwImg, ref int runningFileIndex)
        {
            var subEntryCount = -parent.SubEntryCountOrFileSize;

            for (var i = 0; i < subEntryCount; i++)
            {
                var entry = parent.SubEntries[i];
                bwDdt.Write(entry.PathOffset);
                bwDdt.Write(entry.NextDirectoryOffsetOrFileOffset);
                bwDdt.Write(entry.SubEntryCountOrFileSize);
            }

            for (var i = 0; i < subEntryCount; i++)
            {
                // TODO: Adjust parent.SubEntries[i].PathOffset if we ever allow changes to the directory tree
                //bw.BaseStream.Position = parent.SubEntries[i].PathOffset;
                bwDdt.Write(Encoding.GetEncoding("EUC-JP").GetBytes(parent.SubEntries[i].Name));
                bwDdt.Write((byte)0x0);
            }
            bwDdt.WriteAlignment(4);

            for (var i = 0; i < subEntryCount; i++)
            {
                var entry = parent.SubEntries[i];

                if (entry.SubEntryCountOrFileSize >= 0)
                {
                    var fileInfo = Files[runningFileIndex];
                    fileInfo.FileData.CopyTo(bwImg.BaseStream);
                    bwImg.WriteAlignment((int)Alignment);
                    runningFileIndex++;
                }
            }

            for (var i = 0; i < subEntryCount; i++)
            {
                var entry = parent.SubEntries[i];

                if (entry.SubEntryCountOrFileSize < 0)
                    WriteEntries(entry, bwDdt, bwImg, ref runningFileIndex);
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
