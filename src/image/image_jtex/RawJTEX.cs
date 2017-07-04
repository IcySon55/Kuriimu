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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RawHeader
        {
            public uint dataStart;
            uint formTmp;
            public int virWidth;
            public int virHeight;
            public int width;
            public int height;

            public Format format => (Format)(formTmp & 0xFF);
        }

        public bool lz11_compressed = false;

        public RawHeader JTEXRawHeader;
        public Bitmap Image { get; set; }

        public RawJTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                Stream stream;

                if (br.ReadByte() == 0x11)
                {
                    br.BaseStream.Position = 0;
                    lz11_compressed = true;
                    byte[] decomp = LZ11.Decompress(br.BaseStream);
                    stream = new MemoryStream(decomp);
                }
                else
                {
                    br.BaseStream.Position = 0;
                    stream = br.BaseStream;
                }

                using (BinaryReaderX br2 = new BinaryReaderX(stream))
                {
                    JTEXRawHeader = br2.ReadStruct<RawHeader>();
                    br2.BaseStream.Position = JTEXRawHeader.dataStart;
                    var settings = new ImageSettings
                    {
                        Width = JTEXRawHeader.width,
                        Height = JTEXRawHeader.height,
                        Format = JTEXRawHeader.format,
                        PadToPowerOf2 = false
                    };
                    Image = Common.Load(br2.ReadBytes((int)(br2.BaseStream.Length - br2.BaseStream.Position)), settings);
                }
            }
        }

        public void Save(Stream output)
        {
            var settings = new ImageSettings
            {
                Width = Image.Width,
                Height = Image.Height,
            };
            byte[] resBmp = Common.Save(Image, settings);

            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                JTEXRawHeader.width = Image.Width;
                JTEXRawHeader.height = Image.Height;
                //JTEXRawHeader.virWidth = Image.Width;
                //JTEXRawHeader.virHeight = Image.Height;
                bw.WriteStruct(JTEXRawHeader);
                bw.BaseStream.Position = JTEXRawHeader.dataStart;
                bw.Write(resBmp);
                bw.BaseStream.Position = 0;

                if (lz11_compressed)
                {
                    byte[] comp = LZ11.Compress(bw.BaseStream);
                    bw.Write(comp);
                }
                else
                {
                    bw.Write(new BinaryReaderX(bw.BaseStream).ReadBytes((int)bw.BaseStream.Length), 0, (int)bw.BaseStream.Length);
                }
            }
        }
    }
}
