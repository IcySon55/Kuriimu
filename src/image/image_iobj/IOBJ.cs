using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.IO;
using System.Text;
using System.Linq;

namespace image_iobj
{
    public class IOBJ
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        List<byte[]> table1Entries = new List<byte[]>();
        PTGTHeader ptgtHeader;
        byte[] ptgtEntries;
        //List<PTGTEntry> ptgtEntries = new List<PTGTEntry>();
        List<byte[]> imgMetaInf = new List<byte[]>();

        public IOBJ(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                var header = br.ReadStruct<Header>();
                var imgCount = br.ReadInt32();
                var imgOffsets = br.ReadMultiple<int>(imgCount);

                //Table1
                br.BaseStream.Position = header.table1Offset;
                var t1entryCount = br.ReadInt32();
                var offsets = br.ReadMultiple<int>(t1entryCount);
                for (int i = 0; i < offsets.Count; i++)
                {
                    br.BaseStream.Position = header.table1Offset + offsets[i];
                    table1Entries.Add(br.ReadBytes((i + 1 < offsets.Count) ? offsets[i + 1] - offsets[i] : header.PtgtOffset - (offsets[i] + header.table1Offset)));
                }

                //PTGT
                br.BaseStream.Position = header.PtgtOffset;
                ptgtHeader = br.ReadStruct<PTGTHeader>();
                //var entryCount = br.ReadInt32();
                //var ptgtEntriesL = br.ReadMultiple<PTGTOffEntry>(entryCount);
                ptgtEntries = br.ReadBytes(imgOffsets[0] - (header.PtgtOffset + 8));
                /*foreach (var entry in ptgtEntriesL)
                {
                    br.BaseStream.Position = header.PtgtOffset + entry.offset + 0xc;
                    ptgtEntries.Add(new PTGTEntry
                    {
                        offsetEntry = entry,
                        floats = br.ReadMultiple<float>(0x10).ToArray()
                    });
                }*/

                //Images
                for (int i = 0; i < imgCount; i++)
                {
                    br.BaseStream.Position = imgOffsets[i];
                    var dataSize = br.ReadInt32();
                    var width = br.ReadInt32();
                    var height = br.ReadInt32();
                    var format = (Format)br.ReadInt32();

                    imgMetaInf.Add(br.ReadBytes(0x70));

                    var settings = new ImageSettings
                    {
                        Width = width,
                        Height = height,
                        Orientation = Orientation.XFlip,
                        Format = ImageSettings.ConvertFormat(((int)format > 0x12) ? Format.ETC1A4 : format)
                    };

                    bmps.Add(Common.Load(br.ReadBytes(dataSize), settings));
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                var table1Offset = 0x10 + bmps.Count * 0x4;
                var ptgtTableOffset = table1Offset + 0x4 + table1Entries.Count * 0x4 + table1Entries.SelectMany(x => x.SelectMany(b => new[] { b })).Count();
                var imgDataSize = (ptgtTableOffset + 0x8 + ptgtEntries.Length + 0x7f) & ~0x7f;//ptgtEntries.Count * 0x8 + ptgtEntries.Count * 0xe + 0x7f) & ~0x7f;

                //Header
                bw.Write(Encoding.ASCII.GetBytes("IOBJ"));
                bw.Write(table1Offset);
                bw.Write(ptgtTableOffset);
                bw.Write(bmps.Count);

                //Table1
                bw.BaseStream.Position = table1Offset;
                bw.Write(table1Entries.Count);
                var offset = 0x4 + table1Entries.Count * 0x4;
                foreach (var entry in table1Entries)
                {
                    bw.Write(offset);
                    offset += entry.Length;
                }
                foreach (var entry in table1Entries)
                    bw.Write(entry);

                //PTGT
                bw.WriteStruct(ptgtHeader);
                bw.Write(ptgtEntries);
                /*bw.Write(ptgtEntries.Count);
                foreach (var entry in ptgtEntries)
                    bw.WriteStruct(entry.offsetEntry);
                foreach (var entry in ptgtEntries)
                    foreach (var floatEntry in entry.floats)
                        bw.Write(floatEntry);
                bw.WriteAlignment(0x80);*/

                //Images
                var imgOffsets = new List<int>();
                var count = 0;
                foreach (var bmp in bmps)
                {
                    imgOffsets.Add((int)bw.BaseStream.Position);

                    Format format;
                    using (var br = new BinaryReaderX(new MemoryStream(imgMetaInf[count])))
                    {
                        br.BaseStream.Position = 0xc;
                        format = (Format)br.ReadInt32();
                    }

                    var settings = new ImageSettings
                    {
                        Width = bmp.Width,
                        Height = bmp.Height,
                        Orientation = Orientation.XFlip,
                        Format = ImageSettings.ConvertFormat(((int)format > 0x12) ? Format.ETC1A4 : format)
                    };

                    var pic = Common.Save(bmp, settings);

                    bw.Write(pic.Length);
                    bw.Write(bmp.Width);
                    bw.Write(bmp.Height);
                    bw.Write((int)format);
                    bw.Write(imgMetaInf[count++]);

                    bw.Write(pic);
                    bw.WriteAlignment(0x80);
                }

                //imgOffsets in Header
                bw.BaseStream.Position = 0x10;
                foreach (var imgOff in imgOffsets)
                    bw.Write(imgOff);
            }
        }
    }
}
