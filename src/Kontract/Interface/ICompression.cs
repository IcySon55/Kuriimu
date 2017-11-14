using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Kontract.Interface
{
    public interface ICompression
    {
        byte[] Compress(Stream input);
        byte[] Decompress(Stream input, long decompSize);
    }

    public interface ICompressionCollection : ICompression
    {
        void SetMethod(byte Method);
    }
}
