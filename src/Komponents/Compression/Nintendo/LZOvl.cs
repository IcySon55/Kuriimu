using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.Interface;

namespace Compression
{
    [Export("LZOvl", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class LZOvl : ICompression
    {
        public string Name { get; } = "LZOvl";

        public string TabPathCompress { get; } = "";
        public string TabPathDecompress { get; } = "";

        public byte[] Decompress(Stream input, long decompSize = 0)
        {
            return null;
        }

        public byte[] Compress(Stream input)
        {
            return null;
        }
    }
}
