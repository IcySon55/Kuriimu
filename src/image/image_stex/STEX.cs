using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract;
using Kontract.IO;

/*Original image and data types by xdaniel and his tool Tharsis
 * https://github.com/xdanieldzd/Tharsis */

namespace image_stex
{
    public class STEX
    {
        public Header header;
        public TexInfo texInfo;

        public uint format;
        public Bitmap bmp;
        public ImageSettings settings;

        public STEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                header = br.ReadStruct<Header>();
                texInfo = new TexInfo(br.BaseStream);

                format = (uint)((header.type << 16) | header.imageFormat);
                settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = Support.Format[(uint)((header.type << 16) | header.imageFormat)],
                    Swizzle = new CTRSwizzle(header.width, header.height)
                };
                br.BaseStream.Position = texInfo.offset;
                bmp = Common.Load(br.ReadBytes(header.dataSize), settings);
            }
        }

        public void Save(string filename, Bitmap bitmap)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                byte[] pic = Common.Save(bitmap, settings);

                header.width = bitmap.Width;
                header.height = bitmap.Height;
                header.type = format >> 16;
                header.imageFormat = format & 0xFFFF;
                header.dataSize = pic.Length;
                bw.WriteStruct(header);

                bw.Write(0x80);
                bw.Write(texInfo.unk1);
                bw.WriteASCII(texInfo.name);
                bw.Write((byte)0);

                bw.BaseStream.Position = 0x80;
                bw.Write(pic);
            }
        }
    }
}
