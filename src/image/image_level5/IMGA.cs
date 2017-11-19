using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.Image.Swizzle;
using Komponent.Image;
using Komponent.IO;

namespace image_level5.imga
{
    public class IMGA
    {
        public Bitmap bmp;

        public static Header header;
        private Import imports = new Import();

        public IMGA(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();
                header.checkFormat();

                //get tile table
                br.BaseStream.Position = header.tableDataOffset;
                byte[] table = imports.level5.Decompress(new MemoryStream(br.ReadBytes(header.tableSize1)), 0);

                //get image data
                br.BaseStream.Position = header.tableDataOffset + header.tableSize2;
                byte[] tex = imports.level5.Decompress(new MemoryStream(br.ReadBytes(header.imgDataSize)), 0);

                //order pic blocks by table
                byte[] pic = Order(new MemoryStream(table), new MemoryStream(tex));

                //return finished image
                var settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = Support.Format[header.imageFormat],
                    Swizzle = new CTRSwizzle(header.width, header.height)
                };
                bmp = Common.Load(pic, settings);
            }
        }

        public static byte[] Order(MemoryStream tableStream, MemoryStream texStream)
        {
            using (var table = new BinaryReaderX(tableStream))
            using (var tex = new BinaryReaderX(texStream))
            {
                int tableLength = (int)table.BaseStream.Length;

                var ms = new MemoryStream();
                for (int i = 0; i < tableLength; i += 2)
                {
                    int entry = table.ReadUInt16();
                    if (entry == 0xFFFF)
                    {
                        for (int j = 0; j < 64 * header.bitDepth / 8; j++)
                        {
                            ms.WriteByte(0);
                        }
                    }
                    else
                    {
                        tex.BaseStream.Position = entry * (64 * header.bitDepth / 8);
                        for (int j = 0; j < 64 * header.bitDepth / 8; j++)
                        {
                            ms.WriteByte(tex.ReadByte());
                        }
                    }
                }
                return ms.ToArray();
            }
        }

        public void Save(string filename)
        {
            int width = (bmp.Width + 0x7) & ~0x7;
            int height = (bmp.Height + 0x7) & ~0x7;

            var settings = new ImageSettings
            {
                Width = width,
                Height = height,
                Format = Support.Format[header.imageFormat],
                Swizzle = new CTRSwizzle(width, height)
            };
            byte[] pic = Common.Save(bmp, settings);

            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //Header
                header.bitDepth = 32;
                header.width = (short)width;
                header.height = (short)height;

                //tile table
                List<short> table;
                byte[] importPic = Deflate(pic, out table);

                header.width = (short)bmp.Width;
                header.height = (short)bmp.Height;
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

        public static byte[] Deflate(byte[] pic, out List<short> table)
        {
            table = new List<short>();
            List<byte[]> parts = new List<byte[]>();
            byte[] result = new byte[header.width * header.height * header.bitDepth / 8];

            using (BinaryReaderX br = new BinaryReaderX(new MemoryStream(pic)))
            {
                for (int i = 0; i < header.width * header.height / 64; i++)
                {
                    byte[] tmp = br.ReadBytes(64 * header.bitDepth / 8);
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
