using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.IO;

namespace Cetera.Compression
{
    public sealed class RevLZ77
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CompFooter
        {
            public int bufferTopAndBottom;
            public int originalBottom;
        }
        public class SCompressInfo
        {
            public SCompressInfo(byte[] work)
            {
                WindowLen = 0;
                WindowPos = 0;
                offsetTablePos = 0;
                reversedOffsetTablePos = 4098;
                byteTablePos = 4098 + 4098;
                endTablePos = 4098 + 4098 + 256;

                for (int i = 0; i < 256 * 2; i += 2)
                {
                    work[byteTablePos * 2 + i] = 0xFF;
                    work[byteTablePos * 2 + i + 1] = 0xFF;
                    work[endTablePos * 2 + i] = 0xFF;
                    work[endTablePos * 2 + i + 1] = 0xFF;
                }
            }
            public ushort WindowPos;
            public ushort WindowLen;
            public short offsetTablePos;
            public short reversedOffsetTablePos;
            public short byteTablePos;
            public short endTablePos;
        }

        public static byte[] Decompress(byte[] input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var compFooter = input.ToStruct<CompFooter>(input.Length - 8);
            int dest = input.Length + compFooter.originalBottom;
            int src = input.Length - (compFooter.bufferTopAndBottom >> 24 & 0xFF);
            int end = input.Length - (compFooter.bufferTopAndBottom & 0xFFFFFF);
            Array.Resize(ref input, dest);

            while (true)
            {
                int flag = input[--src], mask = 0x80;
                do
                {
                    if ((flag & mask) == 0)
                    {
                        input[--dest] = input[--src];
                    }
                    else
                    {
                        int size = input[--src];
                        int offset = (((size & 0x0F) << 8) | input[--src]) + 3;
                        size = (size >> 4 & 0x0F) + 3;

                        for (int j = 0; j < size; j++)
                        {
                            input[--dest] = input[dest + offset];
                        }
                    }

                    if (src - end <= 0)
                    {
                        return input;
                    }
                } while ((mask >>= 1) != 0);
            }
        }

