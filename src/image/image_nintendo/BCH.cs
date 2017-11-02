using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kontract.IO;
using Cetera.PICA;

//Code ported from Ohana3DS

namespace image_nintendo.BCH
{
    public sealed class BCH
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        Header header;
        List<PICACommandReader> picaEntries = new List<PICACommandReader>();
        List<TexEntry> origValues = new List<TexEntry>();

        byte[] _file = null;

        public BCH(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                _file = br.ReadAllBytes();

                //Header
                header = new Header(br.BaseStream);

                if (header.dataSize != 0)
                {
                    //get entryCount
                    br.BaseStream.Position = header.gpuCommandsOffset;
                    int entryCount = 0;
                    List<uint> wordCount = new List<uint>();
                    wordCount.Add(0);
                    using (var br2 = new BinaryReaderX(new MemoryStream(br.ReadBytes((int)header.gpuCommandsSize))))
                    {
                        while (br2.BaseStream.Position < br2.BaseStream.Length - 2)
                        {
                            br2.BaseStream.Position += 4;
                            var comp = br2.ReadUInt16();
                            wordCount[wordCount.Count - 1] += 2;
                            if (comp == 0x23d)
                            {
                                entryCount++;
                                wordCount.Add(0);
                            }
                            br2.BaseStream.Position += 2;
                        }
                    }

                    //get commands
                    br.BaseStream.Position = header.gpuCommandsOffset;
                    for (int i = 0; i < entryCount; i++)
                    {
                        picaEntries.Add(new PICACommandReader(br.BaseStream, wordCount[i]));
                    }

                    //loop through commandReaders to get textures
                    br.BaseStream.Position = header.dataOffset;
                    for (int i = 0; i < entryCount; i++)
                    {
                        var size = picaEntries[i].getTexUnit0Size();
                        if (size.Height != 0 && size.Width != 0)
                        {
                            var format = (Format)picaEntries[i].getTexUnit0Format();
                            int bitDepth = Common.GetBitDepth(format);

                            for (int j = 0; j <= picaEntries[i].getTexUnit0LoD(); j++)
                            {
                                origValues.Add(new TexEntry(size.Width >> j, size.Height >> j, format));

                                var settings = new ImageSettings
                                {
                                    Width = size.Width >> j,
                                    Height = size.Height >> j,
                                    Format = format,
                                    PadToPowerOf2 = false
                                };

                                bmps.Add(Common.Load(br.ReadBytes((((size.Width >> j) * (size.Height >> j)) * bitDepth) / 8), settings));
                            }

                            br.BaseStream.Position = (br.BaseStream.Position + 0x7f) & ~0x7f;
                        }
                    }
                }
            }
        }

        public void Save(string filename)
        {
            for (int i = 0; i < bmps.Count(); i++)
            {
                if (origValues[i].width != bmps[i].Width || origValues[i].height != bmps[i].Height)
                    throw new Exception("All BCH textures have to be the same size as the original!");
            }

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.Write(_file);

                bw.BaseStream.Position = header.dataOffset;
                for (int i = 0; i < bmps.Count(); i++)
                {
                    var settings = new ImageSettings
                    {
                        Width = bmps[i].Width,
                        Height = bmps[i].Height,
                        Format = origValues[i].format,
                        PadToPowerOf2 = false
                    };
                    bw.Write(Common.Save(bmps[i], settings));

                    bw.WriteAlignment(0x80);
                }
            }
        }
    }
}
