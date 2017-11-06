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

                bmps.Add(Kontract.Image.Image.Load(br.ReadBytes((int)br.BaseStream.Length - HeaderLength), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.WriteStruct(Header);

                settings.Width = bmps[0].Width;
                settings.Height = bmps[0].Height;

                bw.Write(Kontract.Image.Image.Save(bmps[0], settings));
            }
        }
    }
}
