using System.Collections.Generic;
using System.Linq;
using System.IO;
using Cetera.IO;

namespace Cetera.Compression
{
    public class Huffman
    {
        //supports 4bit & 8bit
        public static byte[] Decompress(Stream instream, int num_bits, long decompressedLength)
        {
            using (var br = new BinaryReaderX(instream, true))
            {
                var result = new List<byte>();

                var tree_size = br.ReadByte();
                var tree_root = br.ReadByte();
                var tree_buffer = br.ReadBytes(tree_size * 2);

                for (int i = 0, code = 0, next = 0, pos = tree_root; ; i++)
                {
                    if (i % 32 == 0)
                    {
                        code = br.ReadInt32();
                    }
                    next += (pos & 0x3F) * 2 + 2;
                    int direction = (code >> (31 - i)) % 2 == 0 ? 2 : 1;
                    var leaf = (pos >> 5 >> direction) % 2 != 0;
                    pos = tree_buffer[next - direction];
                    if (leaf)
                    {
                        result.Add((byte)pos);
                        pos = tree_root;
                        next = 0;
                    }

                    if (result.Count == decompressedLength * 8 / num_bits)
                    {
                        return num_bits == 8 ? result.ToArray() :
                            Enumerable.Range(0, (int)decompressedLength).Select(j => (byte)(result[2 * j + 1] * 16 + result[2 * j])).ToArray();
                    }
                }
            }
        }
    }
}
