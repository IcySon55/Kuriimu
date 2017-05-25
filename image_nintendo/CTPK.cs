using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kuriimu.IO;

namespace image_ctpk
{
    public sealed class CTPK
    {
        private CTPKHeader header;
        private List<CTPKEntry> entries;
        private List<int> texSizeList;
        private List<string> nameList;
        private List<CTPKHashEntry> crc32List;
        private List<uint> texInfoList2;
        private byte[] rest;

        public Bitmap bmp;
        bool isRaw = false;

        public CTPK(string filename, bool isRaw = false)
        {
            if (isRaw)
                GetRaw(filename);
            else
                using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
                {
                    //Header
                    header = br.ReadStruct<CTPKHeader>();

                    //TexEntries
                    entries = new List<CTPKEntry>();
                    entries.AddRange(br.ReadMultiple<CTPKEntry>(header.texCount));

                    //TexInfo List
                    texSizeList = new List<int>();
                    texSizeList.AddRange(br.ReadMultiple<int>(header.texCount));

                    //Name List
                    nameList = new List<string>();
                    for (int i = 0; i < entries.Count; i++)
                        nameList.Add(br.ReadCStringA());

                    //Hash List
                    br.BaseStream.Position = header.crc32SecOffset;
                    crc32List = new List<CTPKHashEntry>();
                    crc32List.AddRange(br.ReadMultiple<CTPKHashEntry>(header.texCount).OrderBy(e => e.entryNr));

                    //TexInfo List 2
                    br.BaseStream.Position = header.texInfoOffset;
                    texInfoList2 = new List<uint>();
                    texInfoList2.AddRange(br.ReadMultiple<uint>(header.texCount));

                    br.BaseStream.Position = entries[0].texOffset + header.texSecOffset;
                    var settings = new ImageSettings
                    {
                        Width = entries[0].width,
                        Height = entries[0].height,
                        Format = ImageSettings.ConvertFormat(entries[0].imageFormat),
                    };
                    bmp = Common.Load(br.ReadBytes(entries[0].texDataSize), settings);

                    if (br.BaseStream.Position < br.BaseStream.Length)
                        rest = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                }
        }

        public int size;

        public void GetRaw(string filename)
        {
            isRaw = true;
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                int pixelCount = (int)br.BaseStream.Length / 2;

                int count = 1;
                bool found = false;
                while (!found && count * count < br.BaseStream.Length)
                    if (pixelCount / count == count)
                        found = true;
                    else
                        count++;

                if (found)
                {
                    size = count;
                    var settings = new ImageSettings
                    {
                        Width = size,
                        Height = size,
                        Format = Format.RGB565,
                        PadToPowerOf2 = false
                    };
                    bmp = Common.Load(br.ReadBytes((int)br.BaseStream.Length), settings);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void Save(string filename)
        {
            if (isRaw)
                SaveRaw(filename);
            else
                using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
                {
                    var settings = new ImageSettings
                    {
                        Width = bmp.Width,
                        Height = bmp.Height,
                        Format = ImageSettings.ConvertFormat(entries[0].imageFormat),
                        PadToPowerOf2 = false
                    };
                    byte[] resBmp = Common.Save(bmp, settings);

                    int diff = resBmp.Length - entries[0].texDataSize;
                    entries[0].width = (short)bmp.Width;
                    entries[0].height = (short)bmp.Height;
                    entries[0].texDataSize = resBmp.Length;
                    for (int i = 1; i < header.texCount; i++)
                        entries[i].texOffset -= diff;

                    texSizeList[0] = resBmp.Length;

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

                    //write texData
                    bw.BaseStream.Position = header.texSecOffset;
                    bw.Write(resBmp);
                    bw.Write(rest);

                    header.texSecSize = (int)bw.BaseStream.Length - header.texSecOffset;
                    bw.BaseStream.Position = 0;
                    bw.WriteStruct(header);
                }
        }

        public void SaveRaw(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                var settings = new ImageSettings
                {
                    Width = size,
                    Height = size,
                    Format = Format.RGB565,
                    PadToPowerOf2 = false
                };
                bw.Write(Common.Save(bmp, settings));
            }
        }
    }
}
