using System.ComponentModel.Composition;
using System.Linq;
using Komponent.Interface;

namespace Komponent.CTR.Hash
{
    [ExportMetadata("Name", "3DS SimpleHash")]
    [ExportMetadata("TabPathCreate", "Custom/SimpleHash")]
    [Export("SimpleHash_3DS", typeof(IHash))]
    [Export(typeof(IHash))]
    public class SimpleHash3DS : IHash
    {
        public byte[] Create(byte[] input, uint seed = 0)
        {
            var aggHash = input.Aggregate(0u, (hash, c) => hash * seed + c);

            var retValue = new byte[] { (byte)((aggHash) >> 24), (byte)((aggHash) >> 16), (byte)((aggHash) >> 8), (byte)(aggHash) };
            return (retValue);
        }
    }
}
