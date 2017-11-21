using System;
using System.IO;
using System.IO.Compression;
using System.ComponentModel.Composition;
using System.Linq;
using Komponent.IO;
using Komponent.Interface;

namespace Komponent.Compression
{
    [ExportMetadata("Name", "ZLib")]
    [ExportMetadata("TabPathCompress", "General/ZLib/No Compression,2;General/ZLib/Optimal,0;General/ZLib/Fastest,1")]
    [ExportMetadata("TabPathDecompress", "General/ZLib")]
    [Export("ZLib", typeof(ICompressionCollection))]
    [Export(typeof(ICompressionCollection))]
    public class ZLib : ICompressionCollection
    {
        CompressionLevel method;

        public void SetMethod(byte method)
        {
            this.method = (CompressionLevel)method;
        }

        public byte[] Compress(Stream instream)
        {
            var inData = new BinaryReaderX(instream, true).ReadBytes((int)instream.Length);
            var ms = new MemoryStream();
            ms.Write(new byte[] { 0x78, 0xDA }, 0, 2);
            using (var ds = new DeflateStream(ms, method, true))
                ds.Write(inData, 0, inData.Length);
            var adler = inData.Aggregate(Tuple.Create(1, 0), (x, n) => Tuple.Create((x.Item1 + n) % 65521, (x.Item1 + x.Item2 + n) % 65521));
            ms.Write(new[] { (byte)(adler.Item2 >> 8), (byte)adler.Item2, (byte)(adler.Item1 >> 8), (byte)adler.Item1 }, 0, 4);
            return ms.ToArray();
        }

        public byte[] Decompress(Stream inData, long decompSize = 0)
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
