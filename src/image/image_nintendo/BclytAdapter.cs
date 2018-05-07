using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace image_nintendo.BCLYT
{
    public class BclytAdapter : IImageAdapter
    {
        private BCLYT _bclyt = null;

        #region Properties

        public string Name => "BCLYT";
        public string Description => "Standard Nintendo Layout format";
        public string Extension => "*.bclyt";
        public string About => "This is the BCLYT image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadString(4) == "CLYT";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _bclyt = new BCLYT(FileInfo.OpenRead(), filename);
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                //_bclyt.Save(FileInfo.FullName);
            }
            catch (Exception) { }
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => new List<BitmapInfo> { new BitmapInfo { Bitmap = _bclyt.bmps[0] } };

        public bool ShowProperties(Icon icon) => false;
    }
}
