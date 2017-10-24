using System;
using System.IO;
using System.IO.Compression;

namespace Kontract.Compression
{
    public class GZip
    {
        public static byte[] Compress(Stream input)
        {
            var ms = new MemoryStream();
            using (var gz = new GZipStream(ms, CompressionLevel.Optimal))
            {
                input.CopyTo(gz);
            }
            return ms.ToArray();
        }

        public static byte[] Decompress(Stream input)
        {
            var ms = new MemoryStream();
            using (var gz = new GZipStream(input, CompressionMode.Decompress))
            {
                gz.CopyTo(ms);
            }
            return ms.ToArray();
        }

        public static Stream OpenRead(string path)
        {
            var ms = new MemoryStream();
            using (var gz = new GZipStream(File.OpenRead(path), CompressionMode.Decompress))
            {
                gz.CopyTo(ms);
            }
            ms.Position = 0;
            return ms;
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
