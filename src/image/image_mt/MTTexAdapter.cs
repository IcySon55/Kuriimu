using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace image_mt
{
    public class MTTexAdapter : IImageAdapter
    {
        private MTTEX _tex = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "MTTEX";
        public string Description => "MT Framework Texture";
        public string Extension => "*.tex";
        public string About => "This is the MT Framework image adapter for Kukkii.";

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
                return magic == "TEX" || magic == "\0XET";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tex = new MTTEX(FileInfo.OpenRead());
                _bitmaps = _tex.Bitmaps.Select(b => new MTTexBitmapInfo { Bitmap = b, Format = _tex.HeaderInfo.Format }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            if (_bitmaps.Count >= 1)
                _tex.HeaderInfo.Format = ((MTTexBitmapInfo)_bitmaps[0]).Format;
            _tex.Bitmaps = _bitmaps.Select(b => b.Bitmap).ToList();
            _tex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
