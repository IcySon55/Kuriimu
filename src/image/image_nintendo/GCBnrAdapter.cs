using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace image_nintendo.GCBnr
{
    public sealed class GzfAdapter : IImageAdapter
    {
        private GCBnr _bnr;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "Gamecube Banner";
        public string Description => "Banner for GC games";
        public string Extension => "*.bnr";
        public string About => "This is the BNR image adapter for Kukkii.";

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
                var magic = br.ReadString(4);
                return magic == "BNR1" || magic == "BNR2";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bnr = new GCBnr(FileInfo.FullName);
                _bitmaps = _bnr.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _bnr.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _bnr.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
