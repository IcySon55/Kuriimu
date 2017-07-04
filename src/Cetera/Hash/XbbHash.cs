using System.Linq;

namespace Cetera.Hash
{
    public class XbbHash
    {
        public static uint Create(string input)
        {
            return (uint)input.Aggregate(new int[] { 0, 0 }, (p, c) => new int[] { p[0] + (c << (p[1] += c) | c >> -p[1]), p[1] })[0];
        }
    }
}
