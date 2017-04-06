using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Compression;
using Kuriimu.IO;

namespace Cetera.Image
{
    public class RawJTEX
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RawHeader
        {
            public uint dataStart;
            uint formTmp;
            uint unk1;
            uint unk2;
            public int width;
            public int height;

            public Format format => (Format)(formTmp & 0xFF);
        }

        public bool lz11_compressed = false;

        public RawHeader JTEXRawHeader;
        public Bitmap Image { get; set; }
        public ImageSettings Settings { get; set; }

        public RawJTEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                Stream stream;

                if (br.ReadByte() == 0x11)
                {
                    br.BaseStream.Position = 0;
                    uint size = br.ReadUInt32() >> 8;
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

                //File.OpenWrite("test.decomp").Write(new BinaryReaderX(stream).ReadBytes((int)stream.Length), 0, (int)stream.Length);

                using (BinaryReaderX br2 = new BinaryReaderX(stream))
                {
                    JTEXRawHeader = br2.ReadStruct<RawHeader>();
                    br2.BaseStream.Position = JTEXRawHeader.dataStart;
                    Settings = new ImageSettings { Width = JTEXRawHeader.width, Height = JTEXRawHeader.height, Format = JTEXRawHeader.format };
                    Image = Common.Load(br2.ReadBytes((int)(br2.BaseStream.Length - br2.BaseStream.Position)), Settings);
                }
            }
        }

        public void Save(Stream output)
        {
            ImageSettings modSettings = Settings;
            modSettings.Width = Image.Width;
            modSettings.Height = Image.Height;

            byte[] data = Common.Save(Image, modSettings);
            using (BinaryWriterX br = new BinaryWriterX(new MemoryStream()))
            {
                JTEXRawHeader.width = (ushort)Image.Width; JTEXRawHeader.height = (ushort)Image.Height;
                br.WriteStruct<RawHeader>(JTEXRawHeader);
                br.BaseStream.Position = JTEXRawHeader.dataStart;
                br.Write(data);
                br.BaseStream.Position = 0;

                if (lz11_compressed)
                {
                    byte[] comp = LZ11.Compress(br.BaseStream);
                    output.Write(comp, 0, comp.Length);
                }
                else
                {
                    output.Write(new BinaryReaderX(br.BaseStream).ReadBytes((int)br.BaseStream.Length), 0, (int)br.BaseStream.Length);
                }
                output.Close();
            }
        }
    }
}
