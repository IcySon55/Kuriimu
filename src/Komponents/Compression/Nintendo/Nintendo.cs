using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.IO;
using Komponent.IO;
using Kontract.Interface;

namespace Compression
{
    [ExportMetadata("Name", "Nintendo Compression Header")]
    [ExportMetadata("TabPathCompress", "Nintendo/Generic Header/LZ10,16;Nintendo/Generic Header/LZ11,17;Nintendo/Generic Header/Huffman/4 Bit,36;" +
            "Nintendo/Generic Header/Huffman/8 Bit,40;Nintendo/Generic Header/RLE,48;Nintendo/Generic Header/LZ60,96")]
    [ExportMetadata("TabPathDecompress", "Nintendo/Generic Header")]
    [Export("Nintendo", typeof(ICompressionCollection))]
    [Export(typeof(ICompressionCollection))]
    public class Nintendo : ICompressionCollection
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

        Method method;

        public void SetMethod(byte Method)
        {
            method = (Method)Method;
        }

        public byte[] Decompress(Stream input, long decompSize = 0)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var methodSize = br.ReadUInt32();
                var method = (Method)(methodSize & 0xff);
                int size = (int)((methodSize & 0xffffff00) >> 8);

                using (var brB = new BinaryReaderX(new MemoryStream(br.ReadBytes((int)input.Length - 4))))
                    switch (method)
                    {
                        case Method.LZ10:
                            return new LZ10().Decompress(brB.BaseStream, size);
                        case Method.LZ11:
                            return new LZ11().Decompress(brB.BaseStream, size);
                        case Method.Huff4:
                            return Huffman.Decompress(brB.BaseStream, 4, size, ByteOrder.BigEndian);
                        case Method.Huff8:
                            return Huffman.Decompress(brB.BaseStream, 8, size);
                        case Method.RLE:
                            return new RLE().Decompress(brB.BaseStream, size);
                        case Method.LZ60:
                            throw new Exception("LZ60 isn't implemented yet");
                        //return LZ60.Decompress(brB.BaseStream);
                        default:
                            br.BaseStream.Position -= 4;
                            return br.BaseStream.StructToBytes();
                    }
            }
        }

        public byte[] Compress(Stream input)
        {
            if (input.Length > 0xffffff)
                throw new Exception("File too big to be compressed with Nintendo compression!");

            var res = new List<byte>();
            res.AddRange(new byte[] { (byte)method, (byte)(input.Length & 0xff), (byte)(input.Length >> 8 & 0xff), (byte)(input.Length >> 16 & 0xff) });

            switch (method)
            {
                case Method.LZ10:
                    res.AddRange(new LZ10().Compress(input));
                    return res.ToArray();
                case Method.LZ11:
                    res.AddRange(new LZ11().Compress(input));
                    return res.ToArray();
                case Method.Huff4:
                    res.AddRange(Huffman.Compress(input, 4, ByteOrder.BigEndian));
                    return res.ToArray();
                case Method.Huff8:
                    res.AddRange(Huffman.Compress(input, 8));
                    return res.ToArray();
                case Method.RLE:
                    res.AddRange(new RLE().Compress(input));
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
