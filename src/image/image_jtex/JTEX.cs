using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Cetera.Image;
using Kontract.IO;

namespace image_jtex
{
    public class JTEX
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public Header JTEXHeader;

        public JTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                JTEXHeader = br.ReadStruct<Header>();

                //Add image
                var settings = new ImageSettings
                {
                    Width = JTEXHeader.width,
                    Height = JTEXHeader.height,
                    Format = ImageSettings.ConvertFormat(JTEXHeader.format)
                };
                bmps.Add(Common.Load(br.ReadBytes(JTEXHeader.unk3[0]), settings));
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //get texture
                var settings = new ImageSettings
                {
                    Width = bmps[0].Width,
                    Height = bmps[0].Height,
                    Format = ImageSettings.ConvertFormat(JTEXHeader.format)
                };
                byte[] texture = Common.Save(bmps[0], settings);

                //edit header
                JTEXHeader.width = (short)bmps[0].Width;
                JTEXHeader.height = (short)bmps[0].Height;
                JTEXHeader.unk3[0] = texture.Length;

                bw.WriteStruct(JTEXHeader);
                bw.Write(texture);
            }
        }
    }
}
