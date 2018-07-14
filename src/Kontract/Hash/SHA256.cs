using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.IO;
using System.IO;

namespace Kontract.Hash
{
    public class SHA256
    {
        public static byte[] Create(byte[] to_hash) => System.Security.Cryptography.SHA256.Create().ComputeHash(to_hash);

        public static byte[] Create(string to_hash) => System.Security.Cryptography.SHA256.Create().ComputeHash(to_hash.Hexlify());

        public static byte[] Create(Stream instream, long offset = 0, long length = -1)
        {
            var bkOffset = instream.Position;
            var hash = System.Security.Cryptography.SHA256.Create().ComputeHash(new SubStream(instream, offset, (length <= 0) ? instream.Length : length));
            instream.Position = bkOffset;
            return hash;
        }
    }
}
