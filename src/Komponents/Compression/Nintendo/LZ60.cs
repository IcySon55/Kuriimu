using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace Compression
{
    [Export("LZ60", typeof(ICompression))]
    public class LZ60 : ICompression
    {
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
