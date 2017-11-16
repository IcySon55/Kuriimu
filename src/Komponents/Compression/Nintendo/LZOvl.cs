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
    [ExportMetadata("Name", "LZOvl")]
    [ExportMetadata("TabPathCompress", "")]
    [ExportMetadata("TabPathDecompress", "")]
    [Export("LZOvl", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class LZOvl : ICompression
    {
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
