using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace image_level5.XF
{
    public sealed class XfAdapter : IImageAdapter
    {
        private Bitmap _xf = null;

        #region Properties

        public string Name => "XF";
        public string Description => "Level 5 Font";
        public string Extension => "*.xf";
        public string About => "This is the XF image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "XPCK" && Path.GetExtension(filename) == ".xf";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _xf = new XF(File.OpenRead(FileInfo.FullName)).bmp;
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                //_xf.Save(FileInfo.Create());
            }
            catch (Exception) { }
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => new List<BitmapInfo> { new BitmapInfo { Bitmap = _xf } };

        public bool ShowProperties(Icon icon) => false;
    }
}
