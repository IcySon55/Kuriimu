using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Compression;
using Cetera.Image;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tex
{
    class TEX
    {
        public static bool lz11_compressed = false;
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

        public TEX(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                BinaryReaderX stream;

                //check for lz11 compression
                if (br.ReadByte() == 0x11)
                {
                    br.BaseStream.Position = 0;
                    byte[] decomp = LZ11.Decompress(br.BaseStream);
                    br.BaseStream.Position = 0;
                    uint size = br.ReadUInt32() >> 8;
                    if (decomp.Length == size)
                    {
                        lz11_compressed = true;
                    }

                    stream = new BinaryReaderX(new MemoryStream(decomp, 0, decomp.Length));
                }
                else
                {
                    br.BaseStream.Position = 0;
                    stream = new BinaryReaderX(input);
                }

                //map the file
                header = stream.ReadStruct<Header>();

                settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = ImageSettings.ConvertFormat(header.format),
                    PadToPowerOf2 = false
                };

                stream.BaseStream.Position = header.dataStart;

                //throw new Exception((stream.BaseStream.Length - stream.BaseStream.Position).ToString());

                Image = Common.Load(stream.ReadBytes((int)(stream.BaseStream.Length - stream.BaseStream.Position)), settings);
            }
        }

        public void Save(Stream input)
        {
            ImageSettings modSettings = settings;
            modSettings.Width = Image.Width;
            modSettings.Height = Image.Height;

            byte[] data = Common.Save(Image, modSettings);
            using (BinaryWriterX br = new BinaryWriterX(new MemoryStream()))
            {
                header.width = (ushort)Image.Width; header.height = (ushort)Image.Height;
                br.WriteStruct<Header>(header);
                br.BaseStream.Position = header.dataStart;
                br.Write(data);
                br.BaseStream.Position = 0;

                if (lz11_compressed)
                {
                    byte[] comp = LZ11.Compress(br.BaseStream);
                    input.Write(comp, 0, comp.Length);
                }
                else
                {
                    input.Write(new BinaryReaderX(br.BaseStream).ReadBytes((int)br.BaseStream.Length), 0, (int)br.BaseStream.Length);
                }
                input.Close();
            }
        }
    }
}
