using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontract.Interface
{
    public interface IHash
    {
        byte[] Create(byte[] input, uint seed);
    }
}
