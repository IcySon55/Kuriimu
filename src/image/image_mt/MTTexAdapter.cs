using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace image_mt
{
    [FilePluginMetadata(Name = "MTTEX", Description = "MT Framework Texture", Extension = "*.tex",
        Author = "IcySon55", About = "This is the MT Framework image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class MTTexAdapter : IImageAdapter
    {
        private MTTEX _tex = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4) return Identification.False;
                var magic = br.ReadString(4);
                if (magic == "TEX" || magic == "\0XET") return Identification.Raw;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tex = new MTTEX(FileInfo.OpenRead());
                _bitmaps = _tex.Bitmaps.Select(b => new MTTexBitmapInfo { Bitmap = b, Format = _tex.Settings.Format.FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            //if (_bitmaps.Count >= 1)
            //    _tex.HeaderInfo.Format = ((MTTexBitmapInfo)_bitmaps[0]).Format;
            _tex.Bitmaps = _bitmaps.Select(b => b.Bitmap).ToList();
            _tex.Save(FileInfo.Create());
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
