using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.IO;

namespace Kontract.Compression
{
    public class LZ77
    {
        public static int searchBufferSize = 250;
        public static int lookAheadBufferSize = 250;

        public static byte[] Decompress(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                List<byte> result = new List<byte>();
                BitArray ba = new BitArray(br.ReadBytes((int)br.BaseStream.Length));
                GetBigEndian(ba);
                br.BaseStream.Position = 0;

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
        }

        public static byte[] Compress(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                List<bool> result = new List<bool>();

                List<byte> searchBuffer = new List<byte>();
                for (int i = 0; i < searchBufferSize; i++) searchBuffer.Add(0x00);
                List<byte> NonePlaceholder = new List<byte>();
                for (int i = 0; i < searchBufferSize; i++) NonePlaceholder.Add(0x00);

                List<byte> lookAheadBuffer = br.ReadBytes(lookAheadBufferSize).ToList();

                int bk;
                int count = 0;

                while (lookAheadBuffer.Count() != 0)
                {
                    byte length; byte offset; byte symbol;
                    Search(lookAheadBuffer, searchBuffer, NonePlaceholder, out length, out offset);
                    count++;

                    try
                    {
                        symbol = lookAheadBuffer[length];
                    }
                    catch
                    {
                        symbol = 0x24;
                    }

                    if (length > 0)
                    {
                        if (symbol == 0x24 && br.BaseStream.Length - br.BaseStream.Position > 0)
                        {
                            symbol = br.ReadByte();

                            searchBuffer.AddRange(lookAheadBuffer); searchBuffer.Add(symbol);
                            for (int i = 0; i < lookAheadBuffer.Count(); i++) NonePlaceholder.Add(1); NonePlaceholder.Add(1);

                            bk = searchBuffer.Count();
                            searchBuffer = GetLastElements(searchBuffer, bk - (length + 1));
                            NonePlaceholder = GetLastElements(NonePlaceholder, bk - (length + 1));

                            lookAheadBuffer.AddRange(br.ReadBytes(length + 1));
                            lookAheadBuffer = GetLastElements(lookAheadBuffer, lookAheadBuffer.Count() - (length + 1));
                        }
                        else
                        {
                            searchBuffer.AddRange(GetFirstElements(lookAheadBuffer, length + 1));
                            for (int i = 0; i < length + 1; i++) NonePlaceholder.Add(1);

                            bk = searchBuffer.Count();
                            searchBuffer = GetLastElements(searchBuffer, bk - (length + 1));
                            NonePlaceholder = GetLastElements(NonePlaceholder, bk - (length + 1));

                            lookAheadBuffer.AddRange(br.ReadBytes(length + 1));
                            lookAheadBuffer = GetLastElements(lookAheadBuffer, lookAheadBuffer.Count() - (length + 1));
                        }
                    }
                    else
                    {
                        bk = searchBuffer.Count();
                        searchBuffer = GetLastElements(searchBuffer, bk - 1);
                        NonePlaceholder = GetLastElements(NonePlaceholder, bk - 1);

                        byte e = lookAheadBuffer[0]; lookAheadBuffer = GetLastElements(lookAheadBuffer, lookAheadBuffer.Count() - 1);

                        searchBuffer.Add(e);
                        NonePlaceholder.Add(1);

                        try
                        {
                            e = br.ReadByte();
                            lookAheadBuffer.Add(e);
                        }
                        catch
                        {

                        }
                    }

                    if (offset != 0)
                    {
                        result.Add(true);
                        result.AddRange(GetBoolArray(offset));
                        result.AddRange(GetBoolArray(length));
                        result.AddRange(GetBoolArray(symbol));
                    }
                    else
                    {
                        result.Add(false);
                        result.AddRange(GetBoolArray(symbol));
                    }
                }

                while (result.Count() % 8 != 0)
                {
                    result.Add(false);
                }

                return GetByteArray(result);
            }
        }

        //Support functions
        public static void Search(List<byte> lookAheadBuffer, List<byte> searchBuffer, List<byte> NonePlaceholder, out byte length, out byte offset)
        {
            offset = 0; length = 0;

            for (byte i = 1; i < searchBufferSize + 1; i++)
            {
                byte l = 0;
                if (NonePlaceholder[searchBufferSize - i] == 0x00)
                {
                    if (l >= length)
                    {
                        length = l;
                        offset = i;
                    }
                    break;
                }

                for (int j = 0; j < i; j++)
                {
                    if (j >= lookAheadBuffer.Count() - 1)
                    {
                        if (l >= length)
                        {
                            length = l;
                            offset = i;
                        }
                        break;
                    }

                    if (searchBuffer[searchBufferSize - i + j] == lookAheadBuffer[j])
                    {
                        l += 1;
                    }
                    else
                    {
                        if (l >= length)
                        {
                            length = l;
                            offset = i;
                        }
                        break;
                    }

                    if (l == i)
                    {
                        if (l >= length)
                        {
                            length = l;
                            offset = i;
                        }
                        break;
                    }
                }
            }
        }

        public static List<byte> GetFirstElements(List<byte> sb, int nrElem)
        {
            List<byte> res = new List<byte>();

            for (int i = 0; i < nrElem; i++)
            {
                res.Add(sb[i]);
            }

            return res;
        }
        public static List<byte> GetLastElements(List<byte> sb, int nrElem)
        {
            List<byte> res = new List<byte>();

            for (int i = sb.Count() - nrElem; i < sb.Count(); i++)
            {
                res.Add(sb[i]);
            }

            return res;
        }

        public static byte[] GetByteArray(List<bool> boolA)
        {
            byte[] result = new byte[boolA.Count / 8];

            for (int i = 0; i < result.Count(); i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (boolA[i * 8 + j])
                        result[i] |= (byte)(1 << (7 - j));
                }
            }

            return result;
        }
        public static bool[] GetBoolArray(byte inp)
        {
            bool[] result = new bool[8];
            for (byte i = 0; i < 8; i++)
            {
                if ((inp >> (7 - i)) % 2 == 0)
                {
                    result[i] = false;
                }
                else
                {
                    result[i] = true;
                }
            }

            return result;
        }

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
