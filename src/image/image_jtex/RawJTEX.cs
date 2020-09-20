using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;

namespace image_rawJtex
{
    public class RawJTEX
    {
        public RawHeader JTEXRawHeader;
        public Bitmap Image;
        public ImageSettings settings;

        public RawJTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                JTEXRawHeader = br.ReadStruct<RawHeader>();

                //Add image
                br.BaseStream.Position = JTEXRawHeader.dataStart;
                settings = new ImageSettings
                {
                    Width = JTEXRawHeader.virWidth,
                    Height = JTEXRawHeader.virHeight,
                    Format = Support.Format[JTEXRawHeader.format],
                    Swizzle = new CTRSwizzle(JTEXRawHeader.width, JTEXRawHeader.height)
                };
                Image = Kontract.Image.Common.Load(br.ReadBytes((int)(br.BaseStream.Length - JTEXRawHeader.dataStart)), settings);
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                //check for oringinal size
                //if (JTEXRawHeader.width != Image.Width || JTEXRawHeader.height != Image.Height)
                //    throw new System.Exception($"Image has to be {JTEXRawHeader.width}x{JTEXRawHeader.height}px!");

                //get texture
                settings = new ImageSettings
                {
                    Width = Image.Width,
                    Height = Image.Height,
                    Format = Support.Format[JTEXRawHeader.format],
                    Swizzle = new CTRSwizzle(Image.Width, Image.Height)
                };
                byte[] resBmp = Kontract.Image.Common.Save(Image, settings);

                //Header
                JTEXRawHeader.width = NextPowerOfTwo(Image.Width);
                JTEXRawHeader.height = NextPowerOfTwo(Image.Height);
                JTEXRawHeader.virWidth = Image.Width;
                JTEXRawHeader.virHeight = Image.Height;
                bw.WriteStruct(JTEXRawHeader);

                //Image
                bw.BaseStream.Position = JTEXRawHeader.dataStart;
                bw.Write(resBmp);
            }
        }

        public int NextPowerOfTwo(int val)
        {
            var v = (uint)val;
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return (int)v;
        }
    }
}
