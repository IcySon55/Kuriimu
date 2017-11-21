using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Komponent.IO;
using Komponent.Interface;
using LZ4;

namespace Komponent.Compression
{
    [ExportMetadata("Name", "LZ4")]
    [ExportMetadata("TabPathCompress", "General/LZ4")]
    [ExportMetadata("TabPathDecompress", "General/LZ4")]
    [Export("LZ4", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class LZ4 : ICompression
    {
        public byte[] Decompress(Stream instream, long decompSize = 0) => LZ4Codec.Unwrap(new BinaryReaderX(instream, true).ReadAllBytes());

        public byte[] Compress(Stream instream) => LZ4Codec.Wrap(new BinaryReaderX(instream, true).ReadAllBytes());
    }
}
