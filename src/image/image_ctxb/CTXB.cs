using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Image;
using Kontract;
using Kontract.IO;

/*Original image and data types by xdaniel and his tool Tharsis
 * https://github.com/xdanieldzd/Tharsis */

namespace image_ctxb
{
    public class CTXB
    {
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

        public enum Format : ushort
        {
            RGBA8888 = 0x6752,
            RGB888 = 0x6754,
            A8 = 0x6756, L8, LA44,
            ETC1 = 0x675A, ETC1A4
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
        List<Chunk> chunks = new List<Chunk>();

        public List<Bitmap> bmps = new List<Bitmap>();

        public CTXB(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Chunks
                for (int i = 0; i < header.chunkCount; i++)
                    chunks.Add(new Chunk(br.BaseStream));

                br.BaseStream.Position = header.texDataOffset;
                for (int i = 0; i < header.chunkCount; i++)
                {
                    for (int j = 0; j < chunks[i].texCount; j++)
                    {
                        var settings = new ImageSettings
                        {
                            Width = chunks[i].textures[j].width,
                            Height = chunks[i].textures[j].height,
                            Format = (chunks[i].textures[j].dataType == DataTypes.UnsignedShort565)
                                ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGB565)
                                : (chunks[i].textures[j].dataType == DataTypes.UnsignedShort5551)
                                    ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA5551)
                                    : (chunks[i].textures[j].dataType == DataTypes.UnsignedShort4444)
                                        ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA4444)
                                        : ImageSettings.ConvertFormat(chunks[i].textures[j].imageFormat),
                            PadToPowerOf2 = false
                        };

                        bmps.Add(Common.Load(br.ReadBytes(chunks[i].textures[j].dataLength), settings));
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.BaseStream.Position = header.texDataOffset;
                var count = 0;
                for (int i = 0; i < header.chunkCount; i++)
                {
                    for (int j = 0; j < chunks[i].texCount; j++)
                    {
                        var settings = new ImageSettings
                        {
                            Width = bmps[count].Width,
                            Height = bmps[count].Height,
                            Format = (chunks[i].textures[j].dataType == DataTypes.UnsignedShort565)
                                ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGB565)
                                : (chunks[i].textures[j].dataType == DataTypes.UnsignedShort5551)
                                    ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA5551)
                                    : (chunks[i].textures[j].dataType == DataTypes.UnsignedShort4444)
                                        ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA4444)
                                        : ImageSettings.ConvertFormat(chunks[i].textures[j].imageFormat),
                            PadToPowerOf2 = false
                        };

                        var resBmp = Common.Save(bmps[count], settings);
                        bw.Write(resBmp);

                        chunks[i].textures[j].dataLength = resBmp.Length;
                        chunks[i].textures[j].height = (ushort)bmps[count].Height;
                        chunks[i].textures[j].width = (ushort)bmps[count].Width;

                        count++;
                    }
                }

                //Ctxb Header
                header.fileSize = (uint)bw.BaseStream.Length;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);

                //Chunks
                for (int i = 0; i < header.chunkCount; i++)
                {
                    chunks[i].Write(bw.BaseStream);
                    for (int j = 0; j < chunks[i].texCount; j++)
                    {
                        bw.WriteStruct(chunks[i].textures[j]);
                    }
                }
            }
        }
    }
}
