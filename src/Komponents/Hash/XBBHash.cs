using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;

namespace Hash
{
    [ExportMetadata("Name", "XBB Hash")]
    [ExportMetadata("TabPathCreate", "Custom/XBB")]
    [Export("XBBHash", typeof(IHash))]
    [Export(typeof(IHash))]
    public class XBBHash : IHash
    {
        public byte[] Create(byte[] input, uint seed = 0)
        {
            var aggHash = (uint)input.Aggregate(new int[] { 0, 0 }, (p, c) => new int[] { p[0] + (c << (p[1] += c) | c >> -p[1]), p[1] })[0];

            var retValue = new byte[] { (byte)((aggHash) >> 24), (byte)((aggHash) >> 16), (byte)((aggHash) >> 8), (byte)(aggHash) };
            return (retValue);
        }
    }
}