        public static byte[] Compress(byte[] input)
        {
            int a_uUncompressedSize = input.Length;
            int a_uCompressedSize = input.Length;
            byte[] result = new byte[input.Length];

            bool bResult = true;

            using (BinaryReaderX br = new BinaryReaderX(new MemoryStream(input)))
            {
                if (a_uUncompressedSize > 8 && a_uCompressedSize >= a_uUncompressedSize)
                {

                    byte[] pWork = new byte[(4098 + 4098 + 256 + 256) * 2];

                    do
                    {
                        SCompressInfo info = new SCompressInfo(pWork);
                        int nMaxSize = 0xF + 3;
                        int pSrc = a_uUncompressedSize;
                        int pDest = a_uUncompressedSize;
                        while (pSrc > 0 && pDest > 0)
                        {
                            int pFlag = --pDest;
                            result[pFlag] = 0;

                            for (int i = 0; i < 8; i++)
                            {
                                int nOffset;
                                int nSize = Search(pWork, br, pSrc, info, out nOffset, Math.Min(Math.Min(nMaxSize, pSrc), (int)br.BaseStream.Length - pSrc));

                                if (nSize < 3)
                                {
                                    if (pDest < 1)
                                    {
                                        bResult = false;
                                        break;
                                    }
                                    Slide(pWork, br, pSrc, info, 1);
                                    br.BaseStream.Position = --pSrc;
                                    result[--pDest] = br.ReadByte();
                                }
                                else
                                {
                                    if (pDest < 2)
                                    {
                                        bResult = false;
                                        break;
                                    }

                                    result[pFlag] |= (byte)(0x80 >> i);
                                    Slide(pWork, br, pSrc, info, nSize);
                                    pSrc -= nSize;
                                    nSize -= 3;
                                    result[--pDest] = (byte)((nSize << 4 & 0xF0) | ((nOffset - 3) >> 8 & 0x0F));
                                    result[--pDest] = (byte)((nOffset - 3) & 0xFF);
                                }

                                if (pSrc <= 0)
                                {
                                    break;
                                }
                            }

                            if (!bResult)
                            {
                                break;
                            }
                        }

                        if (!bResult)
                        {
                            break;
                        }

                        a_uCompressedSize = a_uUncompressedSize - pDest;
                    } while (false);
                }
                else
                {
                    bResult = false;
                }

                if (bResult)
                {
                    int uOrigSize = a_uUncompressedSize;
                    int pCompressBuffer = a_uUncompressedSize - a_uCompressedSize;
                    int uCompressBufferSize = a_uCompressedSize;
                    int uOrigSafe = 0;
                    int uCompressSafe = 0;
                    bool bOver = false;

                    while (uOrigSize > 0)
                    {
                        byte uFlag = result[pCompressBuffer + (--uCompressBufferSize)];

                        for (int i = 0; i < 8; i++)
                        {
                            if ((uFlag << i & 0x80) == 0)
                            {
                                uCompressBufferSize--;
                                uOrigSize--;
                            }
                            else
                            {
                                int nSize = (result[pCompressBuffer + (--uCompressBufferSize)] >> 4 & 0x0F) + 3;
                                uCompressBufferSize--;
                                uOrigSize -= nSize;

                                if (uOrigSize < uCompressBufferSize)
                                {
                                    uOrigSafe = uOrigSize;
                                    uCompressSafe = uCompressBufferSize;
                                    bOver = true;
                                    break;
                                }
                            }

                            if (uOrigSize <= 0)
                            {
                                break;
                            }
                        }

                        if (bOver)
                        {
                            break;
                        }
                    }

                    int uCompressedSize = a_uCompressedSize - uCompressSafe;
                    int uPadOffset = uOrigSafe + uCompressedSize;
                    int uCompFooterOffset = (uPadOffset + 4 - 1) / 4 * 4;
                    a_uCompressedSize = uCompFooterOffset + 8;
                    int uTop = a_uCompressedSize - uOrigSafe;
                    int uBottom = a_uCompressedSize - uPadOffset;

                    if (a_uCompressedSize >= a_uUncompressedSize || uTop > 0xFFFFFF)
                    {
                        bResult = false;
                    }
                    else
                    {
                        //memcpy
                        br.BaseStream.Position = 0;
                        for (int i = 0; i < uOrigSafe; i++) result[i] = br.ReadByte();

                        //memmove
                        byte[] tmp = new byte[uCompressedSize];
                        for (int i = pCompressBuffer + uCompressSafe; i < pCompressBuffer + uCompressSafe + uCompressedSize; i++) tmp[i - (pCompressBuffer + uCompressSafe)] = result[i];
                        for (int i = uOrigSafe; i < uOrigSafe + tmp.Length; i++) result[i] = tmp[i - uOrigSafe];

                        //memset
                        for (int i = uPadOffset; i < uPadOffset + (uCompFooterOffset - uPadOffset); i++) result[i] = 0xFF;

                        int i1 = uTop | (uBottom << 24);
                        result[uCompFooterOffset] = (byte)(i1 & 0xFF); result[uCompFooterOffset + 1] = (byte)(i1 >> 8);
                        result[uCompFooterOffset + 2] = (byte)(i1 >> 16); result[uCompFooterOffset + 3] = (byte)(i1 >> 24);
                        int i2 = a_uUncompressedSize - a_uCompressedSize;
                        result[uCompFooterOffset + 4] = (byte)(i2 & 0xFF); result[uCompFooterOffset + 5] = (byte)(i2 >> 8);
                        result[uCompFooterOffset + 6] = (byte)(i2 >> 16); result[uCompFooterOffset + 7] = (byte)(i2 >> 24);
                    }
                }

                if (!bResult)
                    return null;
                return GetByteArray(result, a_uCompressedSize);
            }
        }

        public static byte[] GetByteArray(byte[] input, int size)
        {
            byte[] result = new byte[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = input[i];
            }
            return result;
        }

