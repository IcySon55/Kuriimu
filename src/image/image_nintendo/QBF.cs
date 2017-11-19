using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Komponent.IO;
using System.Text;

namespace image_nintendo.QBF
{
    public class QBF
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        ImageAttributes attr = new ImageAttributes();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic magic;
            public short entryCount;
            public short unk1;
            public int unk2;
            public byte bpp;
            public byte glyphWidth;
            public byte glyphHeight;
            public byte unk3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LetterInf
        {
            public short id;
            public byte unk1;
            public byte unk2;
            public short unk3;
        }

        Header header;
        public static Dictionary<char, LetterInf> letterInfos;
        List<char> chars;

        public QBF(Stream input)
        {
            letterInfos = new Dictionary<char, LetterInf>();
            chars = new List<char>();

            using (var br = new BinaryReaderX(input))
            {
                var unicode = Encoding.Unicode;

                //Header
                header = br.ReadStruct<Header>();

                //Letter information
                for (int i = 0; i < header.entryCount; i++)
                {
                    char letter = unicode.GetChars(br.ReadBytes(2))[0];
                    letterInfos.Add(letter, br.ReadStruct<LetterInf>());
                    chars.Add(letter);
                }

                //Images
                for (int i = 0; i < header.entryCount; i++)
                {
                    if (letterInfos[chars[i]].unk3 == 0)
                    {
                        bmps.Add(new Bitmap(header.glyphWidth, header.glyphHeight));

                        for (int y = 0; y < header.glyphHeight; y++)
                        {
                            for (int x = 0; x < header.glyphWidth; x++)
                            {
                                if (header.bpp == 4)
                                {
                                    var value = br.ReadNibble() * 17;
                                    var argb = (value << 24) | 0xffffff;
                                    bmps.Last().SetPixel(x + 1, y, Color.FromArgb(argb));

                                    value = br.ReadNibble() * 17;
                                    argb = (value << 24) | 0xffffff;
                                    bmps.Last().SetPixel(x++, y, Color.FromArgb(argb));
                                }
                                else if (header.bpp == 8)
                                {
                                    var value = br.ReadByte();
                                    var argb = (value << 24) | 0xffffff;
                                    bmps.Last().SetPixel(x, y, Color.FromArgb(argb));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
