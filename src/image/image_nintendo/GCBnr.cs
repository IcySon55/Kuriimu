using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.IO;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;

namespace image_nintendo.GCBnr
{
    public sealed class GCBnr
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        ImageSettings settings;

        Header header;
        byte[] misc;

        public GCBnr(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Data
                var imgData = br.ReadBytes(0x1800);
                settings = new ImageSettings
                {
                    Width = 96,
                    Height = 32,
                    Format = new GCBnrSupport.GC_5A3(),
                    Swizzle = new GCBnrSupport.GCSwizzle(96, 32)
                };
                bmps.Add(Common.Load(imgData, settings));

                //Misc
                misc = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
            }
        }

        public void Save(string filename)
        {
            if (bmps[0].Width != 96 || bmps[0].Height != 32)
                throw new Exception("The banner has to be 96x32!");

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                //Header
                bw.WriteStruct(header);

                //Image
                var imgData = Common.Save(bmps[0], settings);
                bw.Write(imgData);

                //Misc
                bw.Write(misc);
            }
        }
    }
}
