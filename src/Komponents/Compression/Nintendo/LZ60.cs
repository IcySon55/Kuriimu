using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace Compression
{
    [ExportMetadata("Name", "LZ60")]
    [ExportMetadata("TabPathCompress", "")]
    [ExportMetadata("TabPathDecompress", "")]
    [Export("LZ60", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class LZ60 : ICompression
    {
        public string Name { get; } = "LZ60";

        public string TabPathCompress { get; } = "";
        public string TabPathDecompress { get; } = "";

        public byte[] Decompress(Stream instream, long decompSize = 0)
        {
            return null;
        }

        public byte[] Compress(Stream instream)
        {
            return null;
        }
    }
}
