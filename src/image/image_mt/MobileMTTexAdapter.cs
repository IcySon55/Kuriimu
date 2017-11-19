using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace image_mt.Mobile
{
    [FilePluginMetadata(Name = "Mobile MTTEX", Description = "Mobile MT Framework Texture", Extension = "*.tex",
        Author = "IcySon55,onepiecefreak", About = "This is the Mobile MT Framework image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class MobileMTTexAdapter : IImageAdapter
    {
        private MobileMTTEX _tex = null;
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
                if (br.ReadString(4) == "TEX ") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tex = new MobileMTTEX(FileInfo.OpenRead());
                _bitmaps = _tex.bmps.Select(b => new MobileMTTexBitmapInfo { Bitmap = b, format = _tex.headerInfo.format }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            if (_bitmaps.Count >= 1)
                _tex.headerInfo.format = ((MobileMTTexBitmapInfo)_bitmaps[0]).format;
            _tex.bmps = _bitmaps.Select(b => b.Bitmap).ToList();
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
