using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using Kontract.Compression;

namespace archive_aatri.aabin
{
    public class AABIN
    {
        public List<AAbinFileInfo> Files = new List<AAbinFileInfo>();

        List<Entry> entries = new List<Entry>();

        private Stream _stream = null;

        public AABIN(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                int folderCount = 0;
                int fileCount = 0;
                int totalEntryCount = 0;
                int dataOffset = 0;
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var entryCount = br.ReadInt32();
                    if (entryCount >> 16 != 0)
                    {
                        br.BaseStream.Position -= 4;
                        Files.Add(new AAbinFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = $"{fileCount++:00000000}.bin",
                            FileData = new MemoryStream(Nintendo.Decompress(br.BaseStream))
                        });
                    }
                    else
                    {
                        entries.AddRange(br.ReadMultiple<Entry>(entryCount));

                        int count = 0;
                        for (int i = totalEntryCount; i < entries.Count; i++)
                        {
                            var absOffset = dataOffset + entries[i].offset;
                            br.BaseStream.Position = absOffset;

                            try
                            {
                                Files.Add(new AAbinFileInfo
                                {
                                    FileName = $"{folderCount:00000000}/{count++:00000000}.bin",
                                    State = ArchiveFileState.Archived,
                                    Entry = entries[i],
                                    FileData = new SubStream(br.BaseStream, absOffset, entries[i].compSize)
                                });
                            }
                            catch (System.Exception)
                            {
                                throw new System.Exception(i.ToString() + "   " + br.BaseStream.Position.ToString() + "   " + entries[i].compSize.ToString());
                            }

                            br.BaseStream.Position = absOffset + entries[i].compSize;
                        }
                    }

                    while (br.BaseStream.Position % 4 != 0) br.BaseStream.Position++;
                    dataOffset = (int)br.BaseStream.Position;
                    folderCount++;
                    totalEntryCount = entries.Count;
                }
            }
        }

        public void Save(Stream input)
        {

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
