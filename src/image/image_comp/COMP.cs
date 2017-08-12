using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.IO;

namespace image_comp
{
    public class COMP
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public Header header;

        public COMP(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //add image
                br.BaseStream.Position = 0x10;
                var settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = header.format,
                    PadToPowerOf2 = false
                };
                bmps.Add(Common.Load(br.ReadBytes((int)header.dataSize), settings));
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //get texture
                var settings = new ImageSettings
                {
                    Width = bmps[0].Width,
                    Height = bmps[0].Height,
                    Format = header.format,
                    PadToPowerOf2 = false
                };
                byte[] tex = Common.Save(bmps[0], settings);

                //Write header
                header.width = (short)bmps[0].Width;
                header.height = (short)bmps[0].Height;
                bw.WriteStruct(header);
                bw.WriteAlignment();

                //Write image data
                bw.Write(tex);
            }
        }
    }
}