        public static int Search(byte[] work, BinaryReaderX br, int a_pSrc, SCompressInfo a_pInfo, out int a_nOffset, int a_nMaxSize)
        {
            a_nOffset = 0;

            if (a_nMaxSize < 3)
            {
                return 0;
            }

            int pSearch;
            int nSize = 2;
            ushort uWindowPos = a_pInfo.WindowPos;
            ushort uWindowLen = a_pInfo.WindowLen;
            int pReversedOffsetTable = a_pInfo.reversedOffsetTablePos;

            br.BaseStream.Position = a_pSrc - 1;
            short pos = br.ReadByte();

            for (short nOffset = GetShort(work, a_pInfo.endTablePos + pos); nOffset != -1; nOffset = GetShort(work, a_pInfo.reversedOffsetTablePos + nOffset))
            {
                if (nOffset < uWindowPos)
                {
                    pSearch = a_pSrc + uWindowPos - nOffset;
                }
                else
                {
                    pSearch = a_pSrc + uWindowPos + uWindowLen - nOffset;
                }

                if (pSearch - a_pSrc < 3)
                {
                    continue;
                }

                br.BaseStream.Position = pSearch - 3;
                byte i1 = br.ReadByte();
                byte i2 = br.ReadByte();
                br.BaseStream.Position = a_pSrc - 3;
                byte i3 = br.ReadByte();
                byte i4 = br.ReadByte();
                if (i2 != i4 || i1 != i3)
                {
                    continue;
                }

                int nMaxSize = Math.Min(a_nMaxSize, pSearch - a_pSrc);
                int nCurrentSize = 3;

                byte i5 = 0;
                byte i6 = 0;
                if (nCurrentSize < nMaxSize)
                {
                    br.BaseStream.Position = pSearch - nCurrentSize - 1;
                    i5 = br.ReadByte();
                    br.BaseStream.Position = a_pSrc - nCurrentSize - 1;
                    i6 = br.ReadByte();
                }
                while (nCurrentSize < nMaxSize && i5 == i6)
                {
                    nCurrentSize++;
                    if (nCurrentSize < nMaxSize)
                    {
                        br.BaseStream.Position = pSearch - nCurrentSize - 1;
                        i5 = br.ReadByte();
                        br.BaseStream.Position = a_pSrc - nCurrentSize - 1;
                        i6 = br.ReadByte();
                    }
                }

                if (nCurrentSize > nSize)
                {
                    nSize = nCurrentSize;
                    a_nOffset = pSearch - a_pSrc;
                    if (nSize == a_nMaxSize)
                    {
                        break;
                    }
                }
            }

            if (nSize < 3)
            {
                return 0;
            }

            return nSize;
        }

        public static void Slide(byte[] work, BinaryReaderX br, int a_pSrc, SCompressInfo info, int a_nSize)
        {
            for (int i = 0; i < a_nSize; i++)
            {
                SlideByte(work, br, info, a_pSrc--);
            }
            //Console.WriteLine(info.WindowLen + "   " + info.WindowPos);
        }

        public static void SlideByte(byte[] work, BinaryReaderX br, SCompressInfo info, int a_pSrc)
        {
            br.BaseStream.Position = a_pSrc - 1;
            byte uInData = br.ReadByte();
            ushort uInsertOffset = 0;
            ushort uWindowPos = info.WindowPos;
            ushort uWindowLen = info.WindowLen;

            int pOffsetTable = info.offsetTablePos;
            int pReversedOffsetTable = info.reversedOffsetTablePos;
            int pByteTable = info.byteTablePos;
            int pEndTable = info.endTablePos;

            if (uWindowLen == 4098)
            {
                br.BaseStream.Position = a_pSrc + 4097;
                byte uOutData = br.ReadByte();

                WriteShort(work, pByteTable + uOutData, GetShort(work, pOffsetTable + GetShort(work, pByteTable + uOutData)));
                if (GetShort(work, pByteTable + uOutData) == -1)
                {
                    WriteShort(work, pEndTable + uOutData, -1);
                }
                else
                {
                    WriteShort(work, pReversedOffsetTable + GetShort(work, pByteTable + uOutData), -1);
                }

                uInsertOffset = uWindowPos;
            }
            else
            {
                uInsertOffset = uWindowLen;
            }

            short nOffset = GetShort(work, pEndTable + uInData);
            if (nOffset == -1)
            {
                WriteShort(work, pByteTable + uInData, (short)uInsertOffset);
            }
            else
            {
                WriteShort(work, pOffsetTable + nOffset, (short)uInsertOffset);
            }

            WriteShort(work, pEndTable + uInData, (short)uInsertOffset);
            WriteShort(work, pOffsetTable + uInsertOffset, -1);
            WriteShort(work, pReversedOffsetTable + uInsertOffset, nOffset);

            if (uWindowLen == 4098)
            {
                info.WindowPos = (ushort)((uWindowPos + 1) % 4098);
            }
            else
            {
                info.WindowLen++;
            }
        }

        public static short GetShort(byte[] input, int offset)
        {
            return (short)(input[offset * 2] | (input[offset * 2 + 1] << 8));
        }

        public static void WriteShort(byte[] input, int offset, short value)
        {
            input[offset * 2] = (byte)(value & 0xFF);
            input[offset * 2 + 1] = (byte)(value >> 8);
        }
    }
}
