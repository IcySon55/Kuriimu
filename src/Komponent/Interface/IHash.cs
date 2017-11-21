using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komponent.Interface
{
    public interface IHash
    {
        byte[] Create(byte[] input, uint seed);
    }

    public interface IHashMetadata
    {
        [DefaultValue("")]
        string Name { get; }

        [DefaultValue("")]
        string TabPathCreate { get; }
    }
}
