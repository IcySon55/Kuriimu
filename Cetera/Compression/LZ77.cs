using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cetera.IO;
using System.IO;
using System.Collections;

namespace Cetera.Compression
{
    public class LZ77
    {
        public static byte[] Decompress(byte[] input)
        {
            List<byte> result = new List<byte>();
            BitArray ba = new BitArray(input);
            GetBigEndian(ba);

            bool flag; byte symbol;
            int pos = 0; byte offset; byte length;
            while (ba.Length - pos >= 9)
            {
                flag = ba[pos++];
                if (flag == false)
                {
                    symbol = GetByte(ba, pos);
                    offset = 0;
                    length = 0;
                    pos += 8;
                }
                else
                {
                    offset = GetByte(ba, pos);
                    length = GetByte(ba, pos + 8);
                    symbol = GetByte(ba, pos + 16);
                    pos += 24;
                }

                result.AddRange(GetArray(offset, length, symbol, result));
            }

            return result.ToArray();
        }

        public static byte[] Compress(byte[] input)
        {
            return new byte[] { 0, 0 };
        }

        //Support functions
        private static void GetBigEndian(BitArray ba)
        {
            for (int i = 0; i < ba.Length; i += 8)
            {
                bool help1 = ba[i]; bool help2 = ba[i + 1]; bool help3 = ba[i + 2]; bool help4 = ba[i + 3];
                ba[i] = ba[i + 7]; ba[i + 1] = ba[i + 6]; ba[i + 2] = ba[i + 5]; ba[i + 3] = ba[i + 4];
                ba[i + 4] = help4; ba[i + 5] = help3; ba[i + 6] = help2; ba[i + 7] = help1;
            }
        }

        private static List<byte> GetArray(byte offset, byte length, byte symbol, List<byte> result)
        {
            List<byte> tmp = new List<byte>();

            if (-offset + length >= 0)
            {
                for (int i = offset; i > 0; i--)
                {
                    if (result.Count() - i >= 0)
                    {
                        tmp.Add(result[result.Count() - i]);
                    }
                }

                for (int i = 0; i < -offset + length; i++)
                {
                    tmp.Add(result[i]);
                }
            }
            else
            {
                for (int i = offset; i > offset - length; i--)
                {
                    if (result.Count() - i >= 0)
                    {
                        tmp.Add(result[result.Count() - i]);
                    }
                }
            }

            tmp.Add(symbol);

            return tmp;
        }

        private static byte GetByte(BitArray ba, int pos)
        {
            byte res = 0;
            for (int i = 0; i < 8; i++)
            {
                res += (byte)(Convert.ToInt32(ba.Get(pos + i)) << (7 - i));
            }
            return res;
        }
    }
}
