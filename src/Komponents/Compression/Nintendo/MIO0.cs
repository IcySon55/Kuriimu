using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;
using Kontract.Interface;
using System.Collections;

/*C# Decompressor Source by LordNed
 https://github.com/LordNed/WArchive-Tools/tree/master/ArchiveToolsLib/Compression
 
  Python Compressor Source
  https://pastebin.com/GUHMwpjT

  C# Compressor Optimization in Index search by Kosmos
  */

namespace Compression
{
    [Export("MIO0LE", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class MIO0LE : ICompression
    {
        public string Name { get; } = "MIO0LE";

        public string TabPathCompress { get; } = "Nintendo/MIO0/LE";
        public string TabPathDecompress { get; } = "Nintendo/MIO0/LE";

        public byte[] Compress(Stream input) => MIO0.Compress(input, ByteOrder.LittleEndian);
        public byte[] Decompress(Stream input, long decompSize = 0) => MIO0.Decompress(input, ByteOrder.LittleEndian);
    }

    [Export("MIO0BE", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class MIO0BE : ICompression
    {
        public string Name { get; } = "MIO0BE";

        public string TabPathCompress { get; } = "Nintendo/MIO0/BE";
        public string TabPathDecompress { get; } = "Nintendo/MIO0/BE";

        public byte[] Compress(Stream input) => MIO0.Compress(input, ByteOrder.BigEndian);
        public byte[] Decompress(Stream input, long decompSize = 0) => MIO0.Decompress(input, ByteOrder.BigEndian);
    }

    public class MIO0
    {
        public static byte[] Decompress(Stream instream, ByteOrder byteOrder)
        {
            using (var br = new BinaryReaderX(instream, true, byteOrder))
            {
                var offset = 0;
                List<byte> output = new List<byte>();

                #region 16-byte Header
                if (br.ReadString(4) != "MIO0")
                    throw new Exception("Not a valid MIO0 compressed file!");

                int decompressedLength = br.ReadInt32();
                int compressedOffset = br.ReadInt32() + offset;
                int uncompressedOffset = br.ReadInt32() + offset;
                #endregion

                int currentOffset;

                while (output.Count < decompressedLength)
                {

                    byte bits = br.ReadByte(); //byte of layout bits
                    BitArray arrayOfBits = new BitArray(new byte[1] { bits });

                    for (int i = 7; i > -1 && (output.Count < decompressedLength); i--) //iterate through layout bits
                    {

                        if (arrayOfBits[i] == true)
                        {
                            //non-compressed
                            //add one byte from uncompressedOffset to newFile

                            currentOffset = (int)br.BaseStream.Position;

                            br.BaseStream.Seek(uncompressedOffset, SeekOrigin.Begin);

                            output.Add(br.ReadByte());
                            uncompressedOffset++;

                            br.BaseStream.Seek(currentOffset, SeekOrigin.Begin);

                        }
                        else
                        {
                            //compressed
                            //read 2 bytes
                            //4 bits = length
                            //12 bits = offset

                            currentOffset = (int)br.BaseStream.Position;
                            br.BaseStream.Seek(compressedOffset, SeekOrigin.Begin);

                            byte byte1 = br.ReadByte();
                            byte byte2 = br.ReadByte();
                            compressedOffset += 2;

                            //Note: For Debugging, binary representations can be printed with:  Convert.ToString(numberVariable, 2);

                            byte byte1Upper = (byte)((byte1 & 0x0F));//offset bits
                            byte byte1Lower = (byte)((byte1 & 0xF0) >> 4); //length bits

                            int combinedOffset = ((byte1Upper << 8) | byte2);

                            int finalOffset = 1 + combinedOffset;
                            int finalLength = 3 + byte1Lower;

                            for (int k = 0; k < finalLength; k++) //add data for finalLength iterations
                            {
                                output.Add(output[output.Count - finalOffset]); //add byte at offset (fileSize - finalOffset) to file
                            }

                            br.BaseStream.Seek(currentOffset, SeekOrigin.Begin); //return to layout bits

                        }
                    }
                }

                return output.ToArray();
            }
        }

        public static byte[] Compress(Stream input, ByteOrder byteOrder)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var cap = 0x12;
                var sz = input.Length;

                var cmds = new List<byte>();
                var ctrl = new List<byte>();
                var raws = new List<byte>();

                var cmdpos = 0;
                cmds.Add(0);

                var pos = 0;
                byte flag = 0x80;

                while (pos < sz)
                {
                    var hitp = 0;
                    var hitl = 0;
                    _search(input, pos, sz, cap, ref hitp, ref hitl);

                    if (hitl < 3)
                    {
                        raws.Add(br.ScanBytes(pos)[0]);
                        cmds[cmdpos] |= flag;
                        pos += 1;
                    }
                    else
                    {
                        var tstp = 0;
                        var tstl = 0;
                        _search(input, pos + 1, sz, cap, ref tstp, ref tstl);

                        if ((hitl + 1) < tstl)
                        {
                            raws.Add(br.ScanBytes(pos)[0]);
                            cmds[cmdpos] |= flag;
                            pos += 1;
                            flag >>= 1;
                            if (flag == 0)
                            {
                                flag = 0x80;
                                cmdpos = cmds.Count();
                                cmds.Add(0);
                            }

                            hitl = tstl;
                            hitp = tstp;
                        }

                        var e = pos - hitp - 1;
                        pos += hitl;

                        hitl -= 3;
                        ctrl.AddRange(BitConverter.GetBytes((ushort)((hitl << 12) | e)).Reverse());
                    }

                    flag >>= 1;
                    if (flag == 0)
                    {
                        flag = 0x80;
                        cmdpos = cmds.Count();
                        cmds.Add(0);
                    }
                }

                if (flag == 0x80)
                    cmds.RemoveAt(cmdpos);

                var v = 4 - (cmds.Count() & 3);
                cmds.AddRange(new byte[v & 3]);
                var l = cmds.Count() + 16;
                var o = ctrl.Count() + l;

                List<byte> header = new List<byte>();
                header.AddRange(Encoding.ASCII.GetBytes("MIO0"));
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes((int)sz) : BitConverter.GetBytes((int)sz).Reverse());
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes(l) : BitConverter.GetBytes(l).Reverse());
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes(o) : BitConverter.GetBytes(o).Reverse());
                header.AddRange(cmds);
                header.AddRange(ctrl);
                header.AddRange(raws);

                return header.ToArray();
            }
        }

