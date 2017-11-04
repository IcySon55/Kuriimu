using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.IO;
using System.IO;
using System.Collections;

/*C# Decompressor Source by LordNed
 https://github.com/LordNed/WArchive-Tools/tree/master/ArchiveToolsLib/Compression
 
  Python Compressor Source
  https://pastebin.com/GUHMwpjT

  C# Compressor Optimization in Index search by Kosmos
  */

namespace Kontract.Compression
{
    public class Yay0
    {
        public static byte[] Decompress(Stream instream, ByteOrder byteOrder)
        {
            using (var br = new BinaryReaderX(instream, true, byteOrder))
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

                br.ByteOrder = ByteOrder.BigEndian;
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

        public static byte[] Compress(Stream input, ByteOrder byteOrder)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var cap = 0x111;
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

                var v = 4 - (cmds.Count() & 3);
                cmds.AddRange(new byte[v & 3]);
                var l = cmds.Count() + 16;
                var o = ctrl.Count() + l;

                List<byte> header = new List<byte>();
                header.AddRange(Encoding.ASCII.GetBytes("Yay0"));
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes((int)sz) : BitConverter.GetBytes((int)sz).Reverse());
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes(l) : BitConverter.GetBytes(l).Reverse());
                header.AddRange((byteOrder == ByteOrder.LittleEndian) ? BitConverter.GetBytes(o) : BitConverter.GetBytes(o).Reverse());
                header.AddRange(cmds);
                header.AddRange(ctrl);
                header.AddRange(raws);

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

        public static unsafe long FirstIndexOfNeedleInHaystack(byte[] haystack, byte[] needle)
        {
            fixed (byte* numPtr1 = haystack)
            fixed (byte* numPtr2 = needle)
            {
                long num = 0;
                byte* numPtr3 = numPtr1;
                for (byte* numPtr4 = numPtr1 + haystack.Length; numPtr3 < numPtr4; ++numPtr3)
                {
                    bool flag = true;
                    byte* numPtr5 = numPtr3;
                    byte* numPtr6 = numPtr2;
                    byte* numPtr7 = numPtr2 + needle.Length;
                    while (flag && numPtr6 < numPtr7)
                    {
                        flag = (int)*numPtr6 == (int)*numPtr5;
                        ++numPtr6;
                        ++numPtr5;
                    }
                    if (flag)
                        return num;
                    ++num;
                }
                return -1;
            }
        }
    }
}
