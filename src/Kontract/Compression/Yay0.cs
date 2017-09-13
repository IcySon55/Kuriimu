using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.IO;
using System.IO;
using System.Collections;

/*C# Decompressor Source by LordNed
 https://github.com/LordNed/WArchive-Tools/tree/master/ArchiveToolsLib/Compression
 
  C# Compressor Source by Daniel-McCarthy
  https://github.com/Daniel-McCarthy/Mr-Peeps-Compressor
  */

namespace Kuriimu.Compression
{
    public class Yay0
    {
        public static byte[] Decompress(Stream instream)
        {
            using (var br = new BinaryReaderX(instream, true, ByteOrder.BigEndian))
            {
                #region 16-byte Header
                if (br.ReadString(4) != "Yay0") // "Yay0" Magic
                    throw new InvalidDataException("Invalid Magic, not a Yay0 File");

                int uncompressedSize = br.ReadInt32();
                int linkTableOffset = br.ReadInt32();
                int byteChunkAndCountModifiersOffset = br.ReadInt32();
                #endregion

                int maskBitCounter = 0;
                int currentOffsetInDestBuffer = 0;
                int currentMask = 0;

                byte[] uncompressedData = new byte[uncompressedSize];

                do
                {
                    // If we're out of bits, get the next mask.
                    if (maskBitCounter == 0)
                    {
                        currentMask = br.ReadInt32();
                        maskBitCounter = 32;
                    }

                    // If the next bit is set, the chunk is non-linked and just copy it from the non-link table.
                    if (((uint)currentMask & (uint)0x80000000) == 0x80000000)
                    {
                        var offsetT = br.BaseStream.Position;
                        br.BaseStream.Position = byteChunkAndCountModifiersOffset;
                        uncompressedData[currentOffsetInDestBuffer] = br.ReadByte();
                        br.BaseStream.Position = offsetT;
                        currentOffsetInDestBuffer++;
                        byteChunkAndCountModifiersOffset++;
                    }
                    // Do a copy otherwise.
                    else
                    {
                        // Read 16-bit from the link table
                        var offsetT = br.BaseStream.Position;
                        br.BaseStream.Position = linkTableOffset;
                        ushort link = br.ReadUInt16();
                        br.BaseStream.Position = offsetT;
                        linkTableOffset += 2;

                        // Calculate the offset
                        int offset = currentOffsetInDestBuffer - (link & 0xfff);

                        // Calculate the count
                        int count = link >> 12;

                        if (count == 0)
                        {
                            byte countModifier;
                            offsetT = br.BaseStream.Position;
                            br.BaseStream.Position = byteChunkAndCountModifiersOffset;
                            countModifier = br.ReadByte();
                            br.BaseStream.Position = offsetT;
                            byteChunkAndCountModifiersOffset++;
                            count = countModifier + 18;
                        }
                        else
                        {
                            count += 2;
                        }

                        // Copy the block
                        int blockCopy = offset;

                        for (int i = 0; i < count; i++)
                        {
                            uncompressedData[currentOffsetInDestBuffer] = uncompressedData[blockCopy - 1];
                            currentOffsetInDestBuffer++;
                            blockCopy++;
                        }
                    }

                    // Get the next bit in the mask.
                    currentMask <<= 1;
                    maskBitCounter--;

                } while (currentOffsetInDestBuffer < uncompressedSize);

                return uncompressedData;
            }
        }

