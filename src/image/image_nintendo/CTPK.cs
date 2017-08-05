using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kuriimu.IO;

namespace image_nintendo.CTPK
{
    public sealed class CTPK
    {
        private Header header;
        private List<Entry> entries;
        private List<int> texSizeList;
        private List<string> nameList;
        private List<HashEntry> crc32List;
        private List<uint> texInfoList2;
        private byte[] rest;

        public List<Bitmap> bmps = new List<Bitmap>();

        public CTPK(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();
                br.BaseStream.Position = br.BaseStream.Position + 0xf & ~0xf;

                //TexEntries
                entries = br.ReadMultiple<Entry>(header.texCount);

                //TexInfo List
                texSizeList = br.ReadMultiple<int>(header.texCount);

                //Name List
                nameList = new List<string>();
                for (int i = 0; i < entries.Count; i++)
                    nameList.Add(br.ReadCStringA());

                //Hash List
                br.BaseStream.Position = header.crc32SecOffset;
                crc32List = br.ReadMultiple<HashEntry>(header.texCount).OrderBy(e => e.entryNr).ToList();

                //TexInfo List 2
                br.BaseStream.Position = header.texInfoOffset;
                texInfoList2 = br.ReadMultiple<uint>(header.texCount);

                for (int i = 0; i < entries.Count; i++)
                {
                    br.BaseStream.Position = entries[i].texOffset + header.texSecOffset;
                    var settings = new ImageSettings
                    {
                        Width = entries[i].width,
                        Height = entries[i].height,
                        Format = ImageSettings.ConvertFormat(entries[i].imageFormat),
                    };
                    bmps.Add(Common.Load(br.ReadBytes(entries[i].texDataSize), settings));
                }

                if (br.BaseStream.Position < br.BaseStream.Length)
                    rest = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                var texOffset = 0;
                for (int i = 0; i < bmps.Count; i++)
                {
                    var settings = new ImageSettings
                    {
                        Width = bmps[i].Width,
                        Height = bmps[i].Height,
                        Format = ImageSettings.ConvertFormat(entries[i].imageFormat),
                        PadToPowerOf2 = false
                    };
                    byte[] resBmp = Common.Save(bmps[i], settings);

                    int diff = resBmp.Length - entries[i].texDataSize;
                    entries[i].width = (short)bmps[i].Width;
                    entries[i].height = (short)bmps[i].Height;
                    entries[i].texDataSize = resBmp.Length;
                    entries[i].texOffset = texOffset;

                    texSizeList[i] = resBmp.Length;

                    bw.BaseStream.Position = header.texSecOffset + texOffset;
                    bw.Write(resBmp);

                    texOffset += resBmp.Length;
                }

                //write entries
                bw.BaseStream.Position = 0x20;
                for (int i = 0; i < header.texCount; i++) bw.WriteStruct(entries[i]);

                //write texSizeInfo
                for (int i = 0; i < header.texCount; i++) bw.Write(texSizeList[i]);

                //write names
                for (int i = 0; i < header.texCount; i++)
                {
                    bw.WriteASCII(nameList[i]);
                    bw.Write((byte)0);
                }

                //write hashes
                crc32List = crc32List.OrderBy(e => e.crc32).ToList();
                bw.BaseStream.Position = header.crc32SecOffset;
                for (int i = 0; i < header.texCount; i++)
                {
                    bw.Write(crc32List[i].crc32);
                    bw.Write(crc32List[i].entryNr);
                }

                //write texInfo
                bw.BaseStream.Position = header.texInfoOffset;
                for (int i = 0; i < header.texCount; i++) bw.Write(texInfoList2[i]);

                header.texSecSize = (int)bw.BaseStream.Length - header.texSecOffset;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }
    }
}
