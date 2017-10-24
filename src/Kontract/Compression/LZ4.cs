using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.IO;
using LZ4;

namespace Kontract.Compression
{
    public class LZ4
    {
        public static byte[] Decompress(Stream instream) => LZ4Codec.Unwrap(new BinaryReaderX(instream, true).ReadAllBytes());

        public static byte[] Compress(Stream instream) => LZ4Codec.Wrap(new BinaryReaderX(instream, true).ReadAllBytes());
    }
}
