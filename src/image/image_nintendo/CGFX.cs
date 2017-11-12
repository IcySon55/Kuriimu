using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kontract.IO;
using Kontract.Interface;
using Kontract.Image;
using Kontract.Image.Swizzle;

namespace image_nintendo.CGFX
{
    public sealed class CGFX
    {
        List<TxobEntry> TxObs = new List<TxobEntry>();
        public List<Bitmap> bmps = new List<Bitmap>();

        public List<ImageSettings> _settings = new List<ImageSettings>();

        byte[] list;

        public CGFX(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                //CGFX Header
                var cgfxHeader = br.ReadStruct<CgfxHeader>();

                //Data entries
                List<DataEntry> dataEntries = new List<DataEntry>();
                for (int i = 0; i < cgfxHeader.entryCount; i++)
                {
                    var dataHeader = br.ReadStruct<DataHeader>();
                    if (dataHeader.magic == "DATA")
                    {
                        for (int j = 0; j < 16; j++)
                        {
                            dataEntries.Add(new DataEntry(br.BaseStream));
                        }
                    }
                }

                //TextureEntry
                br.BaseStream.Position = dataEntries[1].offset;
                var dictHeader = br.ReadStruct<DictHeader>();
                var dictEntries = new List<DictEntry>();
                for (int i = 0; i < dictHeader.entryCount; i++)
                {
                    dictEntries.Add(new DictEntry(br.BaseStream));
                }

                //TextureObjects
                for (int i = 0; i < dictHeader.entryCount; i++)
                {
                    br.BaseStream.Position = dictEntries[i].dataOffset;
                    TxObs.Add(new TxobEntry(br.BaseStream));
                }

                //save lists to RAM
                br.BaseStream.Position = 0;
                list = br.ReadBytes((int)TxObs[0].texDataOffset);

                //Add images
                for (int i = 0; i < dictHeader.entryCount; i++)
                {
                    br.BaseStream.Position = TxObs[i].texDataOffset;
                    var width = TxObs[i].height;
                    var height = TxObs[i].width;
                    for (int j = 0; j < ((TxObs[i].mipmapLvls == 0) ? 1 : TxObs[i].mipmapLvls); j++)
                    {
                        var settings = new ImageSettings
                        {
                            Width = (int)width,
                            Height = (int)height,
                            Format = Support.CTRFormat[(byte)TxObs[i].format],
                            Swizzle = new CTRSwizzle((int)width, (int)height, 4)
                        };

                        _settings.Add(settings);
                        bmps.Add(Common.Load(br.ReadBytes((int)(Support.CTRFormat[(byte)TxObs[i].format].BitDepth * width * height / 8)), settings));

                        width /= 2;
                        height /= 2;
                    }
                }
            }
        }

        public void Save(string filename)
        {
            //check original sizes
            for (int i = 0; i < TxObs.Count; i++)
            {
                if (bmps[i].Width != TxObs[i].width || bmps[i].Height != TxObs[i].height)
                    throw new Exception($"Image {i:00} has to be {TxObs[i].width}x{TxObs[i].height}px!");
            }

            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                bw.Write(list);

                var bmpCount = 0;
                for (int i = 0; i < TxObs.Count; i++)
                {
                    bw.BaseStream.Position = TxObs[i].texDataOffset;
                    var width = TxObs[i].height;
                    var height = TxObs[i].width;
                    for (int j = 0; j < ((TxObs[i].mipmapLvls == 0) ? 1 : TxObs[i].mipmapLvls); j++)
                    {
                        var settings = new ImageSettings
                        {
                            Width = (int)width,
                            Height = (int)height,
                            Format = Support.CTRFormat[(byte)TxObs[i].format],
                            Swizzle = new CTRSwizzle((int)width, (int)height, 4)
                        };

                        bw.Write(Common.Save(bmps[bmpCount++], settings));

                        width /= 2;
                        height /= 2;
                    }
                }
            }
        }
    }
}
