using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Cetera.Image;
using Kuriimu.Contract;
using Kuriimu.IO;

/*Original image and data types by xdaniel and his tool Tharsis
 * https://github.com/xdanieldzd/Tharsis */

namespace image_stex
{
    public class STEX
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
            public Magic magic;
            public uint zero0;
            public uint const0; //0xde1
            public int width;
            public int height;
            public DataTypes type;
            public Format imageFormat;
            public int dataSize;

        }

        public class TexInfo
        {
            public TexInfo(Stream input)
            {
                using (BinaryReaderX br = new BinaryReaderX(input, true))
                {
                    offset = br.ReadInt32();
                    unk1 = br.ReadUInt32();
                    name = br.ReadCStringA();
                }
            }
            public int offset = 0x20;
            public uint unk1 = 0xFFFFFFFF;
            public string name = "";
        }

        public Header header;
        public TexInfo texInfo;
        public Bitmap bmp;
        public ImageSettings settings;

        public STEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                header = br.ReadStruct<Header>();
                texInfo = new TexInfo(br.BaseStream);

                settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = (header.type == DataTypes.UnsignedShort565)
                        ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGB565)
                        : (header.type == DataTypes.UnsignedShort5551)
                            ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA5551)
                            : (header.type == DataTypes.UnsignedShort4444)
                                ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA4444)
                                : ImageSettings.ConvertFormat(header.imageFormat),
                };
                br.BaseStream.Position = texInfo.offset;
                bmp = Common.Load(br.ReadBytes(header.dataSize), settings);
            }
        }

        public void Save(string filename, Bitmap bitmap)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                byte[] pic = Common.Save(bitmap, settings);

                header.width = bitmap.Width;
                header.height = bitmap.Height;
                header.dataSize = pic.Length;
                bw.WriteStruct(header);

                bw.Write(0x80);
                bw.Write(texInfo.unk1);
                bw.WriteASCII(texInfo.name);
                bw.Write((byte)0);

                bw.BaseStream.Position = 0x80;
                bw.Write(pic);
            }
        }
    }
}
