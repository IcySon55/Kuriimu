using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Kontract.Interface
{
    public interface IEncryption
    {
        string Name { get; }

        string TabPathEncrypt { get; }
        string TabPathDecrypt { get; }

        byte[] Decrypt(Stream input);
        byte[] Encrypt(Stream input);
    }
}
