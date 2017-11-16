using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;

namespace Hash
{
    [Export("SimpleHash_3DS", typeof(IHash))]
    [Export(typeof(IHash))]
    public class SimpleHash3DS : IHash
    {
        public string Name { get; } = "3DS SimpleHash";

        public string TabPathCreate { get; } = "Custom/SimpleHash";

        public byte[] Create(byte[] input, uint seed = 0)
        {
            var aggHash = input.Aggregate(0u, (hash, c) => hash * seed + c);

            var retValue = new byte[] { (byte)((aggHash) >> 24), (byte)((aggHash) >> 16), (byte)((aggHash) >> 8), (byte)(aggHash) };
            return (retValue);
        }
    }
}
