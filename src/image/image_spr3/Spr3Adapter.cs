using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;
using image_nintendo;

namespace image_spr3
{
    public sealed class Spr3Adapter : IImageAdapter
    {
        private SPR3 _spr3;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "SPR3";
        public string Description => "SPR3";
        public string Extension => "*.spr3";
        public string About => "This is the SPR3 image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 0xc) return false;
                return br.PeekString(0x8, 4) == "SPR3";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                using (var br = new BinaryReaderX(FileInfo.OpenRead()))
                    _spr3 = new SPR3(FileInfo.FullName);
                _bitmaps = _spr3.ctpk.bmps.Select(o => new BitmapInfo { Bitmap = o.bmp }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _spr3.ctpk.bmps = _bitmaps.Select(o => new image_nintendo.CTPK.BitmapClass { bmp = o.Bitmap }).ToList();

            _spr3.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
