using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Kuriimu.IO;

namespace Kuriimu.Compression
{
    public class ZLib
    {
        public static byte[] Compress(Stream instream, CompressionLevel compressionLevel = CompressionLevel.Optimal, bool leaveOpen = false)
        {
            var inData = new BinaryReaderX(instream, leaveOpen).ReadBytes((int)instream.Length);
            var ms = new MemoryStream();
            ms.Write(new byte[] { 0x78, 0xDA }, 0, 2);
            using (var ds = new DeflateStream(ms, compressionLevel, true))
                ds.Write(inData, 0, inData.Length);
            var adler = inData.Aggregate(Tuple.Create(1, 0), (x, n) => Tuple.Create((x.Item1 + n) % 65521, (x.Item1 + x.Item2 + n) % 65521));
            ms.Write(new[] { (byte)(adler.Item2 >> 8), (byte)adler.Item2, (byte)(adler.Item1 >> 8), (byte)adler.Item1 }, 0, 4);
            return ms.ToArray();
        }

        public static byte[] Decompress(Stream inData)
        {
            using (var br = new BinaryReaderX(inData, true))
            {
                var ms = new MemoryStream();
                br.BaseStream.Position = 2;
                using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length - 6)), CompressionMode.Decompress))
                    ds.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
