using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using KuriimuContract;
using Cetera.Compression;
using Cetera.Image;

namespace image_tex
{
    class TEX
    {
        public static bool lz11_compressed = false;
        public Bitmap Image;

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
                Header header = stream.ReadStruct<Header>();

                var settings = new ImageSettings
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
            int t = 0;
        }
    }
}
