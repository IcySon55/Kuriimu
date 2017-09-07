using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace image_cvt
{
    public sealed class CvtAdpter : IImageAdapter
    {
        private CVT _cvt;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "CVT";
        public string Description => "Chase inVestigation Texture";
        public string Extension => "*.cvt";
        public string About => "This is the CVT image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 2) return false;
                return br.ReadString(2) == "n";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _cvt = new CVT(FileInfo.OpenRead());
                _bitmaps = _cvt.bmps.Select(b => new CvtBitmapInfo { Name = _cvt.Header.Name, Bitmap = b, Format = _cvt.Header.Format }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _cvt.bmps = _bitmaps.Select(b => b.Bitmap).ToList();
            _cvt.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
