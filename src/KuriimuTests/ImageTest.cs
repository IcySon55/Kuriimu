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
        //LMT - Not savable for now
        //MODS - video format, not savable
        //MOFLEX - video format, not savable
        //MTTex - samples provided by IcySon55
        //BCFNT - no samples available
        //BCLYT - no real image
        //BXLIM - somehow the alignement doesn't work properly
        //CHNK - Not savable for now
        //GZF - Not savable for now
        //NCGR - Not savable for now
        //QBF - Not savable for now
        //VCG - Not savable for now
        //PicaRg4 - no samples available
        //SPR3 - Only ETC images available. Not testable through our ETC-Encoder
        //TEXI - No samples available
        //TIM2 - Not load-/savable
        //TM2 - Not savable for now

        [TestMethod]
        public void AIFTest() => Test<image_aif.AifAdapter>("com_bg_b1.aif");

        [TestMethod]
        public void COMPTest() => Test<image_comp.CompAdapter>("mz_Name_000.comp");

        [TestMethod]
        public void RawJTEXTest() => Test<image_rawJtex.RawJtexAdapter>("Character.jtex");

        [TestMethod]
        public void ImgcTest() => Test<image_level5.imgc.ImgcAdapter>("AR_Tourbillon.xi");

        [TestMethod]
        public void BchTest() => Test<image_nintendo.BCH.BchAdapter>("por01.bch");

        [TestMethod]
        public void CgfxTest() => Test<image_nintendo.CGFX.CgfxAdapter>("00_PLAYER_M_face_000_normal.bctex");

        [TestMethod]
        public void CtpkTest() => Test<image_nintendo.CTPK.CtpkAdapter>("menu_field_9.ctpk");

        [TestMethod]
        public void StexTest() => Test<image_stex.StexAdapter>("GUILDCARD_05_TROPHY.STEX");

        [TestMethod]
        public void TmxTest() => Test<image_tmx.TmxAdapter>("BE_0001.TMX");
    }
}
