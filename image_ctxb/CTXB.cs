using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using Kuriimu.IO;
using Kuriimu.Contract;
using Cetera.Image;

/*Original image and data types by xdaniel and his tool Tharsis
 * https://github.com/xdanieldzd/Tharsis */

namespace image_ctxb
{
    public class CTXB
    {
        public enum DataTypes : uint
        {
            Byte = 0x1400,
            UByte = 0x1401,
            Short = 0x1402,
            UShort = 0x1403,
            Int = 0x1404,
            UInt = 0x1405,
            Float = 0x1406,
            UnsignedByte44DMP = 0x6760,
            Unsigned4BitsDMP = 0x6761,
            UnsignedShort4444 = 0x8033,
            UnsignedShort5551 = 0x8034,
            UnsignedShort565 = 0x8363
        };

        public enum Format : uint
        {
            RGBA8888 = 0x6752,
            RGB888 = 0x6754,
            A8 = 0x6756,
            L8 = 0x6757,
            LA44 = 0x6758,
            ETC1 = 0x675A,
            ETC1A4 = 0x675B
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public Magic magic; //ctxb
            public int fileSize;
            public int chunkCount;
            public int zero0;
            public int chunkOffset;
            public int texDataOffset;
        }

        public class Chunk
        {
            public Chunk(Stream input)
            {
                using (var br = new BinaryReaderX(input,true))
                {
                    magic = br.ReadStruct<Magic>();
                    chunkSize = br.ReadInt32();
                    texCount = br.ReadInt32();

                    textures = br.ReadMultiple<TexEntry>(texCount);
                }
            }
            public Magic magic; //tex
            public int chunkSize;
            public int texCount;
            public List<TexEntry> textures;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TexEntry
        {
            public int dataLength;
            public short unk0;
            public short unk1;
            public ushort width;
            public ushort height;
            public Format imageFormat;
        }

        public Header header;
        List<Chunk> chunks=new List<Chunk>();

        public Bitmap bmp;
        public ImageSettings settings;

        public CTXB(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Chunks
                for(int i=0;i<header.chunkCount;i++)
                    chunks.Add(new Chunk(br.BaseStream));

                settings = new ImageSettings
                {
                    Width = chunks[0].textures[0].width,
                    Height = chunks[0].textures[0].height,
                    Format=ImageSettings.ConvertFormat(chunks[0].textures[0].imageFormat)
                };
                br.BaseStream.Position = header.texDataOffset;
                bmp = Common.Load(br.ReadBytes(chunks[0].textures[0].dataLength), settings);
            }
        }

        /*Save not supported until Bitmap[] is introduced*/

        /*public void Save(String filename, Bitmap bitmap)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }*/
    }
}
