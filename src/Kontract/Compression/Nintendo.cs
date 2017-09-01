using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Compression;

namespace Kuriimu.Compression
{
    public class Nintendo
    {
        public enum Method : byte
        {
            LZ10 = 0x10,
            LZ11 = 0x11,
            Huff4 = 0x24,
            Huff8 = 0x28,
            RLE = 0x30,
            LZ60 = 0x60
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
                        return Huffman.Decompress(br.BaseStream, 4, ByteOrder.BigEndian);
                    case Method.Huff8:
                        br.BaseStream.Position--;
                        return Huffman.Decompress(br.BaseStream, 8);
                    case Method.RLE:
                        br.BaseStream.Position--;
                        return RLE.Decompress(br.BaseStream);
                    case Method.LZ60:
                        br.BaseStream.Position--;
                        throw new Exception("LZ60 isn't implemented yet");
                    //return LZ60.Decompress(br.BaseStream);
                    default:
                        br.BaseStream.Position--;
                        return br.BaseStream.StructToBytes();
                }
            }
        }

        public static byte[] Compress(Stream input, Method method)
        {
            if (input.Length > 0xffffff)
                throw new Exception("File too big to be compressed with Nintendo compression!");

            var header = ((uint)method << 24) | (input.Length & 0xff) << 16 | (input.Length >> 8 & 0xff) << 8 | (input.Length >> 16 & 0xff);
            var res = new List<byte>();
            res.AddRange(header.StructToBytes());

            switch (method)
            {
                case Method.LZ10:
                    res.AddRange(LZ10.Compress(input));
                    return res.ToArray();
                case Method.LZ11:
                    res.AddRange(LZ11.Compress(input));
                    return res.ToArray();
                case Method.Huff4:
                    res.AddRange(Huffman.Compress(input, 4, ByteOrder.BigEndian));
                    return res.ToArray();
                case Method.Huff8:
                    res.AddRange(Huffman.Compress(input, 8));
                    return res.ToArray();
                case Method.RLE:
                    res.AddRange(RLE.Compress(input));
                    return res.ToArray();
                case Method.LZ60:
                    throw new Exception("LZ60 isn't implemented yet");
                //return LZ60.Compress(input);
                default:
                    return input.StructToBytes();
            }
        }
    }
}
