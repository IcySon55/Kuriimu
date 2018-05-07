using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontract.Hash
{
    public class SHA256
    {
        public static byte[] Create(byte[] to_hash) => System.Security.Cryptography.SHA256.Create().ComputeHash(to_hash);

        public static byte[] Create(string to_hash) => System.Security.Cryptography.SHA256.Create().ComputeHash(to_hash.Hexlify());
    }
}
