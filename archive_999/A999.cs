using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_999
{
    public class A999
    {
        public List<A999FileInfo> Files=new List<A999FileInfo>();

        List<uint> directoryHashes=new List<uint>();
        List<DirectoryEntry> directoryEntries=new List<DirectoryEntry>();
        List<uint> XORs=new List<uint>();

        private uint headXORpad=0xFABACEDA;

        public A999(Stream input)
        {
            using (var br0 = new BinaryReaderX(input, true))
            {
                var tmpHeader = A999Support.deXOR(br0.BaseStream, headXORpad, 0x20);
                var tmpBr = new BinaryReaderX(new MemoryStream(tmpHeader));
                tmpBr.BaseStream.Position = 0x14;
                var inputTopSize = tmpBr.ReadInt64();

                br0.BaseStream.Position = 0;
                var inputTop = A999Support.deXOR(br0.BaseStream, headXORpad, inputTopSize);

                using (var br = new BinaryReaderX(new MemoryStream(inputTop)))
                {
                    //Header
                    var header = br.ReadStruct<Header>();

                    //unknown
                    var directoryTop = br.ReadStruct<TableHeader>();
                    directoryHashes = br.ReadMultiple<uint>(directoryTop.entryCount);
                    while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;
                    directoryEntries = br.ReadMultiple<DirectoryEntry>(directoryTop.entryCount);

                    //FileEntries
                    var entryTop = br.ReadStruct<TableHeader>();
                    XORs = br.ReadMultiple<uint>(entryTop.entryCount);
                    while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;

                    //Files
                    for (int i = 0, overallFileCount = 0; i < directoryTop.entryCount; i++)
                        for (int j = 0; j < directoryEntries[i].fileCount; j++, overallFileCount++)
                        {
                            var entry = br.ReadStruct<Entry>();
                            string nameTmp, name="";
                            if (A999Support.foldernames.TryGetValue(directoryHashes[i], out nameTmp))
                            {
                                if (!A999Support.filenames.TryGetValue(entry.XORpad, out name))
                                {
                                    name = nameTmp + $"File{overallFileCount:00000}";
                                }
                            }
                            else
                            {
                                name= $"{i:00}/File{overallFileCount:00000}";
                            }

                            Files.Add(new A999FileInfo
                            {
                                Entry = entry,
                                XORpad = entry.XORpad,
                                FileName = name,
                                State = ArchiveFileState.Archived,
                                FileData = new SubStream(br0.BaseStream, header.dataOffset + entry.fileOffset, entry.fileSize)
                            });
                        }
                }
            }
        }
    }
}
