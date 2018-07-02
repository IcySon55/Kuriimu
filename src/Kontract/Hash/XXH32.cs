using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontract.Hash
{
    public class XXH32
    {
        const uint PRIME32_1 = 2654435761U;
        const uint PRIME32_2 = 2246822519U;
        const uint PRIME32_3 = 3266489917U;
        const uint PRIME32_4 = 668265263U;
        const uint PRIME32_5 = 374761393U;

        public static uint Create(byte[] input, int len = -1, uint seed = 0)
        {
            uint h32;
            int index = 0;
            if (len == -1)
            {
                len = input.Length;
            }


            if (len >= 16)
            {
                int limit = len - 16;
                uint v1 = seed + PRIME32_1 + PRIME32_2;
                uint v2 = seed + PRIME32_2;
                uint v3 = seed + 0;
                uint v4 = seed - PRIME32_1;

                do
                {
                    v1 = CalcSubHash(v1, input, index);
                    index += 4;
                    v2 = CalcSubHash(v2, input, index);
                    index += 4;
                    v3 = CalcSubHash(v3, input, index);
                    index += 4;
                    v4 = CalcSubHash(v4, input, index);
                    index += 4;
                } while (index <= limit);

                h32 = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
            }
            else
            {
                h32 = seed + PRIME32_5;
            }

            h32 += (uint)len;

            while (index <= len - 4)
            {
                h32 += BitConverter.ToUInt32(input, index) * PRIME32_3;
                h32 = RotateLeft(h32, 17) * PRIME32_4;
                index += 4;
            }

            while (index < len)
            {
                h32 += input[index] * PRIME32_5;
                h32 = RotateLeft(h32, 11) * PRIME32_1;
                index++;
            }

            h32 ^= h32 >> 15;
            h32 *= PRIME32_2;
            h32 ^= h32 >> 13;
            h32 *= PRIME32_3;
            h32 ^= h32 >> 16;

            return h32;
        }

        private static uint CalcSubHash(uint value, byte[] buf, int index)
        {
            uint read_value = BitConverter.ToUInt32(buf, index);
            value += read_value * PRIME32_2;
            value = RotateLeft(value, 13);
            value *= PRIME32_1;
            return value;
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
    }
}
