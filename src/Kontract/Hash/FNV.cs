using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontract.Hash
{
    public static class FNV
    {
        private static uint FNVCore(string str, uint hashInit, uint FNVPrime) => str.Aggregate(hashInit, (o, i) => (o * FNVPrime) ^ i);

        public static class FNV132
        {
            public static uint Create(string str) => FNVCore(str, 0x811c9dc5, 0x1000193);
        }
    }
}
