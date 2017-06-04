using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Contract;
using Cetera.Image;

namespace image_nintendo.CGFX
{
    public sealed class CGFX
    {
        public List<Bitmap> bmps = new List<Bitmap>();

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
                var TxObs = new List<TxobEntry>();
                for (int i = 0; i < dictHeader.entryCount; i++)
                {
                    br.BaseStream.Position = dictEntries[i].dataOffset;
                    TxObs.Add(new TxobEntry(br.BaseStream));
                }

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

        }
    }
}
