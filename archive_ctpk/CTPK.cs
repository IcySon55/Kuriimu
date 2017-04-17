using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cetera.Image;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_ctpk
{
    public sealed class CTPK
    {
        public List<CTPKFileInfo> Files = new List<CTPKFileInfo>();

        public CTPK(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //Header
                Header header = br.ReadStruct<Header>();

                //TexEntries
                List<Entry> entries = new List<Entry>();
                entries.AddRange(br.ReadMultiple<Entry>(header.texCount));

                //TexInfo List
                List<int> texSizeList = new List<int>();
                texSizeList.AddRange(br.ReadMultiple<int>(header.texCount));

                //Name List
                List<String> nameList = new List<String>();
                for (int i = 0; i < entries.Count; i++)
                    nameList.Add(br.ReadCStringA());

                //Hash List
                br.BaseStream.Position = header.crc32SecOffset;
                List<HashEntry> crc32List = new List<HashEntry>();
                crc32List.AddRange(br.ReadMultiple<HashEntry>(header.texCount).OrderBy(e=>e.entryNr));

                //TexInfo List 2
                br.BaseStream.Position = header.texInfoOffset;
                List<uint> texInfoList2 = new List<uint>();
                texInfoList2.AddRange(br.ReadMultiple<uint>(header.texCount));

                //Get FileData
                for (int i = 0; i < header.texCount; i++)
                    Files.Add(new CTPKFileInfo()
                    {
                        State = ArchiveFileState.Archived,
                        FileName = nameList[i],
                        FileData = new SubStream(br.BaseStream, entries[i].texOffset+header.texSecOffset, entries[i].texDataSize),
                        Entry = entries[i],
                        hashEntry = crc32List[i],
                        texInfo = texInfoList2[i]
                    });
            }
        }

        public void Save(Stream input)
        {
            using (BinaryWriterX bw = new BinaryWriterX(input))
            {
                //get nameList Length
                int nameListLength=0;
                for (int i = 0; i < Files.Count; i++) nameListLength += Files[i].FileName.Length + 1;
                while (nameListLength % 4 != 0) nameListLength += 1;

                //Offsets
                int nameOffset = (Files.Count + 1) * 0x20 + Files.Count * 0x4;
                int dataOffset = nameOffset + nameListLength + Files.Count * 0x4 * 2;

                //Header
                bw.WriteStruct(new Header
                {
                    texCount = (short)Files.Count,
                    texSecOffset= dataOffset,
                    crc32SecOffset = nameOffset+nameListLength,
                    texInfoOffset = nameOffset + nameListLength+Files.Count*0x4
                });

                //entryList
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.WriteStruct(new Entry
                    {
                        nameOffset = nameOffset,
                        texDataSize = (int) Files[i].FileData.Length,
                        texOffset = dataOffset,
                        format = Files[i].Entry.format,
                        width = Files[i].Entry.width,
                        height = Files[i].Entry.height,
                        mipLvl = Files[i].Entry.mipLvl,
                        type = Files[i].Entry.type,
                        bitmapSizeOffset = Files[i].Entry.bitmapSizeOffset,
                        timeStamp = Files[i].Entry.timeStamp,
                    });
                    nameOffset += Files[i].FileName.Length + 1;
                    dataOffset += (int) Files[i].FileData.Length;
                }

                //texInfo 1 List
                for (int i = 0; i < Files.Count; i++) bw.Write((int)Files[i].FileData.Length);

                //nameList
                for (int i = 0; i < Files.Count; i++) { bw.WriteASCII(Files[i].FileName); bw.Write((byte)0); }
                while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;

                //crc32List
                for (int i = 0; i < Files.Count; i++) {bw.Write(Files[i].hashEntry.crc32); bw.Write(Files[i].hashEntry.entryNr); }

                //texInfo 2 List
                for (int i = 0; i < Files.Count; i++) bw.Write(Files[i].texInfo);

                //Write data
                int texSecSize = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write(new BinaryReaderX(Files[i].FileData,true).ReadBytes((int)Files[i].FileData.Length));
                    texSecSize += (int) Files[i].FileData.Length;
                }
                bw.BaseStream.Position = 0xc;
                bw.Write(texSecSize);
            }
        }
    }
}
