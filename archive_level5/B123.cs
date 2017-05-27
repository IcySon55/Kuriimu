using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text;
using Cetera.Hash;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_fa
{
    public sealed class FA
    {
        public List<FAFileInfo> Files = new List<FAFileInfo>();
        Stream _stream = null;

        private Header header;
        private byte[] unk1;
        private byte[] unk2;

        private List<Entry> entries;
        private List<string> fileNames;
        private List<string> dirStruct = new List<string>();
        private List<int> folderCounts = new List<int>();

        public FA(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //unknown lists
                unk1 = br.ReadBytes(header.offset1 - header.offset0);
                unk2 = br.ReadBytes(header.entryOffset - header.offset1);

                //Entries
                entries = br.ReadMultiple<Entry>(header.fileCount);

                //Names
                br.BaseStream.Position = header.nameOffset;

                string currentFolder = "";
                br.BaseStream.Position++;
                string tmp = ReadString(br.BaseStream);
                br.BaseStream.Position--;
                fileNames = new List<string>();
                folderCounts.Add(0);

                while (tmp !="" && br.BaseStream.Position < header.dataOffset)
                {
                    if (tmp.Last() == '/')
                    {
                        folderCounts.Add(0);
                        dirStruct.Add(tmp);
                        currentFolder = tmp;
                    }
                    else
                    {
                        dirStruct.Add(currentFolder + tmp);
                        fileNames.Add(currentFolder + tmp);
                        folderCounts[folderCounts.Count - 1] += 1;
                    }

                    br.BaseStream.Position++;
                    if (br.BaseStream.Position < header.dataOffset)
                    {
                        tmp = ReadString(br.BaseStream);
                        br.BaseStream.Position--;
                    }
                }

                //FileData
                int pos = 0;
                foreach (var folderCount in folderCounts)
                {
                    var tmpFiles = new List<NameEntry>();
                    for (int i = 0; i < folderCount; i++)
                        tmpFiles.Add(new NameEntry {
                            name = fileNames[pos + i],
                            crc32 = Crc32.Create(Encoding.GetEncoding("SJIS").GetBytes(fileNames[pos + i].Split('/').Last().ToLower()))
                        });
                    tmpFiles = tmpFiles.OrderBy(x => x.crc32).ToList();

                    foreach (var nameEntry in tmpFiles)
                    {
                        if (nameEntry.crc32 == entries[pos].crc32)
                            Files.Add(new FAFileInfo
                            {
                                State = ArchiveFileState.Archived,
                                FileName = nameEntry.name,
                                FileData = new SubStream(br.BaseStream, entries[pos].fileOffset + header.dataOffset, entries[pos].fileSize),
                                crc32 = entries[pos++].crc32
                            });
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                bw.BaseStream.Position = 0x48;

                //first unknown half of info section
                bw.Write(unk1);
                bw.Write(unk2);

                //entryList and Data
                uint dataOffset=0;
                uint movDataOffset = (uint)(0x48 + unk1.Length + unk2.Length + Files.Count * 0x10);
                foreach (var name in dirStruct) movDataOffset += 1 + (uint)Encoding.GetEncoding("SJIS").GetBytes((name.Last() != '/') ? name.Split('/').Last() : name).Length;
                while (movDataOffset % 4 != 0) movDataOffset++;

                header.dataOffset = movDataOffset;
                header.nameOffset = (uint)(0x48 + unk1.Length + unk2.Length + Files.Count * 0x10);
                header.folderCount = (short)folderCounts.Count;
                header.fileCount = Files.Count;
                header.fileCount2 = Files.Count;

                int pos = 0;
                foreach (var folderCount in folderCounts)
                {
                    var nameSorted = new List<NameEntry>();
                    for (int i = 0; i < folderCount; i++) nameSorted.Add(new NameEntry { name = Files[pos + i].FileName, crc32 = Files[pos + i].crc32, size = (uint)Files[pos + i].FileSize });
                    nameSorted = nameSorted.OrderBy(x => x.name, StringComparer.OrdinalIgnoreCase).ToList();

                    var entriesTmp = new List<Entry>();
                    uint nameOffset = 0;
                    for (int i = 0; i < folderCount; i++)
                    {
                        entriesTmp.Add(new Entry { crc32 = Files[pos + i].crc32 });
                    }
                    for (int i = 0; i < folderCount; i++)
                    {
                        var foundEntry = entriesTmp.Find(x => x.crc32 == nameSorted[i].crc32);
                        foundEntry.nameOffsetInFolder = nameOffset;
                        foundEntry.fileOffset = dataOffset;
                        foundEntry.fileSize = nameSorted[i].size;

                        var t = "";
                        if (bw.BaseStream.Position == 0x74fc)
                            t = nameSorted[i].name;
                        t = "";

                        nameOffset += 1 + (uint)nameSorted[i].name.Split('/').Last().Length;

                        long bk = bw.BaseStream.Position;
                        bw.BaseStream.Position = movDataOffset;
                        Files.Find(x => x.FileName == nameSorted[i].name).FileData.CopyTo(bw.BaseStream);
                        bw.BaseStream.Position++;
                        while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;
                        dataOffset += (uint)bw.BaseStream.Position - movDataOffset;
                        movDataOffset = (uint)bw.BaseStream.Position;
                        bw.BaseStream.Position = bk;
                    }
                    for (int i = 0; i < folderCount; i++)
                    {
                        bw.WriteStruct(entriesTmp[i]);
                    }

                    pos += folderCount;
                }
                

                //nameList
                foreach(var name in dirStruct)
                {
                    bw.Write((byte)0);
                    if (name.Last() != '/')
                        bw.Write(Encoding.GetEncoding("SJIS").GetBytes(name.Split('/').Last()));
                    else
                        bw.Write(Encoding.GetEncoding("SJIS").GetBytes(name));
                }
                bw.BaseStream.Position++;
                while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;

                //Write Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public string ReadString(Stream input)
        {
            using (var br=new BinaryReaderX(input,true))
            {
                var result = new List<byte>();
                var tmp = br.ReadByte();
                while(tmp!=0x00)
                {
                    result.Add(tmp);
                    tmp = br.ReadByte();
                }

                return Encoding.GetEncoding("SJIS").GetString(result.ToArray());
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
