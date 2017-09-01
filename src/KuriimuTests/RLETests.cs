using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Kuriimu.Compression.RLE;

namespace KuriimuTests
{
    [TestClass]
    public class RLETests
    {
        static void Test(byte[] bytes)
        {
            var bytes2 = Decompress(new MemoryStream(Compress(new MemoryStream(bytes))), bytes.Length);
            Assert.IsTrue(bytes.SequenceEqual(bytes2));
        }

        [TestMethod]
        public void AsciiHelloWorldTest() => Test(Encoding.ASCII.GetBytes("hello world"));

        [TestMethod]
        public void UnicodeHelloWorldTest() => Test(Encoding.Unicode.GetBytes("hello world"));

        [TestMethod]
        public void HundredZeroesTest() => Test(new byte[100]);

        [TestMethod]
        public void AllBytesTest() => Test(Enumerable.Range(0, 256).Select(n => (byte)(n / 0x10)).ToArray());
    }
}
