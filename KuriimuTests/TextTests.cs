using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kuriimu.Contract;

namespace KuriimuTests
{
    [TestClass]
    public class TextTests
    {
        private const string sample_file_path = "../../../sample_files/test_files";
        private const string tmp_path = "tmp";

        public static void Test<T>(string file) where T : ITextAdapter, new()
        {
            Directory.CreateDirectory(Path.Combine(sample_file_path, tmp_path));
            var path1 = Path.Combine(sample_file_path, file);
            var path2 = Path.Combine(sample_file_path, tmp_path, "test-" + Path.GetFileName(file));

            var txt = new T();
            txt.Load(path1);
            txt.Save(path2);
            FileAssert.AreEqual(path1, path2);

            // Delete if successful
            File.Delete(path2);
        }

        // MAPNAME
        [TestMethod]
        public void MapNameTest() => Test<text_srtz.MapNameAdapter>("mapname.bin");
    }
}
