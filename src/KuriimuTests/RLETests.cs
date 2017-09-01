using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kuriimu.Compression;

namespace KuriimuTests
{
    [TestClass]
    public class RLETests
    {
        enum Method : byte
        {
            RLE, Yaz0
        }

        static void Test(byte[] bytes, Method method)
        {
            var bytes2 = new byte[bytes.Length];

            switch (method)
            {
                case Method.RLE:
                    bytes2 = RLE.Decompress(new MemoryStream(RLE.Compress(new MemoryStream(bytes))), bytes.Length);
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.Yaz0:
                    bytes2 = Yaz0.Decompress(new MemoryStream(Yaz0.Compress(new MemoryStream(bytes))));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
            }
        }

        //RLE
        [TestMethod]
        public void AsciiHelloWorldRLETest() => Test(Encoding.ASCII.GetBytes("hello world"), Method.RLE);

        [TestMethod]
        public void UnicodeHelloWorldRLETest() => Test(Encoding.Unicode.GetBytes("hello world"), Method.RLE);

        [TestMethod]
        public void HundredZeroesRLETest() => Test(new byte[100], Method.RLE);

        [TestMethod]
        public void AllBytesRLETest() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.RLE);

        //Yaz0
        [TestMethod]
        public void AsciiHelloWorldYaz0Test() => Test(Encoding.ASCII.GetBytes("hello world"), Method.Yaz0);

        [TestMethod]
        public void UnicodeHelloWorldYaz0Test() => Test(Encoding.Unicode.GetBytes("hello world"), Method.Yaz0);

        [TestMethod]
        public void HundredZeroesYaz0Test() => Test(new byte[100], Method.Yaz0);

        [TestMethod]
        public void AllBytesYaz0Test() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.Yaz0);
    }
}
