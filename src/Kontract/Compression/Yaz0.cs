using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kuriimu.IO;

/*C# Decompressor by LordNed
 https://github.com/LordNed/WArchive-Tools/tree/master/ArchiveToolsLib/Compression

  Python Compressor Source
  https://pastebin.com/GUHMwpjT
  */

namespace Kuriimu.Compression
{
    public class Yaz0
    {
        public static byte[] Decompress(Stream instream, ByteOrder byteOrder)
        {
            using (var br = new BinaryReaderX(instream, true, byteOrder))
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

        public static byte[] Compress(Stream input, ByteOrder byteOrder)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var cap = 0x111;
                var sz = input.Length;

                var itr = new List<byte>();
                var cmds = itr;
                var ctrl = itr;
                var raws = itr;

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

                        if (hitl < 0x12)
                        {
                            hitl -= 2;
                            ctrl.AddRange(BitConverter.GetBytes((ushort)((hitl << 12) | e)).Reverse());
                        }
                        else
                        {
                            ctrl.AddRange(BitConverter.GetBytes((ushort)(e)).Reverse());
                            raws.Add((byte)(hitl - 0x12));
                        }
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

                List<byte> header = new List<byte>();
                header.AddRange(Encoding.ASCII.GetBytes("Yaz0"));
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes((int)sz) : BitConverter.GetBytes((int)sz).Reverse());
                header.AddRange(BitConverter.GetBytes(0));
                header.AddRange(BitConverter.GetBytes(0));
                header.AddRange(cmds);
                return header.ToArray();
            }
        }

        public static void _search(Stream data, int pos, long sz, int cap, ref int hitp, ref int hitl)
        {
            var t = 0;
            if (pos == 734)
                t = 0;
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
                    var hl = IndexOf(br.ScanBytes(mp, (pos + hitl) - mp), br.ScanBytes(pos, hitl));
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

                        hl = IndexOf(br.ScanBytes(mp, (pos + hitl) - mp), br.ScanBytes(pos, hitl));
                    }
                }

                if (hitl < 4)
                    hitl = 1;

                hitl -= 1;
                return;
            }
        }

        private static int IndexOf(byte[] input, byte[] search)
        {
            var index = -1;

            var searchCount = 0;
            for (int i = 0; i < input.Count(); i++)
            {
                if (searchCount != 0)
                {
                    index = i - 1;
                    break;
                }

                if ((input.Count() - i < search.Count()) && (searchCount == 0))
                    break;

                for (int j = 0; j < search.Count(); j++)
                {
                    if (input[i + j] == search[j])
                        searchCount++;
                    else
                    {
                        searchCount = 0;
                        break;
                    }
                }
            }

            return index;
        }
    }
}
