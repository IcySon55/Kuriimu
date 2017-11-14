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
        byte[] Decrypt(Stream input);
        byte[] Encrypt(Stream input);
    }
}
