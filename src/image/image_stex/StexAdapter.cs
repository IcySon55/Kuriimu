using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using System.ComponentModel;
using Kontract.IO;

namespace image_stex
{
    public sealed class StexAdapter : IImageAdapter
    {
        private STEX _stex = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "STEX";
        public string Description => "Atlus Simple Texture";
        public string Extension => "*.stex";
        public string About => "This is the STEX image adapter for Kukkii.";

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
                return br.ReadString(4) == "STEX";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _stex = new STEX(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new StexBitmapInfo { Bitmap = _stex.bmp, Format = (Support.FormatSwitcher)Enum.Parse(typeof(Support.FormatSwitcher), _stex.settings.Format.FormatName) } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _stex.format = (uint)(_bitmaps[0] as StexBitmapInfo).Format;
            _stex.settings.Format = Support.Format[(uint)(_bitmaps[0] as StexBitmapInfo).Format];
            _stex.bmp = _bitmaps[0].Bitmap;
            _stex.Save(FileInfo.FullName, _stex.bmp);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class StexBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            public Support.FormatSwitcher Format { get; set; }
        }
    }
}
