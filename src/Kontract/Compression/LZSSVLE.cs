using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Kontract.IO;

namespace Kontract.Compression
{
    public class LZSSVLE
    {
        public static byte[] Decompress(Stream input, bool leaveOpen = false)
        {
            using (var br = new BinaryReader(input, Encoding.Default, leaveOpen))
            {
                Tuple<int, int> GetNibbles(byte b) => new Tuple<int, int>(b >> 4, b & 0xF);

                int ReadVLC(int seed = 0)
                {
                    while (true)
                    {
                        var b = br.ReadByte();
                        seed = (seed << 7) | (b / 2);
                        if (b % 2 != 0) return seed;
                    }
                }

                var filesize = ReadVLC();
                var unk1 = ReadVLC(); // filetype maybe???
                var unk2 = ReadVLC(); // compression type = 1 (LZSS?)

                var buffer = new List<byte>();
                while (buffer.Count < filesize)
                {
                    // literal
                    var copiesSize = GetNibbles(br.ReadByte());
                    var copies = copiesSize.Item1;
                    var size = copiesSize.Item2;
                    if (size == 0) size = ReadVLC();
                    if (copies == 0) copies = ReadVLC();
                    buffer.AddRange(br.ReadBytes(size));

                    // copy stuff
                    while (copies-- > 0)
                    {
                        var lengthOffset = GetNibbles(br.ReadByte());
                        var length = lengthOffset.Item1;
                        var offset = lengthOffset.Item2;
                        if (offset % 2 == 0) offset = ReadVLC(offset / 2) * 2;
                        else offset >>= 1;
                        if (length == 0) length = ReadVLC(length);
                        while (length-- >= 0) buffer.Add(buffer[buffer.Count - offset / 2 - 1]);
                    }
                }
                return buffer.ToArray();
            }
        }
    }
}
