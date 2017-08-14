using System;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace Kuriimu.Compression
{
    public class Level5
    {
        public enum Method
        {
            NoCompression,
            LZSS,
            Huffman4Bit,
            Huffman8Bit,
            RLE
        }

        public static byte[] Decompress(Stream stream)
        {
            using (var br = new BinaryReader(stream, Encoding.Default, true))
            {
                int sizeAndMethod = br.ReadInt32();
                int size = sizeAndMethod / 8;
                var method = (Method)(sizeAndMethod % 8);

                switch (method)
                {
                    case Method.NoCompression:
                        return br.ReadBytes(size);
                    case Method.LZSS:
                        return LZSS.Decompress(br.BaseStream, size);
                    case Method.Huffman4Bit:
                    case Method.Huffman8Bit:
                        int num_bits = method == Method.Huffman4Bit ? 4 : 8;
                        return Huffman.Decompress(br.BaseStream, num_bits, size);
                    case Method.RLE:
                        return RLE.Decompress(br.BaseStream, size);
                    default:
                        throw new NotSupportedException($"Unknown compression method {method}");
                }
            }
        }

        public static byte[] Compress(Stream stream, Method method)
        {
            switch (method)
            {
                case Method.NoCompression:
                    var methodSize = stream.Length << 3;
                    using (var bw = new BinaryWriterX(stream))
                    {
                        bw.Write(methodSize);
                        stream.CopyTo(bw.BaseStream);
                        return new BinaryReaderX(bw.BaseStream, true).ReadBytes((int)bw.BaseStream.Length);
                    }
                default:
                    throw new Exception($"Unsupported compression method {method}!");
            }
        }
    }
}
