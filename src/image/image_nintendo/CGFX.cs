using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Kontract;
using Cetera.Image;

namespace image_nintendo.CGFX
{
    public sealed class CGFX
    {
        List<TxobEntry> TxObs = new List<TxobEntry>();
        public List<Bitmap> bmps = new List<Bitmap>();

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
                    var settings = new ImageSettings
                    {
                        Width = (int)TxObs[i].width,
                        Height = (int)TxObs[i].height,
                        Format = ImageSettings.ConvertFormat(TxObs[i].format),
                        PadToPowerOf2 = false
                    };
                    bmps.Add(Common.Load(br.ReadBytes((int)TxObs[i].texDataSize), settings));
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

                for (int i = 0; i < TxObs.Count; i++)
                {
                    bw.BaseStream.Position = TxObs[i].texDataOffset;
                    var settings = new ImageSettings
                    {
                        Width = (int)TxObs[i].width,
                        Height = (int)TxObs[i].height,
                        Format = ImageSettings.ConvertFormat(TxObs[i].format),
                        PadToPowerOf2 = false
                    };

                    bw.Write(Common.Save(bmps[i], settings));
                }
            }
        }
    }
}