        #region Support
        static void _search(Stream data, int pos, long sz, int cap, ref int hitp, ref int hitl)
        {
            using (var br = new BinaryReaderX(data, true))
            {
                var ml = Math.Min(cap, sz - pos);
                if (ml < 3)
                    return;

                var mp = Math.Max(0, pos - 0x1000);
                hitp = 0;
                hitl = 3;

                if (mp < pos)
                {
                    var hl = (int)FirstIndexOfNeedleInHaystack(br.ScanBytes(mp, (pos + hitl) - mp), br.ScanBytes(pos, hitl));
                    while (hl < (pos - mp))
                    {
                        while ((hitl < ml) && (br.ScanBytes(pos + hitl)[0] == br.ScanBytes(mp + hl + hitl)[0]))
                            hitl += 1;

                        mp += hl;
                        hitp = mp;
                        if (hitl == ml)
                            return;

                        mp += 1;
                        hitl += 1;
                        if (mp >= pos)
                            break;

                        hl = (int)FirstIndexOfNeedleInHaystack(br.ScanBytes(mp, (pos + hitl) - mp), br.ScanBytes(pos, hitl));
                    }
                }

                if (hitl < 4)
                    hitl = 1;

                hitl -= 1;
                return;
            }
        }
        static unsafe long FirstIndexOfNeedleInHaystack(byte[] haystack, byte[] needle)
        {
            int m = needle.Length;
            int n = haystack.Length;

            int[] badChar = new int[256];

            BadCharHeuristic(needle, m, ref badChar);

            int s = 0;
            while (s <= (n - m))
            {
                int j = m - 1;

                while (j >= 0 && needle[j] == haystack[s + j])
                    --j;

                if (j < 0) return s;
                else s += Math.Max(1, j - badChar[haystack[s + j]]);
            }

            return -1;
        }
        static void BadCharHeuristic(byte[] input, int size, ref int[] badChar)
        {
            int i;

            for (i = 0; i < 256; i++)
                badChar[i] = -1;

            for (i = 0; i < size; i++)
                badChar[input[i]] = i;
        }
        #endregion
    }
}
