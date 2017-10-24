using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kontract.Interface;
using Kontract.IO;

namespace image_ctx
{
    public class CTX
    {
        public Header header;

        public List<Bitmap> bmps = new List<Bitmap>();

        public CTX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Image
                br.BaseStream.Position = (br.BaseStream.Position + 0x3f) & ~0x3f;
                var settings = new ImageSettings
                {
                    Width = (int)header.width,
                    Height = (int)header.height,
                    Format = ImageSettings.ConvertFormat(header.format)
                };
                bmps.Add(Common.Load(br.ReadBytes((int)header.dataSize), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                //Header
                header.width = (uint)bmps[0].Width;
                header.width2 = (uint)bmps[0].Width;
                header.height = (uint)bmps[0].Height;
                header.height2 = (uint)bmps[0].Height;
                bw.WriteStruct(header);

                //Image
                var settings = new ImageSettings
                {
                    Width = (int)header.width,
                    Height = (int)header.height,
                    Format = ImageSettings.ConvertFormat(header.format)
                };
                bw.BaseStream.Position = (bw.BaseStream.Position + 0x3f) & ~0x3f;
                bw.Write(Common.Save(bmps[0], settings));
            }
        }
    }
}
