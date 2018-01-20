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

        //Compress with LZ window lookback
        const int window_size = 0x1FFF;
        public static byte[] Compress(Stream input)
        {
            var uncompBuffer = new List<byte>();
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true))
            using (var br = new BinaryReaderX(input, true))
            {
                bw.BaseStream.Position = 0xc;
                uncompBuffer.AddRange(br.ReadBytes(4));

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    (int windowOffset, int inputOffset, int count) = GetMaxOccurence(br.BaseStream);

                    if (count >= 4)
                    {
                        try
                        {
                            uncompBuffer.AddRange(br.ReadBytes(inputOffset));
                        }
                        catch
                        {
                            ;
                        }
                        bw.Write(CompressRepetitiveBytes(new MemoryStream(uncompBuffer.ToArray())));
                        uncompBuffer = new List<byte>();

                        WriteLZData(bw.BaseStream, windowOffset, count);
                        br.BaseStream.Position += count;
                    }
                    else
                    {
                        uncompBuffer.AddRange(br.ReadBytes(Math.Min(window_size, (int)input.Position)));
                    }
                }

                //Writing Header
                bw.BaseStream.Position = 0;
                bw.Write(0xa755aafc);
                bw.Write((int)br.BaseStream.Length);
                bw.Write((int)bw.BaseStream.Length);

                //Pad compressed Data to 0x10
                bw.BaseStream.Position = bw.BaseStream.Length;
                bw.WriteAlignment();
            }

            return ms.ToArray();
        }

        //(offset to get encoded, offset in uncompressed data, bytes to be copied)
        static (int, int, int) GetMaxOccurence(Stream input)
        {
            int inputPos = (int)input.Position;
            int startOffset = Math.Max(0, inputPos - window_size);
            int windowSize = Math.Min(window_size, inputPos);
            int uncompSize = Math.Min(windowSize, (int)input.Length - inputPos);

            //Get Window Data
            var window = new byte[windowSize];
            input.Position = startOffset;
            input.Read(window, 0, windowSize);
            int windowOffset = 0;

            //Get uncompressedData
            var uncompBuf = new byte[uncompSize];
            input.Position = inputPos;
            input.Read(uncompBuf, 0, uncompSize);
            var uncompOffset = 0;

            int maxLength = 0;
            int maxOffset = 0;
            while (windowOffset < windowSize && uncompOffset < uncompSize)
            {
                int occ = 0;
                while (window[windowOffset++] == uncompBuf[uncompOffset++])
                {
                    occ++;
                    if (windowOffset >= window.Length || uncompOffset >= uncompBuf.Length)
                        break;
                }
                if (occ > maxLength)
                {
                    maxLength = occ;
                    if (windowOffset >= window.Length || uncompOffset >= uncompBuf.Length)
                        maxOffset = windowOffset - occ;
                    else
                        maxOffset = windowOffset - occ - 1;
                }
            }

            input.Position = inputPos;
            return (inputPos - maxOffset, maxOffset, maxLength);
        }

        #region Compress only repetitive single bytes
        public static byte[] CompressRepetitiveBytes(Stream input)
        {
            //First approach - Creating "compressed" data by only using the flag for uncompressed data and repetitive bytes
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true))
            using (var br = new BinaryReaderX(input, true))
            {
                var uncompBuffer = new List<byte>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    if (br.BaseStream.Length - br.BaseStream.Position < 2)
                    {
                        uncompBuffer.AddRange(br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position)));
                        break;
                    }

                    var rep = 1;
                    var b = br.ReadByte();
                    var b2 = br.ReadByte();
                    while (b2 == b)
                    {
                        rep++;
                        if (br.BaseStream.Position < br.BaseStream.Length)
                            b2 = br.ReadByte();
                        else
                            break;
                    }
                    if (br.BaseStream.Position < br.BaseStream.Length || b2 != b)
                        br.BaseStream.Position--;

                    //if repeats too much
                    if (rep >= 4)
                    {
                        //check uncompressed buffer, which data to write first
                        if (uncompBuffer.Count > 0)
                        {
                            WriteUncompData(bw.BaseStream, uncompBuffer);
                            uncompBuffer = new List<byte>();
                        }

                        //write repetitive byte
                        WriteRLEData(bw.BaseStream, b, rep);
                    }
                    //Filling uncompressed Buffer
                    else
                    {
                        for (int i = 0; i < rep; i++)
                            uncompBuffer.Add(b);
                    }
                }

                //check uncompressed buffer, which data to write first
                if (uncompBuffer.Count > 0)
                {
                    WriteUncompData(bw.BaseStream, uncompBuffer);
                }
            }

            return ms.ToArray();
        }

        static void WriteRLEData(Stream input, byte repByte, int rep)
        {
            using (var bw = new BinaryWriterX(input, true))
                while (rep >= 4)
                {
                    if (rep - 4 > 0xFFF)
                    {
                        rep -= 0x1003;
                        bw.Write((byte)0x5F);
                        bw.Write((byte)0xFF);
                    }
                    else if (rep - 4 > 0xF)
                    {
                        bw.Write((byte)(0x50 | ((rep - 4) >> 8)));
                        bw.Write((byte)((rep - 4) & 0xFF));
                        rep = 0;
                    }
                    else
                    {
                        bw.Write((byte)(0x40 | (rep - 4)));
                        rep = 0;
                    }
                    bw.Write(repByte);
                }
        }

        static void WriteUncompData(Stream input, List<byte> uncompBuffer)
        {
            using (var bw = new BinaryWriterX(input, true))
                while (uncompBuffer.Count > 0)
                {
                    if (uncompBuffer.Count > 0x1FFF)
                    {
                        bw.Write((byte)0x3F);
                        bw.Write((byte)0xFF);
                        bw.Write(uncompBuffer.Take(0x1FFF).ToArray());
                        uncompBuffer.RemoveRange(0, 0x1FFF);
                    }
                    else if (uncompBuffer.Count > 0x1F)
                    {
                        bw.Write((byte)(0x20 | (uncompBuffer.Count >> 8)));
                        bw.Write((byte)(uncompBuffer.Count & 0xFF));
                        bw.Write(uncompBuffer.ToArray());
                        uncompBuffer = new List<byte>();
                    }
                    else
                    {
                        bw.Write((byte)(0x00 | uncompBuffer.Count));
                        bw.Write(uncompBuffer.ToArray());
                        uncompBuffer = new List<byte>();
                    }
                }
        }
        #endregion

        static void WriteLZData(Stream input, int windowOffset, int count)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.Write((byte)(0x80 | ((count - 4 < 3) ? (count - 4) << 5 : 0x60) | (windowOffset >> 8)));
                bw.Write((byte)(windowOffset & 0xFF));
                count -= 0x7;

                while (count > 0)
                {
                    if (count > 0x1F)
                    {
                        bw.Write((byte)(0x7F));
                        count -= 0x1F;
                    }
                    else
                    {
                        bw.Write((byte)(0x60 | count));
                        count = 0;
                    }
                }
            }
        }
    }
}
