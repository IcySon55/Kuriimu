using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_ctx
{
    public class CTX
    {
        /*public class Chunk
        {
            public Chunk(Stream input)
            {
                using (var br = new BinaryReaderX(input, true))
                {
                    magic = br.ReadStruct<Magic>();
                    chunkSize = br.ReadInt32();
                    texCount = br.ReadInt32();

                    textures = br.ReadMultiple<TexEntry>(texCount);
                }
            }
            public Magic magic; //"tex "
            public int chunkSize;
            public int texCount;
            public List<TexEntry> textures;

            public void Write(Stream input)
            {
                using (var bw = new BinaryWriterX(input, true))
                {
                    bw.WriteASCII(magic);
                    bw.Write(chunkSize);
                    bw.Write(texCount);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class TexEntry
        {
            public int dataLength;
            public short unk0;
            public short unk1;
            public ushort width;
            public ushort height;
            public Format imageFormat;
            public DataTypes dataType;
        }

        public Header header;
        List<Chunk> chunks = new List<Chunk>();*/

        public Header header;

        public List<Bitmap> bmps = new List<Bitmap>();

        public CTX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Image
                br.BaseStream.Position = (br.BaseStream.Position + 0x3f) & ~0x3f;
                var settings = new ImageSettings
                {
                    Width = (int)header.width,
                    Height = (int)header.height,
                    Format = ImageSettings.ConvertFormat(header.format)
                };
                bmps.Add(Common.Load(br.ReadBytes((int)header.dataSize), settings));
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                //Header
                header.width = (uint)bmps[0].Width;
                header.width2 = (uint)bmps[0].Width;
                header.height = (uint)bmps[0].Height;
                header.height2 = (uint)bmps[0].Height;
                bw.WriteStruct(header);

                //Image
                var settings = new ImageSettings
                {
                    Width = (int)header.width,
                    Height = (int)header.height,
                    Format = ImageSettings.ConvertFormat(header.format)
                };
                bw.BaseStream.Position = (bw.BaseStream.Position + 0x3f) & ~0x3f;
                bw.Write(Common.Save(bmps[0], settings));
            }
        }
    }
}
