using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_level5.XI
{
    public sealed class XiAdapter : IImageAdapter
    {
        private Bitmap _xi = null;

        #region Properties

        public string Name => "XI";
        public string Description => "Level 5 Compressed Image";
        public string Extension => "*.xi";
        public string About => "This is the XI image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "IMGC";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _xi = XI.Load(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                XI.Save(FileInfo.FullName, _xi);
            }
            catch (Exception) { }
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => new List<BitmapInfo> { new BitmapInfo { Bitmap = _xi } };

        public bool ShowProperties(Icon icon) => false;
    }
}
