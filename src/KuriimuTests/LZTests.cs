using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kuriimu.Compression;

using static Kuriimu.Compression.LZ10;
using static Kuriimu.Compression.LZ11;
using static Kuriimu.Compression.LZ77;
using static Kuriimu.Compression.RevLZ77;
using static Kuriimu.Compression.LZECD;
using static Kuriimu.Compression.LZ60;
using static Kuriimu.Compression.LZ4;

namespace KuriimuTests
{
    [TestClass]
    public class LZTests
    {
        enum Method : byte
        {
            LZ10, LZ11, LZ60, LZ77, RevLZ77, LZ4, LZECD
        }

        static void Test(byte[] bytes, Method method)
        {
            byte[] bytes2 = new byte[bytes.Length];

            switch (method)
            {
                case Method.LZ10:
                    bytes2 = LZ10.Decompress(new MemoryStream(LZ10.Compress(new MemoryStream(bytes))), bytes.Length);
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.LZ11:
                    bytes2 = LZ11.Decompress(new MemoryStream(LZ11.Compress(new MemoryStream(bytes))), bytes.Length);
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.LZ60:
                    bytes2 = LZ60.Decompress(new MemoryStream(LZ60.Compress(new MemoryStream(bytes))), bytes.Length);
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.LZ77:
                    bytes2 = LZ77.Decompress(new MemoryStream(LZ77.Compress(new MemoryStream(bytes))));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.RevLZ77:
                    bytes2 = RevLZ77.Decompress(new MemoryStream(RevLZ77.Compress(new MemoryStream(bytes))));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.LZ4:
                    bytes2 = LZ4.Decompress(new MemoryStream(LZ4.Compress(new MemoryStream(bytes))), bytes.Length);
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
                case Method.LZECD:
                    bytes2 = LZECD.Decompress(new MemoryStream(LZECD.Compress(new MemoryStream(bytes))));
                    Assert.IsTrue(bytes.SequenceEqual(bytes2));
                    break;
            }
        }

        //LZ10
        [TestMethod]
        public void AsciiHelloWorldLZ10Test() => Test(Encoding.ASCII.GetBytes("hello world"), Method.LZ10);

        [TestMethod]
        public void UnicodeHelloWorldLZ10Test() => Test(Encoding.Unicode.GetBytes("hello world"), Method.LZ10);

        [TestMethod]
        public void HundredZeroesLZ10Test() => Test(new byte[100], Method.LZ10);

        [TestMethod]
        public void AllBytesLZ10Test() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.LZ10);

        //LZ11
        [TestMethod]
        public void AsciiHelloWorldLZ11Test() => Test(Encoding.ASCII.GetBytes("hello world"), Method.LZ11);

        [TestMethod]
        public void UnicodeHelloWorldLZ11Test() => Test(Encoding.Unicode.GetBytes("hello world"), Method.LZ11);

        [TestMethod]
        public void HundredZeroesLZ11Test() => Test(new byte[100], Method.LZ11);

        [TestMethod]
        public void AllBytesLZ11Test() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.LZ11);

        //LZ60
        [TestMethod]
        public void AsciiHelloWorldLZ60Test() => Test(Encoding.ASCII.GetBytes("hello world"), Method.LZ60);

        [TestMethod]
        public void UnicodeHelloWorldLZ60Test() => Test(Encoding.Unicode.GetBytes("hello world"), Method.LZ60);

        [TestMethod]
        public void HundredZeroesLZ60Test() => Test(new byte[100], Method.LZ60);

        [TestMethod]
        public void AllBytesLZ60Test() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.LZ60);

        //LZ4
        [TestMethod]
        public void AsciiHelloWorldLZ4Test() => Test(Encoding.ASCII.GetBytes("hello world"), Method.LZ4);

        [TestMethod]
        public void UnicodeHelloWorldLZ4Test() => Test(Encoding.Unicode.GetBytes("hello world"), Method.LZ4);

        [TestMethod]
        public void HundredZeroesLZ4Test() => Test(new byte[100], Method.LZ4);

        [TestMethod]
        public void AllBytesLZ4Test() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.LZ4);

        //LZ77
        [TestMethod]
        public void AsciiHelloWorldLZ77Test() => Test(Encoding.ASCII.GetBytes("hello world"), Method.LZ77);

        [TestMethod]
        public void UnicodeHelloWorldLZ77Test() => Test(Encoding.Unicode.GetBytes("hello world"), Method.LZ77);

        [TestMethod]
        public void HundredZeroesLZ77Test() => Test(new byte[100], Method.LZ77);

        [TestMethod]
        public void AllBytesLZ77Test() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.LZ77);

        //RevLZ77
        [TestMethod]
        public void AsciiHelloWorldRevLZ77Test() => Test(Encoding.ASCII.GetBytes("hello world"), Method.RevLZ77);

        [TestMethod]
        public void UnicodeHelloWorldRevLZ77Test() => Test(Encoding.Unicode.GetBytes("hello world"), Method.RevLZ77);

        [TestMethod]
        public void HundredZeroesRevLZ77Test() => Test(new byte[100], Method.RevLZ77);

        [TestMethod]
        public void AllBytesRevLZ77Test() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.RevLZ77);

        //LZECD
        [TestMethod]
        public void AsciiHelloWorldLZECDTest() => Test(Encoding.ASCII.GetBytes("hello world"), Method.LZECD);

        [TestMethod]
        public void UnicodeHelloWorldLZECDTest() => Test(Encoding.Unicode.GetBytes("hello world"), Method.LZECD);

        [TestMethod]
        public void HundredZeroesLZECDTest() => Test(new byte[100], Method.LZECD);

        [TestMethod]
        public void AllBytesLZECDTest() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray(), Method.LZECD);
    }
}
