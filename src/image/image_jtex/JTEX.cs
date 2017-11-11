using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;

namespace image_jtex
{
    public class JTEX
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public Header JTEXHeader;
        public ImageSettings settings;

        public JTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                JTEXHeader = br.ReadStruct<Header>();

                //Add image
                settings = new ImageSettings
                {
                    Width = JTEXHeader.width,
                    Height = JTEXHeader.height,
                    Format = Support.Format[JTEXHeader.format],
                    Swizzle = new CTRSwizzle(JTEXHeader.width, JTEXHeader.height)
                };
                bmps.Add(Kontract.Image.Common.Load(br.ReadBytes(JTEXHeader.unk3[2]), settings));
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                //get texture
                settings = new ImageSettings
                {
                    Width = bmps[0].Width,
                    Height = bmps[0].Height,
                    Format = Support.Format[JTEXHeader.format],
                    Swizzle = new CTRSwizzle(bmps[0].Width, bmps[0].Height)
                };
                byte[] texture = Kontract.Image.Common.Save(bmps[0], settings);

                //edit header
                JTEXHeader.width = (short)bmps[0].Width;
                JTEXHeader.height = (short)bmps[0].Height;
                JTEXHeader.unk3[2] = texture.Length;

                bw.WriteStruct(JTEXHeader);
                bw.Write(texture);
            }
        }
    }
}
