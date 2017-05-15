using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kuriimu.Contract;

namespace KuriimuTests
{
    [TestClass]
    public class ArchiveTests
    {
        private const string sample_file_path = "../../../sample_files/test_files";
        private const string tmp_path = "tmp";

        public static void Test<T>(string file) where T : IArchiveManager, new()
        {
            Directory.CreateDirectory(Path.Combine(sample_file_path, tmp_path));
            var path1 = Path.Combine(sample_file_path, file);
            var path2 = Path.Combine(sample_file_path, tmp_path, "test-" + Path.GetFileName(file));

            var mgr = new T();
            mgr.Load(path1);
            mgr.Save(path2);
            FileAssert.AreEqual(path1, path2);
            mgr.Unload();

            // Delete if successful
            File.Delete(path2);
        }

        public static void HpiHpbTest(string hpiFile)
        {
            Test<archive_hpi_hpb.HpiHpbManager>(hpiFile);

            // Additionally compare the hpb files
            var hpbFile = hpiFile.Remove(hpiFile.Length - 1) + "b";
            var path1 = Path.Combine(sample_file_path, hpbFile);
            var path2 = Path.Combine(sample_file_path, tmp_path, "test-" + Path.GetFileName(hpbFile));
            FileAssert.AreEqual(path1, path2);

            // Delete if successful
            File.Delete(path2);
        }

        // HPIHPB
        [TestMethod]
        public void HpiHpbTest1() => HpiHpbTest("mori1r.hpi");

        [TestMethod]
        public void HpiHpbTest2() => HpiHpbTest("mori2r.hpi");

        [TestMethod]
        public void HpiHpbTest3() => HpiHpbTest("mori5.hpi");

        //PlainMSBT Archive
        [TestMethod]
        public void PlainMSBTTest() => Test<archive_umsbt.UmsbtManager>("EU_English.LZ.decomp");

        //GAR
        [TestMethod]
        public void GarTest() => Test<archive_gar.GarManager>("Joker.Main.gar");

        //ZAR
        [TestMethod]
        public void ZarTest() => Test<archive_zar.ZarManager>("God.zar");

        // SSARC
        [TestMethod]
        public void SimpleSarcTest() => Test<archive_sarc.SimpleSarcManager>("fs2.sarc");

        // SARC
        [TestMethod]
        public void SarcTest1() => Test<archive_sarc.SarcManager>("svn_font.sarc");

        [TestMethod]
        public void SarcTest2() => Test<archive_sarc.SarcManager>("svn_message.sarc");

        [TestMethod]
        public void SarcTest3() => Test<archive_sarc.SarcManager>("MainLang.arc");

        [TestMethod]
        public void SarcTest4() => Test<archive_sarc.SarcManager>("lovelevel.sarc");

        // DARC
        [TestMethod]
        public void DarcTest1() => Test<archive_darc.DarcManager>("Africa.arc");

        [TestMethod]
        public void DarcTest2() => Test<archive_darc.DarcManager>("Australia.arc");

        [TestMethod]
        public void DarcTest3() => Test<archive_darc.DarcManager>("WestAustralia.arc");

        [TestMethod]
        public void DarcTest4() => Test<archive_darc.DarcManager>("BgGtrA_L.arc");

        // CGRP
        [TestMethod]
        public void CgrpTest() => Test<archive_cgrp.CgrpManager>("GROUP_BADMINTON_EN.bcgrp");

        // CTPK
        [TestMethod]
        public void CtpkTest1() => Test<archive_ctpk.CtpkManager>("bt_recipe.ctpk");

        [TestMethod]
        public void CtpkTest2() => Test<archive_ctpk.CtpkManager>("Edit_Ptrn_Key3.ctpk");

        // PCK
        [TestMethod]
        public void PckTest() => Test<archive_pck.PckManager>("A01.pck");

        // XPCK
        [TestMethod]
        public void XpckTest() => Test<archive_xpck.XpckManager>("ef_etc_0000.xc");

        // ARC0 - There is no plugin for these files
        //[TestMethod]
        public void FaTest1() => Test<archive_fa.FaManager>("yw2_a.fa");

        //[TestMethod]
        public void FaTest2() => Test<archive_fa.FaManager>("yw2_lg_en.fa");

        // UMSBT
        [TestMethod]
        public void UmsbtTest() => Test<archive_umsbt.UmsbtManager>("AN_3P_An.umsbt");
    }
}
