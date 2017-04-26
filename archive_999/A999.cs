using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_999
{
    public class A999
    {
        public List<A999FileInfo> Files=new List<A999FileInfo>();

        const uint headXORpad = 0xFABACEDA;

        public A999(Stream input)
        {
            using (var br = new BinaryReaderX(new XorStream(input, headXORpad), true))
            {
                //Header
                var header = br.ReadStruct<Header>();

                //Directories
                var directoryTop = br.ReadStruct<TableHeader>();
                var directoryHashes = br.ReadMultiple<uint>(directoryTop.entryCount);
                while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;
                var directoryEntries = br.ReadMultiple<DirectoryEntry>(directoryTop.entryCount);

                //FileEntries
                var entryTop = br.ReadStruct<TableHeader>();
                var XORs = br.ReadMultiple<uint>(entryTop.entryCount);
                while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;

                //Files
                for (int i = 0, overallFileCount = 0; i < directoryTop.entryCount; i++)
                    for (int j = 0; j < directoryEntries[i].fileCount; j++, overallFileCount++)
                    {
                        var entry = br.ReadStruct<Entry>();
                        string nameTmp, name = "";
                        if (A999Support.foldernames.TryGetValue(directoryHashes[i], out nameTmp))
                        {
                            if (!A999Support.filenames.TryGetValue(entry.XORpad, out name))
                            {
                                name = nameTmp + $"File{overallFileCount:00000}";
                            }
                        }
                        else
                        {
                            name = $"{i:00}/File{overallFileCount:00000}";
                        }

                        Files.Add(new A999FileInfo
                        {
                            Entry = entry,
                            FileName = name,
                            State = ArchiveFileState.Archived,
                            FileData = new XorStream(new SubStream(input, header.dataOffset + entry.fileOffset, entry.fileSize), entry.XORpad)
                        });
                    }
            }
        }
    }
}
