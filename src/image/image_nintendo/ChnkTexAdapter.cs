using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_nintendo
{
    public class ChnkTexAdapter : IImageAdapter
    {
        private CHNKTEX _tex = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "CHNKTEX";
        public string Description => "NDS Chunk Texture";
        public string Extension => "*.tex";
        public string About => "This is the NDS Chunk Texture image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "CHNK";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tex = new CHNKTEX(FileInfo.OpenRead());
                _bitmaps = _tex.Bitmaps.Select(b => new ChnkTexBitmapInfo { Bitmap = b, Format= ChnkTexFormat.BGR555 }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            //if (_bitmaps.Count >= 1)
            //    _tex.Txif.Format = ((TximBitmapInfo)_bitmaps[0]).Format;
            _tex.Bitmaps = _bitmaps.Select(b => b.Bitmap).ToList();
            _tex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
