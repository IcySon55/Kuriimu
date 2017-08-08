using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_level5.imga
{
    public sealed class ImgaAdapter : IImageAdapter
    {
        private Bitmap _imga = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "IMGA";
        public string Description => "Level 5 Compressed Image";
        public string Extension => "*.xi";
        public string About => "This is the IMGA image adapter for Kukkii.";

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
                return br.ReadString(4) == "IMGA";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _imga = IMGA.Load(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _imga } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _imga = _bitmaps[0].Bitmap;
            IMGA.Save(FileInfo.FullName, _imga);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
