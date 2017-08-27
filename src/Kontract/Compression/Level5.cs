using System;
using System.IO;
using System.Text;
using Kuriimu.IO;
using System.Linq;

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
            uint methodSize = 0;
            switch (method)
            {
                case Method.NoCompression:
                case Method.Huffman4Bit:
                case Method.Huffman8Bit:
                    methodSize = (uint)stream.Length << 3;
                    using (var bw = new BinaryWriterX(new MemoryStream()))
                    {
                        bw.Write(methodSize);
                        stream.Position = 0;
                        stream.CopyTo(bw.BaseStream);
                        bw.BaseStream.Position = 0;
                        return new BinaryReaderX(bw.BaseStream).ReadBytes((int)bw.BaseStream.Length);
                    }
                case Method.LZSS:
                    methodSize = (uint)(stream.Length << 3 | 0x1);
                    using (var bw = new BinaryWriterX(new MemoryStream()))
                    {
                        bw.Write(methodSize);
                        stream.Position = 0;
                        var comp = LZ10.Compress(stream);
                        for (int i = 0; i < 4; i++) comp.ToList().RemoveAt(0);
                        bw.Write(comp);
                        bw.BaseStream.Position = 0;
                        return new BinaryReaderX(bw.BaseStream).ReadBytes((int)bw.BaseStream.Length);
                    }
                case Method.RLE:
                    methodSize = (uint)(stream.Length << 3 | 0x4);
                    using (var bw = new BinaryWriterX(new MemoryStream()))
                    {
                        bw.Write(methodSize);
                        stream.Position = 0;
                        var comp = RLE.Compress(stream);
                        bw.Write(comp);
                        bw.BaseStream.Position = 0;
                        return new BinaryReaderX(bw.BaseStream).ReadBytes((int)bw.BaseStream.Length);
                    }
                default:
                    throw new Exception($"Unsupported compression method {method}!");
            }
        }
    }
}
