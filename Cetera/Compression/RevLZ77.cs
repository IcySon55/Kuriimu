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
            public uint bufferTopAndBottom;
            public uint originalBottom;
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

        public static byte[] Decompress(byte[] input, uint decompSize)
        {
            bool res = true;
            List<byte> result = new List<byte>();

            if (input.Length >= 8)
            {
                for (int i = 0; i < decompSize; i++) result.Add(0x00);

                using (BinaryReaderX br = new BinaryReaderX(new MemoryStream(input)))
                {
                    br.BaseStream.Position = br.BaseStream.Length - 8; CompFooter pCompFooter = br.ReadStruct<CompFooter>();
                    uint uTop = pCompFooter.bufferTopAndBottom & 0xFFFFFF;
                    uint uBottom = pCompFooter.bufferTopAndBottom >> 24 & 0xFF;
                    if (uBottom >= 8 && uBottom <= 8 + 3 && uTop >= uBottom && uTop <= br.BaseStream.Length && decompSize >= br.BaseStream.Length + pCompFooter.originalBottom)
                    {
                        decompSize = (uint)br.BaseStream.Length + pCompFooter.originalBottom;
                        br.BaseStream.Position = 0; for (int i = 0; i < br.BaseStream.Length; i++) result[i] = br.ReadByte();
                        uint destPos = decompSize;
                        uint srcPos = (uint)br.BaseStream.Length - uBottom;
                        uint end = (uint)br.BaseStream.Length - uTop;

                        while (srcPos - end > 0)
                        {
                            br.BaseStream.Position = --srcPos;
                            byte uFlag = br.ReadByte();

                            for (int i = 0; i < 8; i++)
                            {
                                if ((uFlag << i & 0x80) == 0)
                                {
                                    if (destPos - end < 1 || srcPos - end < 1)
                                    {
                                        res = false;
                                        break;
                                    }
                                    br.BaseStream.Position = --srcPos;
                                    byte get = br.ReadByte();
                                    result[(int)--destPos] = get;
                                }
                                else
                                {
                                    if (srcPos - end < 2)
                                    {
                                        res = false;
                                        break;
                                    }
                                    br.BaseStream.Position = --srcPos;
                                    int nSize = br.ReadByte();

                                    br.BaseStream.Position = --srcPos;
                                    int nOffset = (((nSize & 0x0F) << 8) | br.ReadByte()) + 3;

                                    nSize = (nSize >> 4 & 0x0F) + 3;

                                    if (nSize > destPos - end)
                                    {
                                        res = false;
                                        break;
                                    }

                                    uint dataPos = destPos + (uint)nOffset;
                                    if (dataPos > decompSize)
                                    {
                                        res = false;
                                        break;
                                    }

                                    for (int j = 0; j < nSize; j++)
                                    {
                                        byte get = result[(int)--dataPos];
                                        result[(int)--destPos] = get;
                                    }
                                }

                                if (srcPos - end <= 0)
                                {
                                    break;
                                }
                            }

                            if (res == false)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        res = false;
                    }
                }
            }
            else
            {
                res = false;
            }

            if (res)
            {
                return result.ToArray();
            }
            else
            {
                throw new Exception("Decompression failed!");
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
                        result[uCompFooterOffset] = (byte)(i1 & 0xFF); result[uCompFooterOffset + 1] = (byte)((i1 & 0xFF00) >> 8);
                        result[uCompFooterOffset + 2] = (byte)((i1 & 0xFF0000) >> 16); result[uCompFooterOffset + 3] = (byte)((i1 & 0xFF000000) >> 24);
                        int i2 = a_uUncompressedSize - a_uCompressedSize;
                        result[uCompFooterOffset + 4] = (byte)(i2 & 0xFF); result[uCompFooterOffset + 5] = (byte)((i2 & 0xFF00) >> 8);
                        result[uCompFooterOffset + 6] = (byte)((i2 & 0xFF0000) >> 16); result[uCompFooterOffset + 7] = (byte)((i2 & 0xFF000000) >> 24);

                        Console.WriteLine(i1 + "   " + i2);
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
