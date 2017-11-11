using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;

namespace image_cvt
{
    public class CVT
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public ImageSettings settings;

        public Header Header;
        private const int HeaderLength = 0x50;

        public CVT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                Header = br.ReadStruct<Header>();

                settings = new ImageSettings
                {
                    Width = Header.Width,
                    Height = Header.Height,
                    Format = Support.Format[Header.Format],
                    Swizzle = new CTRSwizzle(Header.Width, Header.Height)
                };

                bmps.Add(Kontract.Image.Common.Load(br.ReadBytes((int)br.BaseStream.Length - HeaderLength), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.WriteStruct(Header);

                settings = new ImageSettings
                {
                    Width = bmps[0].Width,
                    Height = bmps[0].Height,
                    Format = Support.Format[Header.Format],
                    Swizzle = new CTRSwizzle(bmps[0].Width, bmps[0].Height)
                };
                bw.Write(Kontract.Image.Common.Save(bmps[0], settings));
            }
        }
    }
}
