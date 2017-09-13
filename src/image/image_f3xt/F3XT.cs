using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Image;
using Kuriimu.Compression;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_f3xt
{
    class F3XT
    {
        public Bitmap Image;
        public ImageSettings settings;
        public Header header;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            Magic magic;
            uint texEntries;
            uint flags;
            uint unk1;
            public ushort width;
            public ushort height;
            public uint dataStart;

            public Format format => (Format)((int)flags & 0xFF);
        }

        public F3XT(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                //map the file
                header = br.ReadStruct<Header>();

                settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = ImageSettings.ConvertFormat(header.format),
                    PadToPowerOf2 = false
                };

                br.BaseStream.Position = header.dataStart;

                //throw new Exception((stream.BaseStream.Length - stream.BaseStream.Position).ToString());

                Image = Common.Load(br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position)), settings);
            }
        }

        public void Save(Stream input)
        {
            using (BinaryWriterX br = new BinaryWriterX(input))
            {
                ImageSettings modSettings = settings;
                modSettings.Width = Image.Width;
                modSettings.Height = Image.Height;

                byte[] data = Common.Save(Image, modSettings);

                header.width = (ushort)Image.Width; header.height = (ushort)Image.Height;
                br.WriteStruct<Header>(header);
                br.BaseStream.Position = header.dataStart;
                br.Write(data);
            }
        }
    }
}
