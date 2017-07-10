using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Compression;

namespace Kuriimu.Compression
{
    public class Nintendo
    {
        enum Method : byte
        {
            LZ10 = 0x10,
            LZ11 = 0x11,
            Huff4 = 0x24,
            Huff8 = 0x28,
            RLE = 0x30,
        }

        public static byte[] Decompress(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var method = (Method)br.ReadByte();

                switch (method)
                {
                    case Method.LZ10:
                        br.BaseStream.Position--;
                        return LZ10.Decompress(br.BaseStream);
                    case Method.LZ11:
                        br.BaseStream.Position--;
                        return LZ11.Decompress(br.BaseStream);
                    case Method.Huff4:
                        br.BaseStream.Position--;
                        return Huffman.Decompress(br.BaseStream, 4);
                    case Method.Huff8:
                        br.BaseStream.Position--;
                        return Huffman.Decompress(br.BaseStream, 8);
                    case Method.RLE:
                        br.BaseStream.Position--;
                        return RLE.Decompress(br.BaseStream);
                    default:
                        br.BaseStream.Position--;
                        return null;
                }
            }
        }
    }
}
