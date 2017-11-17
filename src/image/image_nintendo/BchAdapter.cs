using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace image_nintendo.BCH
{
    public sealed class BchAdapter : IImageAdapter
    {
        private BCH _bch;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "BCH";
        public string Description => "BCH";
        public string Extension => "*.bch";
        public string About => "This is the BCH image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 3) return false;
                return br.ReadString(3) == "BCH";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bch = new BCH(FileInfo.FullName);

                _bitmaps = _bch.bmps.Select((o, i) => new BchBitmapInfo { Bitmap = o, Format = _bch._settings[i].Format.FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _bch.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _bch.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
