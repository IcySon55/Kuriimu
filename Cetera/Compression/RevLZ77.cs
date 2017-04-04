using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cetera.IO;
using System.IO;

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
                reversedOffsetTablePos = 4098 * 2;
                byteTablePos = (4098 + 4098) * 2;
                endTablePos = (4098 + 4098 + 256) * 2;

                for (int i = 0; i < 256 * 2; i += 2)
                {
                    work[byteTablePos + i] = 0xFF;
                    work[byteTablePos + i + 1] = 0xFF;
                    work[endTablePos + i] = 0xFF;
                    work[endTablePos + i + 1] = 0xFF;
                }
            }
            public short WindowPos;
            public short WindowLen;
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

                    byte[] work = new byte[(4098 + 4098 + 256 + 256) * 2];

                    do
                    {
                        SCompressInfo info = new SCompressInfo(work);
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
                                int nSize = Search(work, br, pSrc, info, out nOffset, Math.Min(Math.Min(nMaxSize, pSrc), (int)br.BaseStream.Length - pSrc));

                                if (nSize < 3)
                                {
                                    if (pDest < 1)
                                    {
                                        bResult = false;
                                        break;
                                    }
                                    Slide(work, br, pSrc, info, 1);
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
                                    Slide(work, br, pSrc, info, nSize);
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
                        result[uCompFooterOffset] = (byte)(i1 & 0xFF); result[uCompFooterOffset + 1] = (byte)(i1 & 0xFF00);
                        result[uCompFooterOffset + 2] = (byte)(i1 & 0xFF0000); result[uCompFooterOffset + 3] = (byte)(i1 & 0xFF000000);
                        int i2 = a_uUncompressedSize - a_uCompressedSize;
                        result[uCompFooterOffset + 4] = (byte)(i2 & 0xFF); result[uCompFooterOffset + 5] = (byte)(i2 & 0xFF00);
                        result[uCompFooterOffset + 6] = (byte)(i2 & 0xFF0000); result[uCompFooterOffset + 7] = (byte)(i2 & 0xFF000000);
                    }
                }

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
            short uWindowPos = a_pInfo.WindowPos;
            short uWindowLen = a_pInfo.WindowLen;
            int pReverseOffsetTable = a_pInfo.reversedOffsetTablePos;

            br.BaseStream.Position = a_pSrc - 1;
            short tmpPos = (short)(br.ReadByte() * 2);
            short tmp = (short)(work[a_pInfo.endTablePos + tmpPos] | work[a_pInfo.endTablePos + tmpPos + 1] << 8);

            for (short nOffset = tmp; nOffset != -1; nOffset = (short)(work[pReverseOffsetTable + nOffset * 2] | (work[pReverseOffsetTable + nOffset * 2 + 1] << 8)))
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

                br.BaseStream.Position = pSearch - nCurrentSize - 1;
                byte i5 = br.ReadByte();
                br.BaseStream.Position = a_pSrc - nCurrentSize - 1;
                byte i6 = br.ReadByte();
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

        public static void Slide(byte[] worko, BinaryReaderX br, int a_pSrc, SCompressInfo info, int a_nSize)
        {
            for (int i = 0; i < a_nSize; i++)
            {
                SlideByte(worko, br, info, a_pSrc--);
            }
        }

        public static void SlideByte(byte[] worko, BinaryReaderX br, SCompressInfo info, int a_pSrc)
        {
            br.BaseStream.Position = a_pSrc - 1;
            byte uInData = br.ReadByte();
            short uInsertOffset = 0;
            short uWindowPos = info.WindowPos;
            short uWindowLen = info.WindowLen;

            int pOffsetTable = info.offsetTablePos;
            int pReversedOffsetTable = info.reversedOffsetTablePos;
            int pByteTable = info.byteTablePos;
            int pEndTable = info.endTablePos;

            if (uWindowLen == 4098)
            {
                br.BaseStream.Position = a_pSrc + 4097;
                byte uOutData = br.ReadByte();



                short tmp1 = (short)(worko[pByteTable + uOutData * 2] | worko[pByteTable + uOutData * 2 + 1] << 8);
                if ((worko[pByteTable + uOutData * 2] = worko[pOffsetTable + tmp1 * 2]) == 0xFF && (worko[pByteTable + uOutData * 2 + 1] = worko[pOffsetTable + tmp1 * 2 + 1]) == 0xFF)
                {
                    worko[pEndTable + uOutData * 2] = 0xFF;
                    worko[pEndTable + uOutData * 2 + 1] = 0xFF;
                }
                else
                {
                    short tmp2 = (short)(worko[pByteTable + uOutData * 2] | worko[pByteTable + uOutData * 2 + 1] << 8);
                    worko[pReversedOffsetTable + tmp2 * 2] = 0xFF;
                    worko[pReversedOffsetTable + tmp2 * 2 + 1] = 0xFF;
                }
                uInsertOffset = uWindowPos;
            }
            else
            {
                uInsertOffset = uWindowLen;
            }

            short nOffset = (short)(worko[pEndTable + uInData * 2] | worko[pEndTable + uInData * 2 + 1] << 8);
            if (nOffset == -1)
            {
                worko[pByteTable + uInData * 2] = (byte)(uInsertOffset & 0xFF);
                worko[pByteTable + uInData * 2 + 1] = (byte)(uInsertOffset >> 8);
            }
            else
            {
                worko[pOffsetTable + nOffset * 2] = (byte)(uInsertOffset & 0xFF);
                worko[pOffsetTable + nOffset * 2 + 1] = (byte)(uInsertOffset >> 8);
            }

            worko[pEndTable + uInData * 2] = (byte)(uInsertOffset & 0xFF);
            worko[pEndTable + uInData * 2 + 1] = (byte)(uInsertOffset >> 8);

            worko[pOffsetTable + uInsertOffset * 2] = 0xFF;
            worko[pOffsetTable + uInsertOffset * 2 + 1] = 0xFF;

            worko[pReversedOffsetTable + uInsertOffset * 2] = (byte)(nOffset & 0xFF);
            worko[pReversedOffsetTable + uInsertOffset * 2 + 1] = (byte)(nOffset >> 8);

            if (uWindowLen == 4098)
            {
                info.WindowPos = (short)((uWindowPos + 1) % 4098);
            }
            else
            {
                info.WindowLen++;
            }
        }
    }
}
