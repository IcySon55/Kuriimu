using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using Kuriimu.Compression;

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
            using (var br = new BinaryReaderX(input,true))
            {
                int folderCount = 0;
                int totalEntryCount = 0;
                int dataOffset = 0;
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var entryCount = br.ReadInt32();
                    if (entryCount == 0x00c20010) break;
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
                                FileName = $"{folderCount:x8}/{count++:x8}.bin",
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

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
