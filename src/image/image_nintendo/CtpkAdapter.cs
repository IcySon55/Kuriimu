using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace image_nintendo.CTPK
{
    public sealed class CtpkAdapter : IImageAdapter
    {
        private CTPK _ctpk;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "CTPK";
        public string Description => "CTR Texture PacKage";
        public string Extension => "*.ctpk;*.amt";
        public string About => "This is the CTPK image adapter for Kukkii.";

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
                return br.ReadString(4) == "CTPK";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                using (var br = new BinaryReaderX(FileInfo.OpenRead()))
                    if (br.ReadString(4) == "CTPK")
                        _ctpk = new CTPK(FileInfo.FullName);
                    else
                        _ctpk = new CTPK(FileInfo.FullName, true);
                _bitmaps = _ctpk.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ctpk.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ctpk.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
