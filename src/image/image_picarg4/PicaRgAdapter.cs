using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_picarg4
{
    [FilePluginMetadata(Name = "PicaRg4", Description = "Pica image 4", Extension = "*.lzb",
        Author = "onepiecefreak", About = "This is the PicaRg4 image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class PicaRgAdapter : IImageAdapter
    {
        private PICARG _picaRg = null;
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
                if (br.ReadString(7) == "picaRg4") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _picaRg = new PICARG(FileInfo.OpenRead());

                var _bmpList = _picaRg.bmps.Select(o => new PicaRg4BitmapInfo { Bitmap = o, Format = _picaRg.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _picaRg.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _picaRg.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class PicaRg4BitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
