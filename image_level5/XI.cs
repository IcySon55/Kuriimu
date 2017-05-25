using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kuriimu.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_xi
{
    public class XI
    {
        public static byte[] Decomp(BinaryReaderX br)
        {
            // above to be restored eventually with some changes to Cetera
            return Level5.Decompress(br.BaseStream);
        }

        private static Header header;

        public static Compression tableComp = 0;
        public static Compression picComp = 0;

        public static Bitmap Load(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //check header
                header = br.ReadStruct<Header>();
                header.checkConst();

                //decompress table
                br.BaseStream.Position = header.tableDataOffset;
                tableComp = (Compression)(br.ReadUInt32() % 8);
                br.BaseStream.Position = header.tableDataOffset;
                byte[] table = Level5.Decompress(new MemoryStream(br.ReadBytes(header.tableSize1)));

                //get decompressed picture data
                br.BaseStream.Position = header.tableDataOffset + header.tableSize2;
                picComp = (Compression)(br.ReadUInt32() % 8);
                br.BaseStream.Position = header.tableDataOffset + header.tableSize2;
                byte[] tex = Level5.Decompress(new MemoryStream(br.ReadBytes(header.imgDataSize)));

                //order pic blocks by table
                byte[] pic = Order(new BinaryReaderX(new MemoryStream(table)), table.Length, new BinaryReaderX(new MemoryStream(tex)), header.width, header.height, header.bitDepth);

                //return decompressed picture data
                var settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Orientation = Orientation.TransposeTile,
                    Format = ImageSettings.ConvertFormat(header.imageFormat),
                    PadToPowerOf2 = false
                };
                return Common.Load(pic, settings);
            }
        }

        public static byte[] Order(BinaryReaderX table, int tableLength, BinaryReaderX tex, int w, int h, byte bitDepth)
        {
            var ms = new MemoryStream();
            for (int i = 0; i < tableLength; i += 2)
            {
                int entry = table.ReadUInt16();
                if (entry == 0xFFFF)
                {
                    for (int j = 0; j < 64 * bitDepth / 8; j++)
                    {
                        ms.WriteByte(0);
                    }
                }
                else
                {
                    tex.BaseStream.Position = entry * (64 * bitDepth / 8);
                    for (int j = 0; j < 64 * bitDepth / 8; j++)
                    {
                        ms.WriteByte(tex.ReadByte());
                    }
                }
            }
            return ms.ToArray();
        }

        public static void Save(string filename, Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            while (width % 8 != 0) width++;
            while (height % 8 != 0) height++;

            byte[] pic = Common.Save(bitmap, new ImageSettings
            {
                Width = width,
                Height = height,
                Orientation = Orientation.TransposeTile,
                Format = ImageSettings.ConvertFormat(header.imageFormat),
                PadToPowerOf2 = false
            });

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                List<short> table;
                byte[] importPic = Deflate(pic, out table, 32, width, height);

                header.width = (short)bitmap.Width;
                header.height = (short)bitmap.Height;
                header.tableSize1 = table.Count * 2 + 4;
                header.tableSize2 = (header.tableSize1 + 3) & ~3;
                header.imgDataSize = importPic.Length + 4;
                bw.WriteStruct(header);

                bw.Write(header.tableSize1 << 3);
                foreach (var tableEntry in table) bw.Write(tableEntry);

                bw.BaseStream.Position = 0x48 + header.tableSize2;
                bw.Write(header.imgDataSize << 3);
                bw.Write(importPic);
            }
        }

        public static byte[] Deflate(byte[] pic, out List<short> table, int bitDepth, int width, int height)
        {
            table = new List<short>();
            List<byte[]> parts = new List<byte[]>();
            byte[] result = new byte[width * height * bitDepth / 8];

            using (BinaryReaderX br = new BinaryReaderX(new MemoryStream(pic)))
            {
                for (int i = 0; i < width * height / 64; i++)
                {
                    byte[] tmp = br.ReadBytes(64 * bitDepth / 8);
                    bool found = false;
                    int count = 0;
                    while (found == false && count < parts.Count)
                    {
                        if (tmp.SequenceEqual(parts[count]))
                        {
                            found = true;
                            table.Add((short)count);
                        }
                        count++;
                    }
                    if (!found)
                    {
                        table.Add((short)parts.Count);
                        parts.Add(tmp);
                    }
                }

                int resOffset = 0;
                foreach (var part in parts) part.CopyTo(result, (resOffset += part.Length) - part.Length);
            }

            return result;
        }
    }
}
