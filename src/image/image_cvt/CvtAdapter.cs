using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_cvt
{
    [FilePluginMetadata(Name = "CVT", Description = "Chase inVestigation Texture", Extension = "*.cvt",
        Author = "onepiecefreak", About = "This is the CVT image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class CvtAdpter : IImageAdapter
    {
        private CVT _cvt;
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
                if (br.BaseStream.Length < 2) return Identification.False;
                if (br.ReadString(2) == "n") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _cvt = new CVT(FileInfo.OpenRead());
                _bitmaps = _cvt.bmps.Select(b => new CvtBitmapInfo { Name = _cvt.Header.Name, Bitmap = b, Format = _cvt.settings.Format.FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _cvt.bmps = _bitmaps.Select(b => b.Bitmap).ToList();
            _cvt.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
