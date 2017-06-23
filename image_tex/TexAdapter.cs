using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_tex
{
    public class TexAdapter : IImageAdapter
    {
        private TEX _tex = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "TEX";
        public string Description => "MT Framework Texture";
        public string Extension => "*.tex";
        public string About => "This is the MT Framework TEX image adapter for Kukkii. Many thanks to dasding for bootstrapping this plugin.";

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
                return br.ReadString(3) == "TEX";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tex = new TEX(FileInfo.OpenRead());
                _bitmaps = _tex.Bitmaps.Select(b => new TexBitmapInfo { Bitmap = b, Format = _tex.Header.Format }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            if (_bitmaps.Count >= 1)
                _tex.Header.Format = ((TexBitmapInfo)_bitmaps[0]).Format;
            _tex.Bitmaps = _bitmaps.Select(b => b.Bitmap).ToList();
            _tex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
