using System;
using System.IO;
using System.IO.Compression;
using System.ComponentModel.Composition;
using Komponent.Interface;

namespace Komponent.Compression
{
    [ExportMetadata("Name", "GZip")]
    [ExportMetadata("TabPathCompress", "General/GZip")]
    [ExportMetadata("TabPathDecompress", "General/GZip")]
    [Export("GZip", typeof(ICompression))]
    [Export(typeof(ICompression))]
    public class GZip : ICompression
    {
        public byte[] Compress(Stream input)
        {
            var ms = new MemoryStream();
            using (var gz = new GZipStream(ms, CompressionLevel.Optimal))
            {
                input.CopyTo(gz);
            }
            return ms.ToArray();
        }

        public byte[] Decompress(Stream input, long decompSize = 0)
        {
            var ms = new MemoryStream();
            using (var gz = new GZipStream(input, CompressionMode.Decompress))
            {
                gz.CopyTo(ms);
            }
            return ms.ToArray();
        }
    }
}