        public static byte[] Compress(Stream instream)
        {
            var offset = 0;
            var file = new BinaryReaderX(instream, true).ReadAllBytes();

            List<byte> layoutBits = new List<byte>();
            List<byte> dictionary = new List<byte>();

            List<byte> uncompressedData = new List<byte>();
            List<int[]> compressedData = new List<int[]>();

            int maxDictionarySize = 4096;
            int maxMatchLength = 255 + 0x12;
            int minMatchLength = 3;
            int decompressedSize = 0;

            for (int i = 0; i < file.Length; i++)
            {
                if (dictionary.Contains(file[i]))
                {
                    //check for best match
                    int[] matches = findAllMatches(ref dictionary, file[i]);
                    int[] bestMatch = findLargestMatch(ref dictionary, matches, ref file, i, maxMatchLength);

                    if (bestMatch[1] >= minMatchLength)
                    {
                        //add to compressedData
                        layoutBits.Add(0);
                        bestMatch[0] = dictionary.Count - bestMatch[0]; //sets offset in relation to end of dictionary

                        for (int j = 0; j < bestMatch[1]; j++)
                        {
                            dictionary.Add(file[i + j]);
                        }

                        i = i + bestMatch[1] - 1;

                        compressedData.Add(bestMatch);
                        decompressedSize += bestMatch[1];
                    }
                    else
                    {
                        //add to uncompressed data
                        layoutBits.Add(1);
                        uncompressedData.Add(file[i]);
                        dictionary.Add(file[i]);
                        decompressedSize++;
                    }
                }
                else
                {
                    //uncompressed data
                    layoutBits.Add(1);
                    uncompressedData.Add(file[i]);
                    dictionary.Add(file[i]);
                    decompressedSize++;
                }

                if (dictionary.Count > maxDictionarySize)
                {
                    int overflow = dictionary.Count - maxDictionarySize;
                    dictionary.RemoveRange(0, overflow);
                }
            }

            return buildYAY0CompressedBlock(ref layoutBits, ref uncompressedData, ref compressedData, decompressedSize, offset);
        }

        public static byte[] buildYAY0CompressedBlock(ref List<byte> layoutBits, ref List<byte> uncompressedData, ref List<int[]> offsetLengthPairs, int decompressedSize, int offset)
        {
            List<byte> finalYAY0Block = new List<byte>();
            List<byte> layoutBytes = new List<byte>();
            List<byte> compressedDataBytes = new List<byte>();
            List<byte> extendedLengthBytes = new List<byte>();

            int compressedOffset = 16 + offset; //header size
            int uncompressedOffset;

            //add Yay0 magic number
            finalYAY0Block.AddRange(Encoding.ASCII.GetBytes("Yay0"));

            //add decompressed data size
            byte[] decompressedSizeArray = BitConverter.GetBytes(decompressedSize);
            Array.Reverse(decompressedSizeArray);
            finalYAY0Block.AddRange(decompressedSizeArray);

            //assemble layout bytes
            while (layoutBits.Count > 0)
            {
                while (layoutBits.Count < 8)
                {
                    layoutBits.Add(0);
                }

                string layoutBitsString = layoutBits[0].ToString() + layoutBits[1].ToString() + layoutBits[2].ToString() + layoutBits[3].ToString()
                        + layoutBits[4].ToString() + layoutBits[5].ToString() + layoutBits[6].ToString() + layoutBits[7].ToString();

                byte[] layoutByteArray = new byte[1];
                layoutByteArray[0] = Convert.ToByte(layoutBitsString, 2);
                layoutBytes.Add(layoutByteArray[0]);
                layoutBits.RemoveRange(0, (layoutBits.Count < 8) ? layoutBits.Count : 8);

            }

            //assemble offsetLength shorts
            foreach (int[] offsetLengthPair in offsetLengthPairs)
            {
                //if < 18, set 4 bits -2 as matchLength
                //if >= 18, set matchLength == 0, write length to new byte - 0x12

                int adjustedOffset = offsetLengthPair[0];
                int adjustedLength = (offsetLengthPair[1] >= 18) ? 0 : offsetLengthPair[1] - 2; //vital, 4 bit range is 0-15. Number must be at least 3 (if 2, when -2 is done, it will think it is 3 byte format), -2 is how it can store up to 17 without an extra byte because +2 will be added on decompression

                int compressedInt = ((adjustedLength << 12) | adjustedOffset - 1);

                byte[] compressed2Byte = new byte[2];
                compressed2Byte[0] = (byte)(compressedInt & 0xFF);
                compressed2Byte[1] = (byte)((compressedInt >> 8) & 0xFF);

                compressedDataBytes.Add(compressed2Byte[1]);
                compressedDataBytes.Add(compressed2Byte[0]);

                if (adjustedLength == 0)
                {
                    extendedLengthBytes.Add((byte)(offsetLengthPair[1] - 18));
                }
            }

            //pad layout bits if needed
            while (layoutBytes.Count % 4 != 0)
            {
                layoutBytes.Add(0);
            }

            compressedOffset += layoutBytes.Count;

            //add final compressed offset
            byte[] compressedOffsetArray = BitConverter.GetBytes(compressedOffset);
            Array.Reverse(compressedOffsetArray);
            finalYAY0Block.AddRange(compressedOffsetArray);

            //add final uncompressed offset
            uncompressedOffset = compressedOffset + compressedDataBytes.Count;
            byte[] uncompressedOffsetArray = BitConverter.GetBytes(uncompressedOffset);
            Array.Reverse(uncompressedOffsetArray);
            finalYAY0Block.AddRange(uncompressedOffsetArray);

            //add layout bits
            foreach (byte layoutByte in layoutBytes)                 //add layout bytes to file
            {
                finalYAY0Block.Add(layoutByte);
            }

            //add compressed data
            foreach (byte compressedByte in compressedDataBytes)     //add compressed bytes to file
            {
                finalYAY0Block.Add(compressedByte);
            }

            //non-compressed/additional-length bytes
            {
                for (int i = 0; i < layoutBytes.Count; i++)
                {
                    BitArray arrayOfBits = new BitArray(new byte[1] { layoutBytes[i] });

                    for (int j = 7; ((j > -1) && ((uncompressedData.Count > 0) || (compressedDataBytes.Count > 0))); j--)
                    {
                        if (arrayOfBits[j] == true)
                        {
                            finalYAY0Block.Add(uncompressedData[0]);
                            uncompressedData.RemoveAt(0);
                        }
                        else
                        {
                            if (compressedDataBytes.Count > 0)
                            {
                                int length = compressedDataBytes[0] >> 4;
                                compressedDataBytes.RemoveRange(0, 2);

                                if (length == 0)
                                {
                                    finalYAY0Block.Add(extendedLengthBytes[0]);
                                    extendedLengthBytes.RemoveAt(0);
                                }


                            }
                        }
                    }
                }
            }

            return finalYAY0Block.ToArray();
        }

