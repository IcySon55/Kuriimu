using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_nintendo.CTPK
{
    [FilePluginMetadata(Name = "CTPK", Description = "CTR Texture PacKage", Extension = "*.ctpk;*.amt",
        Author = "onepiecefreak", About = "This is the CTPK image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class CtpkAdapter : IImageAdapter
    {
        private CTPK _ctpk;
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
                if (br.ReadString(4) == "CTPK") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _ctpk = new CTPK(FileInfo.OpenRead());
                _bitmaps = _ctpk.bmps.Select((o, i) => new CtpkBitmapInfo { Bitmap = o, Format = _ctpk._settings[i].Format.FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ctpk.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ctpk.Save(FileInfo.Create(), false);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
