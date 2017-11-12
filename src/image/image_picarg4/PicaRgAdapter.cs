using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace image_picarg4
{
    public sealed class PicaRgAdapter : IImageAdapter
    {
        private PICARG _picaRg = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "PicaRg4";
        public string Description => "Pica image 4";
        public string Extension => "*.lzb";
        public string About => "This is the PicaRg4 image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadString(7) == "picaRg4";
            }
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
