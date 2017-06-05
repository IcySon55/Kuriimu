using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_rawJtex
{
    class RawJtexAdapter : IImageAdapter
    {
        private RawJTEX _rawjtex;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "RawJTEX";
        public string Description => "J Texture without header";
        public string Extension => "*.jtex";
        public string About => "This is the RawJTEX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            return Path.GetExtension(filename) == ".jtex";
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _rawjtex = new RawJTEX(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _rawjtex.Image } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _rawjtex.Image = _bitmaps[0].Bitmap;
            _rawjtex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
