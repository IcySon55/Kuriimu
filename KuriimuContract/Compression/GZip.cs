using System;
using System.IO;
using System.IO.Compression;

namespace Kuriimu.Compression
{
    public class GZip
    {
        public static byte[] Compress(byte[] bytes)
        {
            var ms = new MemoryStream();
            using (var gz = new GZipStream(ms, CompressionLevel.Optimal))
            {
                new MemoryStream(bytes).CopyTo(gz);
            }
            return ms.ToArray();
        }

        public static byte[] Decompress(byte[] bytes)
        {
            var ms = new MemoryStream();
            using (var gz = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
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
