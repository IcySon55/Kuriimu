using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Kuriimu.Compression
{
    public class ZLib
    {
        public static byte[] Compress(byte[] inData)
        {
            var ms = new MemoryStream();
            ms.Write(new byte[] { 0x78, 0xDA }, 0, 2);
            using (var ds = new DeflateStream(ms, CompressionLevel.Optimal, true))
            {
                ds.Write(inData, 0, inData.Length);
            }
            var adler = inData.Aggregate(Tuple.Create(1, 0), (x, n) => Tuple.Create((x.Item1 + n) % 65521, (x.Item1 + x.Item2 + n) % 65521));
            ms.Write(new[] { (byte)(adler.Item2 >> 8), (byte)adler.Item2, (byte)(adler.Item1 >> 8), (byte)adler.Item1 }, 0, 4);
            return ms.ToArray();
        }

        public static byte[] Decompress(byte[] inData)
        {
            var ms = new MemoryStream();
            using (var ds = new DeflateStream(new MemoryStream(inData, 2, inData.Length - 6), CompressionMode.Decompress))
            {
                ds.CopyTo(ms);
            }
            return ms.ToArray();
        }
    }
}
