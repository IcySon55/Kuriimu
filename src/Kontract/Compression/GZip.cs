using System;
using System.IO;
using System.IO.Compression;
using Kontract.IO;

namespace Kontract.Compression
{
    public class GZip
    {
        public static byte[] Compress(Stream input, bool size_first = false)
        {
            var ms = new MemoryStream();
            if (size_first) new BinaryWriterX(ms, true).Write((int)input.Length);
            using (var gz = new GZipStream(ms, CompressionLevel.Optimal))
            {
                input.CopyTo(gz);
            }
            return ms.ToArray();
        }

        public static byte[] Decompress(Stream input)
        {
            var ms = new MemoryStream();
            var magic = new byte[2]; input.Read(magic, 0, 2);
            if (magic[0] != 0x1f || magic[0] != 0x8b) input.Position += 2; else input.Position -= 2;
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
