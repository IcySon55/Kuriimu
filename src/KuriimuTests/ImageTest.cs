using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Kontract.Interface;

namespace KuriimuTests
{
    [TestClass]
    public class ImageTest
    {
        private const string sample_file_path = "../../../sample_files/image_files";
        private const string tmp_path = "tmp";

        public static void Test<T>(string file) where T : IImageAdapter, new()
        {
            Directory.CreateDirectory(Path.Combine(sample_file_path, tmp_path));
            var path1 = Path.Combine(sample_file_path, file);
            var path2 = Path.Combine(sample_file_path, tmp_path, "test-" + Path.GetFileName(file));

            var mgr = new T();
            mgr.Load(path1);
            mgr.Save(path2);
            FileAssert.AreEqual(path1, path2);

            // Delete if successful
            File.Delete(path2);
        }

        //BNR - Order of paletted colors couldn't be reconstructed, but they work after saving
        //CTX - unknown build errors (giving an error that namespace is missing, but VS and rebuilding manually doesn't give any build errors. Also Kukkii can handle CTX properly)
        //CTXB - Only ETC images available. Not testable through our ETC-Encoder
        //CVT - Only ETC images available. Not testable through our ETC-Encoder
        //F3XT - no samples available
        //IOBJ - Not revised, no sample files, seems unfinished
        //jIMG - Only ETC images available. Not testable through our ETC-Encoder

        [TestMethod]
        public void AIFTest() => Test<image_aif.AifAdapter>("com_bg_b1.aif");

        [TestMethod]
        public void COMPTest() => Test<image_comp.CompAdapter>("mz_Name_000.comp");

        [TestMethod]
        public void RawJTEXTest() => Test<image_rawJtex.RawJtexAdapter>("Character.jtex");

        [TestMethod]
        public void ImgcTest() => Test<image_level5.imgc.ImgcAdapter>("AR_Tourbillon.xi");
    }
}
