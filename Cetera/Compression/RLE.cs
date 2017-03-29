using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cetera.IO;

namespace Cetera.Compression
{
    class RLE
    {
        public static byte[] Decompress(Stream instream, long decompressedLength)
        {
            using (var br = new BinaryReaderX(instream, true))
            {
                var result = new List<byte>();

                while (true)
                {
                    var flag = br.ReadByte();
                    result.AddRange(flag >= 128
                        ? Enumerable.Repeat(br.ReadByte(), flag - 128 + 3)
                        : br.ReadBytes(flag + 1));

                    if (result.Count == decompressedLength)
                    {
                        return result.ToArray();
                    }
                    else if (result.Count > decompressedLength)
                    {
                        throw new InvalidDataException("Went past the end of the stream");
                    }
                }
            }
        }
    }
}
