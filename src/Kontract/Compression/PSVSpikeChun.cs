using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;

namespace Kontract.Compression
{
    public class PSVSpikeChun
    {
        public static byte[] Decompress(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var magic = br.ReadString(4);
                var decompSize = br.ReadInt32();
                var compSize = br.ReadInt32();

                var result = new byte[decompSize];

                int outOffset = 0;
                int windowOffset = 0, count = 0, prevOffset = 0;

                while (outOffset < decompSize)
                {
                    byte flags = br.ReadByte();

                    if ((flags & 0x80) == 0x80)
                    {
                        /* Copy data from the output.
                         * 1xxyyyyy yyyyyyyy
                         * Count -> x + 4
                         * Offset -> y
                         */
                        count = (((flags >> 5) & 0x3) + 4);
                        windowOffset = (((flags & 0x1F) << 8) + br.ReadByte());
                        prevOffset = windowOffset;

                        for (int i = 0; i < count; i++)
                            result[outOffset + i] = result[(outOffset - windowOffset) + i];

                        outOffset += count;
                    }
                    else if ((flags & 0x60) == 0x60)
                    {
                        /* Continue copying data from the output.
                         * 011xxxxx
                         * Count -> x
                         * Offset -> reused from above
                         */
                        count = (flags & 0x1F);
                        windowOffset = prevOffset;

                        for (int i = 0; i < count; i++)
                            result[outOffset + i] = result[(outOffset - windowOffset) + i];

                        outOffset += count;
                    }
                    else if ((flags & 0x40) == 0x40)
                    {
                        /* Insert multiple copies of the next byte.
                         * 0100xxxx yyyyyyyy
                         * 0101xxxx xxxxxxxx yyyyyyyy
                         * Count -> x + 4
                         * Data -> y
                         */
                        if ((flags & 0x10) == 0x00)
                            count = ((flags & 0x0F) + 4);
                        else
                            count = ((((flags & 0x0F) << 8) + br.ReadByte()) + 4);

                        byte data = br.ReadByte();
                        for (int i = 0; i < count; i++)
                        {
                            result[outOffset++] = data;
                        }
                    }
                    else if ((flags & 0xC0) == 0x00)
                    {
                        /* Insert raw bytes from the input.
                         * 000xxxxx
                         * 001xxxxx xxxxxxxx
                         * Count -> x
                         */
                        if ((flags & 0x20) == 0x00)
                            count = (flags & 0x1F);
                        else
                            count = (((flags & 0x1F) << 8) + br.ReadByte());

                        for (int i = 0; i < count; i++)
                            result[outOffset++] = br.ReadByte();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                return result;
            }
        }

        private static int window_size = 0x1FFF;
        public static byte[] Compress(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                var inputArr = br.ReadAllBytes();

                (var rleScan, var lzScan) = Prescan(inputArr);
                ProcessPrescans(ref rleScan, lzScan);

                //Replace compression areas
                List<(int rangeStart, int count, int compressedSize, int method)> merge = rleScan.Select(e => (e.rangeStart, e.count, e.compressedSize, 0)).ToList();
                merge.AddRange(lzScan.Select(e => (e.rangeStart, e.count, e.compressedSize, 1)));
                merge = merge.OrderBy(e => e.rangeStart).ToList();

                var ms = new MemoryStream();
                ms.Position = 0xc;
                using (var bw = new BinaryWriterX(ms, true))
                {
                    foreach (var m in merge)
                    {
                        WriteUncompressedData(bw.BaseStream, br.ReadBytes(m.rangeStart - (int)br.BaseStream.Position));
                        if (m.method == 0)
                        {
                            WriteRLEData(bw.BaseStream, m.count, br.ReadByte());
                            br.BaseStream.Position += m.count - 1;
                        }
                        else
                        {
                            WriteLZData(bw.BaseStream, GetMaxOccurence(inputArr, m.rangeStart));
                            br.BaseStream.Position += m.count;
                        }
                    }
                    WriteUncompressedData(bw.BaseStream, br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position)));

                    bw.BaseStream.Position = 0;
                    bw.Write(0xa755aafc);
                    bw.Write((int)br.BaseStream.Length);
                    bw.Write((int)bw.BaseStream.Length);
                }

