using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.IO;

namespace image_cvt
{
    public class CVT
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public Header Header;
        private const int HeaderLength = 0x50;

        public CVT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                Header = br.ReadStruct<Header>();

                var settings = new ImageSettings
                {
                    Width = Header.Width,
                    Height = Header.Height,
                    Format = ImageSettings.ConvertFormat(Header.Format),
                    PadToPowerOf2 = false
                };

                bmps.Add(Common.Load(br.ReadBytes((int)br.BaseStream.Length - HeaderLength), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.WriteStruct(Header);

                var settings = new ImageSettings
                {
                    Width = Header.Width,
                    Height = Header.Height,
                    Format = ImageSettings.ConvertFormat(Header.Format)
                };

                bw.Write(Common.Save(bmps[0], settings));
            }
        }
    }
}
