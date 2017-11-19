using System.Drawing;
using System.IO;
using Komponent.Image;
using Komponent.Image.Swizzle;
using Komponent.IO;

namespace image_f3xt
{
    class F3XT
    {
        public Bitmap Image;
        public ImageSettings settings;
        public Header header;

        public F3XT(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = Support.Format[header.format],
                    Swizzle = new CTRSwizzle(header.width, header.height)
                };

                br.BaseStream.Position = header.dataStart;

                Image = Common.Load(br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position)), settings);
            }
        }

        public void Save(Stream input)
        {
            using (BinaryWriterX br = new BinaryWriterX(input))
            {
                settings = new ImageSettings
                {
                    Width = Image.Width,
                    Height = Image.Height,
                    Format = Support.Format[header.format],
                    Swizzle = new CTRSwizzle(Image.Width, Image.Height)
                };
                byte[] data = Common.Save(Image, settings);

                header.width = (ushort)Image.Width;
                header.height = (ushort)Image.Height;
                br.WriteStruct(header);

                br.BaseStream.Position = header.dataStart;
                br.Write(data);
            }
        }
    }
}
