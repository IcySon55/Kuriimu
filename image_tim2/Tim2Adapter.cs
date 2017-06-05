using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tim2
{
    public sealed class Tim2Adapter : IImageAdapter
    {
        private TIM2 _tim2 = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "TIM2";
        public string Description => "Default PS2 Image Format v2";
        public string Extension => "*.tm2";
        public string About => "This is the TIM2 image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        Bitmap tmp;

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return (br.ReadString(4) == "TIM2");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _tim2 = new TIM2(FileInfo.OpenRead());

            _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _tim2.bmp } };
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _tim2.bmp = _bitmaps[0].Bitmap;
            //_tim2.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
