using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace image_texi
{
    public sealed class TexiAdapter : IImageAdapter
    {
        private TEXI _texi = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "TEXI";
        public string Description => "NLP Texture";
        public string Extension => "*.tex";
        public string About => "This is the TEXI image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var texiName = filename + "i";
            if (!File.Exists(texiName)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(texiName)))
            {
                br.BaseStream.Position = 0x18;
                return br.ReadString(4) == "SERI";
            }
        }

        public void Load(string filename)
        {
            var texiName = filename + "i";
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _texi = new TEXI(FileInfo.OpenRead(), File.OpenRead(texiName));

                var _bmpList = _texi.bmps.Select(o => new TexiBitmapInfo { Bitmap = o, Format = _texi.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            var texiName = filename + "i";

            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _texi.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _texi.Save(File.Create(FileInfo.FullName), File.Create(texiName));
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class TexiBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
