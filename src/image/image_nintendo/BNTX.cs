using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Image.Format;
using Kontract.Interface;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;
using System.Linq;

namespace image_nintendo.BNTX
{
    public sealed class BNTX
    {
        public List<BitmapMeta> Images = new List<BitmapMeta>();
        public ImageSettings Settings { get; set; }

        private ByteOrder byteOrder { get; set; }
        public BNTXImageHeader BNTXHeader { get; private set; }
        public NXImageHeader NXHeader { get; private set; }
        public List<BRTIEntry> brtiEntries = new List<BRTIEntry>();

        public BNTX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //BNTX Header
                BNTXHeader = br.ReadStruct<BNTXImageHeader>();
                br.ByteOrder = BNTXHeader.bom;

                //NX Header
                NXHeader = br.ReadStruct<NXImageHeader>();

                //Image Meta
                br.BaseStream.Position = NXHeader.infoPtrAddr;
                var metaOffsets = br.ReadMultiple<long>(NXHeader.imgCount);
                foreach (var offset in metaOffsets)
                {
                    br.BaseStream.Position = offset;
                    brtiEntries.Add(br.ReadStruct<BRTIEntry>());
                }

                //Images
                foreach (var entry in brtiEntries)
                {
                    br.BaseStream.Position = entry.nameAddr + 2;
                    var texName = br.ReadCStringA();
                    for (int i = 0; i < entry.mipCount; i++)
                    {
                        try
                        {
                            var settings = new ImageSettings()
                            {
                                Width = entry.width >> i,
                                Height = entry.height >> i,
                                Format = BntxSupport.Formats[(byte)(entry.format >> 8)],
                                Swizzle = new SwitchSwizzle(entry.width >> i, entry.height >> i, BntxSupport.Formats[(byte)(entry.format >> 8)].BitDepth, (SwitchSwizzle.Format)(entry.format >> 8))
                            };
                            br.BaseStream.Position = entry.ptrAddr;
                            br.BaseStream.Position = br.ReadInt64();

                            Images.Add(new BitmapMeta
                            {
                                bmp = Common.Load(br.ReadBytes(settings.Width * settings.Height * settings.Format.BitDepth / 8), settings),
                                name = texName + $" (Mipmap {i + 1})",
                                format = settings.Format.FormatName
                            });
                        }
                        catch (Exception e)
                        {
                            ;
                        }
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, byteOrder))
            {
            }
        }
    }
}
