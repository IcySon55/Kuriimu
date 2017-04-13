using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Cetera.Image;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_ctpk
{
    public sealed class CTPK
    {
        public Bitmap bmp;

        public CTPK(String filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
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
                crc32List.AddRange(br.ReadMultiple<HashEntry>(header.texCount).OrderBy(e => e.entryNr));

                //TexInfo List 2
                br.BaseStream.Position = header.texInfoOffset;
                List<uint> texInfoList2 = new List<uint>();
                texInfoList2.AddRange(br.ReadMultiple<uint>(header.texCount));

                br.BaseStream.Position = entries[0].texOffset;
                var settings = new ImageSettings
                {
                    Width = entries[0].width,
                    Height = entries[0].height,
                    Format = ImageSettings.ConvertFormat(entries[0].imageFormat),
                };
                bmp = Common.Load(br.ReadBytes(entries[0].texDataSize), settings);
            }
        }

        /*public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                //get nameList Length
                int nameListLength = 0;
                for (int i = 0; i < Textures.Count; i++) nameListLength += Textures[i].FileName.Length + 1;
                while (nameListLength % 4 != 0) nameListLength += 1;

                //Offsets
                int nameOffset = (Textures.Count + 1) * 0x20 + Textures.Count * 0x4;
                int dataOffset = nameOffset + nameListLength + Textures.Count * 0x4 * 2;

                //Header
                bw.WriteStruct(new Header
                {
                    texCount = (short)Textures.Count,
                    texSecOffset = dataOffset,
                    crc32SecOffset = nameOffset + nameListLength,
                    texInfoOffset = nameOffset + nameListLength + Textures.Count * 0x4
                });

                //entryList
                for (int i = 0; i < Textures.Count; i++)
                {
                    bw.WriteStruct(new Entry
                    {
                        nameOffset = nameOffset,
                        texDataSize = (int)Textures[i].FileData.Length,
                        texOffset = dataOffset,
                        format = Textures[i].Entry.format,
                        width = Textures[i].Entry.width,
                        height = Textures[i].Entry.height,
                        mipLvl = Textures[i].Entry.mipLvl,
                        type = Textures[i].Entry.type,
                        bitmapSizeOffset = Textures[i].Entry.bitmapSizeOffset,
                        timeStamp = Textures[i].Entry.timeStamp,
                    });
                    nameOffset += Textures[i].FileName.Length + 1;
                    dataOffset += (int)Textures[i].FileData.Length;
                }

                //texInfo 1 List
                for (int i = 0; i < Textures.Count; i++) bw.Write((int)Textures[i].FileData.Length);

                //nameList
                for (int i = 0; i < Textures.Count; i++) { bw.WriteASCII(Textures[i].FileName); bw.Write((byte)0); }
                while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;

                //crc32List
                for (int i = 0; i < Textures.Count; i++) { bw.Write(Textures[i].hashEntry.crc32); bw.Write(Textures[i].hashEntry.entryNr); }

                //texInfo 2 List
                for (int i = 0; i < Textures.Count; i++) bw.Write(Textures[i].texInfo);

                //Write data
                int texSecSize = 0;
                for (int i = 0; i < Textures.Count; i++)
                {
                    bw.Write(new BinaryReaderX(Textures[i].FileData, true).ReadBytes((int)Textures[i].FileData.Length));
                    texSecSize += (int)Textures[i].FileData.Length;
                }
                bw.BaseStream.Position = 0xc;
                bw.Write(texSecSize);
            }
        }*/
    }
}
