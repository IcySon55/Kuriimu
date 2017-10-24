using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace image_level5.imgc
{
    public sealed class ImgcAdapter : IImageAdapter
    {
        private Bitmap _imgc = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "IMGC";
        public string Description => "Level 5 Compressed Image";
        public string Extension => "*.xi";
        public string About => "This is the IMGC image adapter for Kukkii.";

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
            {
                _imgc = IMGC.Load(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _imgc } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _imgc = _bitmaps[0].Bitmap;
            IMGC.Save(FileInfo.FullName, _imgc);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
