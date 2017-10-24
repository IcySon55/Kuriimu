using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kontract.Compression;

namespace KuriimuTests
{
    [TestClass]
    public class CompHeaderTests
    {
        enum Method : byte
        {
            Ninty, L5
        }

        static void Test(byte[] bytes, Method method)
        {
            var bytes2 = new byte[bytes.Length];

            switch (method)
            {
                case Method.Ninty:
                    bytes2 = Nintendo.Decompress(new MemoryStream(Nintendo.Compress(new MemoryStream(bytes), Nintendo.Method.LZ10)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Nintendo.Decompress(new MemoryStream(Nintendo.Compress(new MemoryStream(bytes), Nintendo.Method.LZ11)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Nintendo.Decompress(new MemoryStream(Nintendo.Compress(new MemoryStream(bytes), Nintendo.Method.Huff4)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Nintendo.Decompress(new MemoryStream(Nintendo.Compress(new MemoryStream(bytes), Nintendo.Method.Huff8)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Nintendo.Decompress(new MemoryStream(Nintendo.Compress(new MemoryStream(bytes), Nintendo.Method.RLE)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.L5:
                    bytes2 = Level5.Decompress(new MemoryStream(Level5.Compress(new MemoryStream(bytes), Level5.Method.NoCompression)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Level5.Decompress(new MemoryStream(Level5.Compress(new MemoryStream(bytes), Level5.Method.LZ10)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Level5.Decompress(new MemoryStream(Level5.Compress(new MemoryStream(bytes), Level5.Method.Huffman4Bit)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Level5.Decompress(new MemoryStream(Level5.Compress(new MemoryStream(bytes), Level5.Method.Huffman8Bit)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    bytes2 = Level5.Decompress(new MemoryStream(Level5.Compress(new MemoryStream(bytes), Level5.Method.RLE)));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
            }
        }

        //Nintendo
        [TestMethod]
        public void HundredZeroesNintyTest() => Test(new byte[100], Method.Ninty);

        //Level 5
        [TestMethod]
        public void HundredZeroesL5Test() => Test(new byte[100], Method.L5);
    }
}
