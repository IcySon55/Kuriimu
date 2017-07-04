using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kuriimu.IO;

//Code ported from Ohana3DS

namespace image_nintendo.BCH
{
    public sealed class BCH
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        Header header;
        List<TexEntry> entries = new List<TexEntry>();

        Stream _stream = null;

        public BCH(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                _stream = br.BaseStream;

                //Header
                header = new Header(br.BaseStream);

                if (header.dataSize != 0)
                {
                    //get entrySize
                    uint entrySize = 0;
                    br.BaseStream.Position = header.gpuCommandsOffset;
                    var wh = br.ReadBytes(4);
                    while (!br.ReadBytes(4).SequenceEqual(wh)) entrySize += 4;
                    entrySize += 4;

                    //get entryCount
                    int entryCount = 0;
                    for (uint o = header.gpuCommandsOffset; o < header.gpuCommandsOffset + header.gpuCommandsSize;)
                    {
                        br.BaseStream.Position = o;

                        bool legit = true;
                        byte[] check;
                        var count = 0;
                        do
                        {
                            if (count >= entrySize)
                            {
                                legit = false;
                                break;
                            }
                            check = br.ReadBytes(4);
                            count += 4;
                        } while (!check.SequenceEqual(new byte[] { 0x85, 0, 0xf, 0 }));

                        if (legit)
                        {
                            entries.Add(new TexEntry
                            {
                                entrySize = entrySize
                            });
                            entryCount++;
                            o += 3 * entrySize;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //Texture Entries
                    for (int o = (int)header.gpuCommandsOffset, i = 0; i < entryCount; i++)
                    {
                        br.BaseStream.Position = o;

                        var height = br.ReadUInt16();
                        var width = br.ReadUInt16();

                        byte[] check;
                        do { check = br.ReadBytes(4); }
                        while (!check.SequenceEqual(new byte[] { 0x85, 0, 0xf, 0 }));
                        Format format = (Format)br.ReadByte();

                        entries[i].width = width;
                        entries[i].height = height;
                        entries[i].format = format;

                        o += 3 * (int)entrySize;
                    }

                    //Textures
                    br.BaseStream.Position = header.dataOffset;
                    for (int i = 0; i < entryCount; i++)
                    {
                        int bitDepth = 0;
                        switch (entries[i].format)
                        {
                            case Format.RGBA8888:
                                bitDepth = 32;
                                break;
                            case Format.RGB888:
                                bitDepth = 24;
                                break;
                            case Format.RGBA5551:
                            case Format.RGB565:
                            case Format.RGBA4444:
                            case Format.LA88:
                            case Format.HL88:
                                bitDepth = 16;
                                break;
                            case Format.L8:
                            case Format.A8:
                            case Format.LA44:
                            case Format.ETC1A4:
                                bitDepth = 8;
                                break;
                            case Format.L4:
                            case Format.A4:
                            case Format.ETC1:
                                bitDepth = 4;
                                break;
                        }

                        var settings = new ImageSettings
                        {
                            Width = entries[i].width,
                            Height = entries[i].height,
                            Format = entries[i].format,
                            PadToPowerOf2 = false
                        };

                        bmps.Add(Common.Load(br.ReadBytes(((entries[i].width * entries[i].height) * bitDepth) / 8), settings));
                    }
                }
            }
        }

        public void Save(string filename)
        {
            var count = 0;
            foreach (var bmp in bmps)
            {
                if (entries[count].width != bmp.Width || entries[count].height != bmp.Height)
                    throw new Exception("BCH textures have to be the same size");
            }

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                _stream.Position = 0;
                _stream.CopyTo(bw.BaseStream);

                bw.BaseStream.Position = header.dataOffset;
                for (int i = 0; i < entries.Count(); i++)
                {
                    var settings = new ImageSettings
                    {
                        Width = entries[i].width,
                        Height = entries[i].height,
                        Format = entries[i].format,
                        PadToPowerOf2 = false
                    };
                    bw.Write(Common.Save(bmps[i], settings));
                }
            }
        }
    }
}
