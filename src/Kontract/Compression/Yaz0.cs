using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kuriimu.IO;

/*C# Source by LordNed
 https://github.com/LordNed/WArchive-Tools/tree/master/ArchiveToolsLib/Compression
 */

namespace Kuriimu.Compression
{
    public class Yaz0
    {
        public static byte[] Decompress(Stream instream)
        {
            using (var br = new BinaryReaderX(instream, true))
            {
                #region 16-byte Header
                if (br.ReadString(4) != "Yaz0") // "Yaz0" Magic
                    throw new InvalidDataException("Invalid Magic, not a Yaz0 File");

                int uncompressedSize = br.ReadInt32();
                br.ReadBytes(8); // Padding
                #endregion

                byte[] output = new byte[uncompressedSize];
                int destPos = 0;

                byte curCodeByte = 0;
                uint validBitCount = 0;

                while (destPos < uncompressedSize)
                {
                    // The codeByte specifies what to do for the next 8 steps. Read a new one if we've exhausted the current one.
                    if (validBitCount == 0)
                    {
                        curCodeByte = br.ReadByte();
                        validBitCount = 8;
                    }

                    if ((curCodeByte & 0x80) != 0)
                    {
                        // If the bit is set then there is no compression, just write the data to the output.
                        output[destPos] = br.ReadByte();
                        destPos++;
                    }
                    else
                    {
                        // If the bit is not set, then the data needs to be decompressed. The next two bytes tells the data location and size.
                        // The decompressed data has already been written to the output stream, so we go and retrieve it.
                        byte byte1 = br.ReadByte();
                        byte byte2 = br.ReadByte();

                        int dist = ((byte1 & 0xF) << 8) | byte2;
                        int copySource = destPos - (dist + 1);

                        int numBytes = byte1 >> 4;
                        if (numBytes == 0)
                        {
                            // Read the third byte which tells you how much data to read.
                            numBytes = br.ReadByte() + 0x12;
                        }
                        else
                        {
                            numBytes += 2;
                        }

                        // Copy Run
                        for (int k = 0; k < numBytes; k++)
                        {
                            output[destPos] = output[copySource];
                            copySource++;
                            destPos++;
                        }
                    }

                    // Use the next bit from the code byte
                    curCodeByte <<= 1;
                    validBitCount -= 1;
                }

                return output;
            }
        }

        static int sNumBytes1, sMatchPos;
        static bool sPrevFlag = false;

        public static byte[] Compress(Stream instream)
        {
            if (instream == null)
                throw new Exception("File should not be null!");
            if (instream.Length == 0)
                throw new Exception("File should not be empty!");

            using (var bw = new BinaryWriterX(new MemoryStream((int)instream.Length)))
            {
                #region Yaz0 Header
                bw.WriteASCII("Yaz0");
                bw.Write((int)instream.Length);
                bw.Write((long)0);
                #endregion

                int srcPos = 0;
                int dstPos = 0;
                byte[] dst = new byte[24]; // 8 codes * 3 bytes maximum per code.

                int validBitCount = 0;
                byte curCodeByte = 0;

                byte[] src = new BinaryReaderX(instream, true).ReadAllBytes();

                while (srcPos < src.Length)
                {
                    int numBytes, matchPos;
                    NintendoYaz0Encode(src, srcPos, out numBytes, out matchPos);
                    if (numBytes < 3)
                    {
                        // Straight Copy
                        dst[dstPos] = src[srcPos];
                        srcPos++;
                        dstPos++;

                        // Set flag for straight copy
                        curCodeByte |= (byte)(0x80 >> validBitCount);
                    }
                    else
                    {
                        // RLE part
                        uint dist = (uint)(srcPos - matchPos - 1);
                        byte byte1, byte2, byte3;

                        // Requires a 3 byte encoding
                        if (numBytes >= 0x12)
                        {
                            byte1 = (byte)(0 | (dist >> 8));
                            byte2 = (byte)(dist & 0xFF);
                            dst[dstPos++] = byte1;
                            dst[dstPos++] = byte2;

                            // Maximum run length for 3 byte encoding.
                            if (numBytes > 0xFF + 0x12)
                                numBytes = 0xFF + 0x12;
                            byte3 = (byte)(numBytes - 0x12);
                            dst[dstPos++] = byte3;
                        }
                        // 2 byte encoding
                        else
                        {
                            byte1 = (byte)((uint)((numBytes - 2) << 4) | (dist >> 8));
                            byte2 = (byte)(dist & 0xFF);
                            dst[dstPos++] = byte1;
                            dst[dstPos++] = byte2;
                        }
                        srcPos += numBytes;
                    }

                    validBitCount++;

                    // Write 8 codes if we've filled a block
                    if (validBitCount == 8)
                    {
                        // Write the code byte 
                        bw.Write(curCodeByte);

                        // And then any bytes in the dst buffer.
                        for (int i = 0; i < dstPos; i++)
                            bw.Write(dst[i]);

                        //output.Flush();                    

                        curCodeByte = 0;
                        validBitCount = 0;
                        dstPos = 0;
                    }
                }

                // If we didn't finish off on a whole byte, add the last code byte.
                if (validBitCount > 0)
                {
                    // Write the code byte 
                    bw.Write(curCodeByte);

                    // And then any bytes in the dst buffer.
                    for (int i = 0; i < dstPos; i++)
                        bw.Write(dst[i]);

                    curCodeByte = 0;
                    validBitCount = 0;
                    dstPos = 0;
                }

                return new BinaryReaderX(bw.BaseStream).ReadAllBytes();
            }
        }

        private static void NintendoYaz0Encode(byte[] src, int srcPos, out int outNumBytes, out int outMatchPos)
        {
            int startPos = srcPos - 0x1000;
            int numBytes = 1;

            // If prevFlag is set, it means that the previous position was determined by the look-ahead try so use
            // that. This is not the best optimization, but apparently Nintendo's choice for speed.
            if (sPrevFlag)
            {
                outMatchPos = sMatchPos;
                sPrevFlag = false;
                outNumBytes = sNumBytes1;
                return;
            }

            sPrevFlag = false;
            SimpleRLEEncode(src, srcPos, out numBytes, out sMatchPos);
            outMatchPos = sMatchPos;

            // If this position is RLE encoded, then compare to copying 1 byte and next pos (srcPos + 1) encoding.
            if (numBytes >= 3)
            {
                SimpleRLEEncode(src, srcPos + 1, out sNumBytes1, out sMatchPos);

                // If the next position encoding is +2 longer than current position, choose it.
                // This does not gurantee the best optimization, but fairly good optimization with speed.
                if (sNumBytes1 >= numBytes + 2)
                {
                    numBytes = 1;
                    sPrevFlag = true;
                }
            }

            outNumBytes = numBytes;
        }

        private static void SimpleRLEEncode(byte[] src, int srcPos, out int outNumBytes, out int outMatchPos)
        {
            int startPos = srcPos - 0x400;
            int numBytes = 1;
            int matchPos = 0;

            if (startPos < 0)
                startPos = 0;

            // Search backwards through the stream for an already encoded bit.
            for (int i = startPos; i < srcPos; i++)
            {
                int j;
                for (j = 0; j < src.Length - srcPos; j++)
                {
                    if (src[i + j] != src[j + srcPos])
                        break;
                }

                if (j > numBytes)
                {
                    numBytes = j;
                    matchPos = i;
                }
            }

            outMatchPos = matchPos;
            if (numBytes == 2)
                numBytes = 1;

            outNumBytes = numBytes;
        }
    }
}
