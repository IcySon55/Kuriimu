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

namespace Compression
{
    [ExportMetadata("Name", "LZECD")]
    [ExportMetadata("TabPathCompress", "")]
    [ExportMetadata("TabPathDecompress", "Custom/LZECD")]
    [Export("LZECD", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class LZECD : ICompression
    {
        public byte[] Decompress(Stream input, long decompSize = 0)
        {
            using (var br = new BinaryReaderX(input))
            {
                int ReadBE() => BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);

                var magic = br.ReadBytes(4);
                var init = ReadBE();
                var compSize = ReadBE();
                var uncompSize = ReadBE();
                var lst = br.ReadBytes(init).ToList();

                foreach (var flag in Enumerable.Repeat(0, int.MaxValue).SelectMany(_ => new BitArray(new[] { br.ReadByte() }).Cast<bool>()))
                {
                    if (flag) lst.Add(br.ReadByte());
                    else
                    {
                        var b1 = br.ReadByte();
                        var b2 = br.ReadByte();
                        lst.AddRange(Enumerable.Range((b2 / 64 * 256 + b1 + 74) % 1024, (b2 % 64) + 3)
                            .Select(i => i >= init && i < lst.Count ? lst[i] : (byte)0));
                    }
                    if (lst.Count == uncompSize) break;
                    if (lst.Count > uncompSize) throw new EndOfStreamException();
                }

                return lst.ToArray();
            }
        }

        public byte[] Compress(Stream input)
        {
            return null;
        }
    }
}
