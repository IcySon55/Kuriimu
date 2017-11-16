using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace Compression
{
    [ExportMetadata("Name", "LZSS")]
    [ExportMetadata("TabPathCompress", "")]
    [ExportMetadata("TabPathDecompress", "Nintendo/LZSS")]
    [Export("LZSS", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class LZSS : ICompression
    {
        public byte[] Decompress(Stream instream, long decompSize = 0)
        {
            using (BinaryReaderX br = new BinaryReaderX(instream, true))
            {
                List<byte> result = new List<byte>();

                for (int i = 0, flags = 0; ; i++)
                {
                    if (i % 8 == 0) flags = br.ReadByte();
                    if ((flags & 0x80) == 0) result.Add(br.ReadByte());
                    else
                    {
                        int lengthDist = BitConverter.ToUInt16(br.ReadBytes(2).Reverse().ToArray(), 0);
                        int offs = lengthDist % 4096 + 1;
                        int length = lengthDist / 4096 + 3;
                        while (length > 0)
                        {
                            result.Add(result[result.Count - offs]);
                            length--;
                        }
                    }

                    if (result.Count == decompSize)
                    {
                        return result.ToArray();
                    }
                    else if (result.Count > decompSize)
                    {
                        throw new InvalidDataException("Went past the end of the stream");
                    }
                    flags <<= 1;
                }
            }
        }

        public byte[] Compress(Stream input)
        {
            return null;
        }

        /*public long GetDecompressedSize(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                if (br.ReadString(4) == "SSZL")
                {
                    br.BaseStream.Position = 0xc;
                    return br.ReadUInt32();
                }

                return 0;
            }
        }*/
    }
}
