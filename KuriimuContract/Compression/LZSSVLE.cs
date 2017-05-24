using System;
using System.IO;
using System.Collections.Generic;
using Kuriimu.IO;

namespace Kuriimu.Compression
{
    public class LZSSVLE
    {
        public static byte[] Decompress(Stream instream)
        {
            using (var br = new BinaryReader(instream))
            {
                Tuple<int,int> GetNibbles(byte b) => new Tuple<int,int>(b >> 4, b & 0xF);
                int ReadVLC(int seed = 0)
                {
                    while (true)
                    {
                        var b = br.ReadByte();
                        seed = (seed << 7) | b;
                        if (b % 2 != 0) return seed;
                    }
                }

                br.ReadUInt32(); // hash = ????????
                br.ReadByte(); // compression type = 1 (LZSS?)

                var buffer = new List<byte>();
                while (br.BaseStream.Position != br.BaseStream.Length)
                {
                    // literal
                    var copiesSize = GetNibbles(br.ReadByte());
                    var copies = copiesSize.Item1;
                    var size = copiesSize.Item2;
                    if (copies == 0 && size == 0) break;

                    if (copies == 0) copies = ReadVLC() / 2;
                    if (size == 0) size = ReadVLC() / 2;
                    buffer.AddRange(br.ReadBytes(size));

                    // copy stuff
                    while (copies-- > 0)
                    {
                        var lengthOffset = GetNibbles(br.ReadByte());
                        var length = lengthOffset.Item1;
                        var offset = lengthOffset.Item2;
                        if (offset % 2 == 0) offset = ReadVLC(offset);
                        if (length == 0) length = ReadVLC() / 2;
                        while (length-- >= 0) buffer.Add(buffer[buffer.Count - offset / 2 - 1]);
                    }
                }

                return buffer.ToArray();
            }
        }
    }
}
