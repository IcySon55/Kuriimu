using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Komponent.Image.Swizzle;
using Kontract.Interface;
using Komponent.Image;
using Komponent.Image.Format;
using Komponent.IO;
using System.Text;

namespace image_nintendo.GZF
{
    public class GZF
    {
        static Dictionary<byte, IImageFormat> Format = new Dictionary<byte, IImageFormat>
        {
            [0] = new RGBA(8, 8, 8, 8),
            [1] = new RGBA(8, 8, 8),
            [2] = new RGBA(5, 5, 5, 1),
            [3] = new RGBA(5, 6, 5),
            [4] = new RGBA(4, 4, 4, 4),
            [5] = new LA(8, 8),
            [6] = new HL(8, 8),
            [7] = new LA(8, 0),
            [8] = new LA(0, 8),
            [9] = new LA(4, 4),
            [10] = new LA(4, 0),
            [11] = new LA(0, 4),
            [12] = new ETC1(),
            [13] = new ETC1(true)
        };

        public List<Bitmap> bmps = new List<Bitmap>();
        ImageAttributes attr = new ImageAttributes();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic magic;
            public int version;
            public ushort imgInfoOffset;
            public ushort unk1;
            public int entryLength;

            public int imgCount;
            public int entryCount;
            public uint unk2;
            public uint unk3;

            public byte format;

            public byte pad1;
            public short unk4;
            public short unk5;
            public short unk6;

            public int unk7;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ImgInfo
        {
            public uint offset;
            public ushort width;
            public ushort height;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LetterInf
        {
            public short height;
            public short imgId;
            public short width;
            public short id;
        }

        Header header;
        List<ImgInfo> imgInfos = new List<ImgInfo>();
        public static Dictionary<char, LetterInf> letterInfos = new Dictionary<char, LetterInf>();

        public GZF(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var unicode = Encoding.Unicode;

                //Header
                header = br.ReadStruct<Header>();

                //Image information
                br.BaseStream.Position = header.imgInfoOffset;
                imgInfos = br.ReadMultiple<ImgInfo>(header.imgCount);

                //Letter information
                for (int i = 0; i < header.entryCount; i++)
                {
                    try
                    {
                        char letter = unicode.GetChars(br.ReadBytes(2))[0];
                        br.BaseStream.Position += 2;
                        letterInfos.Add(letter, br.ReadStruct<LetterInf>());
                    }
                    catch
                    { }
                }

                //Images
                for (int i = 0; i < header.imgCount; i++)
                {
                    br.BaseStream.Position = imgInfos[i].offset;
                    var settings = new ImageSettings
                    {
                        Width = imgInfos[i].width,
                        Height = imgInfos[i].height,
                        Format = Format[header.format],
                        Swizzle = new CTRSwizzle(imgInfos[i].width, imgInfos[i].height)
                    };

                    bmps.Add(Common.Load(br.ReadBytes(imgInfos[i].width * imgInfos[i].height / 2), settings));
                }
            }
        }
    }
}
