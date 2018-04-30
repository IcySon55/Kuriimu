using System.Drawing;
using System.Collections.Generic;
using System.IO;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;
using System;

namespace image_g1t
{
    class G1T
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public List<ImageSettings> settings = new List<ImageSettings>();

        private Header header;
        private List<Meta> meta = new List<Meta>();

        public G1T(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
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

                    var setting = new ImageSettings
                    {
                        Width = metainfo.width,
                        Height = metainfo.height,
                        Swizzle = new G1TSwizzle(metainfo.width, metainfo.height, Support.Format[metainfo.format].FormatName.Contains("DXT") || Support.Format[metainfo.format].FormatName.Contains("PVR")),
                        Format = Support.Format[metainfo.format]
                    };
                    settings.Add(setting);

                    bmps.Add(Common.Load(br.ReadBytes(metainfo.width * metainfo.height * Support.Format[metainfo.format].BitDepth / 8), setting));
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

            using (BinaryWriterX bw = new BinaryWriterX(input))
            {
                //Create offsetlist
                var offsetList = new List<int>();
                for (int i = 0; i < header.texCount; i++)
                    offsetList.Add(header.texCount * 4 + i * 0x10);

                //Update meta
                for (int i = 0; i < header.texCount; i++)
                    meta[i].dimension = (byte)((int)(Math.Log(bmps[i].Width - 1, 2) + 1) | ((int)(Math.Log(bmps[i].Height - 1, 2) + 1) * 16));

                //Write updated data
                bw.BaseStream.Position = 0x20;
                foreach (var off in offsetList) bw.Write(off);
                foreach (var m in meta) bw.WriteStruct(m);

                //Write images
                for (int i = 0; i < bmps.Count; i++)
                {
                    var setting = new ImageSettings
                    {
                        Width = 2 << (int)Math.Log(bmps[i].Width - 1, 2),
                        Height = 2 << (int)Math.Log(bmps[i].Height - 1, 2),
                        Swizzle = new G1TSwizzle(2 << (int)Math.Log(bmps[i].Width - 1, 2), 2 << (int)Math.Log(bmps[i].Height - 1, 2), Support.Format[meta[i].format].FormatName.Contains("DXT") || Support.Format[meta[i].format].FormatName.Contains("PVR")),
                        Format = Support.Format[meta[i].format]
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
