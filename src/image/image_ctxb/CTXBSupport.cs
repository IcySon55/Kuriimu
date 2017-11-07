using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract;
using System.IO;
using Kontract.IO;
using Kontract.Interface;
using Kontract.Image.Format;

namespace image_ctxb
{
    public class Support
    {
        public static Dictionary<uint, IImageFormat> Format = new Dictionary<uint, IImageFormat>
        {
            [0x6752] = new RGBA(8, 8, 8, 8),
            [0x6754] = new RGBA(8, 8, 8),
            [0x6756] = new LA(0, 8),
            [0x6757] = new LA(8, 0),
            [0x6758] = new LA(4, 4),
            [0x675A] = new ETC1(),
            [0x675B] = new ETC1(true)
        };
    }

    public enum DataTypes : ushort
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic; //ctxb
        public uint fileSize;
        public long chunkCount;
        public int chunkOffset;
        public int texDataOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TexEntry
    {
        public int dataLength;
        public short unk0;
        public short unk1;
        public ushort width;
        public ushort height;
        public ushort imageFormat;
        public DataTypes dataType;
    }

    public class Chunk
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
}