                return ms.ToArray();
            }
        }

        private static void WriteLZData(Stream input, (int windowOffset, int inputOffset, int count) maxOcc)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.Write((byte)(0x80 | ((maxOcc.count - 4 < 3) ? (maxOcc.count - 4) << 5 : 0x60) | (maxOcc.windowOffset >> 8)));
                bw.Write((byte)(maxOcc.windowOffset & 0xFF));
                maxOcc.count -= 0x7;

                while (maxOcc.count > 0)
                {
                    if (maxOcc.count > 0x1F)
                    {
                        bw.Write((byte)(0x7F));
                        maxOcc.count -= 0x1F;
                    }
                    else
                    {
                        bw.Write((byte)(0x60 | maxOcc.count));
                        maxOcc.count = 0;
                    }
                }
            }
        }

        private static void WriteRLEData(Stream input, int count, byte repByte)
        {
            using (var bw = new BinaryWriterX(input, true))
                while (count >= 4)
                {
                    if (count - 4 > 0xFFF)
                    {
                        count -= 0x1003;
                        bw.Write((byte)0x5F);
                        bw.Write((byte)0xFF);
                    }
                    else if (count - 4 > 0xF)
                    {
                        bw.Write((byte)(0x50 | ((count - 4) >> 8)));
                        bw.Write((byte)((count - 4) & 0xFF));
                        count = 0;
                    }
                    else
                    {
                        bw.Write((byte)(0x40 | (count - 4)));
                        count = 0;
                    }
                    bw.Write(repByte);
                }
        }

        private static void WriteUncompressedData(Stream write, byte[] uncomp)
        {
            if (uncomp.Count() <= 0)
                return;

            using (var bw = new BinaryWriterX(write, true, ByteOrder.BigEndian))
            {
                var pos = 0;
                while (pos < uncomp.Length)
                {
                    if (uncomp.Length - pos > 0x1F)
                    {
                        bw.Write((ushort)(0x2000 | Math.Min(0x1FFF, uncomp.Length - pos)));
                        bw.Write(uncomp.GetBytes(pos, Math.Min(0x1FFF, uncomp.Length - pos)));
                        pos += Math.Min(0x1FFF, uncomp.Length - pos);
                    }
                    else
                    {
                        bw.Write((byte)(0 | uncomp.Length - pos));
                        bw.Write(uncomp.GetBytes(pos, uncomp.Length - pos));
                        pos += uncomp.Length - pos;
                    }
                }
            }
        }

        private static void ProcessPrescans(ref List<(int rangeStart, int count, int compressedSize)> rleScan, List<(int rangeStart, int count, int compressedSize)> lzScan)
        {
            int lzIndex = 0;
            while (AreScansOverlapping(rleScan, lzScan))
            {
                var lzElement = lzScan[lzIndex];

                var lzRangeEnd = lzElement.rangeStart + lzElement.count - 1;
                var overlapRle = rleScan.Where(rle =>
                      (rle.rangeStart >= lzElement.rangeStart && rle.rangeStart <= lzRangeEnd) ||
                      (rle.rangeStart + rle.count - 1 >= lzElement.rangeStart && rle.rangeStart + rle.count - 1 <= lzRangeEnd) ||
                      rle.rangeStart < lzElement.rangeStart && rle.rangeStart + rle.count - 1 > lzRangeEnd
                      );

                if (overlapRle.Count() <= 0)
                {
                    lzIndex++;
                    continue;
                }

                var lzRange = lzElement.count;
                var lzCompCount = lzElement.compressedSize;

                var rleRange = overlapRle.Aggregate(0, (o, i) => o += i.count);
                var rleCompCount = overlapRle.Aggregate(0, (o, i) => o += i.compressedSize);

                if (lzRange < rleRange)
                    lzCompCount = (int)((double)rleRange / lzRange * lzCompCount);
                else
                    rleCompCount = (int)((double)lzRange / rleRange * rleCompCount);

                if (lzCompCount < rleCompCount)
                {
                    //Check if out-of-range RLE can still be used
                    var overlap0 = overlapRle.ElementAt(0);
                    if (overlap0.rangeStart < lzElement.rangeStart &&
                        overlap0.rangeStart + overlap0.count - 1 <= lzRangeEnd)
                    {
                        if (lzElement.rangeStart - overlap0.rangeStart >= 4)
                        {
                            overlapRle = overlapRle.Except(new List<(int, int, int)> { overlap0 });
                            rleScan[rleScan.IndexOf(overlap0)] =
                                (rleScan[rleScan.IndexOf(overlap0)].rangeStart,
                                lzElement.rangeStart - overlap0.rangeStart,
                                (lzElement.rangeStart - overlap0.rangeStart - 4 <= 0xF) ? 2 : 3);
                        }
                    }

                    if (overlapRle.Count() > 0)
                    {
                        var overlapL = overlapRle.Last();
                        if (overlapL.rangeStart >= lzElement.rangeStart &&
                            overlapL.rangeStart + overlapL.count - 1 > lzRangeEnd)
                        {
                            if (overlapL.rangeStart + overlapL.count - lzRangeEnd >= 4)
                            {
                                overlapRle = overlapRle.Except(new List<(int, int, int)> { overlapL });
                                rleScan[rleScan.IndexOf(overlapL)] =
                                    (lzRangeEnd + 1,
                                    overlapL.rangeStart + overlapL.count - lzRangeEnd,
                                    (overlapL.rangeStart + overlapL.count - lzRangeEnd - 4 <= 0xF) ? 2 : 3);
                            }
                        }
                    }

                    lzIndex++;
                    rleScan = rleScan.Except(overlapRle).ToList();
                }
                else
                {
                    lzScan.Remove(lzElement);
                    if (lzIndex >= lzScan.Count)
                        lzIndex = lzScan.Count - 1;
                }
            }
        }

        private static bool AreScansOverlapping(List<(int rangeStart, int count, int compressedSize)> rleScan, List<(int rangeStart, int count, int compressedSize)> lzScan)
        {
            if (rleScan.Count <= 0 || lzScan.Count <= 0)
                return false;

            //take LZ as base for ranges, since it can range over larger areas
            foreach (var lz in lzScan)
            {
                var lzRangeEnd = lz.rangeStart + lz.count - 1;
                if (rleScan.Count(rle =>
                    (rle.rangeStart >= lz.rangeStart && rle.rangeStart <= lzRangeEnd) ||
                    (rle.rangeStart + rle.count - 1 >= lz.rangeStart && rle.rangeStart + rle.count - 1 <= lzRangeEnd) ||
                    rle.rangeStart < lz.rangeStart && rle.rangeStart + rle.count - 1 > lzRangeEnd
                    ) > 0)
                    return true;
            }
            return false;
        }

        private static (List<(int rangeStart, int count, int compressedSize)>, List<(int rangeStart, int count, int compressedSize)>) Prescan(byte[] scan)
        {
            var rle = new List<(int, int, int)>();
            var lz = new List<(int, int, int)>();

            //Scan for RLE relevant compression
            for (int i = 0; i < scan.Length; i++)
            {
                var check = scan[i];
                var rep = 1;
                while (++i < scan.Length && scan[i] == check && rep < 0xFFF + 4)
                    rep++;
                if (rep >= 4)
                {
                    rle.Add((i - rep, rep, (rep - 4 <= 0xF) ? 2 : 3));
                }
                --i;
            }

            //Scan for LZ relevant compression
            var pos = 4;
            while (pos < scan.Length)
            {
                var maxOcc = GetMaxOccurence(scan, pos);

                if (maxOcc.count >= 4)
                {
                    lz.Add((pos, maxOcc.count, GetCompressedLZCount(maxOcc.count)));
                    pos += maxOcc.count;
                }
                else
                {
                    pos++;
                }
            }

            return (rle, lz);
        }

        static int GetCompressedLZCount(int count)
        {
            count = Math.Max(0, count - 4);

            var byteCount = 1;
            while (count > 3)
            {
                count -= Math.Min(count - 3, 0x1F);
                byteCount++;
            }

            return byteCount;
        }

        //(offset to get encoded, offset in uncompressed data, bytes to be copied)
        static (int windowOffset, int inputOffset, int count) GetMaxOccurence(byte[] input, int inputPos)
        {
            int windowSize = Math.Min(window_size, inputPos);
            int windowStart = inputPos - windowSize;

            int checkOffset = inputPos - 1;
            (int windowOffset, int inputOffset, int count) maxOcc = (0, inputPos, 0);
            while (checkOffset >= windowStart)
            {
                int occ = 0;
                while (inputPos + occ < input.Length && input[checkOffset + occ] == input[inputPos + occ])
                    occ++;
                if (maxOcc.count < occ)
                {
                    maxOcc.windowOffset = inputPos - checkOffset;
                    maxOcc.inputOffset = checkOffset;
                    maxOcc.count = occ;
                }
                checkOffset--;
            }

            return maxOcc;
        }
    }
}
