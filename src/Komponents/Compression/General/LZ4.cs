using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;
using LZ4;

namespace Compression
{
    [Export("LZ4", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class LZ4 : ICompression
    {
        public string Name { get; } = "LZ4";

        public string TabPathCompress { get; } = "General/LZ4";
        public string TabPathDecompress { get; } = "General/LZ4";

        public byte[] Decompress(Stream instream, long decompSize = 0) => LZ4Codec.Unwrap(new BinaryReaderX(instream, true).ReadAllBytes());

        public byte[] Compress(Stream instream) => LZ4Codec.Wrap(new BinaryReaderX(instream, true).ReadAllBytes());
    }
}
