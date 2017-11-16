using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontract.Interface
{
    public interface IHash
    {
        string Name { get; }

        string TabPathCreate { get; }

        byte[] Create(byte[] input, uint seed);
    }
}
