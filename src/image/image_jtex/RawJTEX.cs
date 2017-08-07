using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Image;
using Kuriimu.Compression;
using Kuriimu.IO;

namespace image_rawJtex
{
    public class RawJTEX
    {
        public RawHeader JTEXRawHeader;
        public Bitmap Image;

        public RawJTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                JTEXRawHeader = br.ReadStruct<RawHeader>();

                //Add image
                br.BaseStream.Position = JTEXRawHeader.dataStart;
                var settings = new ImageSettings
                {
                    Width = JTEXRawHeader.width,
                    Height = JTEXRawHeader.height,
                    Format = ImageSettings.ConvertFormat(JTEXRawHeader.format),
                    PadToPowerOf2 = false
                };
                Image = Common.Load(br.ReadBytes((int)(br.BaseStream.Length - JTEXRawHeader.dataStart)), settings);
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                //check for oringinal size
                if (JTEXRawHeader.width != Image.Width || JTEXRawHeader.height != Image.Height)
                    throw new System.Exception($"Image has to be {JTEXRawHeader.width}x{JTEXRawHeader.height}px!");

                //get texture
                var settings = new ImageSettings
                {
                    Width = Image.Width,
                    Height = Image.Height,
                    Format = ImageSettings.ConvertFormat(JTEXRawHeader.format),
                    PadToPowerOf2 = false
                };
                byte[] resBmp = Common.Save(Image, settings);

                //Header
                JTEXRawHeader.width = Image.Width;
                JTEXRawHeader.height = Image.Height;
                bw.WriteStruct(JTEXRawHeader);

                //Image
                bw.BaseStream.Position = JTEXRawHeader.dataStart;
                bw.Write(resBmp);
            }
        }
    }
}
