using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_spr3.SPR3
{
    [FilePluginMetadata(Name = "SPR3", Description = "SPR3", Extension = "*.spr3",
        Author = "onepiecefreak", About = "This is the SPR3 image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class Spr3Adapter : IImageAdapter
    {
        private SPR3 _spr3;
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
                if (br.BaseStream.Length < 0xc) return Identification.False;
                if (br.PeekString(0x8, 4) == "SPR3") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                using (var br = new BinaryReaderX(FileInfo.OpenRead()))
                    _spr3 = new SPR3(FileInfo.FullName);

                var _bmpList = _spr3.ctpk.bmps.Select((o, i) => new Spr3BitmapInfo { Bitmap = o, Format = _spr3.ctpk._settings[i].Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _spr3.ctpk.bmps = _bitmaps.Select(o => o.Bitmap).ToList();

            _spr3.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class Spr3BitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