        public static int[] findAllMatches(ref List<byte> dictionary, byte match)
        {
            List<int> matchPositons = new List<int>();

            for (int i = 0; i < dictionary.Count; i++)
            {
                if (dictionary[i] == match)
                {
                    matchPositons.Add(i);
                }
            }

            return matchPositons.ToArray();
        }

        public static int[] findLargestMatch(ref List<byte> dictionary, int[] matchesFound, ref byte[] file, int fileIndex, int maxMatch)
        {
            int[] matchSizes = new int[matchesFound.Length];

            for (int i = 0; i < matchesFound.Length; i++)
            {
                int matchSize = 1;
                bool matchFound = true;

                //NOTE: This could be relevant to compression issues? I suspect it's more related to writing
                while (matchFound && matchSize < maxMatch && (fileIndex + matchSize < file.Length) && (matchesFound[i] + matchSize < dictionary.Count))
                {
                    if (file[fileIndex + matchSize] == dictionary[matchesFound[i] + matchSize])
                    {
                        matchSize++;
                    }
                    else
                    {
                        matchFound = false;
                    }

                }

                matchSizes[i] = matchSize;
            }

            int[] bestMatch = new int[2];

            bestMatch[0] = matchesFound[0];
            bestMatch[1] = matchSizes[0];

            for (int i = 1; i < matchesFound.Length; i++)
            {
                if (matchSizes[i] > bestMatch[1])
                {
                    bestMatch[0] = matchesFound[i];
                    bestMatch[1] = matchSizes[i];
                }
            }

            return bestMatch;

        }
    }
}
