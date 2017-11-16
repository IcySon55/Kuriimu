using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using Kontract.IO;
using Kontract.Interface;
using System.IO;

namespace Compression
{
    [ExportMetadata("Name", "RevLZ77")]
    [ExportMetadata("TabPathCompress", "Nintendo/RevLZ77")]
    [ExportMetadata("TabPathDecompress", "Nintendo/RevLZ77")]
    [Export("RevLZ77", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public sealed class RevLZ77 : ICompression
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CompFooter
        {
            public int bufferTopAndBottom;
            public int originalBottom;
        }
        private class SCompressInfo
        {
            int windowPos;
            int windowLen;
            int[] offsetTable = new int[4098];
            int[] reversedOffsetTable = new int[4098];
            int[] byteTable = Enumerable.Repeat(-1, 256).ToArray();
            int[] endTable = Enumerable.Repeat(-1, 256).ToArray();
            byte[] input;

            public SCompressInfo(byte[] input)
            {
                this.input = input;
            }

            public int Search(int src, out int offset, int maxSize)
            {
                offset = 0;

                if (maxSize < 3)
                {
                    return 0;
                }

                int size = 2;

                for (int nOffset = endTable[input[src - 1]]; nOffset != -1; nOffset = reversedOffsetTable[nOffset])
                {
                    int search = src + windowPos - nOffset;
                    if (nOffset >= windowPos)
                    {
                        search += windowLen;
                    }

                    if (search - src < 3)
                    {
                        continue;
                    }

                    if (input[search - 2] != input[src - 2] || input[search - 3] != input[src - 3])
                    {
                        continue;
                    }

                    int nMaxSize = Math.Min(maxSize, search - src);
                    int nCurrentSize = 3;
                    while (nCurrentSize < nMaxSize && input[search - nCurrentSize - 1] == input[src - nCurrentSize - 1])
                    {
                        nCurrentSize++;
                    }

                    if (nCurrentSize > size)
                    {
                        size = nCurrentSize;
                        offset = search - src;
                        if (size == maxSize)
                        {
                            break;
                        }
                    }
                }

                if (size < 3)
                {
                    return 0;
                }

                return size;
            }

            public void SlideByte(int src)
            {
                byte uInData = input[src];
                int uInsertOffset = 0;

                if (windowLen == 4098)
                {
                    byte uOutData = input[src + 4098];

                    if ((byteTable[uOutData] = offsetTable[byteTable[uOutData]]) == -1)
                    {
                        endTable[uOutData] = -1;
                    }
                    else
                    {
                        reversedOffsetTable[byteTable[uOutData]] = -1;
                    }

                    uInsertOffset = windowPos;
                }
                else
                {
                    uInsertOffset = windowLen;
                }

                int nOffset = endTable[uInData];
                if (nOffset == -1)
                {
                    byteTable[uInData] = uInsertOffset;
                }
                else
                {
                    offsetTable[nOffset] = uInsertOffset;
                }

                endTable[uInData] = uInsertOffset;
                offsetTable[uInsertOffset] = -1;
                reversedOffsetTable[uInsertOffset] = nOffset;

                if (windowLen == 4098)
                {
                    windowPos++;
                    windowPos %= 4098;
                }
                else
                {
                    windowLen++;
                }
            }
        }

        public byte[] Decompress(Stream instream, long decompSize = 0)
        {
            byte[] input = new BinaryReaderX(instream, true).ReadBytes((int)instream.Length);

            if (input == null) throw new ArgumentNullException(nameof(input));

            var compFooter = input.BytesToStruct<CompFooter>(input.Length - 8);
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

        public byte[] Compress(Stream instream)
        {
            byte[] input = new BinaryReaderX(instream, true).ReadBytes((int)instream.Length);

            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.Length <= 8) return null;

            var result = new byte[input.Length];

            var info = new SCompressInfo(input);
            const int maxSize = 0xF + 3;
            int src = input.Length;
            int dest = input.Length;

            while (src > 0 && dest > 0)
            {
                int flagPos = --dest, mask = 0x80;
                result[flagPos] = 0;

                do
                {
                    int nOffset;
                    int size = info.Search(src, out nOffset, Math.Min(Math.Min(maxSize, src), input.Length - src));

                    if (size < 3)
                    {
                        if (dest < 1)
                        {
                            return null;
                        }
                        info.SlideByte(--src);
                        result[--dest] = input[src];
                    }
                    else
                    {
                        if (dest < 2)
                        {
                            return null;
                        }

                        for (int i = 0; i < size; i++)
                            info.SlideByte(--src);
                        result[flagPos] |= (byte)mask;
                        result[--dest] = (byte)(((size - 3) << 4) | ((nOffset - 3) >> 8 & 0x0F));
                        result[--dest] = (byte)(nOffset - 3);
                    }

                    if (src <= 0)
                    {
                        break;
                    }
                } while ((mask >>= 1) != 0);
            }

            Func<int, int, byte[]> func = (origSize, unusedSize) =>
            {
                int compressedRegion = input.Length - unusedSize;
                int padOffset = origSize + compressedRegion;
                int compressedSize = padOffset + 11 & ~3;

                if (compressedSize >= Math.Min(input.Length, origSize + 0x1000000))
                {
                    return null;
                }

                var compFooter = new CompFooter
                {
                    bufferTopAndBottom = (compressedSize - origSize) | ((compressedSize - padOffset) << 24),
                    originalBottom = input.Length - compressedSize
                };
                return input.Take(origSize)
                    .Concat(new ArraySegment<byte>(result, input.Length - compressedRegion, compressedRegion))
                    .Concat(Enumerable.Repeat((byte)0xFF, -padOffset & 3))
                    .Concat(compFooter.StructToBytes())
                    .ToArray();
            };

            int orig = input.Length;
            int unused = input.Length;

            while (true)
            {
                int flag = result[--unused], mask = 0x80;
                do
                {
                    if ((flag & mask) == 0)
                    {
                        orig--;
                        unused--;
                    }
                    else
                    {
                        orig -= (result[--unused] >> 4 & 0x0F) + 3;
                        unused--;

                        if (orig + dest < unused)
                        {
                            return func(orig, unused);
                        }
                    }

                    if (orig <= 0)
                    {
                        return func(0, dest);
                    }
                } while ((mask >>= 1) != 0);
            }

        }
    }
}
