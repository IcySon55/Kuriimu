using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontract.Hash
{
    public static class FNV
    {
        private static uint FNV1Core(byte[] ba, uint hashInit, uint FNVPrime) => ba.Aggregate(hashInit, (o, i) => (o * FNVPrime) ^ i);

        private static uint FNV1aCore(byte[] ba, uint hashInit, uint FNVPrime) => ba.Aggregate(hashInit, (o, i) => (o ^ i) * FNVPrime);

        public static class FNV132
        {
            public static uint Create(string str) => Create(Encoding.ASCII.GetBytes(str));
            public static uint Create(byte[] ba) => FNV1Core(ba, 0x811c9dc5, 0x1000193);
        }

        public static class FNV1a32
        {
            public static uint Create(string str) => Create(Encoding.ASCII.GetBytes(str));
            public static uint Create(byte[] ba) => FNV1aCore(ba, 0x811c9dc5, 0x1000193);
        }
    }
}
