using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kuriimu.Contract;

namespace KuriimuTests
{
    [TestClass]
    public class ArchiveTests
    {
        const string sample_file_path = "../../../sample_files/test_files";
        const string tmp_path = "tmp";

        public static void Test<T>(string file1) where T : IArchiveManager, new()
        {
            Directory.CreateDirectory(Path.Combine(sample_file_path, tmp_path));
            var path1 = Path.Combine(sample_file_path, file1);
            var path2 = Path.Combine(sample_file_path, tmp_path, "test-" + Path.GetFileName(file1));

            var mgr = new T();
            mgr.Load(path1);
            mgr.Save(path2);
            FileAssert.AreEqual(path1, path2);
        }

        [TestMethod]
        public void HpiHpbTest() => Test<archive_hpi_hpb.HpiHpbManager>("mori5.hpi");

        [TestMethod]
        public void SimpleSarcTest() => Test<archive_sarc.SimpleSarcManager>("fs2.sarc");

        [TestMethod]
        public void SarcTest1() => Test<archive_sarc.SarcManager>("svn_font.sarc");

        [TestMethod]
        public void SarcTest2() => Test<archive_sarc.SarcManager>("svn_message.sarc");

        [TestMethod]
        public void DarcTest1() => Test<archive_darc.DARCManager>("Australia.arc");
    }
}
