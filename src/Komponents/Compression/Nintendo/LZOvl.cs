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
    public class LZOvl
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
