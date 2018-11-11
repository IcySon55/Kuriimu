using System.Drawing;
using System.Collections.Generic;
using System.IO;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.Interface;
using Kontract.IO;
using System;

namespace image_g1t
{
    class G1T
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public List<ImageSettings> settings = new List<ImageSettings>();

        private ByteOrder ByteOrder;
        private Header header;
        private List<Meta> meta = new List<Meta>();
        private List<byte[]> metaExt = new List<byte[]>();

        private Platform _platform;

        public G1T(Stream input, Platform platform)
        {
            _platform = platform;

            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                //Deciding on Endianess
                var magic1 = br.ReadString(4);
                br.ByteOrder = ByteOrder = (magic1 == "GT1G") ? ByteOrder.LittleEndian : ByteOrder.BigEndian;
                br.BaseStream.Position = 0;

                //Header
                header = br.ReadStruct<Header>();

                //OffsetList
                br.BaseStream.Position = header.dataOffset;
                var offsetList = br.ReadMultiple<int>(header.texCount);

                //Meta
                meta = new List<Meta>();
                for (int i = 0; i < header.texCount; i++)
                {
                    br.BaseStream.Position = header.dataOffset + offsetList[i];
                    var metainfo = br.ReadStruct<Meta>();
                    meta.Add(metainfo);
                    byte[] ext = null;
                    if (metainfo.extHeader > 0)
                    {
                        var extSize = br.ReadInt32();
                        ext = br.ReadBytes(extSize - 4);
                    }
                    metaExt.Add(ext);

                    var format = (_platform == Platform.Vita) ? Support.VitaFormat[meta[i].format] : (_platform == Platform.N3DS) ? Support.N3DSFormat[meta[i].format] : Support.PSFormat[meta[i].format];

                    //Check if format exists
                    switch (_platform)
                    {
                        case Platform.N3DS:
                            if (!Support.N3DSFormat.ContainsKey(metainfo.format))
                                throw new Exception($"Unsupported image format 0x{metainfo.format:X2}.");
                            break;
                        case Platform.PS:
                            if (!Support.PSFormat.ContainsKey(metainfo.format))
                                throw new Exception($"Unsupported image format 0x{metainfo.format:X2}.");
                            break;
                        case Platform.Vita:
                            if (!Support.VitaFormat.ContainsKey(metainfo.format))
                                throw new Exception($"Unsupported image format 0x{metainfo.format:X2}.");
                            break;
                    }

                    IImageSwizzle swizzle = null;
                    if (_platform == Platform.Vita) swizzle = new VitaSwizzle(metainfo.width, metainfo.height, format.FormatName.Contains("DXT"));
                    else if (_platform == Platform.N3DS) swizzle = new CTRSwizzle(metainfo.width, metainfo.height, 2);
                    else if (format.FormatName.Contains("DXT")) swizzle = new BlockSwizzle(metainfo.width, metainfo.height);

                    var setting = new ImageSettings
                    {
                        Width = metainfo.width,
                        Height = metainfo.height,
                        Swizzle = swizzle,
                        Format = format
                    };
                    settings.Add(setting);

                    bmps.Add(Common.Load(br.ReadBytes(metainfo.width * metainfo.height * format.BitDepth / 8), setting));
                }
            }
        }

        public void Save(Stream input)
        {
            //Sanity check
            for (int i = 0; i < bmps.Count; i++)
            {
                var padWidth = 2 << (int)Math.Log(bmps[i].Width - 1, 2);
                var padHeight = 2 << (int)Math.Log(bmps[i].Height - 1, 2);
                if (padWidth >= Math.Pow(2, 16) || padHeight >= Math.Pow(2, 16))
                    throw new Exception($"Image {i} has to be smaller than {Math.Pow(2, 15)}x{Math.Pow(2, 15)}");
            }

            using (BinaryWriterX bw = new BinaryWriterX(input, ByteOrder))
            {
                //Create offsetlist
                var offsetList = new List<int>();
                var off = header.texCount * 4;
                for (int i = 0; i < header.texCount; i++)
                {
                    var format = (_platform == Platform.Vita) ? Support.VitaFormat[meta[i].format] : (_platform == Platform.N3DS) ? Support.N3DSFormat[meta[i].format] : Support.PSFormat[meta[i].format];

                    offsetList.Add(off);
                    off += 0x8 + ((metaExt[i] != null) ? metaExt[i].Length + 4 : 0);
                    off += bmps[i].Width * bmps[i].Height * format.BitDepth / 8;
                }

                //Update meta
                for (int i = 0; i < header.texCount; i++)
                    meta[i].dimension = (byte)((int)(Math.Log(bmps[i].Width - 1, 2) + 1) | ((int)(Math.Log(bmps[i].Height - 1, 2) + 1) * 16));

                //Write updated data
                bw.BaseStream.Position = header.dataOffset;
                foreach (var offInt in offsetList) bw.Write(offInt);

                //Write images
                for (int i = 0; i < bmps.Count; i++)
                {
                    var format = (_platform == Platform.Vita) ? Support.VitaFormat[meta[i].format] : (_platform == Platform.N3DS) ? Support.N3DSFormat[meta[i].format] : Support.PSFormat[meta[i].format];

                    bw.WriteStruct(meta[i]);
                    if (metaExt[i] != null)
                    {
                        bw.Write(metaExt[i].Length + 4);
                        bw.Write(metaExt[i]);
                    }

                    IImageSwizzle swizzle = null;
                    if (_platform == Platform.Vita) swizzle = new VitaSwizzle(2 << (int)Math.Log(bmps[i].Width - 1, 2), 2 << (int)Math.Log(bmps[i].Height - 1, 2), format.FormatName.Contains("DXT"));
                    else if (_platform == Platform.N3DS) swizzle = new CTRSwizzle(2 << (int)Math.Log(bmps[i].Width - 1, 2), 2 << (int)Math.Log(bmps[i].Height - 1, 2), 2);
                    else if (format.FormatName.Contains("DXT")) swizzle = new BlockSwizzle(2 << (int)Math.Log(bmps[i].Width - 1, 2), 2 << (int)Math.Log(bmps[i].Height - 1, 2));

                    var setting = new ImageSettings
                    {
                        Width = 2 << (int)Math.Log(bmps[i].Width - 1, 2),
                        Height = 2 << (int)Math.Log(bmps[i].Height - 1, 2),
                        Swizzle = swizzle,
                        Format = format
                    };
                    bw.Write(Common.Save(bmps[i], setting));
                }
                header.fileSize = (int)bw.BaseStream.Length;

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }
    }
}
