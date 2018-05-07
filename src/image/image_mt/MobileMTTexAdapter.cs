using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace image_mt.Mobile
{
    public class MobileMTTexAdapter : IImageAdapter
    {
        private MobileMTTEX _tex = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "Mobile MTTEX";
        public string Description => "Mobile MT Framework Texture";
        public string Extension => "*.tex";
        public string About => "This is the Mobile MT Framework image adapter for Kukkii.";

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
                return br.ReadString(4) == "TEX ";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tex = new MobileMTTEX(FileInfo.OpenRead());
                _bitmaps = _tex.bmps.Select((b, i) => new MobileMTTexBitmapInfo { Bitmap = b, Format = _tex.formatNames[i] }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _tex.bmps = _bitmaps.Select(b => b.Bitmap).ToList();
            _tex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
