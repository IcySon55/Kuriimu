using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KuriimuTests
{
    public static class FileAssert
    {
        public static void AreEqual(string path1, string path2)
        {
            using (var fs1 = File.OpenRead(path1))
            {
                using (var fs2 = File.OpenRead(path2))
                {
                    Assert.AreEqual(fs1.Length, fs2.Length, "The two files have different lengths.");

                    int b1;
                    while ((b1 = fs1.ReadByte()) != -1)
                    {
                        int b2 = fs2.ReadByte();
                        Assert.AreEqual(b1, b2, $"Byte mismatch at position 0x{fs1.Position - 1:X}");
                    }
                }
            }
        }
    }
}
